using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Nebula.Launcher.ViewModels;
using Nebula.Launcher.ViewModels.Pages;
using Nebula.Shared;
using Nebula.Shared.Models;
using Nebula.Shared.Services;
using Nebula.Shared.Utils;

namespace Nebula.Launcher.ServerListProviders;

[ServiceRegister(), ConstructGenerator]
public sealed partial class FavoriteServerListProvider : IServerListProvider, IServerListDirtyInvoker
{
    [GenerateProperty] private ConfigurationService ConfigurationService { get; }
    [GenerateProperty] private RestService RestService { get; }
    [GenerateProperty] private ServerViewContainer ServerViewContainer { get; }

    private List<IFilterConsumer> _serverLists = [];
    
    public bool IsLoaded { get; private set; }
    public Action? OnLoaded { get; set; }
    public Action? Dirty { get; set; }
    public IEnumerable<IFilterConsumer> GetServers()
    {
        return _serverLists;
    }

    public IEnumerable<Exception> GetErrors()
    {
        return [];
    }

    public void LoadServerList()
    {
        IsLoaded = false;
        _serverLists.Clear();
        var servers = GetFavoriteEntries();
        
        _serverLists.AddRange(
            servers.Select(s => 
                ServerViewContainer.Get(s.ToRobustUrl())
            )
        );
        IsLoaded = true;
        OnLoaded?.Invoke();
    }
    
    public void AddFavorite(ServerEntryModelView entryModelView)
    {
        entryModelView.IsFavorite = true;
        AddFavorite(entryModelView.Address);
    }

    public void AddFavorite(RobustUrl robustUrl)
    {
        var servers = GetFavoriteEntries();
        servers.Add(robustUrl.ToString());
        ConfigurationService.SetConfigValue(LauncherConVar.Favorites, servers.ToArray());
        Dirty?.Invoke();
    }

    public void RemoveFavorite(ServerEntryModelView entryModelView)
    {
        var servers = GetFavoriteEntries();
        servers.Remove(entryModelView.Address.ToString());
        ConfigurationService.SetConfigValue(LauncherConVar.Favorites, servers.ToArray());
        Dirty?.Invoke();
    }

    private List<string> GetFavoriteEntries()
    {
        return ConfigurationService.GetConfigValue(LauncherConVar.Favorites)?.ToList() ?? [];
    }
    
    private void Initialise(){}
    private void InitialiseInDesignMode(){}
}