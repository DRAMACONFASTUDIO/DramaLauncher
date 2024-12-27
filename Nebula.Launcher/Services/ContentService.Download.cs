using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Nebula.Launcher.FileApis.Interfaces;
using Nebula.Launcher.Models;
using Nebula.Launcher.Utils;

namespace Nebula.Launcher.Services;

public partial class ContentService
{
    public bool CheckManifestExist(RobustManifestItem item)
    {
        return _fileService.ContentFileApi.Has(item.Hash);
    }

    public async Task<List<RobustManifestItem>> EnsureItems(ManifestReader manifestReader, Uri downloadUri,
        CancellationToken cancellationToken)
    {
        List<RobustManifestItem> allItems = [];
        List<RobustManifestItem> items = [];

        while (manifestReader.TryReadItem(out var item))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _debugService.Log("ensuring is cancelled!");
                return [];
            }

            if (!CheckManifestExist(item.Value))
                items.Add(item.Value);
            allItems.Add(item.Value);
        }

        _debugService.Log("Download Count:" + items.Count);

        await Download(downloadUri, items, cancellationToken);

        _fileService.ManifestItems = allItems;

        return allItems;
    }

    public async Task<List<RobustManifestItem>> EnsureItems(RobustManifestInfo info,
        CancellationToken cancellationToken)
    {
        _debugService.Log("Getting manifest: " + info.Hash);

        if (_fileService.ManifestFileApi.TryOpen(info.Hash, out var stream))
        {
            _debugService.Log("Loading manifest from: " + info.Hash);
            return await EnsureItems(new ManifestReader(stream), info.DownloadUri, cancellationToken);
        }

        _debugService.Log("Fetching manifest from: " + info.ManifestUri);

        var response = await _http.GetAsync(info.ManifestUri, cancellationToken);
        if (!response.IsSuccessStatusCode) throw new Exception();

        await using var streamContent = await response.Content.ReadAsStreamAsync(cancellationToken);
        _fileService.ManifestFileApi.Save(info.Hash, streamContent);
        streamContent.Seek(0, SeekOrigin.Begin);
        using var manifestReader = new ManifestReader(streamContent);
        return await EnsureItems(manifestReader, info.DownloadUri, cancellationToken);
    }

    public async Task Unpack(RobustManifestInfo info, IWriteFileApi otherApi, CancellationToken cancellationToken)
    {
        _debugService.Log("Unpack manifest files");
        var items = await EnsureItems(info, cancellationToken);
        foreach (var item in items)
            if (_fileService.ContentFileApi.TryOpen(item.Hash, out var stream))
            {
                _debugService.Log($"Unpack {item.Hash} to: {item.Path}");
                otherApi.Save(item.Path, stream);
                stream.Close();
            }
            else
            {
                _debugService.Error("OH FUCK!! " + item.Path);
            }
    }

    public async Task Download(Uri contentCdn, List<RobustManifestItem> toDownload, CancellationToken cancellationToken)
    {
        if (toDownload.Count == 0 || cancellationToken.IsCancellationRequested)
        {
            _debugService.Log("Nothing to download! Fuck this!");
            return;
        }

        _debugService.Log("Downloading from: " + contentCdn);

        var requestBody = new byte[toDownload.Count * 4];
        var reqI = 0;
        foreach (var item in toDownload)
        {
            BinaryPrimitives.WriteInt32LittleEndian(requestBody.AsSpan(reqI, 4), item.Id);
            reqI += 4;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, contentCdn);
        request.Headers.Add(
            "X-Robust-Download-Protocol",
            _varService.GetConfigValue(CurrentConVar.ManifestDownloadProtocolVersion).ToString(CultureInfo.InvariantCulture));

        request.Content = new ByteArrayContent(requestBody);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("zstd"));
        var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            _debugService.Log("Downloading is cancelled!");
            return;
        }

        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var bandwidthStream = new BandwidthStream(stream);
        stream = bandwidthStream;
        if (response.Content.Headers.ContentEncoding.Contains("zstd"))
            stream = new ZStdDecompressStream(stream);

        await using var streamDispose = stream;

        // Read flags header
        var streamHeader = await stream.ReadExactAsync(4, null);
        var streamFlags = (DownloadStreamHeaderFlags)BinaryPrimitives.ReadInt32LittleEndian(streamHeader);
        var preCompressed = (streamFlags & DownloadStreamHeaderFlags.PreCompressed) != 0;

        // compressContext.SetParameter(ZSTD_cParameter.ZSTD_c_nbWorkers, 4);
        // If the stream is pre-compressed we need to decompress the blobs to verify BLAKE2B hash.
        // If it isn't, we need to manually try re-compressing individual files to store them.
        var compressContext = preCompressed ? null : new ZStdCCtx();
        var decompressContext = preCompressed ? new ZStdDCtx() : null;

        // Normal file header:
        // <int32> uncompressed length
        // When preCompressed is set, we add:
        // <int32> compressed length
        var fileHeader = new byte[preCompressed ? 8 : 4];


        try
        {
            // Buffer for storing compressed ZStd data.
            var compressBuffer = new byte[1024];

            // Buffer for storing uncompressed data.
            var readBuffer = new byte[1024];

            var i = 0;
            foreach (var item in toDownload)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _debugService.Log("Downloading is cancelled!");
                    decompressContext?.Dispose();
                    compressContext?.Dispose();
                    return;
                }

                // Read file header.
                await stream.ReadExactAsync(fileHeader, null);

                var length = BinaryPrimitives.ReadInt32LittleEndian(fileHeader.AsSpan(0, 4));

                EnsureBuffer(ref readBuffer, length);
                var data = readBuffer.AsMemory(0, length);

                // Data to write to database.
                var compression = ContentCompressionScheme.None;
                var writeData = data;

                if (preCompressed)
                {
                    // Compressed length from extended header.
                    var compressedLength = BinaryPrimitives.ReadInt32LittleEndian(fileHeader.AsSpan(4, 4));

                    if (compressedLength > 0)
                    {
                        EnsureBuffer(ref compressBuffer, compressedLength);
                        var compressedData = compressBuffer.AsMemory(0, compressedLength);
                        await stream.ReadExactAsync(compressedData, null);

                        // Decompress so that we can verify hash down below.

                        var decompressedLength = decompressContext!.Decompress(data.Span, compressedData.Span);

                        if (decompressedLength != data.Length)
                            throw new Exception($"Compressed blob {i} had incorrect decompressed size!");

                        // Set variables so that the database write down below uses them.
                        compression = ContentCompressionScheme.ZStd;
                        writeData = compressedData;
                    }
                    else
                    {
                        await stream.ReadExactAsync(data, null);
                    }
                }
                else
                {
                    await stream.ReadExactAsync(data, null);
                }

                if (!preCompressed)
                {
                    // File wasn't pre-compressed. We should try to manually compress it to save space in DB.


                    EnsureBuffer(ref compressBuffer, ZStd.CompressBound(data.Length));
                    var compressLength = compressContext!.Compress(compressBuffer, data.Span);

                    // Don't bother saving compressed data if it didn't save enough space.
                    if (compressLength + 10 < length)
                    {
                        // Set variables so that the database write down below uses them.
                        compression = ContentCompressionScheme.ZStd;
                        writeData = compressBuffer.AsMemory(0, compressLength);
                    }
                }

                using var fileStream = new MemoryStream(data.ToArray());
                _fileService.ContentFileApi.Save(item.Hash, fileStream);

                _debugService.Log("file saved:" + item.Path);
                i += 1;
            }
        }
        finally
        {
            decompressContext?.Dispose();
            compressContext?.Dispose();
        }
    }


    private static void EnsureBuffer(ref byte[] buf, int needsFit)
    {
        if (buf.Length >= needsFit)
            return;

        var newLen = 2 << BitOperations.Log2((uint)needsFit - 1);

        buf = new byte[newLen];
    }
}