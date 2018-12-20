using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marketplace.Core.EventBus.Base;
using Marketplace.Core.EventBus.Exceptions;
using Marketplace.Core.EventBus.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Marketplace.Core.EventBus
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
        private readonly ConnectionFactory connectionFactory;

        /// <summary>
        /// The publisher connection
        /// </summary>
        private IConnection publisherConnection;

        /// <summary>
        /// The subscriber connection
        /// </summary>
        private IConnection subscriberConnection;

        /// <summary>
        /// The data subscriber channel
        /// </summary>
        private IModel dataSubscriberChannel = null;

        /// <summary>
        /// The log channel
        /// </summary>
        private IModel logSubscriberChannel = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public override bool IsConnected => publisherConnection != null && publisherConnection.IsOpen
                                            && subscriberConnection != null && subscriberConnection.IsOpen;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqEventBusClient"/> class.
        /// </summary>
        /// <param name="busId"></param>
        /// <param name="applicationId"></param>
        /// <param name="host"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public RabbitMqEventBusClient(string busId, string applicationId, string host = "localhost",
            string userName = "guest", string password = "guest") : base(busId, applicationId)
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

        #region Public methods

        /// <summary>
        /// Connects this instance if not already connected.
        /// </summary>
        /// <returns>Indicates if connection is established.</returns>
        public override bool Connect()
        {
            if (publisherConnection == null || !publisherConnection.IsOpen)
            {
                publisherConnection?.Dispose();
                publisherConnection?.Close();
                publisherConnection = connectionFactory.CreateConnection();
            }

            if (subscriberConnection == null || !subscriberConnection.IsOpen)
            {
                publisherConnection?.Dispose();
                subscriberConnection?.Close();
                subscriberConnection = connectionFactory.CreateConnection();
            }

            return this.IsConnected;
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        public override void Dispose()
        {
            this.publisherConnection?.Close();
            this.subscriberConnection?.Close();
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Publishes the specified valid event bus message.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        protected override void PublishValidMessage(IEventBusMessage message)
        {
            if (!this.Connect())
            {
                throw new EventBusException($"Event bus is not connected and can't publish {message.MessageType} " +
                    $"message with ID {message.MessageId} from {message.DateAdded}");
            }

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
        /// Performs bus-specific ops after addition of event message handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        protected override void OnMessageHandlerAdd(IEventBusMessageHandler handler)
        {
            string messageQueue = handler.MessageType.ToString().ToLower();
            this.AddMessageHandlerSubscriber(handler.MessageType == MessageType.Log ? logSubscriberChannel : dataSubscriberChannel, 
                messageQueue, handler.MessageEventId);
        }

        /// <summary>
        /// Performs bus-specific ops after removal of event message handlers.
        /// </summary>
        /// <param name="messageEventId">Message event ID.</param>
        /// <param name="messageType">The message type handler is intended for.</param>
        protected override void OnMessageHandlersRemove(string messageEventId, MessageType messageType)
        {
            if (this.EventHandlers.All(h => h.MessageEventId != messageEventId && h.MessageType != messageType))
            {
                if (!this.Connect())
                {
                    throw new EventBusException($"Can't establish connection to unbind from queue after removing all message handlers " +
                                                $"for message type {messageType} with ID {messageEventId}");
                }

                var subscriberChannel = messageType == MessageType.Log ? logSubscriberChannel :
                    dataSubscriberChannel;
                string messageQueue = messageType.ToString().ToLowerInvariant();

                subscriberChannel.QueueUnbind(messageQueue, this.BusId, messageEventId);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Adds the message handler subscriber.
        /// </summary>
        /// <param name="subChannel">Subscriber channel.</param>
        /// <param name="messageQueue">Message queue name.</param>
        /// <param name="messageEventId">Message event ID.</param>
        private void AddMessageHandlerSubscriber(IModel subChannel, string messageQueue, string messageEventId)
        {
            if (!this.Connect())
            {
                throw new EventBusException($"Can't establish connection to finish adding message handler for message queue {messageQueue} " +
                                            $"and event ID {messageEventId}");
            }

            if (subChannel == null)
            {
                subChannel = subscriberConnection.CreateModel();

                subChannel.ExchangeDeclare(this.BusId, ExchangeType.Direct);

                var consumer = new EventingBasicConsumer(subChannel);

                consumer.Received += async (model, eventArgs) =>
                {
                    string messageBodyString = Encoding.UTF8.GetString(eventArgs.Body);
                    var tasks = this.EventHandlers.Where(h => h.MessageEventId == eventArgs.RoutingKey)
                        .Select(t => t.Handler.Invoke(messageBodyString));
                    await Task.WhenAll(tasks);
                    subChannel.BasicAck(eventArgs.DeliveryTag, false);
                };

                subChannel.BasicConsume(messageQueue, false, consumer);

                if (messageQueue == MessageType.Log.ToString().ToLowerInvariant())
                {
                    this.logSubscriberChannel = subChannel;
                }
                else
                {
                    this.dataSubscriberChannel = subChannel;
                }
            }

            subChannel.QueueBind(messageQueue, this.BusId, messageEventId);
        }

        #endregion
    }
}
