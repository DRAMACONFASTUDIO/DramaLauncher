using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nebula.Launcher.Views;

public partial class MainWindow : Window
{
    // This constructor is used when the view is created by the XAML Previewer
    public MainWindow()
    {
        InitializeComponent();
    }

    // This constructor is used when the view is created via dependency injection
    public MainWindow(MainView mainView)
        : this()
    {
        Control.Content = mainView;
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void Minimize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
    
    private void Maximize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
    
    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}