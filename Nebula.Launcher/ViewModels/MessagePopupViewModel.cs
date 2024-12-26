using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views.Popup;

namespace Nebula.Launcher.ViewModels;

[ViewRegister(typeof(MessagePopupView))]
public partial class MessagePopupViewModel : ViewModelBase
{
    public MessagePopupViewModel() : base()
    {
    }

    public MessagePopupViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
    
    public Action? OnCloseRequired;
    public Action? OnOpenRequired;

    public Queue<PopupViewModelBase> ViewQueue = new();
     
    [ObservableProperty]
    private PopupViewModelBase? _currentPopup;

    [ObservableProperty] 
    private string _currentTitle = "Default";
    
    public void PopupMessage(PopupViewModelBase viewModelBase)
    {
        Console.WriteLine(viewModelBase.Title);
        if (CurrentPopup == null)
        {
            CurrentPopup = viewModelBase;
            CurrentTitle = viewModelBase.Title;
            OnOpenRequired?.Invoke();
        }
        else
        {
            ViewQueue.Enqueue(viewModelBase);
        }
    }
    
    [RelayCommand]
    private void TriggerClose()
    {
       ClosePopup();
    }

    [RelayCommand]
    private void ClosePopup()
    {
        Console.WriteLine("Gadeem");
        if (!ViewQueue.TryDequeue(out var viewModelBase))
            OnCloseRequired?.Invoke();
        else
            CurrentTitle = viewModelBase.Title;
        
        CurrentPopup = viewModelBase;
        
    }
}