using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ServerListViewModel = Nebula.Launcher.ViewModels.Pages.ServerListViewModel;

namespace Nebula.Launcher.Views.Pages;

public partial class ServerListView : UserControl
{
    // This constructor is used when the view is created by the XAML Previewer
    public ServerListView()
    {
        InitializeComponent();
    }

    // This constructor is used when the view is created via dependency injection
    public ServerListView(ServerListViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
    }

    private void TextBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var context = (ServerListViewModel?)DataContext;
        context?.OnSearchChange?.Invoke();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var send = sender as CheckBox;
        var context = (ServerListViewModel?)DataContext;
        context?.OnFilterChanged(send.Name, send.IsChecked.Value);
    }
}