using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.ViewHelper;
using Nebula.Shared.Models;
using Nebula.Shared.Services;

namespace Nebula.Launcher.ViewModels;

[ViewModelRegister(isSingleton:false)]
public partial class ServerEntryModelView : ViewModelBase
{
    private readonly AuthService _authService = default!;
    private readonly ContentService _contentService = default!;
    private readonly CancellationService _cancellationService = default!;
    private readonly DebugService _debugService = default!;
    private readonly RunnerService _runnerService;

    [ObservableProperty] private bool _runVisible = true;

    public ServerHubInfo ServerHubInfo { get; set; } = default!;

    public ServerEntryModelView() : base()
    {
    }

    public ServerEntryModelView(
        IServiceProvider serviceProvider,
        AuthService authService, 
        ContentService contentService, 
        CancellationService cancellationService,
        DebugService debugService, 
        RunnerService runnerService
        ) : base(serviceProvider)
    {
        _authService = authService;
        _contentService = contentService;
        _cancellationService = cancellationService;
        _debugService = debugService;
        _runnerService = runnerService;
    }

    private Process? _process;

    public async void RunInstance()
    {
        var authProv = _authService.SelectedAuth;
        
        var buildInfo = await _contentService.GetBuildInfo(new RobustUrl(ServerHubInfo.Address), _cancellationService.Token);

        await _runnerService.PrepareRun(buildInfo, _cancellationService.Token);
        
        _process = Process.Start(new ProcessStartInfo()
        {
            FileName = "dotnet.exe",
            Arguments = "./Nebula.Runner.dll",
            Environment = {
                { "ROBUST_AUTH_USERID", authProv?.UserId.ToString() } ,
                { "ROBUST_AUTH_TOKEN",  authProv?.Token.Token } ,
                { "ROBUST_AUTH_SERVER", authProv?.AuthLoginPassword.AuthServer } ,
                { "ROBUST_AUTH_PUBKEY", buildInfo.BuildInfo.Auth.PublicKey } ,
                { "GAME_URL",           ServerHubInfo.Address } ,
                { "AUTH_LOGIN",         authProv?.AuthLoginPassword.Login } ,
            },
            CreateNoWindow = true, UseShellExecute = false
        });
        
        if (_process is null)
        {
            return;
        }
        
        _process.OutputDataReceived += OnOutputDataReceived;
        _process.ErrorDataReceived += OnErrorDataReceived;
        
        _process.Exited += OnExited;
    }

    private void OnExited(object? sender, EventArgs e)
    {
        if (_process is null)
        {
            return;
        }
        
        _process.OutputDataReceived -= OnOutputDataReceived;
        _process.ErrorDataReceived -= OnErrorDataReceived;
        _process.Exited -= OnExited;
        
        _debugService.Log("PROCESS EXIT WITH CODE " + _process.ExitCode);
        
        _process.Dispose();
        _process = null;
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null) _debugService.Error(e.Data);
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null) _debugService.Log(e.Data);
    }


    public void ReadLog()
    {
        
    }

    public void StopInstance()
    {
        _process?.Close();
    }
    
    static string FindDotnetPath()
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        var paths = pathEnv?.Split(System.IO.Path.PathSeparator);
        if (paths != null)
        {
            foreach (var path in paths)
            {
                var dotnetPath = System.IO.Path.Combine(path, "dotnet");
                if (System.IO.File.Exists(dotnetPath))
                {
                    return dotnetPath;
                }
            }
        }

        throw new Exception("Dotnet not found!");
    }
}