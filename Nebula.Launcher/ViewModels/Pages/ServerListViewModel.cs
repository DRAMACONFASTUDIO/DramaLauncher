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
    
    public ObservableCollection<ServerEntryModelView> Servers { get; }= new();
    
    public Action? OnSearchChange;
    [GenerateProperty] private HubService HubService { get; }
    [GenerateProperty] private PopupMessageService PopupMessageService { get; }
    [GenerateProperty] private CancellationService CancellationService { get; }
    [GenerateProperty] private DebugService DebugService { get; }
    [GenerateProperty, DesignConstruct] private ViewHelperService ViewHelperService { get; }
    
    private ServerViewContainer ServerViewContainer { get; set; } 
    
    private List<ServerHubInfo> UnsortedServers { get; } = new();

    //Design think
    protected override void InitialiseInDesignMode()
    {
        ServerViewContainer = new ServerViewContainer(this, RestService, CancellationService, DebugService, ViewHelperService);
    }

    //real think
    protected override void Initialise()
    {
        ServerViewContainer = new ServerViewContainer(this, RestService, CancellationService, DebugService, ViewHelperService);
        
        foreach (var info in HubService.ServerList) UnsortedServers.Add(info);

        HubService.HubServerChangedEventArgs += HubServerChangedEventArgs;
        HubService.HubServerLoaded += UpdateServerEntries;
        OnSearchChange += OnChangeSearch;

        if (!HubService.IsUpdating) UpdateServerEntries();
        UpdateFavoriteEntries();
    }

    private void UpdateServerEntries()
    {
        Servers.Clear();
        Task.Run(async () =>
        {
            UnsortedServers.Sort(new ServerComparer());
            foreach (var info in UnsortedServers.Where(a => CheckServerThink(a.StatusData)))
            {
                var view = await ServerViewContainer.Get(info.Address.ToRobustUrl(), info.StatusData);
                Servers.Add(view);
            }
        });
    }

    private void OnChangeSearch()
    {
        if(string.IsNullOrEmpty(SearchText)) return;
        
        if(IsFavoriteMode)
        {
            UpdateFavoriteEntries();
        }
        else
        {
            UpdateServerEntries();
        }
    }

    private void HubServerChangedEventArgs(HubServerChangedEventArgs obj)
    {
        if (obj.Action == HubServerChangeAction.Add)
            foreach (var info in obj.Items)
                UnsortedServers.Add(info);
        if (obj.Action == HubServerChangeAction.Remove)
            foreach (var info in obj.Items)
                UnsortedServers.Remove(info);
        if (obj.Action == HubServerChangeAction.Clear)
        {
            UnsortedServers.Clear();
            ServerViewContainer.Clear();
            Servers.Clear();
            UpdateFavoriteEntries();
        }
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

public class ServerViewContainer(
    ServerListViewModel serverListViewModel, 
    RestService restService, 
    CancellationService cancellationService, 
    DebugService debugService, 
    ViewHelperService viewHelperService
    )
{
    private readonly Dictionary<string, ServerEntryModelView> _entries = new();

    public void Clear()
    {
        _entries.Clear();
    }

    public async Task<ServerEntryModelView> Get(RobustUrl url, ServerStatus? serverStatus = null)
    {
        lock (_entries)
        {
            if (_entries.TryGetValue(url.ToString(), out var entry1))
            {
                return entry1;
            }
        }
        
        Console.WriteLine("Creating new instance... " + url.ToString() + _entries.Keys.ToList().Contains(url.ToString()));

        try
        {
            serverStatus ??= await restService.GetAsync<ServerStatus>(url.StatusUri, cancellationService.Token);
        }
        catch (Exception e)
        {
            debugService.Error(e);
            serverStatus = new ServerStatus("ErrorLand", $"ERROR: {e.Message}", [], "", -1, -1, -1, false, DateTime.Now,
                -1);
        }

        var entry = viewHelperService.GetViewModel<ServerEntryModelView>().WithData(url, serverStatus);
        entry.OnFavoriteToggle += () =>
        {
            if (entry.IsFavorite) serverListViewModel.RemoveFavorite(entry);
            else serverListViewModel.AddFavorite(entry);
        };
        
        lock (_entries)
        {
            if (_entries.TryGetValue(url.ToString(), out var entry1))
            {
                return entry1;
            }
            _entries.Add(url.ToString(), entry);   
        }

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