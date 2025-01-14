using Avalonia.Controls;
using InfoPopupViewModel = Nebula.Launcher.ViewModels.Popup.InfoPopupViewModel;

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