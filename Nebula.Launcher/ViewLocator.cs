using System;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.ViewModels;

namespace Nebula.Launcher;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;
        
        var type = param.GetType().GetCustomAttribute<ViewModelRegisterAttribute>()?.Type;

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + param.GetType()};
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}