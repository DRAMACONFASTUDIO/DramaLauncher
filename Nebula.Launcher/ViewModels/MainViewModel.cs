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
        Items = new ObservableCollection<ListItemTemplate>(_templates);

        SelectedListItem = Items.First(vm => vm.ModelType == typeof(AccountInfoViewModel));
    }
    
    public MainViewModel(AccountInfoViewModel accountInfoViewModel, IServiceProvider serviceProvider): base(serviceProvider)
    {
        _currentPage = accountInfoViewModel;
        Items = new ObservableCollection<ListItemTemplate>(_templates);

        SelectedListItem = Items.First(vm => vm.ModelType == typeof(AccountInfoViewModel));
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

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }
}

