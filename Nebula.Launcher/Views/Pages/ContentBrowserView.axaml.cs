using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nebula.Launcher.ViewModels;

namespace Nebula.Launcher.Views.Pages;

public partial class ContentBrowserView : UserControl
{
    public ContentBrowserView()
    {
        InitializeComponent();
    }
    
    public ContentBrowserView(ContentBrowserViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
    }
}