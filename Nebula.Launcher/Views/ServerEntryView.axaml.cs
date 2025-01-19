using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Nebula.Launcher.ViewModels;

namespace Nebula.Launcher.Views;

public partial class ServerEntryView : UserControl
{
    private DispatcherTimer _scrollTimer; 
    private bool _scrollingDown = true; 
    
    public ServerEntryView()
    {
        InitializeComponent();
        StartAutoScrolling();
    }
    
    public ServerEntryView(ServerEntryModelView modelView) : this()
    {
        DataContext = modelView;
    }
    
    private void StartAutoScrolling()
    {
        _scrollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // Adjust the interval for scroll speed
        };
        _scrollTimer.Tick += AutoScroll;
        _scrollTimer.Start();
    }

    private void AutoScroll(object? sender, EventArgs e)
    {
        // Get the current offset and extent
        var currentOffset = AutoScrollViewer.Offset.X;
        var maxOffset = AutoScrollViewer.Extent.Width - AutoScrollViewer.Viewport.Width;

        if (_scrollingDown)
        {
            // Scroll down
            if (currentOffset < maxOffset)
            {
                AutoScrollViewer.Offset = new Avalonia.Vector(currentOffset + 2, AutoScrollViewer.Offset.Y); // Adjust speed
            }
            else
            {
                // Reverse direction when reaching the bottom
                _scrollingDown = false;
            }
        }
        else
        {
            // Scroll up
            if (currentOffset > 0)
            {
                AutoScrollViewer.Offset = new Avalonia.Vector(currentOffset - 2, AutoScrollViewer.Offset.Y); // Adjust speed
            }
            else
            {
                // Reverse direction when reaching the top
                _scrollingDown = true;
            }
        }
    }
}