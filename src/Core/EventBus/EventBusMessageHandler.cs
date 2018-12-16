using System;
using System.Threading.Tasks;
using Marketplace.Core.EventBus.Base;
using Marketplace.Core.EventBus.Interfaces;

namespace Marketplace.Core.EventBus
{
    /// <summary>
    /// Base event bus message handler class.
    /// </summary>
    /// <seealso cref="IEventBusMessageHandler" />
    public class EventBusMessageHandler : IEventBusMessageHandler
    {
        #region Properties

        /// <summary>
        /// Gets the creator identifier.
        /// </summary>
        /// <value>
        /// The creator identifier.
        /// </value>
        public string CreatorId { get; protected set; }

        /// <summary>
        /// Gets the message event identifier.
        /// </summary>
        /// <value>
        /// The message event identifier.
        /// </value>
        public string MessageEventId { get; protected set; }

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <value>
        /// The handler.
        /// </value>
        public Func<string, Task> Handler { get; protected set; }

        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public MessageType MessageType { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBusMessageHandler"/> class.
        /// </summary>
        /// <param name="creatorId">The creator identifier.</param>
        /// <param name="messageEventId">The message event identifier.</param>
        /// <param name="handler">The event handler.</param>
        /// <param name="messageType">The message type of handler.</param>
        public EventBusMessageHandler(string creatorId, string messageEventId, Func<string, Task> handler,
            MessageType messageType = MessageType.Data)
        {
            this.CreatorId = creatorId;
            this.MessageEventId = messageEventId;
            this.Handler = handler;
            this.MessageType = messageType;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Returns true if handler is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this handler is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(this.CreatorId) &&
                   !string.IsNullOrEmpty(this.MessageEventId) &&
                   this.MessageType != MessageType.Unknown &&
                   this.Handler != null;
        }

        #endregion
    }
}
