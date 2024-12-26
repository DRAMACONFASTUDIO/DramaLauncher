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

        if (vm is not ViewModelBase vmb) return false;

        viewModelBase = vmb;
        return true;
    }
    
    public bool TryGetViewModel<T>([NotNullWhen(true)] out T? viewModelBase) where T: ViewModelBase
    {
        var success = TryGetViewModel(typeof(T), out var vmb);
        viewModelBase = (T?)vmb;
        return success;
    }

    public T GetViewModel<T>() where T: ViewModelBase
    {
        TryGetViewModel<T>(out var viewModelBase);
        return viewModelBase!;
    }

    public void AssertDesignMode()
    {
        if (!Design.IsDesignMode) throw new Exception();
    }
}