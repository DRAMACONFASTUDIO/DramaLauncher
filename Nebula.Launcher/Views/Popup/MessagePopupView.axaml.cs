using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nebula.Launcher.ViewModels;

namespace Nebula.Launcher.Views.Popup;

public partial class MessagePopupView : UserControl
{
    // This constructor is used when the view is created by the XAML Previewer
    public MessagePopupView()
    {
        InitializeComponent();
    }

    // This constructor is used when the view is created via dependency injection
    public MessagePopupView(MessagePopupViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
        Console.WriteLine("NO SOSAL");
        CloseButton.KeyDown += (_,_) => Console.WriteLine("GGG11");
        CloseButton.Click += (_,_) => Console.WriteLine("GGG");
    }
}