using System;
using Marketplace.Core.EventBus.Base;

namespace Marketplace.Core.EventBus.Interfaces
{
    /// <summary>
    /// Event bus client interface
    /// </summary>
    public interface IEventBusClient : IDisposable
    {
        /// <summary>
        /// Gets the application identifier.
        /// </summary>
        /// <value>
        /// The application identifier.
        /// </value>
        string ApplicationId { get; }

        /// <summary>
        /// Gets the bus identifier.
        /// </summary>
        /// <value>
        /// The bus identifier.
        /// </value>
        string BusId { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        bool IsConnected { get; }

        /// <summary>
        /// Connects this instance if not already connected.
        /// </summary>
        /// <returns>Indicates if connection is established.</returns>
        bool Connect();

        /// <summary>
        /// Publishes the specified event bus message.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        void PublishMessage(IEventBusMessage message);

        /// <summary>
        /// Adds the event message hanlder for the specific message event ID.
        /// </summary>
        /// <param name="handler">The handler.</param>
        void AddMessageHanlder(IEventBusMessageHandler handler);

        /// <summary>
        /// Removes event message hanlders.
        /// </summary>
        /// <param name="messageEventId">Message event ID. If creatorId is null or empty, all corresponding handlers will be removed.</param>
        /// <param name="messageType">The message type handler is intended for.</param>
        /// <param name="creatorId">The handler creator identifier.</param>
        void RemoveMessageHanlders(string messageEventId, MessageType messageType, string creatorId = null);
    }
}
