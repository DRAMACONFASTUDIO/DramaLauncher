using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nebula.Launcher.Models;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views;
using Nebula.Launcher.Views.Pages;

namespace Nebula.Launcher.ViewModels;

[ViewRegister(typeof(MainView))]
public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        TryGetViewModel(typeof(AccountInfoViewModel), out var model);
        _currentPage = model!;
        TryGetViewModel(typeof(MessagePopupViewModel), out var viewModelBase);
        _messagePopupViewModel = (MessagePopupViewModel)viewModelBase!;
        
        Items = new ObservableCollection<ListItemTemplate>(_templates);

        SelectedListItem = Items.First(vm => vm.ModelType == typeof(AccountInfoViewModel));
    }
    
    public MainViewModel(AccountInfoViewModel accountInfoViewModel, MessagePopupViewModel messagePopupViewModel, 
        IServiceProvider serviceProvider): base(serviceProvider)
    {
        _currentPage = accountInfoViewModel;
        _messagePopupViewModel = messagePopupViewModel;
        Items = new ObservableCollection<ListItemTemplate>(_templates);
        
        _messagePopupViewModel.OnOpenRequired += () => OnOpenRequired();
        _messagePopupViewModel.OnCloseRequired += () => OnCloseRequired();

        SelectedListItem = Items.First(vm => vm.ModelType == typeof(AccountInfoViewModel));
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

    private readonly List<ListItemTemplate> _templates =
    [
        new ListItemTemplate(typeof(AccountInfoViewModel), "Account", "Account"),
        new ListItemTemplate(typeof(ServerListViewModel), "HomeRegular", "Servers")
    ];

    [ObservableProperty]
    private bool _isPaneOpen;

    [ObservableProperty]
    private ViewModelBase _currentPage;

    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private bool _popup;

    private readonly MessagePopupViewModel _messagePopupViewModel;

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
        
        var model = GetViewModel<InfoPopupViewModel>();
        model.InfoText = "Переключили прикол!";
        
        _messagePopupViewModel.PopupMessage(model);
        
    }

    public ObservableCollection<ListItemTemplate> Items { get; }

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }
}

