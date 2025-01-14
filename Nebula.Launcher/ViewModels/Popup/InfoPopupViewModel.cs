using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.Views.Popup;
using Nebula.Shared.Services;

namespace Nebula.Launcher.ViewModels.Popup;

[ViewModelRegister(typeof(InfoPopupView), false)]
[ConstructGenerator]
public partial class InfoPopupViewModel : PopupViewModelBase
{
    [GenerateProperty] public override PopupMessageService PopupMessageService { get; }

    [ObservableProperty] private string _infoText = "Test";

    public override string Title => "Info";
    public override bool IsClosable => true;

    protected override void Initialise()
    {
    }

    protected override void InitialiseInDesignMode()
    {
    }
}