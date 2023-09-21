namespace DeliveryNotifier.Interfaces
{
    /// <summary>
    ///     Defines a generic Logger.
    /// </summary>
    public interface ILogger
    {
        void LogDebug(string message);

        void LogInfo(string message);

        void LogError(string message, Exception ex);

        void LogError(string message);

        void LogWarning(string message);

        void LogWarning(string message, Exception ex);

    }
}
