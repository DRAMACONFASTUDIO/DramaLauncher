using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Nebula.Launcher.Views;

public partial class ExceptionView : UserControl
{
    public ExceptionView()
    {
        InitializeComponent();
    }

    public ExceptionView(Exception exception): this()
    {
        DataContext = exception;
    }
}