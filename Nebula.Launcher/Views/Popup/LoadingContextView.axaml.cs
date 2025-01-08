using Avalonia.Controls;
using Nebula.Launcher.ViewModels;

namespace Nebula.Launcher.Views.Popup;

public partial class LoadingContextView : UserControl
{
    public LoadingContextView()
    {
        InitializeComponent();
    }
    
    public LoadingContextView(LoadingContextViewModel viewModel): this()
    {
        DataContext = viewModel;
    }
}