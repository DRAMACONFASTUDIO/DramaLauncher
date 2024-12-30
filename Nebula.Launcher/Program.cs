using Avalonia;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Nebula.Launcher;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var uiMode = false;

        if (uiMode)
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        else
        {
            RunNoUI(args);
        }
    }

    private static void RunNoUI(string[] args)
    {
        var services = new ServiceCollection();
        services.AddAvaloniaServices();
        services.AddServices();
        services.AddSingleton<AppNoUi>(); //Separated because no ui
        
        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetService<AppNoUi>()!.Run(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}