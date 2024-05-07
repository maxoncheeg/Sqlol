namespace Sqlol.Loggers;

public interface ILogger
{
    public void SendMessage(string title, string message);
    public void LogMessage(string title, string message);
}