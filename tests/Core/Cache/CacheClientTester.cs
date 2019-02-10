// File: CacheClientTester.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Core.Cache.Clients;
using Marketplace.Core.Cache.Interfaces;
using Marketplace.Core.Serialization.Serializers;
using Marketplace.Core.Tests.Base;
using NUnit.Framework;

namespace Marketplace.Core.Tests.Cache
{
    /// <summary>
    /// Base test class for any cache client implementation.
    /// </summary>
    [Category("Cache")]
    public abstract class CacheClientTester : BasicTester
    {
        #region Fields

        /// <summary>
        /// The cache clients
        /// </summary>
        private readonly List<ICacheClient> cacheClients = new List<ICacheClient>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        protected virtual string Key => "Key";

        /// <summary>
        /// Gets the non-existing key.
        /// </summary>
        /// <value>
        /// The non-existing key.
        /// </value>
        protected string NonExistingKey => "NonExisting" + this.Key;

        #endregion

        #region Configuration

        /// <summary>
        /// Initializes the fixture resources.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task InitFixtureResources()
        {
            var serializer = new JsonSerializer();
            this.cacheClients.Add(new RedisCacheClient(AppId, "test-core-cache.redis.com", serializer, Password, 0, true));
            this.cacheClients.Add(new MemcachedCacheClient(AppId, "test-core-cache.memcached.com", serializer, Password));

            this.cacheClients.ForEach(c =>
            {
                Assert.IsTrue(c.Connect());
            });

            await this.PerformWithAllCacheClientsAsync(
                async client => await client.RemoveUnderlyingValuesAsync(this.Key));
        }

        /// <summary>
        /// Releases the fixture resources.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task ReleaseFixtureResources()
        {
            bool flush = this.CurrentRunningFixturesCount == 0;
            await this.PerformWithAllCacheClientsAsync(async client =>
            {
                if (flush && client != null)
                {
                    await client.FlushAsync();
                }

                client?.Dispose();
            });
        }

        /// <summary>
        /// Releases the test resources.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task ReleaseTestResources()
        {
            await this.PerformWithAllCacheClientsAsync(async client =>
            {
                await client.RemoveUnderlyingValuesAsync(this.Key);
            });
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Performs the action with all cache clients.
        /// </summary>
        /// <param name="action">The action.</param>
        protected void PerformWithAllCacheClients(Action<ICacheClient> action)
        {
            this.PerformWithAllObjects(action, this.cacheClients);
        }

        /// <summary>
        /// Performs the action with all cache clients.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected async Task PerformWithAllCacheClientsAsync(Func<ICacheClient, Task> action)
        {
            await this.PerformWithAllObjectsAsync(action, this.cacheClients);
        }

        /// <summary>
        /// Asserts that both objects are not null and equals.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <param name="isEnumerable">If objects implement IEnumerable.</param>
        protected void AssertNotNullAndEquals<T>(T obj1, T obj2, bool isEnumerable = false)
        {
            Assert.NotNull(obj1);
            Assert.NotNull(obj2);
            if (isEnumerable)
            {
                var firstCollection = (obj1 as IEnumerable).Cast<object>().ToArray();
                var secondCollection = (obj2 as IEnumerable).Cast<object>().ToArray();
                int collectionLength = firstCollection.Length;
                Assert.AreEqual(collectionLength, secondCollection.Length);
                for (int x = 0; x < collectionLength; x++)
                {
                    Assert.IsTrue(firstCollection[x].Equals(secondCollection[x]));
                }
            }
            else
            {
                Assert.IsTrue(obj1.Equals(obj2));
            }
        }

        /// <summary>
        /// Asserts that both equitable objects are not null and equals.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        protected void AssertEquitableNotNullAndEquals<T>(T obj1, T obj2) where T: IEquatable<T>
        {
            Assert.NotNull(obj1);
            Assert.NotNull(obj2);
            Assert.IsTrue(obj1.Equals(obj2));
        }

        #endregion
    }
}
