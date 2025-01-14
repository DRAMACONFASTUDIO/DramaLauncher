using Avalonia.Controls;
using ContentBrowserViewModel = Nebula.Launcher.ViewModels.Pages.ContentBrowserViewModel;

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