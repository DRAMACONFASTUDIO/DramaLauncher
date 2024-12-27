using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

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
    private readonly FileService _fileService;
    private readonly DebugService _debugService;

    public ConfigurationService(FileService fileService, DebugService debugService)
    {
        _fileService = fileService;
        _debugService = debugService;
    }

    public object? GetConfigValue(ConVar conVar)
    {
        if(!_fileService.ConfigurationApi.TryOpen(conVar.Name, out var stream) || 
           !ReadStream(stream, conVar.Type, out var obj))
            return conVar.DefaultValue;

        _debugService.Log("Loading config file: " + conVar.Name);
        
        return obj;
    }

    public void SetConfigValue(ConVar conVar, object value)
    {
        if(conVar.Type != value.GetType())
        {
            _debugService.Error("Error config file " + conVar.Name + ": Type is not equals " + value.GetType() + " " + conVar.Type);
            return;
        }
        
        _debugService.Log("Saving config file: " + conVar.Name);

        var stream = new MemoryStream();
        try
        {
            using var st = new StreamWriter(stream);
            st.Write(JsonSerializer.Serialize(value));
            st.Flush();
            _fileService.ConfigurationApi.Save(conVar.Name, st.BaseStream);
        }
        catch (Exception e)
        {
            _debugService.Error(e.Message);
        }
        
        stream.Close();
    }

    private bool ReadStream(Stream stream, Type type,[NotNullWhen(true)] out object? obj)
    {
        obj = null;
        try
        {
            obj = JsonSerializer.Deserialize(stream, JsonTypeInfo.CreateJsonTypeInfo(type, JsonSerializerOptions.Default));
            return obj != null;
        }
        catch (Exception e)
        {
            _debugService.Error(e.Message);
            return false;
        }
    }
}

public static class ConfigExt
{
    public static T? GetConfigValue<T>(this ConfigurationService configurationService,ConVar conVar)
    {
        var value = configurationService.GetConfigValue(conVar);
        if (value is not T tv) return default;
        return tv;
    }

    public static bool TryGetConfigValue(this ConfigurationService configurationService,ConVar conVar,[NotNullWhen(true)] out object? value)
    {
        value = configurationService.GetConfigValue(conVar);
        return value != null;
    }

    public static bool TryGetConfigValue<T>(this ConfigurationService configurationService,ConVar conVar, [NotNullWhen(true)] out T? value)
    {
        value = configurationService.GetConfigValue<T>(conVar);
        return value != null;
    }
}