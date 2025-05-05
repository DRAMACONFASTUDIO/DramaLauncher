using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Nebula.Launcher.MessageBox;

public partial class MessageView : UserControl, IMessageContainerProvider
{
    public MessageView(out IMessageContainerProvider provider)
    {
        InitializeComponent();
        provider = this;
    }

    public void ShowMessage(string message, string title)
    {
        Title.Content = title;
        Message.Content = message;
    }
}

public interface IMessageContainerProvider
{
    public void ShowMessage(string message, string title);
}