using System.Data;
using Nebula.Shared.Models;

namespace Nebula.Shared.Services;

[ServiceRegister]
public partial class ContentService(
    RestService restService,
    DebugService debugService,
    ConfigurationService varService,
    FileService fileService)
{
    private readonly HttpClient _http = new();

    public async Task<RobustBuildInfo> GetBuildInfo(RobustUrl url, CancellationToken cancellationToken)
    {
        var info = new RobustBuildInfo();
        info.Url = url;
        var bi = await restService.GetAsync<ServerInfo>(url.InfoUri, cancellationToken);
        info.BuildInfo = bi;
        info.RobustManifestInfo = info.BuildInfo.Build.Acz
            ? new RobustManifestInfo(new RobustPath(info.Url, "manifest.txt"), new RobustPath(info.Url, "download"),
                bi.Build.ManifestHash)
            : new RobustManifestInfo(new Uri(info.BuildInfo.Build.ManifestUrl),
                new Uri(info.BuildInfo.Build.ManifestDownloadUrl), bi.Build.ManifestHash);

        return info;
    }
}