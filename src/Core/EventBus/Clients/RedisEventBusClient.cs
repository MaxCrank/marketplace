﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Core.EventBus.Base;
using Marketplace.Core.EventBus.Interfaces;
using StackExchange.Redis;

namespace Marketplace.Core.EventBus.Clients
{
    /// <summary>
    /// Redis event bus client
    /// </summary>
    /// <seealso cref="Marketplace.Core.EventBus.Base.EventBusClient" />
    public class RedisEventBusClient : EventBusClient
    {
        #region Fields

        /// <summary>
        /// The connection
        /// </summary>
        private ConnectionMultiplexer connection = null;

        /// <summary>
        /// The host
        /// </summary>
        private readonly string host;

        /// <summary>
        /// The pub/sub client
        /// </summary>
        private ISubscriber pubSubClient = null;

        /// <summary>
        /// The handler action
        /// </summary>
        private Action<RedisChannel, RedisValue> handlerAction = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public override bool IsConnected => this.connection != null && this.connection.IsConnected
                                            && this.pubSubClient != null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisEventBusClient"/> class.
        /// </summary>
        /// <param name="busId">The bus identifier.</param>
        /// <param name="applicationId">The application identifier.</param>
        /// <param name="host"></param>
        public RedisEventBusClient(string busId, string applicationId, string host = "localhost") : base(busId, applicationId)
        {
            this.host = host;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Connects this instance if not already connected.
        /// </summary>
        /// <returns>Indicates if connection is established.</returns>
        public override bool Connect()
        {
            if (this.connection == null)
            {
                var options = ConfigurationOptions.Parse(this.host);
                options.ClientName = this.ApplicationId;
                this.connection = ConnectionMultiplexer.Connect(options);
            }

            if (this.pubSubClient == null)
            {
                this.pubSubClient = this.connection.GetSubscriber();
            }

            if (this.handlerAction == null)
            {
                this.handlerAction = async (channel, value) =>
                {
                    var tasks = this.EventHandlers.Where(h =>
                            h.UnifiedMessageTypeEventId == channel)
                        .Select(t => t.Handler.Invoke(value));
                    await Task.WhenAll(tasks);
                };
            }

            return this.connection.IsConnected;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            this.connection?.Dispose();
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Publishes the valid event bus message.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <returns></returns>
        protected override void PublishValidMessage(IEventBusMessage message)
        {
            this.pubSubClient.Publish(message.UnifiedMessageTypeEventId, message.ToJson());
        }

        /// <summary>
        /// Publishes the valid event bus message asynchronously.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <returns></returns>
        protected override async Task PublishValidMessageAsync(IEventBusMessage message)
        {
            await this.pubSubClient.PublishAsync(message.UnifiedMessageTypeEventId, message.ToJson());
        }

        /// <summary>
        /// Performs bus-specific ops after addition of event message handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        protected override void OnMessageHandlerAdd(IEventBusMessageHandler handler)
        {
            this.pubSubClient.Subscribe(new RedisChannel(handler.UnifiedMessageTypeEventId, RedisChannel.PatternMode.Literal), 
                this.handlerAction);
        }

        /// <summary>
        ///Performs bus-specific ops in case of complete removal of specific event message handlers.
        /// </summary>
        /// <param name="messageEventId">Message event ID.</param>
        /// <param name="messageType">The message type handler is intended for.</param>
        protected override void OnMessageHandlersRemove(string messageEventId, MessageType messageType)
        {
            this.pubSubClient.Unsubscribe(new RedisChannel($"{messageType.ToString().ToLowerInvariant()}_{messageEventId}",
                RedisChannel.PatternMode.Literal));
        }

        /// <summary>
        /// Called after <see cref="EventBusClient.Pause"/> to perform bus-specific ops.
        /// </summary>
        protected override void OnPause()
        {
            var channels = this.EventHandlers.Select(h => h.UnifiedMessageTypeEventId).Distinct();
            foreach (var channel in channels)
            {
                this.pubSubClient.Unsubscribe(channel);
            }
        }

        /// <summary>
        /// Called after <see cref="EventBusClient.Resume"/> to perform bus-specific ops.
        /// </summary>
        protected override void OnResume()
        {
            var channels = this.EventHandlers.Select(h => h.UnifiedMessageTypeEventId).Distinct();
            foreach (var channelString in channels)
            {
                this.pubSubClient.Subscribe(new RedisChannel(channelString, RedisChannel.PatternMode.Literal),
                    this.handlerAction);
            }
        }

        #endregion
    }
}