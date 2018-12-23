using Marketplace.Core.EventBus.Base;

namespace Marketplace.Core.EventBus.Messages
{
    /// <summary>
    /// Log message class.
    /// </summary>
    /// <seealso cref="EventBusMessage" />
    public class LogMessage : EventBusMessage
    {
        #region Properties

        /// <summary>
        /// Gets or sets the severity.
        /// </summary>
        /// <value>
        /// The severity.
        /// </value>
        public LogSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        #endregion
    }

    /// <summary>
    /// Log severity enumeration.
    /// </summary>
    public enum LogSeverity
    {
        Info,
        Warning,
        Error,
        Fatal
    }
}
