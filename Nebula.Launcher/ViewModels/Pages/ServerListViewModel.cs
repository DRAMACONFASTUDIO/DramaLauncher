using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.Services;
using Nebula.Launcher.ViewModels.Popup;
using Nebula.Launcher.Views.Pages;
using Nebula.Shared.Models;
using Nebula.Shared.Services;
using Nebula.Shared.Utils;

namespace Nebula.Launcher.ViewModels.Pages;

[ViewModelRegister(typeof(ServerListView))]
[ConstructGenerator]
public partial class ServerListViewModel : ViewModelBase, IViewModelPage
{
    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private bool _isFavoriteMode; 

    public Action? OnSearchChange;
    [GenerateProperty] private HubService HubService { get; } = default!;
    [GenerateProperty] private PopupMessageService PopupMessageService { get; }
    [GenerateProperty, DesignConstruct] private ViewHelperService ViewHelperService { get; } = default!;
    public ObservableCollection<ServerEntryModelView> SortedServers { get; } = new();
    private List<ServerHubInfo> UnsortedServers { get; } = new();

    //Design think
    protected override void InitialiseInDesignMode()
    {
        FavoriteVisible = true;
        SortedFavoriteServers.Add(GetServerEntryModelView(("ss14://localhost".ToRobustUrl(),
            new ServerStatus("Nebula", "TestCraft", ["16+", "RU"], "super", 12, 55, 1, false, DateTime.Now, 20))));
        SortedServers.Add(CreateServerView(new ServerHubInfo("ss14://localhost",
            new ServerStatus("Nebula", "TestCraft", ["16+", "RU"], "super", 12, 55, 1, false, DateTime.Now, 20), [])));
        SortedServers.Add(CreateServerView(new ServerHubInfo("ss14://localhost",
            new ServerStatus("Nebula", "TestCraft", ["16+", "RU"], "super", 12, 55, 1, false, DateTime.Now, 20), [])));
        SortedServers.Add(CreateServerView(new ServerHubInfo("ss14://localhost",
            new ServerStatus("Nebula", "TestCraft", ["16+", "RU"], "super", 12, 55, 1, false, DateTime.Now, 20), [])));
    }

    //real think
    protected override void Initialise()
    {
        foreach (var info in HubService.ServerList) UnsortedServers.Add(info);

        HubService.HubServerChangedEventArgs += HubServerChangedEventArgs;
        HubService.HubServerLoaded += SortServers;
        OnSearchChange += OnChangeSearch;

        if (!HubService.IsUpdating) SortServers();
        FetchFavorite();
    }
    
    private void OnChangeSearch()
    {
        if(string.IsNullOrEmpty(SearchText)) return;
        SortServers();
        SortFavorite();
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
            SortedServers.Clear();
            UnsortedServers.Sort(new ServerComparer());
            foreach (var server in UnsortedServers.Where(a => CheckServerThink(a.StatusData))) SortedServers.Add(CreateServerView(server));
        });
    }

    private bool CheckServerThink(ServerStatus hubInfo)
    {
        if (string.IsNullOrEmpty(SearchText)) return true;
        return hubInfo.Name.ToLower().Contains(SearchText.ToLower());
    }

    private ServerEntryModelView CreateServerView(ServerHubInfo serverHubInfo)
    {
        var svn = ViewHelperService.GetViewModel<ServerEntryModelView>().WithData(serverHubInfo);
        svn.OnFavoriteToggle += () => AddFavorite(svn);
        return svn;
    }

    public void FilterRequired()
    {
    }

    public void AddFavoriteRequired()
    {
        var p = ViewHelperService.GetViewModel<AddFavoriteViewModel>();
        PopupMessageService.Popup(p);
    }

    public void UpdateRequired()
    {
        Task.Run(HubService.UpdateHub);
    }

    public void OnPageOpen(object? args)
    {
        if (args is bool fav)
        {
            IsFavoriteMode = fav;
        }
    }
}

public class ServerComparer : IComparer<ServerHubInfo>, IComparer<ServerStatus>, IComparer<(RobustUrl,ServerStatus)>
{
    public int Compare(ServerHubInfo? x, ServerHubInfo? y)
    {
        if (ReferenceEquals(x, y))
            return 0;
        if (ReferenceEquals(null, y))
            return 1;
        if (ReferenceEquals(null, x))
            return -1;

        return Compare(x.StatusData, y.StatusData);
    }

    public int Compare(ServerStatus? x, ServerStatus? y)
    {
        if (ReferenceEquals(x, y))
            return 0;
        if (ReferenceEquals(null, y))
            return 1;
        if (ReferenceEquals(null, x))
            return -1;

        return y.Players.CompareTo(x.Players);
    }

    public int Compare((RobustUrl, ServerStatus) x, (RobustUrl, ServerStatus) y)
    {
        if (ReferenceEquals(x, y))
            return 0;
        if (ReferenceEquals(null, y))
            return 1;
        if (ReferenceEquals(null, x))
            return -1;

        return Compare(x.Item2, y.Item2);
    }
}