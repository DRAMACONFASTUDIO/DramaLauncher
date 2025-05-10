using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nebula.Launcher.ViewModels.Pages;

namespace Nebula.Launcher.Views.Pages;

public partial class ConfigurationView : UserControl
{
    public ConfigurationView()
    {
        InitializeComponent();
    }

    public ConfigurationView(ConfigurationViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
    }
}