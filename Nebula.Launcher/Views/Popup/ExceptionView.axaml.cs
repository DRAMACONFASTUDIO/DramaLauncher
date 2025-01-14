using Avalonia.Controls;
using Nebula.Launcher.ViewModels.Popup;

namespace Nebula.Launcher.Views.Popup;

public partial class ExceptionView : UserControl
{
    public ExceptionView()
    {
        InitializeComponent();
    }

    public ExceptionView(ExceptionViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}