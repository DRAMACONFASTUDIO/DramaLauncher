using System;
using System.Collections.Generic;
using Nebula.Launcher.ViewModels;

namespace Nebula.Launcher.ServerListProviders;

public interface IServerListProvider
{
    public bool IsLoaded { get; }
    public Action? OnLoaded { get; set; }
   
    public IEnumerable<IFilterConsumer> GetServers();
    public IEnumerable<Exception> GetErrors();
   
    public void LoadServerList();
}

public interface IServerListDirtyInvoker
{
    public Action? Dirty { get; set; }
}