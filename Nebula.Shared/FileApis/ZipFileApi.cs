using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Robust.LoaderApi;

namespace Nebula.Shared.FileApis;

public sealed class ZipFileApi : IFileApi
{
    private readonly ZipArchive _archive;
    private readonly string? _prefix;

    public ZipFileApi(ZipArchive archive, string? prefix)
    {
        _archive = archive;
        _prefix = prefix;
    }

    public bool TryOpen(string path, [NotNullWhen(true)] out Stream? stream)
    {
        var entry = _archive.GetEntry(_prefix != null ? _prefix + path : path);
        if (entry == null)
        {
            stream = null;
            return false;
        }

        stream = new MemoryStream();
        lock (_archive)
        {
            using var zipStream = entry.Open();
            zipStream.CopyTo(stream);
        }

        stream.Position = 0;
        return true;
    }

    public IEnumerable<string> AllFiles
    {
        get
        {
            if (_prefix != null)
                return _archive.Entries
                    .Where(e => e.Name != "" && e.FullName.StartsWith(_prefix))
                    .Select(e => e.FullName[_prefix.Length..]);
            return _archive.Entries
                .Where(e => e.Name != "")
                .Select(entry => entry.FullName);
        }
    }

    public static ZipFileApi FromPath(string path)
    {
        var zipArchive = new ZipArchive(File.OpenRead(path), ZipArchiveMode.Read);

        var prefix = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) prefix = "Space Station 14.app/Contents/Resources/";
        return new ZipFileApi(zipArchive, prefix);
    }
}