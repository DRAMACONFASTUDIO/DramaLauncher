using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Shared;
using Nebula.Shared.Models;
using Nebula.Shared.Services;
using Nebula.Shared.Utils;

namespace Nebula.Launcher.ViewModels.Pages;

public partial class ServerListViewModel
{
    [GenerateProperty] private ConfigurationService ConfigurationService { get; }
    [GenerateProperty] private RestService RestService { get; }

    [ObservableProperty] private bool _favoriteVisible = false;
    
    public List<(RobustUrl,ServerStatus)> FavoriteServers = new();
    public ObservableCollection<ServerEntryModelView> SortedFavoriteServers { get; } = new();

    private void SortFavorite()
    {
        SortedFavoriteServers.Clear();
        FavoriteServers.Sort(new ServerComparer());
        foreach (var server in FavoriteServers.Where(a => CheckServerThink(a.Item2))) 
            SortedFavoriteServers.Add(GetServerEntryModelView(server));
    }

    private ServerEntryModelView GetServerEntryModelView((RobustUrl, ServerStatus) server)
    {
        var model = ViewHelperService.GetViewModel<ServerEntryModelView>().WithData(server.Item1, server.Item2);
        model.OnFavoriteToggle += ()=> RemoveFavorite(model);
        model.IsFavorite = true;
        return model;
    }

    private async void FetchFavorite()
    {
        FavoriteServers.Clear();
        
        var servers = ConfigurationService.GetConfigValue(CurrentConVar.Favorites);
        if (servers is null || servers.Length == 0)
        {
            FavoriteVisible = false;
            return;
        }

        FavoriteVisible = true;
        
        foreach (var server in servers)
        {
            var uri = server.ToRobustUrl();
            try
            {
                var serverInfo = await RestService.GetAsync<ServerStatus>(uri.StatusUri, CancellationToken.None);
                if (serverInfo.Value is null)
                {
                    throw new Exception("Server info is null");
                }
            
                FavoriteServers.Add((uri, serverInfo.Value));
            }
            catch (Exception e)
            {
                FavoriteServers.Add((uri, new ServerStatus("ErrorLand",$"ERROR: {e.Message}",[],"",-1,-1,-1,false,DateTime.Now, -1)));
            }
        }
        
        SortFavorite();
    }

    public void AddFavorite(ServerEntryModelView entryModelView)
    {
        entryModelView.IsFavorite = true;
        AddFavorite(entryModelView.Address);
    }

    public void AddFavorite(RobustUrl robustUrl)
    {
        var servers = (ConfigurationService.GetConfigValue(CurrentConVar.Favorites) ?? []).ToList();
        servers.Add(robustUrl.ToString());
        ConfigurationService.SetConfigValue(CurrentConVar.Favorites, servers.ToArray());
        FetchFavorite();
    }

    public void RemoveFavorite(ServerEntryModelView entryModelView)
    {
        var servers = (ConfigurationService.GetConfigValue(CurrentConVar.Favorites) ?? []).ToList();
        servers.Remove(entryModelView.Address.ToString());
        ConfigurationService.SetConfigValue(CurrentConVar.Favorites, servers.ToArray());
        entryModelView.IsFavorite = false;
        FetchFavorite();
    }
}