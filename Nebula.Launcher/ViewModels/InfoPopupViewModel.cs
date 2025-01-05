using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views.Popup;

namespace Nebula.Launcher.ViewModels;

[ViewModelRegister(typeof(InfoPopupView), false)]
public partial class InfoPopupViewModel : PopupViewModelBase
{
    public InfoPopupViewModel()
    {
    }

    public InfoPopupViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
    
    public override string Title => "Info";

    [ObservableProperty] 
    private string _infoText = "Test";
}