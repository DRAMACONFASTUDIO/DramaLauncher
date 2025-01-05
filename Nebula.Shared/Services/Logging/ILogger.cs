namespace Nebula.Shared.Services.Logging;

public interface ILogger
{
    public void Log(LoggerCategory loggerCategory, string message);
}