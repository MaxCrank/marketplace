// File: EventBusClient.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Core.DataTypes.Collections;
using Marketplace.Core.EventBus.Exceptions;
using Marketplace.Core.EventBus.Interfaces;
using Marketplace.Core.EventBus.Messages;
using Marketplace.Core.Serialization;

namespace Marketplace.Core.EventBus.Base
{
    /// <summary>
    /// Event bus client base class.
    /// </summary>
    /// <seealso cref="IEventBusClient" />
    public abstract class EventBusClient : IEventBusClient
    {
        #region Fields

        /// <summary>
        /// The message serializer
        /// </summary>
        protected ISerializer MessageSerializer;

        #endregion

        #region Properties

        /// <summary>
        /// The data event handlers
        /// </summary>
        protected readonly IList<IEventBusMessageHandler<IEventBusMessage>> EventHandlers =
            new ConcurrentList<IEventBusMessageHandler<IEventBusMessage>>();

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

        /// <summary>
        /// Gets a value indicating whether this instance is paused.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is paused; otherwise, <c>false</c>.
        /// </value>
        public bool IsPaused { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBusClient"/> class.
        /// </summary>
        /// <param name="busId">The bus identifier.</param>
        /// <param name="applicationId">The application identifier.</param>
        /// <param name="serializer">Message serializer.</param>
        protected EventBusClient(string busId, string applicationId, ISerializer serializer)
        {
            this.BusId = busId.ToLowerInvariant();
            this.ApplicationId = applicationId.ToLowerInvariant();
            this.MessageSerializer = serializer;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Connects this instance if not already connected.
        /// </summary>
        /// <returns>Indicates if connection is established.</returns>
        public abstract bool Connect();

        /// <summary>
        /// Publishes the specified event bus message.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        public void PublishMessage(IEventBusMessage message)
        {
            this.ValidateMessage(message);

            if (!this.Connect())
            {
                throw new EventBusException($"Can't establish connection to publish message {message}");
            }

            this.PublishValidMessage(message);
        }

        /// <summary>
        /// Publishes the specified event bus message asynchronously.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        public async Task PublishMessageAsync(IEventBusMessage message)
        {
            this.ValidateMessage(message);

            if (!this.Connect())
            {
                throw new EventBusException($"Can't establish connection to publish message {message}");
            }

            await this.PublishValidMessageAsync(message);
        }

        /// <summary>
        /// Adds the event message hanlder for the specific message event ID.
        /// </summary>
        /// <param name="handler">The handler.</param>
        public void AddMessageHanlder<T>(IEventBusMessageHandler<T> handler) where T: IEventBusMessage
        {
            this.ValidateMessageHandler(handler);

            if (!this.Connect())
            {
                throw new EventBusException($"Can't establish connection to finish adding message handler for {handler}");
            }

            this.EventHandlers.Add(handler as IEventBusMessageHandler<IEventBusMessage>);
            this.OnMessageHandlerAdd(handler);
        }

        /// <summary>
        /// Removes event message hanlders.
        /// </summary>
        /// <param name="creatorId">The handler creator identifier. If it's null or empty, all corresponding handlers will be removed.</param>
        public void RemoveMessageHanlders<T>(string creatorId = null) where T: IEventBusMessage
        {
            string messageEventId = EventBusMessageInfoResolver.GetEventMessageId<T>();
            bool removed = this.RemoveMessageHandlersFromList(messageEventId, creatorId);
            if (removed)
            {
                if (!this.Connect())
                {
                    throw new EventBusException($"Can't establish connection to unsubscribe from messages " +
                                                $"with ID {messageEventId} and creator {creatorId}");
                }

                this.OnMessageHandlersRemove(messageEventId, EventBusMessageInfoResolver.GetTagInfo<T>().MessageTagString);
            }
        }

        /// <summary>
        /// Pauses message handling.
        /// </summary>
        public void Pause()
        {
            this.IsPaused = true;
            this.OnPause();
        }

        /// <summary>
        /// Resumes message handling.
        /// </summary>
        public void Resume()
        {
            this.IsPaused = false;
            this.OnResume();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            IDisposable handlersList = this.EventHandlers as IDisposable;
            handlersList?.Dispose();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Publishes the valid event bus message.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        protected abstract void PublishValidMessage(IEventBusMessage message);

        /// <summary>
        /// Publishes the valid event bus message asynchronously.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        protected abstract Task PublishValidMessageAsync(IEventBusMessage message);

        /// <summary>
        /// Performs bus-specific ops after addition of event message handler.
        /// </summary>
        /// <typeparam name="T">Type of the message to process.</typeparam>
        /// <param name="handler">The handler.</param>
        protected abstract void OnMessageHandlerAdd<T>(IEventBusMessageHandler<T> handler) where T: IEventBusMessage;

        /// <summary>
        /// Performs bus-specific ops in case of complete removal of specific event message handlers.
        /// </summary>
        /// <param name="messageEventId">Message event ID.</param>
        /// <param name="affectedTag">The message tag handlers were intended for.</param>
        protected abstract void OnMessageHandlersRemove(string messageEventId, string affectedTag);

        /// <summary>
        /// Called after <see cref="EventBusClient.Pause"/> to perform bus-specific ops.
        /// </summary>
        protected abstract void OnPause();

        /// <summary>
        /// Called after <see cref="EventBusClient.Resume"/> to perform bus-specific ops.
        /// </summary>
        protected abstract void OnResume();

        /// <summary>
        /// Deserializes the message.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="deserializeType">Type to deserialize to.</param>
        /// <returns>Deserialized message.</returns>
        protected IEventBusMessage DeserializeMessage(byte[] data, Type deserializeType)
        {
            return this.MessageSerializer.Deserialize(data, deserializeType) as IEventBusMessage;;
        }

        /// <summary>
        /// Deserializes the message.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="deserializeType">Type to deserialize to.</param>
        /// <returns>Deserialized message.</returns>
        protected IEventBusMessage DeserializeMessage(string data, Type deserializeType)
        {
            return this.MessageSerializer.Deserialize(data, deserializeType) as IEventBusMessage;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Removes the message handlers from list of handlers.
        /// </summary>
        /// <param name="messageEventId">The message event identifier.</param>
        /// <param name="creatorId">The creator identifier.</param>
        /// <returns>If any handler has been removed.</returns>
        private bool RemoveMessageHandlersFromList(string messageEventId, string creatorId = null)
        {
            var handlersToRemove = this.EventHandlers
                .Where(h => h.MessageEventId == messageEventId).ToList();

            if (!string.IsNullOrEmpty(creatorId))
            {
                handlersToRemove = handlersToRemove.Where(h => h.CreatorId == creatorId).ToList();
            }

            handlersToRemove.ForEach(h => this.EventHandlers.Remove(h));
            
            return handlersToRemove.Count > 0;
        }

        /// <summary>
        /// Validates the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ValidateMessage(IEventBusMessage message)
        {
            if (!message.IsValid())
            {
                string logMessage = $"Can't publish invalid message {message}";
                throw new EventBusException(logMessage);
            }
        }

        /// <summary>
        /// Validates the specified handler.
        /// </summary>
        /// <typeparam name="T">Type of the message to process.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <exception cref="EventBusException"></exception>
        private void ValidateMessageHandler<T>(IEventBusMessageHandler<T> handler) where T: IEventBusMessage
        {
            if (!handler.IsValid())
            {
                string logMessage = $"Can't add invalid handler for message tag {handler.MessageTagInfo.MessageTagString} " +
                                    $"and event id {handler.MessageEventId} " +
                                    $"from {handler.CreatorId}; please, set all needed fields properly.";
                throw new EventBusException(logMessage);
            }
        }

        #endregion
    }
}
