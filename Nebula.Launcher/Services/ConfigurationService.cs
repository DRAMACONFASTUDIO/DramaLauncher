using System;
using System.Diagnostics.CodeAnalysis;

namespace Nebula.Launcher.Services;

public class ConVar
{
    public string Name { get; }
    public Type Type { get; }
    public object? DefaultValue { get; }

    private ConVar(string name, Type type, object? defaultValue)
    {
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
    }

    public static ConVar Build<T>(string name, T? defaultValue = default)
    {
        return new ConVar(name, typeof(T), defaultValue);
    }
}

[ServiceRegister]
public class ConfigurationService
{
    public ConfigurationService()
    {
        
    }

    public object? GetConfigValue(ConVar conVar)
    {
        return conVar.DefaultValue;
    }

    public T? GetConfigValue<T>(ConVar conVar)
    {
        var value = GetConfigValue(conVar);
        if (value is not T tv) return default;
        return tv;
    }

    public bool TryGetConfigValue(ConVar conVar,[NotNullWhen(true)] out object? value)
    {
        value = GetConfigValue(conVar);
        return value != null;
    }

    public bool TryGetConfigValue<T>(ConVar conVar, [NotNullWhen(true)] out T? value)
    {
        value = GetConfigValue<T>(conVar);
        return value != null;
    }

    public void SetValue(ConVar conVar, object value)
    {
        if(conVar.Type != value.GetType()) 
            return;
    }
}