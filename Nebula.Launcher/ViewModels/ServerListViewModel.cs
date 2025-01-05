using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views.Pages;
using Nebula.Shared.Models;
using Nebula.Shared.Services;

namespace Nebula.Launcher.ViewModels;

[ViewModelRegister(typeof(ServerListView))]
public partial class ServerListViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HubService _hubService;
    public ObservableCollection<ServerEntryModelView> ServerInfos { get; } = new();

    public Action? OnSearchChange;
    
    [ObservableProperty] private string _searchText;
    private List<ServerHubInfo> UnsortedServers { get; } = new();
    
    //Design think
    public ServerListViewModel()
    {
        ServerInfos.Add(CreateServerView(new ServerHubInfo("ss14://localhost",new ServerStatus("Nebula","TestCraft", ["16+","RU"], "super", 12,55,1,false,DateTime.Now, 20),[])));
    }
    
    //real think
    public ServerListViewModel(IServiceProvider serviceProvider, HubService hubService) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _hubService = hubService;
        hubService.HubServerChangedEventArgs += HubServerChangedEventArgs;
        OnSearchChange += OnChangeSearch;
    }

    private void OnChangeSearch()
    {
        SortServers();
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
        if(obj.Action == HubServerChangeAction.Remove)
        {
            foreach (var info in obj.Items)
            {
                UnsortedServers.Remove(info);
            }
        }
        if(obj.Action == HubServerChangeAction.Clear)
        {
            UnsortedServers.Clear();
        }
        
        SortServers();
    }

    private void SortServers()
    {
        ServerInfos.Clear();
        UnsortedServers.Sort(new ServerComparer());
        foreach (var server in UnsortedServers.Where(CheckServerThink))
        {
            ServerInfos.Add(CreateServerView(server));
        }
    }

    private bool CheckServerThink(ServerHubInfo hubInfo)
    {
        if (string.IsNullOrEmpty(SearchText)) return true;
        return hubInfo.StatusData.Name.ToLower().Contains(SearchText.ToLower());
    }

    private ServerEntryModelView CreateServerView(ServerHubInfo serverHubInfo)
    {
        var svn = GetViewModel<ServerEntryModelView>();
        svn.ServerHubInfo = serverHubInfo;
        return svn;
    }

    public void FilterRequired()
    {
        
    }

    public void UpdateRequired()
    {
        _hubService.UpdateHub();
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