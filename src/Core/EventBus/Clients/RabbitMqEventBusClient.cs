// File: RabbitMqEventBusClient.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Core.EventBus.Base;
using Marketplace.Core.EventBus.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Marketplace.Core.EventBus.Clients
{
    /// <summary>
    /// RabbitMQ event bus client
    /// </summary>
    public class RabbitMqEventBusClient : EventBusClient
    {
        #region Fields

        /// <summary>
        /// The connection factory
        /// </summary>
        private readonly ConnectionFactory connectionFactory = null;

        /// <summary>
        /// The consumer tags
        /// </summary>
        private readonly ConcurrentDictionary<string, string> consumerTags = 
            new ConcurrentDictionary<string, string>();

        /// <summary>
        /// The publisher connection
        /// </summary>
        private IConnection publisherConnection = null;

        /// <summary>
        /// The subscriber connection
        /// </summary>
        private IConnection subscriberConnection = null;

        /// <summary>
        /// The subscriber channel
        /// </summary>
        private IModel subscriberChannel = null;

        /// <summary>
        /// The basic consumer
        /// </summary>
        private EventingBasicConsumer basicConsumer = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public override bool IsConnected => this.publisherConnection != null && this.publisherConnection.IsOpen
                                            && this.subscriberConnection != null && this.subscriberConnection.IsOpen
                                            && this.subscriberChannel != null && this.subscriberChannel.IsOpen
                                            && this.basicConsumer != null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqEventBusClient"/> class.
        /// </summary>
        /// <param name="busId">The bus identifier.</param>
        /// <param name="applicationId">The application identifier.</param>
        /// <param name="host">The host.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        public RabbitMqEventBusClient(string busId, string applicationId, string host, string userName = "guest", string password = "guest") : 
            base(busId, applicationId)
        {
            connectionFactory = new ConnectionFactory
            {
                HostName = host,
                UserName = userName,
                Password = password
            };

            if (!string.IsNullOrEmpty(userName))
            {
                connectionFactory.UserName = userName;
            }

            if (!string.IsNullOrEmpty(password))
            {
                connectionFactory.Password = password;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Connects this instance if not already connected.
        /// </summary>
        /// <returns>Indicates if connection is established.</returns>
        public override bool Connect()
        {
            if (publisherConnection == null)
            {
                publisherConnection = connectionFactory.CreateConnection();
            }

            if (subscriberConnection == null)
            {
                subscriberConnection = connectionFactory.CreateConnection();
            }

            if (this.subscriberChannel == null)
            {
                this.subscriberChannel = subscriberConnection.CreateModel();
            }

            if (this.basicConsumer == null)
            {
                this.basicConsumer = new EventingBasicConsumer(this.subscriberChannel);

                this.basicConsumer.Received += async (model, eventArgs) =>
                {
                    var tasks = this.EventHandlers.Where(h => h.MessageEventId == eventArgs.RoutingKey)
                        .Select(t => t.Handler.Invoke(eventArgs.Body));
                    await Task.WhenAll(tasks);
                    this.subscriberChannel.BasicAck(eventArgs.DeliveryTag, false);
                };
            }

            return this.IsConnected;
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            this.publisherConnection?.Dispose();
            this.subscriberConnection?.Dispose();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Publishes the valid event bus message.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        protected override void PublishValidMessage(IEventBusMessage message)
        {
            using (var channel = publisherConnection.CreateModel())
            {
                string queueName = message.MessageType.ToString().ToLowerInvariant();

                channel.ExchangeDeclare(this.BusId, ExchangeType.Direct);
                channel.QueueDeclare(queueName, true, false, true, new Dictionary<string, object>()
                {
                    { "x-queue-mode", "lazy" }
                });

                channel.QueueBind(queueName, this.BusId, message.MessageEventId);

                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2;
                properties.AppId = this.ApplicationId;

                channel.BasicPublish(this.BusId,
                    queueName,
                    true,
                    properties,
                    message.ToJsonBytes());
            }
        }

        /// <summary>
        /// Publishes the valid event bus message asynchronously.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task PublishValidMessageAsync(IEventBusMessage message)
        {
            await Task.Run(() =>
            {
                this.PublishValidMessage(message);
            });
        }

        /// <summary>
        /// Performs bus-specific ops after addition of event message handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        protected override void OnMessageHandlerAdd(IEventBusMessageHandler handler)
        {
            string messageQueue = handler.MessageType.ToString().ToLower();

            if (this.EventHandlers.Count(h => h.MessageType == handler.MessageType) == 1)
            {
                this.subscriberChannel.ExchangeDeclare(this.BusId, ExchangeType.Direct);

                string tag = this.subscriberChannel.BasicConsume(messageQueue, false, this.basicConsumer);
                consumerTags[messageQueue] = tag;
            }

            this.subscriberChannel.QueueBind(messageQueue, this.BusId, handler.MessageEventId);
        }

        /// <summary>
        /// Performs bus-specific ops in case of complete removal of specific event message handlers.
        /// </summary>
        /// <param name="messageEventId">Message event ID.</param>
        /// <param name="messageType">The message type handler is intended for.</param>
        protected override void OnMessageHandlersRemove(string messageEventId, MessageType messageType)
        {
            string messageQueue = messageType.ToString().ToLowerInvariant();

            this.subscriberChannel.QueueUnbind(messageQueue, this.BusId, messageEventId);
            if (this.EventHandlers.All(h => h.MessageType != messageType))
            {
                this.subscriberChannel.BasicCancel(this.consumerTags[messageQueue]);
                this.consumerTags.Remove(messageQueue, out string removedTag);
            }
        }

        /// <summary>
        /// Called after <see cref="EventBusClient.Pause"/> to perform bus-specific ops.
        /// </summary>
        protected override void OnPause()
        {
            foreach (var tag in consumerTags.Values)
            {
                this.subscriberChannel.BasicCancel(tag);
            }
        }

        /// <summary>
        /// Called after <see cref="EventBusClient.Resume"/> to perform bus-specific ops.
        /// </summary>
        protected override void OnResume()
        {
            var keys = consumerTags.Keys.ToArray();
            foreach (var key in keys)
            {
                this.consumerTags[key] =
                    this.subscriberChannel.BasicConsume(key, false, basicConsumer);
            }
        }

        #endregion
    }
}
