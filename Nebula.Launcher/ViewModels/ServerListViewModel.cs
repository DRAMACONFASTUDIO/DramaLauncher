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
    public ObservableCollection<ServerHubInfo> ServerInfos { get; }
    
    [ObservableProperty]
    private ServerHubInfo? _selectedListItem;
    
    //Design think
    public ServerListViewModel()
    {
        ServerInfos = new ObservableCollection<ServerHubInfo>();
        ServerInfos.Add(new ServerHubInfo("ss14://localhost",new ServerStatus("","TestCraft", [], "super", 12,55,1,false,DateTime.Now, 20),[]));
    }
    
    //real think
    public ServerListViewModel(IServiceProvider serviceProvider, HubService hubService) : base(serviceProvider)
    {
        ServerInfos = new ObservableCollection<ServerHubInfo>();
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