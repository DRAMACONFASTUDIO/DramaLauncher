using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nebula.Launcher.ViewModels;

namespace Nebula.Launcher.Views;

public partial class ServerEntryView : UserControl
{
    public ServerEntryView()
    {
        InitializeComponent();
    }
    
    public ServerEntryView(ServerEntryModelView modelView) : this()
    {
        DataContext = modelView;
    }
}