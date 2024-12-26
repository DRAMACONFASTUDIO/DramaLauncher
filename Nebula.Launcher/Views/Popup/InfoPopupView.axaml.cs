using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nebula.Launcher.ViewModels;

namespace Nebula.Launcher.Views.Popup;

public partial class InfoPopupView : UserControl
{
    public InfoPopupView()
    {
        InitializeComponent();
    }

    public InfoPopupView(InfoPopupViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}