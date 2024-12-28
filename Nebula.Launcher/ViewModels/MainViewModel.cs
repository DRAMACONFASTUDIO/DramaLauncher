using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using Nebula.Launcher.Models;
using Nebula.Launcher.Services;
using Nebula.Launcher.Utils;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views;

namespace Nebula.Launcher.ViewModels;

[ViewRegister(typeof(MainView))]
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
    public MainViewModel(AccountInfoViewModel accountInfoViewModel, PopupMessageService popupMessageService,
        IServiceProvider serviceProvider): base(serviceProvider)
    {
        _currentPage = accountInfoViewModel;
        _popupMessageService = popupMessageService;
        Items = new ObservableCollection<ListItemTemplate>(_templates);

        _popupMessageService.OnPopupRequired += OnPopupRequired;

        SelectedListItem = Items.First(vm => vm.ModelType == typeof(AccountInfoViewModel));
    }

    private readonly Queue<PopupViewModelBase> _viewQueue = new();
    
    private readonly List<ListItemTemplate> _templates =
    [
        new ListItemTemplate(typeof(AccountInfoViewModel), "Account", "Account"),
        new ListItemTemplate(typeof(ServerListViewModel), "HomeRegular", "Servers")
    ];

    [ObservableProperty]
    private bool _isPaneOpen;

    [ObservableProperty]
    private ViewModelBase _currentPage;

    private readonly PopupMessageService _popupMessageService;

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
            _viewQueue.Enqueue(viewModelBase);
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

    private void OnPopupRequired(PopupViewModelBase? viewModelBase)
    {
        if (viewModelBase is null)
        {
            ClosePopup();
        }
        else
        {
            PopupMessage(viewModelBase);
        }
    }

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }
    
    [RelayCommand]
    public void ClosePopup()
    {
        if (!_viewQueue.TryDequeue(out var viewModelBase))
            OnCloseRequired();
        else
            CurrentTitle = viewModelBase.Title;
        
        CurrentPopup = viewModelBase;
    }
}

