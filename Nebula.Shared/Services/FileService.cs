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

        if(!Directory.Exists(RootPath)) 
            Directory.CreateDirectory(RootPath);

        ContentFileApi = CreateFileApi("content");
        EngineFileApi = CreateFileApi("engine");
        ManifestFileApi = CreateFileApi("manifest");
        ConfigurationApi = CreateFileApi("config");
    }

    public bool CheckMigration(ILoadingHandler loadingHandler)
    {
        _debugService.Log("Checking migration...");

        var migrationList = ContentFileApi.AllFiles.Where(f => !f.Contains("\\")).ToList();
        if(migrationList.Count == 0) return false;
        
        _debugService.Log($"Found {migrationList.Count} migration files. Starting migration...");
        Task.Run(() => DoMigration(loadingHandler, migrationList));
        return true;
    }

    private void DoMigration(ILoadingHandler loadingHandler, List<string> migrationList)
    {
        loadingHandler.SetJobsCount(migrationList.Count);
        
        Parallel.ForEach(migrationList, (f,_)=>MigrateFile(f,loadingHandler));
        
        if (loadingHandler is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private void MigrateFile(string file, ILoadingHandler loadingHandler)
    {
        if(!ContentFileApi.TryOpen(file, out var stream))
        {
            loadingHandler.AppendResolvedJob();
            return;
        }
            
        ContentFileApi.Save(HashApi.GetManifestPath(file), stream);
        stream.Dispose();
        ContentFileApi.Remove(file);
        loadingHandler.AppendResolvedJob();
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

public sealed class ConsoleLoadingHandler : ILoadingHandler
{
    private int _currJobs;

    private float _percent;
    private int _resolvedJobs;

    public void SetJobsCount(int count)
    {
        _currJobs = count;

        UpdatePercent();
        Draw();
    }

    public int GetJobsCount()
    {
        return _currJobs;
    }

    public void SetResolvedJobsCount(int count)
    {
        _resolvedJobs = count;

        UpdatePercent();
        Draw();
    }

    public int GetResolvedJobsCount()
    {
        return _resolvedJobs;
    }

    private void UpdatePercent()
    {
        if (_currJobs == 0)
        {
            _percent = 0;
            return;
        }

        if (_resolvedJobs > _currJobs) return;

        _percent = _resolvedJobs / (float)_currJobs;
    }

    private void Draw()
    {
        var barCount = 10;
        var fullCount = (int)(barCount * _percent);
        var emptyCount = barCount - fullCount;

        Console.Write("\r");

        for (var i = 0; i < fullCount; i++) Console.Write("#");

        for (var i = 0; i < emptyCount; i++) Console.Write(" ");

        Console.Write($"\t {_resolvedJobs}/{_currJobs}");
    }
}