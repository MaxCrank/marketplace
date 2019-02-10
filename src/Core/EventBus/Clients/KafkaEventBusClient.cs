// File: KafkaEventBusClient.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Marketplace.Core.EventBus.Base;
using Marketplace.Core.EventBus.Interfaces;
using Marketplace.Core.Serialization;

namespace Marketplace.Core.EventBus.Clients
{
    /// <summary>
    /// Kafka event bus client
    /// </summary>
    /// <seealso cref="Marketplace.Core.EventBus.Base.EventBusClient" />
    public class KafkaEventBusClient : EventBusClient
    {
        #region Fields

        /// <summary>
        /// The producer
        /// </summary>
        private Producer producer = null;

        /// <summary>
        /// The consumer
        /// </summary>
        private Consumer consumer = null;

        /// <summary>
        /// The host
        /// </summary>
        private readonly string host;

        /// <summary>
        /// The poll task
        /// </summary>
        private Task pollTask = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public override bool IsConnected => this.producer != null && this.consumer != null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaEventBusClient"/> class.
        /// </summary>
        /// <param name="busId">The bus identifier.</param>
        /// <param name="applicationId">The application identifier.</param>
        /// <param name="messageSerializer">Message serializer.</param>
        /// <param name="host">The host.</param>
        public KafkaEventBusClient(string busId, string applicationId, ISerializer messageSerializer, string host) : 
            base(busId, applicationId, messageSerializer)
        {
            this.host = host;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Connects this instance if not already connected.
        /// </summary>
        /// <returns>Indicates if connection is established.</returns>
        public override bool Connect()
        {
            var config = new Dictionary<string, object>
            {
                { "bootstrap.servers", this.host },
                { "group.id", this.ApplicationId },
                { "enable.auto.commit", true }
            };

            this.producer = new Producer(config);
            this.consumer = new Consumer(config);

            this.consumer.OnMessage += async (sender, message) =>
            {
                var tasks = this.EventHandlers.Where(h =>
                        h.MessageTagInfo.MessageTagString == message.Topic &&
                        message.Key == Encoding.Unicode.GetBytes(h.MessageEventId))
                    .Select(t => t.Handler.Invoke(this.DeserializeMessage(message.Value, t.InternalTypeToHandle)));
                await Task.WhenAll(tasks);
            };
            
            return this.IsConnected;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            this.producer?.Dispose();
            this.consumer?.Dispose();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Publishes the valid event bus message.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        protected override void PublishValidMessage(IEventBusMessage message)
        {
            this.PublishValidMessageAsync(message).RunSynchronously();
        }

        /// <summary>
        /// Publishes the valid event bus message asynchronously.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task PublishValidMessageAsync(IEventBusMessage message)
        {
            await this.producer.ProduceAsync(message.MessageTagInfo.MessageTagString,
                Encoding.Unicode.GetBytes(message.MessageEventId), this.MessageSerializer.SerializeToBytes(message));
        }

        /// <summary>
        /// Performs bus-specific ops after addition of event message handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        protected override void OnMessageHandlerAdd<T>(IEventBusMessageHandler<T> handler)
        {
            List<string> subscriptions = this.consumer.Subscription ?? new List<string>();
            string topic = handler.MessageTagInfo.MessageTagString;
            if (!subscriptions.Contains(topic))
            {
                subscriptions.Add(topic);
                this.consumer.Subscribe(subscriptions);
            }

            if (this.EventHandlers.Count == 1)
            {
                this.StartNewPoll();
            }
        }

        /// <summary>
        /// Performs bus-specific ops in case of complete removal of specific event message handlers.
        /// </summary>
        /// <param name="messageEventId">Message event ID.</param>
        /// <param name="affectedTag">The message tag handlers were intended for.</param>
        protected override void OnMessageHandlersRemove(string messageEventId, string affectedTag)
        {
            List<string> subscriptions = this.consumer.Subscription;
            if (EventHandlers.All(h => h.MessageTagInfo.MessageTagString != affectedTag))
            {
                subscriptions.Remove(affectedTag);
                this.consumer.Subscribe(subscriptions);
            }
        }

        /// <summary>
        /// Called after <see cref="EventBusClient.Pause"/> to perform bus-specific ops.
        /// </summary>
        protected override void OnPause()
        {
            this.consumer.Unsubscribe();
        }

        /// <summary>
        /// Called after <see cref="EventBusClient.Resume"/> to perform bus-specific ops.
        /// </summary>
        protected override void OnResume()
        {
            this.consumer.Subscribe(this.EventHandlers.Select(h => h.MessageTagInfo.MessageTagString).Distinct());
            this.StartNewPoll();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Starts the new poll.
        /// </summary>
        private void StartNewPoll()
        {
            if (this.pollTask != null && !this.pollTask.IsCompleted)
            {
                return;
            }

            this.pollTask = new Task(() =>
            {
                while (!this.IsPaused && this.EventHandlers.Count > 0)
                {
                    this.consumer.Poll(100);
                }
            });

            this.pollTask.Start();
        }

        #endregion
    }
}
