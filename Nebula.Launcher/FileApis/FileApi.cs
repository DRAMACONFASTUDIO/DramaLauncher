using System;
using System.Collections.Generic;
using System.IO;
using Nebula.Launcher.FileApis.Interfaces;

namespace Nebula.Launcher.FileApis;

public class FileApi : IReadWriteFileApi
{
    public string RootPath;

    public FileApi(string rootPath)
    {
        RootPath = rootPath;
    }

    public bool TryOpen(string path, out Stream? stream)
    {
        if (File.Exists(Path.Join(RootPath, path)))
        {
            stream = File.OpenRead(Path.Join(RootPath, path));
            return true;
        }

        stream = null;
        return false;
    }

    public bool Save(string path, Stream input)
    {
        var currPath = Path.Join(RootPath, path);

        var dirInfo = new DirectoryInfo(Path.GetDirectoryName(currPath));
        if (!dirInfo.Exists) dirInfo.Create();

        using var stream = File.OpenWrite(currPath);
        input.CopyTo(stream);
        stream.Flush(true);
        Console.WriteLine(input.Length + " " + stream.Length);
        stream.Close();
        return true;
    }

    public bool Remove(string path)
    {
        if (!Has(path)) return false;
        File.Delete(Path.Join(RootPath, path));
        return true;
    }

    public bool Has(string path)
    {
        var currPath = Path.Join(RootPath, path);
        return File.Exists(currPath);
    }

    public IEnumerable<string> AllFiles => Directory.EnumerateFiles(RootPath, "*.*", SearchOption.AllDirectories);
}