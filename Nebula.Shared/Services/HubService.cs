using Nebula.Shared.Models;
using Nebula.Shared.Services.Logging;

namespace Nebula.Shared.Services;

[ServiceRegister]
public class HubService
{
    private readonly ConfigurationService _configurationService;
    private readonly RestService _restService;
    private readonly ILogger _logger;

    private readonly List<ServerHubInfo> _serverList = new();

    public Action<HubServerChangedEventArgs>? HubServerChangedEventArgs;
    public Action? HubServerLoaded;
    public Action<Exception>? HubServerLoadingError;

    public HubService(ConfigurationService configurationService, RestService restService, DebugService debugService)
    {
        _configurationService = configurationService;
        _restService = restService;
        _logger = debugService.GetLogger(this);

        UpdateHub();
    }

    public IReadOnlyList<ServerHubInfo> ServerList => _serverList;
    public bool IsUpdating { get; private set; }

    public async void UpdateHub()
    {
        if (IsUpdating) return;

        _serverList.Clear();

        IsUpdating = true;

        HubServerChangedEventArgs?.Invoke(new HubServerChangedEventArgs([], HubServerChangeAction.Clear));

        foreach (var urlStr in _configurationService.GetConfigValue(CurrentConVar.Hub)!)
        {
            var invoked = false;
            Exception? exception = null;
            foreach (var uri in urlStr)
            {
                try
                {
                    var servers =
                        await _restService.GetAsync<List<ServerHubInfo>>(new Uri(uri), CancellationToken.None);
                    _serverList.AddRange(servers);
                    HubServerChangedEventArgs?.Invoke(new HubServerChangedEventArgs(servers, HubServerChangeAction.Add));
                    invoked = true;
                    break;
                }
                catch (Exception e)
                {
                    _logger.Error($"Failed to get servers for {uri}");
                    _logger.Error(e);
                    exception = e;
                }
            }
            
            if(exception is not null && !invoked) 
                HubServerLoadingError?.Invoke(new Exception("No hub is available.", exception));
        }

        IsUpdating = false;
        HubServerLoaded?.Invoke();
    }
}

public class HubServerChangedEventArgs : EventArgs
{
    public HubServerChangeAction Action;
    public List<ServerHubInfo> Items;

    public HubServerChangedEventArgs(List<ServerHubInfo> items, HubServerChangeAction action)
    {
        Items = items;
        Action = action;
    }
}

public enum HubServerChangeAction
{
    Add,
    Remove,
    Clear
}