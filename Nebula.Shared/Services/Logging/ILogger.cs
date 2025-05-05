namespace Nebula.Shared.Services.Logging;

public interface ILogger : IDisposable
{
    public void Log(LoggerCategory loggerCategory, string message);
}