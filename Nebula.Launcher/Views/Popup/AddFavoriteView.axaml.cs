using Avalonia.Controls;
using Nebula.Launcher.ViewModels.Popup;

namespace Nebula.Launcher.Views.Popup;

public partial class AddFavoriteView : UserControl
{
    public AddFavoriteView()
    {
        InitializeComponent();
    }

    public AddFavoriteView(AddFavoriteViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
    }
}