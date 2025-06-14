using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nebula.Launcher.ViewModels.Pages;

namespace Nebula.Launcher.Views;

public partial class FileContentEntryView : UserControl
{
    // This constructor is used when the view is created by the XAML Previewer
    public FileContentEntryView()
    {
        InitializeComponent();
    }

    // This constructor is used when the view is created via dependency injection
    public FileContentEntryView(FolderContentEntry viewModel)
        : this()
    {
        DataContext = viewModel;
    }
}