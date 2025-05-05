using System.Reflection;
using Nebula.Shared.FileApis;
using Nebula.Shared.Services.Logging;
using Robust.LoaderApi;

namespace Nebula.Shared.Services;

[ServiceRegister]
public class DebugService : IDisposable
{
    private ServiceLogger Root {get; set;}
    
    public DebugService()
    {
        Root = new ServiceLogger("Root");
    }

    public ILogger GetLogger(string loggerName)
    {
        return Root.GetLogger(loggerName);
    }

    public ILogger GetLogger(object objectToLog)
    {
        return Root.GetLogger(objectToLog.GetType().Name);
    }

    public void Dispose()
    {
        Root.Dispose();
    }
}

public enum LoggerCategory
{
    Log,
    Debug,
    Error
}

internal class ServiceLogger : ILogger
{
    public ServiceLogger? Root { get; private set; }
    public ServiceLogger(string category)
    {
        Category = category;

        var directory = Path.Combine(FileService.RootPath,"log", Assembly.GetEntryAssembly()?.GetName().Name ?? "App");
        if(!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        
        _fileStream = File.Open(Path.Combine(directory,$"{Category}.log"), FileMode.Create, FileAccess.Write, FileShare.Read);
        _streamWriter = new StreamWriter(_fileStream);
    }

    public string Category { get; init; }
    
    private Dictionary<string, ServiceLogger> Childs { get; init; } = new();
    
    private readonly FileStream _fileStream;
    private readonly StreamWriter _streamWriter;

    public ServiceLogger GetLogger(string category)
    {
        if (Childs.TryGetValue(category, out var logger))
            return logger;
        
        logger = new ServiceLogger(category);
        logger.Root = this;
        Childs.Add(category, logger);
        return logger;
    }
    
    public void Log(LoggerCategory loggerCategory, string message)
    {
        var output =
            $"[{DateTime.Now.ToUniversalTime():yyyy-MM-dd HH:mm:ss}][{Enum.GetName(loggerCategory)}][{Category}]: {message}";
        Console.WriteLine(output);
        
        LogToFile(output);
    }

    private void LogToFile(string output)
    {
        Root?.LogToFile(output);
        _streamWriter.WriteLine(output);
        _streamWriter.Flush();
    }

    public void Dispose()
    {
        _fileStream.Dispose();
        _streamWriter.Dispose();
        foreach (var (_, child) in Childs)
        {
            child.Dispose();
        }
        Childs.Clear();
    }
}

public static class LoggerExtensions
{
    public static void Debug(this ILogger logger,string message)
    {
        logger.Log(LoggerCategory.Debug, message);
    }

    public static void Error(this ILogger logger,string message)
    {
        logger.Log(LoggerCategory.Error, message);
    }

    public static void Log(this ILogger logger,string message)
    {
        logger.Log(LoggerCategory.Log, message);
    }

    public static void Error(this ILogger logger,Exception e)
    {
        Error(logger,e.Message + "\r\n" + e.StackTrace);
        if (e.InnerException != null)
            Error(logger, e.InnerException);
    }
}