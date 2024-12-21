using System;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views.Pages;

namespace Nebula.Launcher.ViewModels;

[ViewRegister(typeof(AccountInfoView))]
public class AccountInfoViewModel : ViewModelBase
{
    public AccountInfoViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
