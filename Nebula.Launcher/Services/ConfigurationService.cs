using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Nebula.Launcher.FileApis;

namespace Nebula.Launcher.Services;

public class ConVar<T>
{
    public string Name { get; }
    public Type Type => typeof(T);
    public T? DefaultValue { get; }
    
    public ConVar(string name, T? defaultValue)
    {
        Name = name;
        DefaultValue = defaultValue;
    }
}

public static class ConVarBuilder
{
    public static ConVar<T> Build<T>(string name, T? defaultValue = default)
    {
        return new ConVar<T>(name, defaultValue);
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

    public T? GetConfigValue<T>(ConVar<T> conVar)
    {
        if(!_fileService.ConfigurationApi.TryOpen(GetFileName(conVar), out var stream) || 
           !ReadStream<T>(stream, out var obj))
            return conVar.DefaultValue;

        _debugService.Log("Loading config file: " + conVar.Name);
        
        return obj;
    }

    public void SetConfigValue<T>(ConVar<T> conVar, object value)
    {
        if(conVar.Type != value.GetType())
        {
            _debugService.Error("Error config file " + conVar.Name + ": Type is not equals " + value.GetType() + " " + conVar.Type);
            return;
        }
        
        _debugService.Log("Saving config file: " + conVar.Name);
        WriteStream(conVar, value);
    }

    private bool ReadStream<T>(Stream stream,[NotNullWhen(true)] out T? obj)
    {
        obj = default;
        try
        {
            obj = JsonSerializer.Deserialize<T>(stream);
            return obj != null;
        }
        catch (Exception e)
        {
            _debugService.Error(e);
            return false;
        }
    }

    private void WriteStream<T>(ConVar<T> conVar, object value)
    {
        using var stream = new MemoryStream();
        
        try
        {
            using var st = new StreamWriter(stream);
            var ser = JsonSerializer.Serialize(value);
            st.Write(ser);
            st.Flush(); 
            stream.Seek(0, SeekOrigin.Begin);
            _fileService.ConfigurationApi.Save(GetFileName(conVar), stream);
        }
        catch (Exception e)
        {
            _debugService.Error(e);
        }
    }

    private string GetFileName<T>(ConVar<T> conVar)
    {
        return conVar.Name + ".json";
    }
}

public static class ConfigExt
{
    
    public static bool TryGetConfigValue<T>(this ConfigurationService configurationService,ConVar<T> conVar, [NotNullWhen(true)] out T? value)
    {
        value = configurationService.GetConfigValue(conVar);
        return value != null;
    }
}