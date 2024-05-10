using System.Diagnostics;

namespace Sqlol.Loggers;

public class SimpleLogger : ILogger
{
    private Action<string, string> _messageReceived;
    public SimpleLogger(Action<string, string> messageReceived)
    {
        _messageReceived = messageReceived;
    }
    
    public void SendMessage(string title, string message)
    {
        _messageReceived?.Invoke(title, message);
    }

    public void LogMessage(string title, string message)
    {
        Debug.WriteLine(title + ": " + message);
    }
}