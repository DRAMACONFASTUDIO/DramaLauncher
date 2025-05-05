using System;
using System.Threading;
using Avalonia;

namespace Nebula.Launcher;

public static class Program
{
    private static Mutex? _mutex;
    public static bool IsNewInstance;
    [STAThread]
    public static void Main(string[] args)
    {
        _mutex = new Mutex(true, $"Global\\Nebula.Launcher", out IsNewInstance);
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        
        if (IsNewInstance)
            _mutex.ReleaseMutex();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}

