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
        /// Gets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public override MessageType MessageType => MessageType.Log;

        /// <summary>
        /// Gets or sets the severity.
        /// </summary>
        /// <value>
        /// The severity.
        /// </value>
        public LogSeverity Severity { get; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessage"/> class.
        /// </summary>
        /// <param name="severity">The severity.</param>
        /// <param name="message">The message.</param>
        public LogMessage(LogSeverity severity, string message)
        {
            this.Severity = severity;
            this.Message = message;
        }

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
