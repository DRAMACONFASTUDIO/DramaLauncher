using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nebula.Launcher.ViewModels;

namespace Nebula.Launcher.Views.Popup;

public partial class LogPopupView : UserControl
{
    public LogPopupView()
    {
        InitializeComponent();
    }

    public LogPopupView(LogPopupModelView viewModel) : this()
    {
        DataContext = viewModel;
    }
}