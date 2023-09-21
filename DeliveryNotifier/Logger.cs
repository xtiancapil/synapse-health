using Serilog;
using ILogger = DeliveryNotifier.Interfaces.ILogger;

namespace DeliveryNotifier
{
    public class Logger : ILogger
    {
        public void LogDebug(string message)
        {
            Log.Debug(message);
        }

        public void LogError(string message, Exception ex)
        {
            Log.Error(ex, message);
        }

        public void LogError(string message)
        {
            Log.Error(message);
        }

        public void LogInfo(string message)
        {
            Log.Information(message);
        }

        public void LogWarning(string message)
        {
            Log.Warning(message);
        }

        public void LogWarning(string message, Exception ex)
        {
            Log.Warning(ex, message);
        }
    }
}
