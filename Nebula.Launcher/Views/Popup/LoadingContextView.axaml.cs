using Avalonia.Controls;
using LoadingContextViewModel = Nebula.Launcher.ViewModels.Popup.LoadingContextViewModel;

namespace Nebula.Launcher.Views.Popup;

public partial class LoadingContextView : UserControl
{
    public LoadingContextView()
    {
        InitializeComponent();
    }

    public LoadingContextView(LoadingContextViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}