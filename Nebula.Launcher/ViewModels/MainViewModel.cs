using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views;
using Nebula.Shared.Models;
using Nebula.Shared.Services;
using Nebula.Shared.Utils;

namespace Nebula.Launcher.ViewModels;

[ViewModelRegister(typeof(MainView))]
public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        TryGetViewModel(typeof(AccountInfoViewModel), out var model);
        _currentPage = model!;
        
        Items = new ObservableCollection<ListItemTemplate>(_templates);

        SelectedListItem = Items.First(vm => vm.ModelType == typeof(AccountInfoViewModel));
    }
    
    [UsedImplicitly]
    public MainViewModel(AccountInfoViewModel accountInfoViewModel,DebugService debugService, PopupMessageService popupMessageService,
        IServiceProvider serviceProvider): base(serviceProvider)
    {
        _currentPage = accountInfoViewModel;
        _debugService = debugService;
        Items = new ObservableCollection<ListItemTemplate>(_templates);

        popupMessageService.OnPopupRequired += OnPopupRequired;
        popupMessageService.OnCloseRequired += OnPopupCloseRequired;

        SelectedListItem = Items.First(vm => vm.ModelType == typeof(AccountInfoViewModel));
    }
    
    private readonly List<PopupViewModelBase> _viewQueue = new();
    
    private readonly List<ListItemTemplate> _templates =
    [
        new ListItemTemplate(typeof(AccountInfoViewModel), "Account", "Account"),
        new ListItemTemplate(typeof(ServerListViewModel), "HomeRegular", "Servers")
    ];

    [ObservableProperty]
    private bool _isPaneOpen;

    [ObservableProperty]
    private ViewModelBase _currentPage;

    private readonly DebugService _debugService;

    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private bool _popup;
    
    [ObservableProperty]
    private PopupViewModelBase? _currentPopup;
    [ObservableProperty] 
    private string _currentTitle = "Default";

    [ObservableProperty]
    private ListItemTemplate? _selectedListItem;

    partial void OnSelectedListItemChanged(ListItemTemplate? value)
    {
        if (value is null) return;

        if(!TryGetViewModel(value.ModelType, out var vmb))
        {
            return;
        }
 
        CurrentPage = vmb;
    }

    public ObservableCollection<ListItemTemplate> Items { get; }
    
    public void PopupMessage(PopupViewModelBase viewModelBase)
    {
        if (CurrentPopup == null)
        {
            CurrentPopup = viewModelBase;
            CurrentTitle = viewModelBase.Title;
            OnOpenRequired();
        }
        else
        {
            _viewQueue.Add(viewModelBase);
        }
    }
    
    private void OnCloseRequired()
    {
        IsEnabled = true;
        Popup = false;
    }

    private void OnOpenRequired()
    {
        IsEnabled = false;
        Popup = true;
    }

    public void OpenLink()
    {
        Helper.OpenBrowser("https://cinka.ru/nebula-launcher/");
    }

    private void OnPopupRequired(object viewModelBase)
    {
        switch (viewModelBase)
        {
            case string str:
            {
                var view = GetViewModel<InfoPopupViewModel>();
                view.InfoText = str;
                PopupMessage(view);
                break;
            }
            case PopupViewModelBase @base:
                PopupMessage(@base);
                break;
            case Exception error:
                var err = GetViewModel<ExceptionViewModel>();
                _debugService.Error(error);
                err.AppendError(error);
                PopupMessage(err);
                break;
        }
    }
    
    private void OnPopupCloseRequired(object obj)
    {
        if(obj is not PopupViewModelBase viewModelBase)
        {
            return;
        }
        
        if (obj == CurrentPopup)
            ClosePopup();
        else
            _viewQueue.Remove(viewModelBase);
    }


    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }
    
    [RelayCommand]
    public void ClosePopup()
    {
        var viewModelBase = _viewQueue.FirstOrDefault();
        if (viewModelBase is null)
            OnCloseRequired();
        else
        {
            CurrentTitle = viewModelBase.Title;
            _viewQueue.RemoveAt(0);
        }
        
        CurrentPopup = viewModelBase;
    }
}

