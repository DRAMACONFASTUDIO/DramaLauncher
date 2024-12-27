using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nebula.Launcher.Models;
using Robust.LoaderApi;

namespace Nebula.Launcher.Services;

[ServiceRegister]
public class RunnerService: IRedialApi
{
    private readonly AssemblyService _assemblyService;
    private readonly AuthService _authService;
    private readonly PopupMessageService _popupMessageService;
    private readonly ContentService _contentService;
    private readonly DebugService _debugService;
    private readonly EngineService _engineService;
    private readonly FileService _fileService;
    private readonly ConfigurationService _varService;

    public RunnerService(ContentService contentService, DebugService debugService, ConfigurationService varService,
        FileService fileService, EngineService engineService, AssemblyService assemblyService, AuthService authService,
        PopupMessageService popupMessageService)
    {
        _contentService = contentService;
        _debugService = debugService;
        _varService = varService;
        _fileService = fileService;
        _engineService = engineService;
        _assemblyService = assemblyService;
        _authService = authService;
        _popupMessageService = popupMessageService;
    }

    public async Task Run(string[] runArgs, RobustBuildInfo buildInfo, IRedialApi redialApi,
        CancellationToken cancellationToken)
    {
        _debugService.Log("Start Content!");

        var engine = await _engineService.EnsureEngine(buildInfo.BuildInfo.Build.EngineVersion);

        if (engine is null)
            throw new Exception("Engine version is not usable: " + buildInfo.BuildInfo.Build.EngineVersion);

        await _contentService.EnsureItems(buildInfo.RobustManifestInfo, cancellationToken);

        var extraMounts = new List<ApiMount>
        {
            new(_fileService.HashApi, "/")
        };

        var module =
            await _engineService.EnsureEngineModules("Robust.Client.WebView", buildInfo.BuildInfo.Build.EngineVersion);
        if (module is not null)
            extraMounts.Add(new ApiMount(module, "/"));

        var args = new MainArgs(runArgs, engine, redialApi, extraMounts);

        if (!_assemblyService.TryOpenAssembly(_varService.GetConfigValue(CurrentConVar.RobustAssemblyName)!, engine, out var clientAssembly))
            throw new Exception("Unable to locate Robust.Client.dll in engine build!");

        if (!_assemblyService.TryGetLoader(clientAssembly, out var loader))
            return;

        await Task.Run(() => loader.Main(args), cancellationToken);
    }
    
    public async Task RunGame(string urlraw)
    {
        var url = new RobustUrl(urlraw);

        using var cancelTokenSource = new CancellationTokenSource();
        var buildInfo = await _contentService.GetBuildInfo(url, cancelTokenSource.Token);
        
        var account = _authService.SelectedAuth;
        if (account is null)
        {
            _popupMessageService.PopupInfo("Error! Auth is required!");
            return;
        }

        if (buildInfo.BuildInfo.Auth.Mode != "Disabled")
        {
            Environment.SetEnvironmentVariable("ROBUST_AUTH_TOKEN", account.Token.Token);
            Environment.SetEnvironmentVariable("ROBUST_AUTH_USERID", account.UserId.ToString());
            Environment.SetEnvironmentVariable("ROBUST_AUTH_PUBKEY", buildInfo.BuildInfo.Auth.PublicKey);
            Environment.SetEnvironmentVariable("ROBUST_AUTH_SERVER", account.AuthLoginPassword.AuthServer);
        }

        var args = new List<string>
        {
            // Pass username to launched client.
            // We don't load username from client_config.toml when launched via launcher.
            "--username", account.AuthLoginPassword.Login,

            // Tell game we are launcher
            "--cvar", "launch.launcher=true"
        };

        var connectionString = url.ToString();
        if (!string.IsNullOrEmpty(buildInfo.BuildInfo.ConnectAddress))
            connectionString = buildInfo.BuildInfo.ConnectAddress;

        // We are using the launcher. Don't show main menu etc..
        // Note: --launcher also implied --connect.
        // For this reason, content bundles do not set --launcher.
        args.Add("--launcher");

        args.Add("--connect-address");
        args.Add(connectionString);

        args.Add("--ss14-address");
        args.Add(url.ToString());
        _debugService.Debug("Connect to " + url.ToString());

        await Run(args.ToArray(), buildInfo, this, cancelTokenSource.Token);
    }

    public async void Redial(Uri uri, string text = "")
    {
        await RunGame(uri.ToString());
    }
}