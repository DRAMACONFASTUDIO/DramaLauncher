using Avalonia.Controls;

namespace Nebula.Launcher.MessageBox;

public partial class MessageWindow : Window
{ 
    public MessageWindow(out IMessageContainerProvider provider)
    {
        InitializeComponent();
        Content = new MessageView(out provider);
    }
}