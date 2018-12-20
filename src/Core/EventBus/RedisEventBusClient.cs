using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marketplace.Core.EventBus.Base;
using Marketplace.Core.EventBus.Exceptions;
using Marketplace.Core.EventBus.Interfaces;
using StackExchange.Redis;

namespace Marketplace.Core.EventBus
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
        private ConnectionMultiplexer connection;

        /// <summary>
        /// The host
        /// </summary>
        private readonly string host;

        /// <summary>
        /// The pub/sub client
        /// </summary>
        private ISubscriber pubSubClient;

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
                this.pubSubClient = this.connection.GetSubscriber();
            }

            return this.connection.IsConnected;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            this.connection?.Dispose();
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Publishes the specified event bus message after check.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        protected override void PublishValidMessage(IEventBusMessage message)
        {
            if (!this.Connect())
            {
                throw new EventBusException($"Event bus is not connected and can't publish {message.MessageType} " +
                                            $"message with ID {message.MessageId} from {message.DateAdded}");
            }

            this.pubSubClient.Publish($"{message.MessageType.ToString().ToLowerInvariant()}_{message.MessageEventId}",
                message.ToJson());
        }

        /// <summary>
        /// Performs bus-specific ops after addition of event message handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        protected override void OnMessageHandlerAdd(IEventBusMessageHandler handler)
        {
            if (!this.Connect())
            {
                throw new EventBusException($"Can't establish connection to finish adding message handler for channel " +
                                            $"{handler.MessageType.ToString().ToLowerInvariant()}_{handler.MessageEventId}");
            }

            this.pubSubClient.Subscribe(new RedisChannel(
                $"{handler.MessageType.ToString().ToLowerInvariant()}_{handler.MessageEventId}",
                RedisChannel.PatternMode.Literal), async (channel, value) =>
                {
                    string messageBodyString = Encoding.UTF8.GetString(value);
                    var tasks = this.EventHandlers.Where(h =>
                            $"{h.MessageType.ToString().ToLowerInvariant()}_{h.MessageEventId}" == channel)
                        .Select(t => t.Handler.Invoke(messageBodyString));
                    await Task.WhenAll(tasks);
                });
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
                    throw new EventBusException($"Can't establish connection to unsubscribe from channel " +
                                                $"{messageType.ToString().ToLowerInvariant()}_{messageEventId}");
                }

                this.pubSubClient.Unsubscribe(new RedisChannel($"{messageType.ToString().ToLowerInvariant()}_{messageEventId}",
                    RedisChannel.PatternMode.Literal));
            }

        }

        #endregion
    }
}
