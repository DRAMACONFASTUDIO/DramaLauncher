using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Nebula.Launcher.Models;
using Nebula.Launcher.Services;

namespace Nebula.Launcher.ViewModels;

public partial class ServerEntryModelView : ViewModelBase
{
    
    private readonly IServiceProvider _serviceProvider;
    private readonly RunnerService _runnerService;
    private readonly PopupMessageService _popupMessageService;
    private readonly RestService _restService;

    public ServerHubInfo ServerHubInfo { get; }

    public ServerEntryModelView(IServiceProvider serviceProvider, ServerHubInfo serverHubInfo) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _runnerService = serviceProvider.GetService<RunnerService>()!;
        _popupMessageService = serviceProvider.GetService<PopupMessageService>()!;
        _restService = serviceProvider.GetService<RestService>()!;
        ServerHubInfo = serverHubInfo;
    }

    public async void OnConnectRequired()
    {
        _popupMessageService.PopupInfo("Running server: " + ServerHubInfo.StatusData.Name);
        await _runnerService.RunGame(ServerHubInfo.Address);
    }
}