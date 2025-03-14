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
    public FileService(DebugService debugService)
    {
        _debugService = debugService;
        ContentFileApi = CreateFileApi("content");
        EngineFileApi = CreateFileApi("engine");
        ManifestFileApi = CreateFileApi("manifest");
        ConfigurationApi = CreateFileApi("config");

        // Some migrating think
        foreach(var file in ContentFileApi.AllFiles){
            if(file.Contains("\\") || !ContentFileApi.TryOpen(file, out var stream)) continue;
            
            ContentFileApi.Save(HashApi.GetManifestPath(file), stream);
            stream.Dispose();
            ContentFileApi.Remove(file);
        }
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

            return new ZipFileApi(zipArchive, "");
        }
        catch (Exception)
        {
            zipStream?.Dispose();
            throw;
        }
    }
}