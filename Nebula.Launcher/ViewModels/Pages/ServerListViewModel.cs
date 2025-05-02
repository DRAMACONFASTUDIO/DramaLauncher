using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.Controls;
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
    
    [ObservableProperty] private bool _isFilterVisible;
    
    public ObservableCollection<ServerEntryModelView> Servers { get; }= new();
    public ObservableCollection<Exception> HubErrors { get; } = new();
    public readonly ServerFilter CurrentFilter = new ServerFilter();
    
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
        ServerViewContainer = new ServerViewContainer(this, ViewHelperService);
        HubErrors.Add(new Exception("UVI"));
    }

    //real think
    protected override void Initialise()
    {
        ServerViewContainer = new ServerViewContainer(this, ViewHelperService);
        
        foreach (var info in HubService.ServerList) UnsortedServers.Add(info);

        HubService.HubServerChangedEventArgs += HubServerChangedEventArgs;
        HubService.HubServerLoaded += UpdateServerEntries;
        HubService.HubServerLoadingError += HubServerLoadingError;
        OnSearchChange += OnChangeSearch;

        if (!HubService.IsUpdating) UpdateServerEntries();
        UpdateFavoriteEntries();
    }
    
    public void ApplyFilter()
    {
        foreach (var entry in ServerViewContainer.Items)
        {
            entry.ProcessFilter(CurrentFilter);
        }
    }
    
    public void OnFilterChanged(FilterBoxChangedEventArgs args)
    {
        if (args.Checked)
            CurrentFilter.Tags.Add(args.Tag);
        else
            CurrentFilter.Tags.Remove(args.Tag);
        ApplyFilter();
    }

    private void HubServerLoadingError(Exception obj)
    {
        HubErrors.Add(obj);
    }

    private void UpdateServerEntries()
    {
        foreach(var fav in Servers.ToList()){
            Servers.Remove(fav);
        }
        
        Task.Run(() =>
        {
            UnsortedServers.Sort(new ServerComparer());
            foreach (var info in UnsortedServers)
            {
                var view = ServerViewContainer.Get(info.Address.ToRobustUrl(), info.StatusData);
                Servers.Add(view);
            }
            ApplyFilter();
        });
    }

    private void OnChangeSearch()
    {
        CurrentFilter.SearchText = SearchText;
        ApplyFilter();
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
            UpdateFavoriteEntries();
        }
    }

    public void FilterRequired()
    {
        IsFilterVisible = !IsFilterVisible;
    }

    public void AddFavoriteRequired()
    {
        var p = ViewHelperService.GetViewModel<AddFavoriteViewModel>();
        PopupMessageService.Popup(p);
    }

    public void UpdateRequired()
    {
        HubErrors.Clear();
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
    ViewHelperService viewHelperService
    )
{
    private readonly Dictionary<string, ServerEntryModelView> _entries = new();
    
    public ICollection<ServerEntryModelView> Items => _entries.Values;

    public void Clear()
    {
        _entries.Clear();
    }

    public ServerEntryModelView Get(RobustUrl url, ServerStatus? serverStatus = null)
    {
        ServerEntryModelView? entry;
        
        lock (_entries)
        {
            if (_entries.TryGetValue(url.ToString(), out entry))
            {
                return entry;
            }

            entry = viewHelperService.GetViewModel<ServerEntryModelView>().WithData(url, serverStatus);
            
            _entries.Add(url.ToString(), entry);
        }
        
        entry.OnFavoriteToggle += () =>
        {
            if (entry.IsFavorite) serverListViewModel.RemoveFavorite(entry);
            else serverListViewModel.AddFavorite(entry);
        };
        
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
        return Compare(x.Item2, y.Item2);
    }
}

public sealed class ServerFilter
{
    public string SearchText { get; set; } = "";
    public HashSet<string> Tags { get; } = new();
    public bool IsMatchByName(string name)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        return name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsMatchByTags(IEnumerable<string> itemTags)
    {
        if (Tags.Count == 0)
            return true;
        
        var itemTagSet = new HashSet<string>(itemTags);
        return Tags.All(tag => itemTagSet.Contains(tag));
    }

    public bool IsMatch(string name, IEnumerable<string> itemTags)
    {
        return IsMatchByName(name) && IsMatchByTags(itemTags);
    }
}