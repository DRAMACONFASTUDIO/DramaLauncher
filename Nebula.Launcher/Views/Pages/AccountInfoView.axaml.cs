using Avalonia.Controls;
using AccountInfoViewModel = Nebula.Launcher.ViewModels.Pages.AccountInfoViewModel;

namespace Nebula.Launcher.Views.Pages;

public interface ITab;

public partial class AccountInfoView : UserControl
{
    public AccountInfoView()
    {
        InitializeComponent();
    }

    public AccountInfoView(AccountInfoViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
    }
}