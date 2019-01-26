// File: IEventBusClient.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;
using System.Threading.Tasks;
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
        /// Gets a value indicating whether this instance is paused.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is paused; otherwise, <c>false</c>.
        /// </value>
        bool IsPaused { get; }

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
        /// Publishes the specified event bus message asynchronously.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task PublishMessageAsync(IEventBusMessage message);

        /// <summary>
        /// Adds the event message hanlder for the specific message event ID.
        /// </summary>
        /// <param name="handler">The handler.</param>
        void AddMessageHanlder(IEventBusMessageHandler handler);

        /// <summary>
        /// Removes event message hanlders.
        /// </summary>
        /// <param name="messageEventId">Message event ID. If <paramref name="creatorId"/> is null or empty, all corresponding handlers will be removed.</param>
        /// <param name="messageType">The message type handler is intended for.</param>
        /// <param name="creatorId">The handler creator identifier.</param>
        void RemoveMessageHanlders(string messageEventId, MessageType messageType, string creatorId = null);

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes this instance.
        /// </summary>
        void Resume();
    }
}
