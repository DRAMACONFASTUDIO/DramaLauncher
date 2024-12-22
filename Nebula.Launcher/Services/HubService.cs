using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using Nebula.Launcher.Models;

namespace Nebula.Launcher.Services;

[ServiceRegister]
public class HubService
{
    private readonly RestService _restService;

    public Action<HubServerChangedEventArgs>? HubServerChangedEventArgs;
    
    public readonly ObservableCollection<string> HubList = new();

    private readonly Dictionary<string, List<ServerInfo>> _servers = new();
    
    
    public HubService(ConfigurationService configurationService, RestService restService)
    {
        _restService = restService;
        HubList.CollectionChanged += HubListCollectionChanged;
        
        foreach (var hubUrl in configurationService.GetConfigValue<string[]>(CurrentConVar.Hub)!)
        {
            HubList.Add(hubUrl);
        }
    }

    private async void HubListCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (var hubUri in e.NewItems)
            {
                var urlStr = (string)hubUri;
                var servers = await _restService.GetAsyncDefault<List<ServerInfo>>(new Uri(urlStr), [], CancellationToken.None);
                _servers[urlStr] = servers;
                HubServerChangedEventArgs?.Invoke(new HubServerChangedEventArgs(servers, HubServerChangeAction.Add));
            }
        }

        if (e.OldItems is not null)
        {
            foreach (var hubUri in e.OldItems)
            {
                var urlStr = (string)hubUri;
                if (_servers.TryGetValue(urlStr, out var serverInfos))
                {
                    _servers.Remove(urlStr);
                    HubServerChangedEventArgs?.Invoke(new HubServerChangedEventArgs(serverInfos, HubServerChangeAction.Remove));
                }
            }
        }
    }
}

public class HubServerChangedEventArgs : EventArgs
{
    public HubServerChangeAction Action;
    public List<ServerInfo> Items;

    public HubServerChangedEventArgs(List<ServerInfo> items, HubServerChangeAction action)
    {
        Items = items;
        Action = action;
    }
}

public enum HubServerChangeAction
{
    Add, Remove,
}