using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.Services;
using Nebula.Launcher.ViewModels.Popup;
using Nebula.Launcher.Views;
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
    public SortedList<float, ServerEntryModelView> Servers { get; } = new(Comparer<float>.Create((x,y)=>y.CompareTo(x)));
    
    private ServerViewContainer ServerViewContainer;

    public Action? OnSearchChange;
    [GenerateProperty] private HubService HubService { get; } = default!;
    [GenerateProperty] private PopupMessageService PopupMessageService { get; }
    [GenerateProperty, DesignConstruct] private ViewHelperService ViewHelperService { get; } = default!;
    
    private List<ServerHubInfo> UnsortedServers { get; } = new();

    //Design think
    protected override void InitialiseInDesignMode()
    {
        ServerViewContainer = new ServerViewContainer(this);
    }

    //real think
    protected override void Initialise()
    {
        ServerViewContainer = new ServerViewContainer(this);
        
        foreach (var info in HubService.ServerList) UnsortedServers.Add(info);

        HubService.HubServerChangedEventArgs += HubServerChangedEventArgs;
        HubService.HubServerLoaded += UpdateServerEntries;
        OnSearchChange += OnChangeSearch;

        if (!HubService.IsUpdating) UpdateServerEntries();
        //FetchFavorite();
    }

    private void UpdateServerEntries()
    {
        Servers.Clear();
        OnPropertyChanged(nameof(Servers));
        foreach (var info in UnsortedServers.Where(a=>CheckServerThink(a.StatusData)))
        {
            var view = ServerViewContainer.Get(info.Address.ToRobustUrl(), info.StatusData);
            float players = info.StatusData.Players;
            
            while (true)
            {
                try
                {
                    Servers.Add(players,view);
                    OnPropertyChanged(nameof(Servers));
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    players += 0.01f;
                }
            }
        }
    }

    private void OnChangeSearch()
    {
        if(string.IsNullOrEmpty(SearchText)) return;
        
        UpdateServerEntries();
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

    private bool CheckServerThink(ServerStatus hubInfo)
    {
        if (string.IsNullOrEmpty(SearchText)) return true;
        return hubInfo.Name.ToLower().Contains(SearchText.ToLower());
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

public class ServerViewContainer(ServerListViewModel serverListViewModel)
{
    private readonly Dictionary<RobustUrl, ServerEntryModelView> _entries = new();

    public ServerEntryModelView Get(RobustUrl url, ServerStatus serverStatus)
    {
        if (_entries.TryGetValue(url, out var entry))
        {
            return entry;
        }

        entry = serverListViewModel.GetServerEntryModelView((url, serverStatus));
        _entries.Add(url, entry);

        return entry;
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