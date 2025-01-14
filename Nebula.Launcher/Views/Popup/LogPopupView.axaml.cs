using Avalonia.Controls;
using Nebula.Launcher.ViewModels.Popup;

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