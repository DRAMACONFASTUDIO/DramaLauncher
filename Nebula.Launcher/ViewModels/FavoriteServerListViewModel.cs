using System;
using System.Collections.ObjectModel;
using Nebula.Shared.Models;

namespace Nebula.Launcher.ViewModels;

public class FavoriteServerListViewModel : ViewModelBase
{
    public FavoriteServerListViewModel() : base(){}
    public FavoriteServerListViewModel(IServiceProvider provider) : base(provider){}

    public ObservableCollection<ServerHubInfo> Servers = new();
}