// File: RedisEventBusClient.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Core.EventBus.Base;
using Marketplace.Core.EventBus.Interfaces;
using Marketplace.Core.Serialization;
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
        /// The host and port
        /// </summary>
        private readonly string hostAndPort;

        /// <summary>
        /// The password
        /// </summary>
        private readonly string password;

        /// <summary>
        /// The admin mode
        /// </summary>
        private readonly bool adminMode;

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
        /// <param name="messageSerializer">Message serializer.</param>
        /// <param name="host">The host (please, do not specify port in this parameter - use <paramref name="port"/> parameter instead).</param>
        /// <param name="password">The password.</param>
        /// <param name="adminMode">Allows admin mode operations.</param>
        /// <param name="port">The port.</param>
        public RedisEventBusClient(string busId, string applicationId, ISerializer messageSerializer, string host, string password = null,
            bool adminMode = false, int port = 6379) : base(busId, applicationId, messageSerializer)
        {
            this.hostAndPort = $"{host.TrimEnd('/')}:{port}";
            this.password = password;
            this.adminMode = adminMode;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Connects this instance if not already connected.
        /// </summary>
        /// <returns>Indicates if connection is established.</returns>
        public override bool Connect()
        {
            if (this.connection == null)
            {
                var options = ConfigurationOptions.Parse(this.hostAndPort);
                options.ClientName = this.ApplicationId;
                options.AllowAdmin = this.adminMode;
                options.ResolveDns = false;
                if (!string.IsNullOrEmpty(this.password))
                {
                    options.Password = this.password;
                }

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
                            h.MessageEventId == channel)
                        .Select(t => t.Handler.Invoke(this.DeserializeMessage((byte[])value, t.InternalTypeToHandle)));
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

        #region Protected Methods

        /// <summary>
        /// Publishes the valid event bus message.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        protected override void PublishValidMessage(IEventBusMessage message)
        {
            this.pubSubClient.Publish(message.MessageEventId, this.MessageSerializer.SerializeToString(message));
        }

        /// <summary>
        /// Publishes the valid event bus message asynchronously.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task PublishValidMessageAsync(IEventBusMessage message)
        {
            await this.pubSubClient.PublishAsync(message.MessageEventId, this.MessageSerializer.SerializeToString(message));
        }

        /// <summary>
        /// Performs bus-specific ops after addition of event message handler.
        /// </summary>
        /// <typeparam name="T">Type of the message to process.</typeparam>
        /// <param name="handler">The handler.</param>
        protected override void OnMessageHandlerAdd<T>(IEventBusMessageHandler<T> handler)
        {
            this.pubSubClient.Subscribe(new RedisChannel(handler.MessageEventId, RedisChannel.PatternMode.Literal), 
                this.handlerAction);
        }

        /// <summary>
        /// Performs bus-specific ops in case of complete removal of specific event message handlers.
        /// </summary>
        /// <param name="messageEventId">Message event ID.</param>
        /// <param name="affectedTag">The message tag handlers were intended for.</param>
        protected override void OnMessageHandlersRemove(string messageEventId, string affectedTag)
        {
            this.pubSubClient.Unsubscribe(new RedisChannel(messageEventId, RedisChannel.PatternMode.Literal));
        }

        /// <summary>
        /// Called after <see cref="EventBusClient.Pause"/> to perform bus-specific ops.
        /// </summary>
        protected override void OnPause()
        {
            var channels = this.EventHandlers.Select(h => h.MessageEventId).Distinct();
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
            var channels = this.EventHandlers.Select(h => h.MessageEventId).Distinct();
            foreach (var channelString in channels)
            {
                this.pubSubClient.Subscribe(new RedisChannel(channelString, RedisChannel.PatternMode.Literal),
                    this.handlerAction);
            }
        }

        #endregion
    }
}
