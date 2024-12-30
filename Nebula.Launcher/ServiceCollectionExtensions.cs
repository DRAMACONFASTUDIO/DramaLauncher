using System;
using System.Collections.Generic;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.ViewModels;
using Nebula.Launcher.Views;
using Nebula.Launcher.Views.Pages;

namespace Nebula.Launcher;

public static class ServiceCollectionExtensions
{
    public static void AddAvaloniaServices(this IServiceCollection services)
    {
        services.AddSingleton<IDispatcher>(_ => Dispatcher.UIThread);
        services.AddSingleton(_ => Application.Current?.ApplicationLifetime ?? throw new InvalidOperationException("No application lifetime is set"));

        services.AddSingleton(sp =>
            sp.GetRequiredService<IApplicationLifetime>() switch
            {
                IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow ?? throw new InvalidOperationException("No main window set"),
                ISingleViewApplicationLifetime singleViewPlatform => TopLevel.GetTopLevel(singleViewPlatform.MainView) ?? throw new InvalidOperationException("Could not find top level element for single view"),
                _ => throw new InvalidOperationException($"Could not find {nameof(TopLevel)} element"),
            }
        );

        services.AddSingleton(sp => sp.GetRequiredService<TopLevel>().StorageProvider);
    }

    public static void AddViews(this IServiceCollection services)
    {
        services.AddTransient<MainWindow>();

        foreach (var (viewModel, view) in GetTypesWithHelpAttribute(Assembly.GetExecutingAssembly()))
        {
            services.AddSingleton(viewModel);
            services.AddTransient(view);
        }
        
    }

    public static void AddServices(this IServiceCollection services)
    {
        foreach (var (type, inference) in GetServicesWithHelpAttribute(Assembly.GetExecutingAssembly()))
        {
            if (inference is null)
            {
                services.AddSingleton(type);
            }
            else
            {
                services.AddSingleton(inference, type);
            }
        }
    }
    
    private static IEnumerable<(Type,Type)> GetTypesWithHelpAttribute(Assembly assembly) {
        foreach(Type type in assembly.GetTypes())
        {
            var attr = type.GetCustomAttribute<ViewRegisterAttribute>();
            if (attr is not null) {
                yield return (type, attr.Type);
            }
        }
    }
    
    private static IEnumerable<(Type,Type?)> GetServicesWithHelpAttribute(Assembly assembly) {
        foreach(Type type in assembly.GetTypes())
        {
            var attr = type.GetCustomAttribute<ServiceRegisterAttribute>();
            if (attr is not null) {
                yield return (type, attr.Inference);
            }
        }
    }
}

public sealed class ServiceRegisterAttribute : Attribute
{
    public Type? Inference { get; }
    public bool IsSingleton { get; }

    public ServiceRegisterAttribute(Type? inference = null, bool isSingleton = true)
    {
        IsSingleton = isSingleton;
        Inference = inference;
    }
}