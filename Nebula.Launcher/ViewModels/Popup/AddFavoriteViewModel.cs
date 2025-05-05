using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.ViewModels.Pages;
using Nebula.Launcher.Views.Pages;
using Nebula.Shared.Services;
using Nebula.Shared.Services.Logging;
using Nebula.Shared.Utils;
using AddFavoriteView = Nebula.Launcher.Views.Popup.AddFavoriteView;

namespace Nebula.Launcher.ViewModels.Popup;

[ViewModelRegister(typeof(AddFavoriteView), false)]
[ConstructGenerator]
public partial class AddFavoriteViewModel : PopupViewModelBase
{
    private ILogger _logger;
    
    protected override void InitialiseInDesignMode()
    {
    }

    protected override void Initialise()
    {
        _logger = DebugService.GetLogger(this);
    }

    [GenerateProperty] 
    public override PopupMessageService PopupMessageService { get; }
    [GenerateProperty] private ServerListViewModel ServerListViewModel { get; }
    [GenerateProperty] private DebugService DebugService { get; }
    public override string Title => "Add to favorite";
    public override bool IsClosable => true;

    [ObservableProperty] private string _ipInput;
    [ObservableProperty] private string _error = "";

    public void OnEnter()
    {
        try
        {
            var uri = IpInput.ToRobustUrl();
            ServerListViewModel.AddFavorite(uri);
            Dispose();
        }
        catch (Exception e)
        {
            Error = e.Message;
            _logger.Error(e);
        }
    }
}