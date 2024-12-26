using System;
using System.Collections.Generic;
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
    public ObservableCollection<ServerHubInfo> ServerInfos { get; } = new ObservableCollection<ServerHubInfo>();
    
    [ObservableProperty]
    private ServerHubInfo? _selectedListItem;

    private List<ServerHubInfo> UnsortedServers { get; } = new List<ServerHubInfo>();
    
    //Design think
    public ServerListViewModel()
    {
        ServerInfos.Add(new ServerHubInfo("ss14://localhost",new ServerStatus("Nebula","TestCraft", ["16+","RU"], "super", 12,55,1,false,DateTime.Now, 20),[]));
    }
    
    //real think
    public ServerListViewModel(IServiceProvider serviceProvider, HubService hubService) : base(serviceProvider)
    {
        hubService.HubServerChangedEventArgs += HubServerChangedEventArgs;
    }

    private void HubServerChangedEventArgs(HubServerChangedEventArgs obj)
    {
        if (obj.Action == HubServerChangeAction.Add)
        {
            foreach (var info in obj.Items)
            {
                UnsortedServers.Add(info);
            }
        }
        else
        {
            foreach (var info in obj.Items)
            {
                UnsortedServers.Remove(info);
            }
        }
        
        ServerInfos.Clear();
        UnsortedServers.Sort(new ServerComparer());
        foreach (var VARIABLE in UnsortedServers)
        {
            ServerInfos.Add(VARIABLE);
        }
    }
}

public class ServerComparer : IComparer<ServerHubInfo>
{
    public int Compare(ServerHubInfo? x, ServerHubInfo? y)
    {
        if (ReferenceEquals(x, y))
            return 0;
        if (ReferenceEquals(null, y))
            return 1;
        if (ReferenceEquals(null, x))
            return -1;

        return y.StatusData.Players.CompareTo(x.StatusData.Players);

    }
}