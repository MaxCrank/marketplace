// File: EventBusClientTester.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marketplace.Core.EventBus.Clients;
using Marketplace.Core.EventBus.Interfaces;
using Marketplace.Core.Serialization.Serializers;
using Marketplace.Core.Tests.Base;
using NUnit.Framework;

namespace Marketplace.Core.Tests.EventBus
{
    /// <summary>
    /// Base test class for any event bus client implementation.
    /// </summary>
    /// <seealso cref="Marketplace.Core.Tests.Base.BasicTester" />
    [Category("EventBus")]
    public abstract class EventBusClientTester : BasicTester
    {
        #region Fields

        /// <summary>
        /// The event bus clients.
        /// </summary>
        private readonly List<IEventBusClient> eventBusClients = new List<IEventBusClient>();

        #endregion

        #region Properties

        protected virtual string BusId => "TestBus";

        #endregion

        #region Configuration

        /// <summary>
        /// Initializes the fixture resources.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task InitFixtureResources()
        {
            var serializer = new JsonSerializer();
            this.eventBusClients.Add(new RedisEventBusClient(BusId, AppId, serializer, "test-core.redis.com", Password, true));
            this.eventBusClients.Add(new KafkaEventBusClient(BusId, AppId, serializer, "test-core.kafka.com"));
            this.eventBusClients.Add(new RabbitMqEventBusClient(BusId, AppId, serializer, "test-core.rabbitmq.com", "testCoreUser", Password));

            this.eventBusClients.ForEach(c =>
            {
                Assert.IsTrue(c.Connect());
            });

            await Task.CompletedTask;
        }

        /// <summary>
        /// Releases the fixture resources.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task ReleaseFixtureResources()
        {
            bool flush = this.CurrentRunningFixturesCount == 0;
            this.PerformWithAllEventBusClients(client =>
            {
                if (flush)
                {
                    client?.Pause();
                }

                client?.Dispose();
            });

            await Task.CompletedTask;
        }

        /// <summary>
        /// Releases the test resources.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task ReleaseTestResources()
        {
            await Task.CompletedTask;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Performs the action with all event bus clients.
        /// </summary>
        /// <param name="action">The action.</param>
        protected void PerformWithAllEventBusClients(Action<IEventBusClient> action)
        {
            this.PerformWithAllObjects(action, this.eventBusClients);
        }

        /// <summary>
        /// Performs the action with all event bus clients.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected async Task PerformWithAllEventBusClientsAsync(Func<IEventBusClient, Task> action)
        {
            await this.PerformWithAllObjectsAsync(action, this.eventBusClients);
        }

        #endregion
    }
}
