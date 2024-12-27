using Avalonia.Controls;
using Nebula.Launcher.ViewModels;

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
}