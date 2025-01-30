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
    
    public readonly List<(RobustUrl,ServerStatus)> RawFavoriteServers = new();
    
    public ServerEntryModelView GetServerEntryModelView((RobustUrl, ServerStatus) server)
    {
        var model = ViewHelperService.GetViewModel<ServerEntryModelView>().WithData(server.Item1, server.Item2);
        model.OnFavoriteToggle += ()=> RemoveFavorite(model);
        model.IsFavorite = true;
        return model;
    }

    private async void FetchFavorite()
    {
        RawFavoriteServers.Clear();
        
        var servers = ConfigurationService.GetConfigValue(CurrentConVar.Favorites);
        if (servers is null || servers.Length == 0)
        {
            return;
        }
        
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
            
                RawFavoriteServers.Add((uri, serverInfo.Value));
            }
            catch (Exception e)
            {
                RawFavoriteServers.Add((uri, new ServerStatus("ErrorLand",$"ERROR: {e.Message}",[],"",-1,-1,-1,false,DateTime.Now, -1)));
            }
        }
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