using Nebula.Shared.Models;
using Robust.LoaderApi;

namespace Nebula.Shared.FileApis;

public class HashApi : IFileApi
{
    private readonly IFileApi _fileApi;
    public Dictionary<string, RobustManifestItem> Manifest;

    public HashApi(List<RobustManifestItem> manifest, IFileApi fileApi)
    {
        _fileApi = fileApi;
        Manifest = new Dictionary<string, RobustManifestItem>();
        foreach (var item in manifest) Manifest.TryAdd(item.Path, item);
    }

    public bool TryOpen(string path, out Stream? stream)
    {
        if (path[0] == '/') path = path.Substring(1);

        if (Manifest.TryGetValue(path, out var a) && _fileApi.TryOpen(a.Hash, out stream))
            return true;

        stream = null;
        return false;
    }

    public IEnumerable<string> AllFiles => Manifest.Keys;
}