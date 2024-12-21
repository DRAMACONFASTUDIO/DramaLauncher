using System;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views.Pages;

namespace Nebula.Launcher.ViewModels;

[ViewRegister(typeof(ServerListView))]
public class ServerListViewModel : ViewModelBase
{
    public ServerListViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}