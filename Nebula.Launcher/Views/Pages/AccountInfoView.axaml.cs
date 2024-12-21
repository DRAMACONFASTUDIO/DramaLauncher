using Avalonia.Controls;
using Nebula.Launcher.ViewModels;

namespace Nebula.Launcher.Views.Pages;

public interface ITab;
public partial class AccountInfoView : UserControl
{
    public AccountInfoView()
    {
        InitializeComponent();
    }
    
    public AccountInfoView(ViewModels.AccountInfoViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
    }
}