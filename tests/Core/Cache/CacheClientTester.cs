using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Core.Cache.Clients;
using Marketplace.Core.Cache.Interfaces;
using NUnit.Framework;

namespace Marketplace.Core.Tests.Cache
{
    /// <summary>
    /// Base test class for any cache client implementation
    /// </summary>
    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    [ExcludeFromCodeCoverage]
    [Category("Cache")]
    public abstract class CacheClientTester
    {
        #region Constants

        /// <summary>
        /// The application identifier
        /// </summary>
        protected const string AppId = "TestApp";

        /// <summary>
        /// The password
        /// </summary>
        private const string Password = "testCorePassword";

        #endregion

        #region Fields

        /// <summary>
        /// The stopwatch
        /// </summary>
        readonly Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// The running fixtures count
        /// </summary>
        private static volatile int runningFixturesCount = 0;

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
        /// Initializes the tests.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [OneTimeSetUp]
        public async Task InitTests()
        {
            runningFixturesCount++;
            this.cacheClients.Add(new RedisCacheClient(AppId, "test-core.redis.com", Password, 0, true));
            this.cacheClients.Add(new MemcachedCacheClient(AppId, "test-core.memcached.com", Password));

            this.cacheClients.ForEach(c =>
            {
                Assert.IsTrue(c.Connect());
            });

            foreach (var client in this.cacheClients)
            {
                await client.RemoveUnderlyingValuesAsync(this.Key);
            }
        }

        /// <summary>
        /// Finalizes the tests.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [OneTimeTearDown]
        public async Task FinalizeTests()
        {
            bool flush = --runningFixturesCount == 0;
            await this.PerformWithAllClientsAsync(async client =>
            {
                if (flush && client != null)
                {
                    await client.FlushAsync();
                }
                
                client?.Dispose();
            });
        }

        /// <summary>
        /// Initializes the test.
        /// </summary>
        [SetUp]
        public void InitTest()
        {
            stopwatch.Restart();
        }

        /// <summary>
        /// Finalizes the test.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [TearDown]
        public async Task FinalizeTest()
        {
            await this.PerformWithAllClientsAsync(async client =>
            {
                await client.RemoveUnderlyingValuesAsync(this.Key);
            });

            stopwatch.Stop();
            var test = TestContext.CurrentContext.Test;
            string argString = string.Empty;
            foreach (var arg in test.Arguments)
            {
                argString += arg + ", ";
            }

            if (!string.IsNullOrEmpty(argString))
            {
                argString = argString.Remove(argString.Length - 1, 1);
            }

            string newLine =
                $"{test.MethodName}({argString}) - {stopwatch.ElapsedMilliseconds} ms{Environment.NewLine}";
            Console.WriteLine(newLine);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Performs the action with all clients.
        /// </summary>
        /// <param name="action">The action.</param>
        protected void PerformWithAllClients(Action<ICacheClient> action)
        {
            foreach (var client in this.cacheClients)
            {
                action.Invoke(client);
            }
        }

        /// <summary>
        /// Performs the action with all clients.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected async Task PerformWithAllClientsAsync(Func<ICacheClient, Task> action)
        {
            await Task.WhenAll(this.cacheClients.Select(action.Invoke));
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
