using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Nebula.Launcher.Views.Config;
using Nebula.Launcher.Views.Pages;
using Nebula.Shared.Services;
using BindingFlags = System.Reflection.BindingFlags;

namespace Nebula.Launcher.ViewModels.Pages;

[ViewModelRegister(typeof(ConfigurationView))]
[ConstructGenerator]
public partial class ConfigurationViewModel : ViewModelBase, IConfigContext
{
    public ObservableCollection<ViewModelBase> ConfigurationVerbose { get; } = new();
    
    [GenerateProperty] private ConfigurationService ConfigurationService { get; } = default!;
    [GenerateProperty] private IServiceProvider ServiceProvider { get; } = default!;


    public void AddConfiguration<T,T1>(ConVar<T> convar) where T1: ViewModelBase, IConfigurationVerbose<T>
    {
        var configurationVerbose = ServiceProvider.GetService<T1>()!;
        configurationVerbose.Context = new ConfigContext<T>(convar, this);
        configurationVerbose.InitializeConfig();
        ConfigurationVerbose.Add(configurationVerbose);
    }

    public void InvokeUpdateConfiguration()
    {
        foreach (var verbose in ConfigurationVerbose)
        {
            if(verbose is not IUpdateInvoker invoker) continue;
            invoker.UpdateConfiguration();
        }
    }
    
    
    protected override void InitialiseInDesignMode()
    {
        AddConfiguration<string, StringConfigurationViewModel>(LauncherConVar.ILSpyUrl);
    }

    protected override void Initialise()
    {
        InitialiseInDesignMode();
    }

    public void SetValue<T>(ConVar<T> conVar, T value)
    {
        ConfigurationService.SetConfigValue(conVar, value);
    }

    public T? GetValue<T>(ConVar<T> convar)
    {
        return ConfigurationService.GetConfigValue<T>(convar);
    }
}

public interface IConfigContext
{
    public void SetValue<T>(ConVar<T> conVar, T value);
    public T? GetValue<T>(ConVar<T> convar);
}

public class ConfigContext<T> : IConfigContext
{
    public ConfigContext(ConVar<T> conVar, IConfigContext parent)
    {
        ConVar = conVar;
        Parent = parent;
    }

    public ConVar<T> ConVar { get; }
    public IConfigContext Parent { get; }

    public T? GetValue()
    {
        return GetValue(ConVar);
    }

    public void SetValue(T? value)
    {
        SetValue(ConVar!, value);
    }

    public void SetValue<T1>(ConVar<T1> conVar, T1 value)
    {
        Parent.SetValue(conVar, value);
    }

    public T1? GetValue<T1>(ConVar<T1> convar)
    {
        return Parent.GetValue(convar);
    }
}

public interface IConfigurationVerbose<T>
{
    public ConfigContext<T> Context { get; set; }
    public void InitializeConfig();
}

public interface IUpdateInvoker
{
    public void UpdateConfiguration();
}

[ViewModelRegister(typeof(StringConfigurationView))]
public partial class StringConfigurationViewModel : ViewModelBase , IConfigurationVerbose<string>, IUpdateInvoker
{
    [ObservableProperty] private string _configText = string.Empty;
    [ObservableProperty] private string? _configName = string.Empty;
    
    private string _oldText = string.Empty;
    
    public required ConfigContext<string> Context { get; set; }
    public void InitializeConfig()
    {
        ConfigName = Context.ConVar.Name;
        _oldText = Context.GetValue() ?? string.Empty;
        ConfigText = _oldText;
    }
    public void UpdateConfiguration()
    {
        if (_oldText == ConfigText) return;
        Context.SetValue(ConfigText);
    }
    
    protected override void InitialiseInDesignMode()
    {
    }

    protected override void Initialise()
    {
    }
}