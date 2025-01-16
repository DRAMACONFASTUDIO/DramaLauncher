using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Media;
using Nebula.Launcher.Services;
using Nebula.Launcher.ViewModels.Popup;
using Nebula.Launcher.Views;
using Nebula.Shared.Models;
using Nebula.Shared.Services;

namespace Nebula.Launcher.ViewModels;

[ViewModelRegister(typeof(ServerEntryView), isSingleton: false)]
[ConstructGenerator]
public partial class ServerEntryModelView : ViewModelBase
{
    private Process? _p;


    public LogPopupModelView CurrLog;
    [GenerateProperty] private AuthService AuthService { get; } = default!;
    [GenerateProperty] private ContentService ContentService { get; } = default!;
    [GenerateProperty] private CancellationService CancellationService { get; } = default!;
    [GenerateProperty] private DebugService DebugService { get; } = default!;
    [GenerateProperty] private RunnerService RunnerService { get; } = default!;
    [GenerateProperty] private PopupMessageService PopupMessageService { get; } = default!;
    [GenerateProperty] private ViewHelperService ViewHelperService { get; } = default!;

    public bool RunVisible => Process == null;

    public ServerHubInfo ServerHubInfo { get; set; } = default!;

    private Process? Process
    {
        get => _p;
        set
        {
            _p = value;
            OnPropertyChanged(nameof(RunVisible));
        }
    }

    protected override void InitialiseInDesignMode()
    {
        
    }

    protected override void Initialise()
    {
        CurrLog = ViewHelperService.GetViewModel<LogPopupModelView>();
    }

    public void RunInstance()
    {
        Task.Run(RunAsync);
    }

    public async Task RunAsync()
    {
        try
        {
            var authProv = AuthService.SelectedAuth;

            var buildInfo =
                await ContentService.GetBuildInfo(new RobustUrl(ServerHubInfo.Address), CancellationService.Token);

            using (var loadingContext = ViewHelperService.GetViewModel<LoadingContextViewModel>())
            {
                loadingContext.LoadingName = "Loading instance...";
                ((ILoadingHandler)loadingContext).AppendJob();

                PopupMessageService.Popup(loadingContext);

                await RunnerService.PrepareRun(buildInfo, loadingContext, CancellationService.Token);

                Process = Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet.exe",
                    Arguments = "./Nebula.Runner.dll",
                    Environment =
                    {
                        { "ROBUST_AUTH_USERID", authProv?.UserId.ToString() },
                        { "ROBUST_AUTH_TOKEN", authProv?.Token.Token },
                        { "ROBUST_AUTH_SERVER", authProv?.AuthLoginPassword.AuthServer },
                        { "ROBUST_AUTH_PUBKEY", buildInfo.BuildInfo.Auth.PublicKey },
                        { "GAME_URL", ServerHubInfo.Address },
                        { "AUTH_LOGIN", authProv?.AuthLoginPassword.Login }
                    },
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8
                });

                ((ILoadingHandler)loadingContext).AppendResolvedJob();
            }

            if (Process is null) return;

            Process.EnableRaisingEvents = true;

            Process.BeginOutputReadLine();
            Process.BeginErrorReadLine();

            Process.OutputDataReceived += OnOutputDataReceived;
            Process.ErrorDataReceived += OnErrorDataReceived;

            Process.Exited += OnExited;
        }
        catch (TaskCanceledException)
        {
            PopupMessageService.Popup("Task canceled");
        }
        catch (Exception e)
        {
            PopupMessageService.Popup(e);
        }
    }

    private void OnExited(object? sender, EventArgs e)
    {
        if (Process is null) return;

        Process.OutputDataReceived -= OnOutputDataReceived;
        Process.ErrorDataReceived -= OnErrorDataReceived;
        Process.Exited -= OnExited;

        DebugService.Log("PROCESS EXIT WITH CODE " + Process.ExitCode);

        Process.Dispose();
        Process = null;
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            DebugService.Error(e.Data);
            CurrLog.Append(e.Data);
        }
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            DebugService.Log(e.Data);
            CurrLog.Append(e.Data);
        }
    }


    public void ReadLog()
    {
        PopupMessageService.Popup(CurrLog);
    }

    public void StopInstance()
    {
        Process?.CloseMainWindow();
    }

    private static string FindDotnetPath()
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        var paths = pathEnv?.Split(Path.PathSeparator);
        if (paths != null)
            foreach (var path in paths)
            {
                var dotnetPath = Path.Combine(path, "dotnet");
                if (File.Exists(dotnetPath)) return dotnetPath;
            }

        throw new Exception("Dotnet not found!");
    }
}

public sealed class LogInfo
{
    public string Category { get; set; } = "LOG";
    public IBrush CategoryColor { get; set; } = Brush.Parse("#424242");
    public string Message { get; set; } = "";

    public static LogInfo FromString(string input)
    {
        var matches = Regex.Matches(input, @"(\[(?<c>.*)\] (?<m>.*))|(?<m>.*)");
        var category = "All";

        if (matches[0].Groups.TryGetValue("c", out var c)) category = c.Value;

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
        return new LogInfo
        {
            Category = category, Message = message, CategoryColor = color
        };
    }
}