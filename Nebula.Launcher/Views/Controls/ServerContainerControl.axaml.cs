using Avalonia;
using Avalonia.Controls;

namespace Nebula.Launcher.Views.Controls;

public partial class ServerContainerControl : UserControl
{
    public static readonly StyledProperty<string> ServerNameProperty 
        = AvaloniaProperty.Register<ServerContainerControl, string>(nameof (ServerName));
    
    public string ServerName
    {
        get => GetValue(ServerNameProperty);
        set
        {
            SetValue(ServerNameProperty, value);
            ServerNameLabel.Text = value;
        }
    }

    public ServerContainerControl()
    {
        InitializeComponent();
    }
}