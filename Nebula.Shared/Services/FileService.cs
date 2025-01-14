using System.IO.Compression;
using System.Runtime.InteropServices;
using Nebula.Shared.FileApis;
using Nebula.Shared.FileApis.Interfaces;
using Nebula.Shared.Models;
using Robust.LoaderApi;

namespace Nebula.Shared.Services;

[ServiceRegister]
public class FileService
{
    public static readonly string RootPath = Path.Join(Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData), "Datum");

    private readonly DebugService _debugService;
    public readonly IReadWriteFileApi ConfigurationApi;

    public readonly IReadWriteFileApi ContentFileApi;
    public readonly IReadWriteFileApi EngineFileApi;
    public readonly IReadWriteFileApi ManifestFileApi;

    private HashApi? _hashApi;

    public FileService(DebugService debugService)
    {
        _debugService = debugService;
        ContentFileApi = CreateFileApi("content");
        EngineFileApi = CreateFileApi("engine");
        ManifestFileApi = CreateFileApi("manifest");
        ConfigurationApi = CreateFileApi("config");
    }

    public List<RobustManifestItem> ManifestItems
    {
        set => _hashApi = new HashApi(value, ContentFileApi);
    }

    public HashApi HashApi
    {
        get
        {
            if (_hashApi is null) throw new Exception("Hash API is not initialized!");
            return _hashApi;
        }
        set => _hashApi = value;
    }

    public IReadWriteFileApi CreateFileApi(string path)
    {
        return new FileApi(Path.Join(RootPath, path));
    }

    public ZipFileApi? OpenZip(string path, IFileApi fileApi)
    {
        Stream? zipStream = null;
        try
        {
            if (!fileApi.TryOpen(path, out zipStream))
                return null;

            var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);

            var prefix = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) prefix = "Space Station 14.app/Contents/Resources/";
            return new ZipFileApi(zipArchive, prefix);
        }
        catch (Exception e)
        {
            zipStream?.Dispose();
            throw;
        }
    }
}