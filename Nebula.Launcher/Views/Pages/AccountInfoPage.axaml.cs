using Avalonia.Controls;
using Nebula.Launcher.ViewModels;

namespace Nebula.Launcher.Views.Pages;

public interface ITab;
public partial class AccountInfoPage : UserControl
{
    public AccountInfoPage()
    {
        InitializeComponent();
    }
    
    public AccountInfoPage(AccountInfoViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
    }
}