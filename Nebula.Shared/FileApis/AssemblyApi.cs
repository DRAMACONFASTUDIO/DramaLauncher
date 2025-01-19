using System.Diagnostics.CodeAnalysis;
using Robust.LoaderApi;

namespace Nebula.Shared.FileApis;

public class AssemblyApi : IFileApi
{
    private readonly IFileApi _root;

    public AssemblyApi(IFileApi root)
    {
        _root = root;
    }

    public bool TryOpen(string path,[NotNullWhen(true)] out Stream? stream)
    {
        return _root.TryOpen(path, out stream);
    }

    public IEnumerable<string> AllFiles => _root.AllFiles;
}