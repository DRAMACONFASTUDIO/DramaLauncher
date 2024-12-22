using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.Models;
using Nebula.Launcher.Services;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views.Pages;

namespace Nebula.Launcher.ViewModels;

[ViewRegister(typeof(ServerListView))]
public partial class ServerListViewModel : ViewModelBase
{
    public ObservableCollection<ServerInfo> ServerInfos { get; }
    
    [ObservableProperty]
    private ServerInfo? _selectedListItem;

    public ServerListViewModel()
    {
        ServerInfos = new ObservableCollection<ServerInfo>();
    }
    public ServerListViewModel(IServiceProvider serviceProvider, HubService hubService) : base(serviceProvider)
    {
        ServerInfos = new ObservableCollection<ServerInfo>();
        hubService.HubServerChangedEventArgs += HubServerChangedEventArgs;
    }

    private void HubServerChangedEventArgs(HubServerChangedEventArgs obj)
    {
        if (obj.Action == HubServerChangeAction.Add)
        {
            foreach (var info in obj.Items)
            {
                ServerInfos.Add(info);
            }
        }
        else
        {
            foreach (var info in obj.Items)
            {
                ServerInfos.Remove(info);
            }
        }
    }
}