using Avalonia.Controls;
using Nebula.Launcher.ViewModels.Popup;

namespace Nebula.Launcher.Views.Popup;

public partial class ExceptionListView : UserControl
{
    public ExceptionListView()
    {
        InitializeComponent();
    }

    public ExceptionListView(ExceptionListViewModel listViewModel) : this()
    {
        DataContext = listViewModel;
    }
}