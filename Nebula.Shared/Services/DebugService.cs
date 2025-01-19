using Nebula.Shared.Services.Logging;

namespace Nebula.Shared.Services;

[ServiceRegister]
public class DebugService : IDisposable
{
    public ILogger Logger;

    public DebugService(ILogger logger)
    {
        Logger = logger;
    }

    public void Dispose()
    {

    }

    public void Debug(string message)
    {
        Log(LoggerCategory.Debug, message);
    }

    public void Error(string message)
    {
        Log(LoggerCategory.Error, message);
    }

    public void Log(string message)
    {
        Log(LoggerCategory.Log, message);
    }

    public void Error(Exception e)
    {
        Error(e.Message + "\r\n" + e.StackTrace);
        if (e.InnerException != null)
            Error(e.InnerException);
    }

    private void Log(LoggerCategory category, string message)
    {
        Logger.Log(category, message);
    }
}

public enum LoggerCategory
{
    Log,
    Debug,
    Error
}