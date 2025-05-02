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
    
    public FileService(DebugService debugService)
    {
        _debugService = debugService;

        if(!Directory.Exists(RootPath)) 
            Directory.CreateDirectory(RootPath);
    }
    
    public IReadWriteFileApi CreateFileApi(string path)
    {
        return new FileApi(Path.Join(RootPath, path));
    }
    
    public IReadWriteFileApi EnsureTempDir(out string path)
    {
        path = Path.Combine(Path.GetTempPath(), "tempThink"+Path.GetRandomFileName());
        Directory.CreateDirectory(path);
        return new FileApi(path);
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