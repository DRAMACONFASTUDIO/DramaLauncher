using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nebula.Launcher.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    public ViewModelBase()
    {
        AssertDesignMode();
        _serviceProvider = default!;
    }
    public ViewModelBase(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool TryGetViewModel(Type type,[NotNullWhen(true)] out ViewModelBase? viewModelBase)
    {
        viewModelBase = null;
        var vm = Design.IsDesignMode
            ? Activator.CreateInstance(type)
            : _serviceProvider.GetService(type);
        
        Console.WriteLine(vm?.ToString());

        if (vm is not ViewModelBase vmb) return false;

        viewModelBase = vmb;
        return true;
    }

    public void AssertDesignMode()
    {
        if (!Design.IsDesignMode) throw new Exception();
    }
}