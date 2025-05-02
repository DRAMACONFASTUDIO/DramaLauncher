using System.Diagnostics.CodeAnalysis;
using Nebula.Shared.FileApis;
using Nebula.Shared.FileApis.Interfaces;
using Nebula.Shared.Models;
using Nebula.Shared.Utils;

namespace Nebula.Shared.Services;

[ServiceRegister]
public sealed class EngineService
{
    private readonly AssemblyService _assemblyService;
    private readonly DebugService _debugService;
    private readonly FileService _fileService;
    private readonly RestService _restService;
    private readonly ConfigurationService _varService;
    private readonly Task _currInfoTask;
    private readonly IReadWriteFileApi _engineFileApi;
    
    private ModulesInfo _modulesInfo = default!;
    private Dictionary<string, EngineVersionInfo> _versionsInfo = default!;

    public EngineService(RestService restService, DebugService debugService, ConfigurationService varService,
        FileService fileService, AssemblyService assemblyService)
    {
        _restService = restService;
        _debugService = debugService;
        _varService = varService;
        _fileService = fileService;
        _assemblyService = assemblyService;

        _engineFileApi = fileService.CreateFileApi("engine");

        _currInfoTask = Task.Run(() => LoadEngineManifest(CancellationToken.None));
    }

    public async Task LoadEngineManifest(CancellationToken cancellationToken)
    {
        _versionsInfo = await LoadExacManifest(CurrentConVar.EngineManifestUrl, CurrentConVar.EngineManifestBackup, cancellationToken);
        _modulesInfo = await LoadExacManifest(CurrentConVar.EngineModuleManifestUrl, CurrentConVar.ModuleManifestBackup, cancellationToken);
    }

    private async Task<T> LoadExacManifest<T>(ConVar<string[]> conVar,ConVar<T> backup,CancellationToken cancellationToken)
    {
        var manifestUrls = _varService.GetConfigValue(conVar)!;

        foreach (var manifestUrl in manifestUrls)
        {
            try
            {
                _debugService.Log("Fetching engine manifest from: " + manifestUrl);
                var info = await _restService.GetAsync<T>(
                    new Uri(manifestUrl), cancellationToken);
            
                _varService.SetConfigValue(backup, info);
                return info;
            }
            catch (Exception e)
            {
                _debugService.Error($"error while attempt fetch engine manifest: {e.Message}");
            }
        }
        
        _debugService.Debug("Trying fallback module manifest...");
        if (!_varService.TryGetConfigValue(backup, out var moduleInfo))
        {
            throw new Exception("No module info data available");
        }
        
        return moduleInfo;
    }

    public EngineBuildInfo? GetVersionInfo(string version)
    {
        CheckAndWaitValidation();

        if (!_versionsInfo.TryGetValue(version, out var foundVersion))
            return null;

        if (foundVersion.RedirectVersion != null)
            return GetVersionInfo(foundVersion.RedirectVersion);

        var bestRid = RidUtility.FindBestRid(foundVersion.Platforms.Keys);
        if (bestRid == null) bestRid = "linux-x64";

        _debugService.Log("Selecting RID" + bestRid);

        return foundVersion.Platforms[bestRid];
    }

    public bool TryGetVersionInfo(string version, [NotNullWhen(true)] out EngineBuildInfo? info)
    {
        info = GetVersionInfo(version);
        return info != null;
    }

    public async Task<AssemblyApi?> EnsureEngine(string version)
    {
        _debugService.Log("Ensure engine " + version);

        if (!TryOpen(version)) await DownloadEngine(version);

        try
        {
            var api = _fileService.OpenZip(version, _engineFileApi);
            if (api != null) return _assemblyService.Mount(api);
        }
        catch (Exception)
        {
            _engineFileApi.Remove(version);
            throw;
        }

        return null;
    }

    public async Task DownloadEngine(string version)
    {
        if (!TryGetVersionInfo(version, out var info))
            return;

        _debugService.Log("Downloading engine version " + version);
        using var client = new HttpClient();
        var s = await client.GetStreamAsync(info.Url);
        _engineFileApi.Save(version, s);
        await s.DisposeAsync();
    }

    public bool TryOpen(string version, [NotNullWhen(true)] out Stream? stream)
    {
        return _engineFileApi.TryOpen(version, out stream);
    }

    public bool TryOpen(string version)
    {
        var a = TryOpen(version, out var stream);
        if (a) stream!.Close();
        return a;
    }

    public EngineBuildInfo? GetModuleBuildInfo(string moduleName, string version)
    {
        CheckAndWaitValidation();

        if (!_modulesInfo.Modules.TryGetValue(moduleName, out var module) ||
            !module.Versions.TryGetValue(version, out var value))
            return null;

        var bestRid = RidUtility.FindBestRid(value.Platforms.Keys);
        if (bestRid == null) throw new Exception("No engine version available for our platform!");

        return value.Platforms[bestRid];
    }

    public bool TryGetModuleBuildInfo(string moduleName, string version, [NotNullWhen(true)] out EngineBuildInfo? info)
    {
        info = GetModuleBuildInfo(moduleName, version);
        return info != null;
    }

    public string ResolveModuleVersion(string moduleName, string engineVersion)
    {
        CheckAndWaitValidation();

        var engineVersionObj = Version.Parse(engineVersion);
        var module = _modulesInfo.Modules[moduleName];
        var selectedVersion = module.Versions.Select(kv => new { Version = Version.Parse(kv.Key), kv.Key, kv })
            .Where(kv => engineVersionObj >= kv.Version)
            .MaxBy(kv => kv.Version);

        if (selectedVersion == null) throw new Exception();

        return selectedVersion.Key;
    }

    public async Task<AssemblyApi?> EnsureEngineModules(string moduleName, string engineVersion)
    {
        var moduleVersion = ResolveModuleVersion(moduleName, engineVersion);
        if (!TryGetModuleBuildInfo(moduleName, moduleVersion, out var buildInfo))
            return null;

        var fileName = ConcatName(moduleName, moduleVersion);

        if (!TryOpen(fileName)) await DownloadEngineModule(moduleName, moduleVersion);

        try
        {
            return _assemblyService.Mount(_fileService.OpenZip(fileName, _engineFileApi) ?? throw new InvalidOperationException($"{fileName} is not exist!"));
        }
        catch (Exception)
        {
            _engineFileApi.Remove(fileName);
            throw;
        }
    }

    public async Task DownloadEngineModule(string moduleName, string moduleVersion)
    {
        if (!TryGetModuleBuildInfo(moduleName, moduleVersion, out var info))
            return;

        _debugService.Log("Downloading engine module version " + moduleVersion);
        using var client = new HttpClient();
        var s = await client.GetStreamAsync(info.Url);
        _engineFileApi.Save(ConcatName(moduleName, moduleVersion), s);
        await s.DisposeAsync();
    }

    public string ConcatName(string moduleName, string moduleVersion)
    {
        return moduleName + "" + moduleVersion;
    }

    private void CheckAndWaitValidation()
    {
        if (_currInfoTask.IsCompleted)
            return;

        _debugService.Debug("thinks is not done yet, please wait");
        _currInfoTask.Wait();
    }
}