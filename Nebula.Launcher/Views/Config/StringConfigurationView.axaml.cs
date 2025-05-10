using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nebula.Launcher.ViewModels.Pages;

namespace Nebula.Launcher.Views.Config;

public partial class StringConfigurationView : UserControl
{
    public StringConfigurationView()
    {
        InitializeComponent();
    }

    public StringConfigurationView(StringConfigurationViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
    }
}