using Nebula.Shared.Models;
using Robust.LoaderApi;

namespace Nebula.Shared.Services;

[ServiceRegister]
public sealed class RunnerService(
    ContentService contentService,
    DebugService debugService,
    ConfigurationService varService,
    FileService fileService,
    EngineService engineService,
    AssemblyService assemblyService,
    AuthService authService,
    PopupMessageService popupMessageService, 
    CancellationService cancellationService)
    : IRedialApi
{
    public async Task PrepareRun(RobustUrl url)
    {
        var buildInfo = await contentService.GetBuildInfo(url, cancellationService.Token);
        await PrepareRun(buildInfo, cancellationService.Token);
    }

    public async Task PrepareRun(RobustBuildInfo buildInfo, CancellationToken cancellationToken)
    {
        debugService.Log("Prepare Content!");

        var engine = await engineService.EnsureEngine(buildInfo.BuildInfo.Build.EngineVersion);

        if (engine is null)
            throw new Exception("Engine version is not usable: " + buildInfo.BuildInfo.Build.EngineVersion);
        
        await contentService.EnsureItems(buildInfo.RobustManifestInfo, cancellationToken);
        await engineService.EnsureEngineModules("Robust.Client.WebView", buildInfo.BuildInfo.Build.EngineVersion);
    }

    public async Task Run(string[] runArgs, RobustBuildInfo buildInfo, IRedialApi redialApi,
        CancellationToken cancellationToken)
    {
        debugService.Log("Start Content!");

        var engine = await engineService.EnsureEngine(buildInfo.BuildInfo.Build.EngineVersion);

        if (engine is null)
            throw new Exception("Engine version is not usable: " + buildInfo.BuildInfo.Build.EngineVersion);

        await contentService.EnsureItems(buildInfo.RobustManifestInfo, cancellationToken);

        var extraMounts = new List<ApiMount>
        {
            new(fileService.HashApi, "/")
        };

        var module =
            await engineService.EnsureEngineModules("Robust.Client.WebView", buildInfo.BuildInfo.Build.EngineVersion);
        if (module is not null)
            extraMounts.Add(new ApiMount(module, "/"));

        var args = new MainArgs(runArgs, engine, redialApi, extraMounts);

        if (!assemblyService.TryOpenAssembly(varService.GetConfigValue(CurrentConVar.RobustAssemblyName)!, engine, out var clientAssembly))
            throw new Exception("Unable to locate Robust.Client.dll in engine build!");

        if (!assemblyService.TryGetLoader(clientAssembly, out var loader))
            return;

        await Task.Run(() => loader.Main(args), cancellationToken);
    }
    
    public async Task RunGame(string urlraw)
    {
        var url = new RobustUrl(urlraw);

        using var cancelTokenSource = new CancellationTokenSource();
        var buildInfo = await contentService.GetBuildInfo(url, cancelTokenSource.Token);
        
        var account = authService.SelectedAuth;
        if (account is null)
        {
            popupMessageService.Popup("Error! Auth is required!");
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
        debugService.Debug("Connect to " + url.ToString() + " " + account.AuthLoginPassword.AuthServer);

        await Run(args.ToArray(), buildInfo, this, cancelTokenSource.Token);
    }

    public async void Redial(Uri uri, string text = "")
    {
        //await RunGame(uri.ToString());
    }
}