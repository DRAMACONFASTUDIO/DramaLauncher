using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace Nebula.Launcher.Services;

public class ConVar<T>
{
    public string Name { get; }
    public Type Type => typeof(T);
    public T? DefaultValue { get; }

    public ConVar(string name, T? defaultValue = default)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DefaultValue = defaultValue;
    }
}

public static class ConVarBuilder
{
    public static ConVar<T> Build<T>(string name, T? defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("ConVar name cannot be null or whitespace.", nameof(name));

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
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _debugService = debugService ?? throw new ArgumentNullException(nameof(debugService));
    }

    public T? GetConfigValue<T>(ConVar<T> conVar)
    {
        ArgumentNullException.ThrowIfNull(conVar);

        try
        {
            if (_fileService.ConfigurationApi.TryOpen(GetFileName(conVar), out var stream))
            {
                using (stream)
                {
                    var obj = JsonSerializer.Deserialize<T>(stream);
                    if (obj != null)
                    {
                        _debugService.Log($"Successfully loaded config: {conVar.Name}");
                        return obj;
                    }
                }
            }
        }
        catch (Exception e)
        {
            _debugService.Error($"Error loading config for {conVar.Name}: {e.Message}");
        }

        _debugService.Log($"Using default value for config: {conVar.Name}");
        return conVar.DefaultValue;
    }

    public void SetConfigValue<T>(ConVar<T> conVar, T value)
    {
        ArgumentNullException.ThrowIfNull(conVar);
        if (value == null) throw new ArgumentNullException(nameof(value));
        
        if (!conVar.Type.IsInstanceOfType(value))
        {
            _debugService.Error($"Type mismatch for config {conVar.Name}. Expected {conVar.Type}, got {value.GetType()}.");
            return;
        }

        try
        {
            _debugService.Log($"Saving config: {conVar.Name}");
            var serializedData = JsonSerializer.Serialize(value);

            using var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(serializedData);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
            }

            _fileService.ConfigurationApi.Save(GetFileName(conVar), stream);
        }
        catch (Exception e)
        {
            _debugService.Error($"Error saving config for {conVar.Name}: {e.Message}");
        }
    }

    private static string GetFileName<T>(ConVar<T> conVar)
    {
        return $"{conVar.Name}.json";
    }
}

public static class ConfigExtensions
{
    public static bool TryGetConfigValue<T>(this ConfigurationService configurationService, ConVar<T> conVar, [NotNullWhen(true)] out T? value)
    {
        ArgumentNullException.ThrowIfNull(configurationService);
        ArgumentNullException.ThrowIfNull(conVar);

        value = configurationService.GetConfigValue(conVar);
        return value != null;
    }
}