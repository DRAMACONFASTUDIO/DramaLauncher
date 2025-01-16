using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.Services;
using Nebula.Launcher.Views.Pages;
using Nebula.Shared.Models;
using Nebula.Shared.Services;

namespace Nebula.Launcher.ViewModels.Pages;

[ViewModelRegister(typeof(ServerListView))]
[ConstructGenerator]
public partial class ServerListViewModel : ViewModelBase
{
    [ObservableProperty] private string _searchText = string.Empty;

    public Action? OnSearchChange;
    [GenerateProperty] private HubService HubService { get; } = default!;
    [GenerateProperty] private ViewHelperService ViewHelperService { get; } = default!;
    public ObservableCollection<ServerEntryModelView> ServerInfos { get; } = new();
    private List<ServerHubInfo> UnsortedServers { get; } = new();

    //Design think
    protected override void InitialiseInDesignMode()
    {
        ServerInfos.Add(CreateServerView(new ServerHubInfo("ss14://localhost",
            new ServerStatus("Nebula", "TestCraft", ["16+", "RU"], "super", 12, 55, 1, false, DateTime.Now, 20), [])));
        ServerInfos.Add(CreateServerView(new ServerHubInfo("ss14://localhost",
            new ServerStatus("Nebula", "TestCraft", ["16+", "RU"], "super", 12, 55, 1, false, DateTime.Now, 20), [])));
        ServerInfos.Add(CreateServerView(new ServerHubInfo("ss14://localhost",
            new ServerStatus("Nebula", "TestCraft", ["16+", "RU"], "super", 12, 55, 1, false, DateTime.Now, 20), [])));
    }

    //real think
    protected override void Initialise()
    {
        foreach (var info in HubService.ServerList) UnsortedServers.Add(info);

        HubService.HubServerChangedEventArgs += HubServerChangedEventArgs;
        HubService.HubServerLoaded += HubServerLoaded;
        OnSearchChange += OnChangeSearch;

        if (!HubService.IsUpdating) SortServers();
    }

    private void HubServerLoaded()
    {
        SortServers();
    }

    private void OnChangeSearch()
    {
        SortServers();
    }

    private void HubServerChangedEventArgs(HubServerChangedEventArgs obj)
    {
        if (obj.Action == HubServerChangeAction.Add)
            foreach (var info in obj.Items)
                UnsortedServers.Add(info);
        if (obj.Action == HubServerChangeAction.Remove)
            foreach (var info in obj.Items)
                UnsortedServers.Remove(info);
        if (obj.Action == HubServerChangeAction.Clear) UnsortedServers.Clear();
    }

    private void SortServers()
    {
        Task.Run(() =>
        {
            ServerInfos.Clear();
            UnsortedServers.Sort(new ServerComparer());
            foreach (var server in UnsortedServers.Where(CheckServerThink)) ServerInfos.Add(CreateServerView(server));
        });
    }

    private bool CheckServerThink(ServerHubInfo hubInfo)
    {
        if (string.IsNullOrEmpty(SearchText)) return true;
        return hubInfo.StatusData.Name.ToLower().Contains(SearchText.ToLower());
    }

    private ServerEntryModelView CreateServerView(ServerHubInfo serverHubInfo)
    {
        var svn = ViewHelperService.GetViewModel<ServerEntryModelView>();
        svn.ServerHubInfo = serverHubInfo;
        return svn;
    }

    public void FilterRequired()
    {
    }

    public void UpdateRequired()
    {
        Task.Run(HubService.UpdateHub);
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