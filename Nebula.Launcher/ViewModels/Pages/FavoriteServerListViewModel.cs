using System.Collections.ObjectModel;
using Nebula.Shared.Models;

namespace Nebula.Launcher.ViewModels.Pages;

public class FavoriteServerListViewModel : ViewModelBase
{
    public ObservableCollection<ServerHubInfo> Servers = new();

    protected override void Initialise()
    {
    }

    protected override void InitialiseInDesignMode()
    {
    }
}