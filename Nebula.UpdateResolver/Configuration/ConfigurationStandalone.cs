using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace Nebula.UpdateResolver.Configuration;

public static class ConfigurationStandalone
{
    private static FileApi _fileApi = new FileApi(Path.Join(MainWindow.RootPath, "config"));
    
    public static T? GetConfigValue<T>(ConVar<T> conVar)
    {
        ArgumentNullException.ThrowIfNull(conVar);

        try
        {
            if (_fileApi.TryOpen(GetFileName(conVar), out var stream))
                using (stream)
                {
                    var obj = JsonSerializer.Deserialize<T>(stream);
                    if (obj != null)
                    {
                        Console.WriteLine($"Successfully loaded config: {conVar.Name}");
                        return obj;
                    }
                }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading config for {conVar.Name}: {e.Message}");
        }

        Console.WriteLine($"Using default value for config: {conVar.Name}");
        return conVar.DefaultValue;
    }
    
    public static bool TryGetConfigValue<T>(ConVar<T> conVar,
        [NotNullWhen(true)] out T? value)
    {
        ArgumentNullException.ThrowIfNull(conVar);
        value = default;
        try
        {
            if (_fileApi.TryOpen(GetFileName(conVar), out var stream))
                using (stream)
                {
                    var obj = JsonSerializer.Deserialize<T>(stream);
                    if (obj != null)
                    {
                        Console.WriteLine($"Successfully loaded config: {conVar.Name}");
                        value = obj;
                        return true;
                    }
                }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading config for {conVar.Name}: {e.Message}");
        }

        Console.WriteLine($"Using default value for config: {conVar.Name}");
        return false;
    }

    public static void SetConfigValue<T>(ConVar<T> conVar, T value)
    {
        ArgumentNullException.ThrowIfNull(conVar);
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (!conVar.Type.IsInstanceOfType(value))
        {
            return;
        }

        try
        {
            var serializedData = JsonSerializer.Serialize(value);

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(serializedData);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            _fileApi.Save(GetFileName(conVar), stream);
        }
        catch (Exception e)
        {
            
        }
    }

    private  static string GetFileName<T>(ConVar<T> conVar)
    {
        return $"{conVar.Name}.json";
    }
}