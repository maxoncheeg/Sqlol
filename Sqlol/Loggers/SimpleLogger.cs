using System.Diagnostics;

namespace Sqlol.Loggers;

public class SimpleLogger : ILogger
{
    private Action<string, string> _messageReceived;
    private Func<string, string, bool> _approveMethod;
    
    public SimpleLogger(Action<string, string> messageReceived, Func<string, string, bool> approveMethod)
    {
        _messageReceived = messageReceived;
        _approveMethod = approveMethod;
    }
    
    public void SendMessage(string title, string message)
    {
        _messageReceived?.Invoke(title, message);
    }

    public void LogMessage(string title, string message)
    {
        Debug.WriteLine(title + ": " + message);
    }

    public bool ApproveAction(string title, string message)
    {
        return _approveMethod?.Invoke(title, message) ?? false;
    }
}