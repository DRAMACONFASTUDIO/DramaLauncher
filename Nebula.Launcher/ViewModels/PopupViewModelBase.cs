using System;

namespace Nebula.Launcher.ViewModels;

public abstract class PopupViewModelBase : ViewModelBase
{
    public PopupViewModelBase()
    {
    }

    public PopupViewModelBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
    
    public abstract string Title { get; } 
}