using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nebula.Launcher.ViewModels;

namespace Nebula.Launcher.Views.Popup;

public partial class ExceptionView : UserControl
{
    public ExceptionView()
    {
        InitializeComponent();
    }

    public ExceptionView(ExceptionViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}