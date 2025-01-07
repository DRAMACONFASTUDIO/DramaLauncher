using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views.Popup;
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
    private readonly RunnerService _runnerService = default!;
    private readonly PopupMessageService _popupMessageService;

    [ObservableProperty] private bool _runVisible = true;

    public ServerHubInfo ServerHubInfo { get; set; } = default!;
    
    
    private Process? _process;

    public LogPopupModelView CurrLog;

    public ServerEntryModelView() : base()
    {
        CurrLog = GetViewModel<LogPopupModelView>();
    }

    public ServerEntryModelView(
        IServiceProvider serviceProvider,
        AuthService authService, 
        ContentService contentService, 
        CancellationService cancellationService,
        DebugService debugService, 
        RunnerService runnerService, PopupMessageService popupMessageService
        ) : base(serviceProvider)
    {
        _authService = authService;
        _contentService = contentService;
        _cancellationService = cancellationService;
        _debugService = debugService;
        _runnerService = runnerService;
        _popupMessageService = popupMessageService;

        CurrLog = GetViewModel<LogPopupModelView>();
    }

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
            CreateNoWindow = true, 
            UseShellExecute = false, 
            RedirectStandardOutput = true, 
            RedirectStandardError = true, StandardOutputEncoding = Encoding.UTF8
        });
        
        
        if (_process is null)
        {
            return;
        }
        
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
        
        RunVisible = false;
        
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
        RunVisible = true;
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            _debugService.Error(e.Data);
            CurrLog.Append(e.Data);
        }
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            _debugService.Log(e.Data);
            CurrLog.Append(e.Data);
        }
    }


    public void ReadLog()
    {
        _popupMessageService.Popup(CurrLog);
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

public sealed class LogInfo
{
    public string Category { get; set; } =  "LOG";
    public IBrush CategoryColor { get; set; } = Brush.Parse("#424242");
    public string Message { get; set; } = "";

    public static LogInfo FromString(string input)
    {
        var matches = Regex.Matches(input, @"(\[(?<c>.*)\] (?<m>.*))|(?<m>.*)");
        string category = "All";

        if( matches[0].Groups.TryGetValue("c", out var c))
        {
            category = c.Value;
        }

        var color = Brush.Parse("#444444");

        switch (category)
        {
            case "DEBG":
                color = Brush.Parse("#2436d4");
                break;
            case "ERRO":
                color = Brush.Parse("#d42436");
                break;
            case "INFO":
                color = Brush.Parse("#0ab3c9");
                break;
        }
            
        var message = matches[0].Groups["m"].Value;
        return new LogInfo()
        {
            Category = category, Message = message, CategoryColor = color
        };
    }
}

[ViewModelRegister(typeof(LogPopupView), false)]
public sealed class LogPopupModelView : PopupViewModelBase
{
    public LogPopupModelView() : base()
    {
        Logs.Add(new LogInfo()
        {
            Category = "DEBG", Message = "MEOW MEOW TEST"
        });
        
        Logs.Add(new LogInfo()
        {
            Category = "ERRO", Message = "MEOW MEOW TEST 11\naaaaa"
        });
    }

    public LogPopupModelView(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
    
    public override string Title => "LOG";

    public ObservableCollection<LogInfo> Logs { get; } = new();

    public void Append(string str)
    {
        Logs.Add(LogInfo.FromString(str));
    }
}