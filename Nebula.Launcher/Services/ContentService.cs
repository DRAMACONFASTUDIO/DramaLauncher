using System;
using System.Data;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nebula.Launcher.Models;

namespace Nebula.Launcher.Services;

[ServiceRegister]
public partial class ContentService
{
    private readonly AssemblyService _assemblyService;
    private readonly DebugService _debugService;
    private readonly EngineService _engineService;
    private readonly FileService _fileService;
    private readonly HttpClient _http = new();
    private readonly RestService _restService;
    private readonly ConfigurationService _varService;

    public ContentService(RestService restService, DebugService debugService, ConfigurationService varService,
        FileService fileService, EngineService engineService, AssemblyService assemblyService)
    {
        _restService = restService;
        _debugService = debugService;
        _varService = varService;
        _fileService = fileService;
        _engineService = engineService;
        _assemblyService = assemblyService;
    }

    public async Task<RobustBuildInfo> GetBuildInfo(RobustUrl url, CancellationToken cancellationToken)
    {
        var info = new RobustBuildInfo();
        info.Url = url;
        var bi = await _restService.GetAsync<ServerInfo>(url.InfoUri, cancellationToken);
        if (bi.Value is null) throw new NoNullAllowedException();
        info.BuildInfo = bi.Value;
        Console.WriteLine(info.BuildInfo);
        info.RobustManifestInfo = info.BuildInfo.Build.Acz
            ? new RobustManifestInfo(new RobustPath(info.Url, "manifest.txt"), new RobustPath(info.Url, "download"),
                bi.Value.Build.ManifestHash)
            : new RobustManifestInfo(new Uri(info.BuildInfo.Build.ManifestUrl),
                new Uri(info.BuildInfo.Build.ManifestDownloadUrl), bi.Value.Build.ManifestHash);

        return info;
    }
}