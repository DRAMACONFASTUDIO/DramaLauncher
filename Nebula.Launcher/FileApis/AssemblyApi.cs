using System.Collections.Generic;
using System.IO;
using Robust.LoaderApi;

namespace Nebula.Launcher.FileApis;

public class AssemblyApi : IFileApi
{
    private readonly IFileApi _root;

    public AssemblyApi(IFileApi root)
    {
        _root = root;
    }

    public bool TryOpen(string path, out Stream? stream)
    {
        return _root.TryOpen(path, out stream);
    }

    public IEnumerable<string> AllFiles => _root.AllFiles;
}