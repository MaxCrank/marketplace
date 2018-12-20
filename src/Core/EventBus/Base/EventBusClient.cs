using System.Collections.Generic;
using System.Linq;
using Marketplace.Core.EventBus.Exceptions;
using Marketplace.Core.EventBus.Interfaces;

namespace Marketplace.Core.EventBus.Base
{
    /// <summary>
    /// Event bus client base class.
    /// </summary>
    /// <seealso cref="IEventBusClient" />
    public abstract class EventBusClient : IEventBusClient
    {
        #region Properties

        /// <summary>
        /// The data event handlers
        /// </summary>
        protected readonly List<IEventBusMessageHandler> EventHandlers =
            new List<IEventBusMessageHandler>();

        /// <summary>
        /// Gets the application identifier.
        /// </summary>
        /// <value>
        /// The application identifier.
        /// </value>
        public string ApplicationId { get; }

        /// <summary>
        /// Gets the bus identifier.
        /// </summary>
        /// <value>
        /// The bus identifier.
        /// </value>
        public string BusId { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsConnected { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBusClient"/> class.
        /// </summary>
        /// <param name="busId">The bus identifier.</param>
        /// <param name="applicationId">The application identifier.</param>
        protected EventBusClient(string busId, string applicationId)
        {
            this.BusId = busId;
            this.ApplicationId = applicationId;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Connects this instance if not already connected.
        /// </summary>
        /// <returns>Indicates if connection is established.</returns>
        public abstract bool Connect();

        /// <summary>
        /// Publishes the specified event bus message.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        public virtual void PublishMessage(IEventBusMessage message)
        {
            this.CheckMessage(message);
            this.PublishValidMessage(message);
        }

        /// <summary>
        /// Adds the event message hanlder for the specific message event ID.
        /// </summary>
        /// <param name="handler">The handler.</param>
        public virtual void AddMessageHanlder(IEventBusMessageHandler handler)
        {
            this.CheckHandlerForAddition(handler);
            this.OnMessageHandlerAdd(handler);
        }

        /// <summary>
        /// Removes event message hanlders.
        /// </summary>
        /// <param name="messageEventId">Message event ID. If creatorId is null or empty, all corresponding handlers will be removed.</param>
        /// <param name="messageType">The message type handler is intended for.</param>
        /// <param name="creatorId">The handler creator identifier.</param>
        public virtual void RemoveMessageHanlders(string messageEventId, MessageType messageType,
            string creatorId = null)
        {
            if (this.RemoveMessageHandlersFromList(messageEventId, messageType, creatorId))
            {
                this.OnMessageHandlersRemove(messageEventId, messageType);
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public abstract void Dispose();

        #endregion

        #region Protected methods

        /// <summary>
        /// Performs bus-specific ops after removal of event message handlers.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        protected abstract void PublishValidMessage(IEventBusMessage message);

        /// <summary>
        /// Performs bus-specific ops after addition of event message handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        protected abstract void OnMessageHandlerAdd(IEventBusMessageHandler handler);

        /// <summary>
        /// Performs bus-specific ops after removal of event message handlers.
        /// </summary>
        /// <param name="messageEventId">Message event ID.</param>
        /// <param name="messageType">The message type handler is intended for.</param>
        protected abstract void OnMessageHandlersRemove(string messageEventId, MessageType messageType);

        #endregion

        #region Private methods

        /// <summary>
        /// Removes the message handlers from list of handlers.
        /// </summary>
        /// <param name="messageEventId">The message event identifier.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="creatorId">The creator identifier.</param>
        /// <returns>If any handler was removed.</returns>
        private bool RemoveMessageHandlersFromList(string messageEventId, MessageType messageType, string creatorId)
        {
            var handlersToRemove = EventHandlers
                .Where(h => h.MessageEventId == messageEventId && h.MessageType == messageType).ToList();
            if (!handlersToRemove.Any())
            {
                return false;
            }

            bool removed = false;

            if (string.IsNullOrEmpty(creatorId))
            {
                var handler = handlersToRemove.FirstOrDefault(h => h.CreatorId == creatorId);
                if (handler != null)
                {
                    this.EventHandlers.Remove(handler);
                    removed = true;
                }
            }
            else
            {
                foreach (var handler in handlersToRemove)
                {
                    this.EventHandlers.Remove(handler);
                }

                removed = true;
            }

            return removed;
        }

        /// <summary>
        /// Checks the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void CheckMessage(IEventBusMessage message)
        {
            if (!message.IsValid())
            {
                string logMessage = $"Can't publish invalid message. JSON representaion {message.ToJson()}";
                this.PublishMessage(new LogMessage { Message = logMessage, Severity = LogSeverity.Error });
                throw new EventBusException(logMessage);
            }
        }

        /// <summary>
        /// Checks the specified handler for addition.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns></returns>
        private void CheckHandlerForAddition(IEventBusMessageHandler handler)
        {
            if (!handler.IsValid())
            {
                string logMessage = $"Can't add invalid handler for message type {handler.MessageType} and event id {handler.MessageEventId} " +
                                    $"from {handler.CreatorId}; please, set all needed fields properly";
                this.PublishMessage(new LogMessage { Message = logMessage, Severity = LogSeverity.Error });
                throw new EventBusException(logMessage);
            }

            if (this.EventHandlers.Any(h => h.MessageEventId == handler.MessageEventId && h.CreatorId == handler.CreatorId))
            {
                string logMessage = $"Can't add another event handler for message event ID {handler.MessageEventId} " +
                                    $"created by {handler.CreatorId} - only single one is allowed";
                this.PublishMessage(new LogMessage { Message = logMessage, Severity = LogSeverity.Error });
                throw new EventBusException(logMessage);
            }
        }

        #endregion
    }
}
