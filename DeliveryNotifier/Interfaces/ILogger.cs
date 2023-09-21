using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryNotifier.Interfaces
{
    /// <summary>
    ///     Defines a generic Logger.
    /// </summary>
    public interface ILogger
    {
        void LogDebug(string debugInfo);

        void LogDebug(object debugInfo);

        void LogDebug(object debugInfo, Exception ex);

        void LogInfo(string info);

        void LogInfo(Guid id, string info);

        void LogError(string message, Exception ex);

        void LogError(string message);

        void LogError(Exception ex);

        void LogWarning(string message);

        void LogWarning(string message, Exception ex);

        void LogWarning(Exception ex);

        void LogFatal(string message);

        void LogFatal(string message, Exception ex);

        void LogFatal(Exception exception);

        void LogAudit(string message);

        void LogAudit(Exception exception);

        void LogAudit(string message, Exception ex);
    }
}
