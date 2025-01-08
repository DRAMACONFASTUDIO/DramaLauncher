using System;
using Microsoft.Extensions.DependencyInjection;
using Nebula.Shared.Services;

namespace Nebula.Launcher.ViewModels;

public abstract class PopupViewModelBase : ViewModelBase, IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    public PopupViewModelBase()
    {
    }

    public PopupViewModelBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public abstract string Title { get; }
    public void Dispose()
    {
        _serviceProvider.GetService<PopupMessageService>()?.ClosePopup(this);
    }
}