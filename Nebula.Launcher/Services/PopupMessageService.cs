using System;
using Microsoft.Extensions.DependencyInjection;
using Nebula.Launcher.ViewModels;

namespace Nebula.Launcher.Services;

[ServiceRegister]
public class PopupMessageService
{
    private readonly IServiceProvider _serviceProvider;
    
    public Action<PopupViewModelBase?>? OnPopupRequired;

    public PopupMessageService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
    }
    
    public void PopupInfo(string info)
    {
        var message = _serviceProvider.GetService<InfoPopupViewModel>();
        message.InfoText = info;
        PopupMessage(message);
    }
    
    public void PopupMessage(PopupViewModelBase viewModelBase)
    {
        OnPopupRequired?.Invoke(viewModelBase);
    }

    public void ClosePopup()
    {
        OnPopupRequired?.Invoke(null);
    }
}