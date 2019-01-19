using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Core.Cache.Exceptions;
using NUnit.Framework;

namespace Marketplace.Core.Tests.Cache
{
    /// <summary>
    /// Cache client dictionary processing tests
    /// </summary>
    /// <seealso cref="Marketplace.Core.Tests.Cache.CacheClientTester" />
    public class DictionaryProcessingTests : CacheClientTester
    {
        #region Fields

        /// <summary>
        /// The dictionary length
        /// </summary>
        private static readonly int dictionaryLength = 5;

        /// <summary>
        /// The dummy object dictionary
        /// </summary>
        private readonly Dictionary<string, DummyCacheObject> dummyObjectDictionary = new Dictionary<string, DummyCacheObject>(
            Enumerable.Range(1, dictionaryLength).Select(i =>
            {
                var dummyObject = DummyCacheObject.GetTestObject();
                dummyObject.DummyInt = i;
                return new KeyValuePair<string, DummyCacheObject>(i.ToString(), dummyObject);
            }));

        /// <summary>
        /// The string dictionary
        /// </summary>
        private readonly Dictionary<string, string> stringDictionary = new Dictionary<string, string>(
            Enumerable.Range(1, dictionaryLength).Select(i => new KeyValuePair<string, string>(i.ToString(), i.ToString())));

        /// <summary>
        /// The int dictionary
        /// </summary>
        private readonly Dictionary<string, int> intDictionary = new Dictionary<string, int>(
            Enumerable.Range(1, dictionaryLength).Select(i => new KeyValuePair<string, int>(i.ToString(), i)));

        /// <summary>
        /// The double dictionary
        /// </summary>
        private readonly Dictionary<string, double> doubleDictionary = new Dictionary<string, double>(
            Enumerable.Range(1, dictionaryLength).Select(i => new KeyValuePair<string, double>(i.ToString(), i + 0.01)));

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Key.
        /// </summary>
        /// <value>
        /// The Key.
        /// </value>
        protected override string Key => $"Dictionary{base.Key}";

        #endregion

        #region Tests

        /// <summary>
        /// Tests the dummy object dictionary.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestDummyObjectDictionary()
        {
            await this.TestEquitableDictionary(this.dummyObjectDictionary);
        }

        /// <summary>
        /// Tests the string dictionary.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestStringDictionary()
        {
            await this.TestEquitableDictionary(this.stringDictionary);
        }

        /// <summary>
        /// Tests the int dictionary.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestIntDictionary()
        {
            await this.TestEquitableDictionary(this.intDictionary);
        }

        /// <summary>
        /// Tests the double dictionary.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestDoubleDictionary()
        {
            await this.TestEquitableDictionary(this.doubleDictionary);
        }

        /// <summary>
        /// Tests the dummy object dictionary elements.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestDummyObjectDictionaryElements()
        {
            await this.TestDictionaryEquitableElements(this.dummyObjectDictionary);
        }

        /// <summary>
        /// Tests the string dictionary elements.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestStringDictionaryElements()
        {
            await this.TestDictionaryEquitableElements(this.stringDictionary);
        }

        /// <summary>
        /// Tests the int dictionary elements.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestIntDictionaryElements()
        {
            await this.TestDictionaryEquitableElements(this.intDictionary);
        }

        /// <summary>
        /// Tests the double dictionary elements.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestDoubleDictionaryElements()
        {
            await this.TestDictionaryEquitableElements(this.doubleDictionary);
        }

        /// <summary>
        /// Tests the non existing dictionary.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestNonExistingDictionary()
        {
            await this.PerformWithAllClientsAsync(async client =>
            {
                Assert.ThrowsAsync<CacheException>(async () => await client.GetDictionaryAsync<DummyCacheObject>(
                    this.Key));
                Assert.ThrowsAsync<CacheException>(async () => await client.GetDictionaryValueAsync<DummyCacheObject>(
                    this.Key, 0.ToString()));

                await client.SetDictionaryAsync(this.Key, this.dummyObjectDictionary);
                var resultObjects = await client.GetDictionaryAsync<DummyCacheObject>(this.Key);
                Assert.NotNull(resultObjects);
                Assert.AreEqual(dictionaryLength, resultObjects.Count);
                for (int x = 1; x < dictionaryLength; x++)
                {
                    Assert.IsTrue(this.dummyObjectDictionary[x.ToString()].Equals(resultObjects[x.ToString()]));
                }

                Assert.ThrowsAsync<CacheException>(async () => await client.GetDictionaryAsync<DummyCacheObject>(
                    this.NonExistingKey));
                Assert.ThrowsAsync<CacheException>(async () => await client.GetDictionaryValueAsync<DummyCacheObject>(
                    this.NonExistingKey, 1.ToString()));
            });
        }

        /// <summary>
        /// Tests the dictionary removal.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestDictionaryRemoval()
        {
            await this.PerformWithAllClientsAsync(async client =>
            {
                Assert.IsFalse(await client.RemoveUnderlyingValuesAsync(this.Key));

                await client.SetDictionaryAsync(this.Key, this.dummyObjectDictionary);
                Assert.IsTrue(await client.RemoveUnderlyingValuesAsync(this.Key));
                Assert.IsFalse(await client.RemoveUnderlyingValuesAsync(this.Key));

                Assert.ThrowsAsync<CacheException>(async () => await client.GetDictionaryAsync<DummyCacheObject>(this.Key));
                Assert.ThrowsAsync<CacheException>(async () => await client.GetDictionaryValueAsync<DummyCacheObject>(this.Key, 1.ToString()));
            });
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Tests the equitable dictionary.
        /// </summary>
        /// <typeparam name="T">The dictionary value type.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task TestEquitableDictionary<T>(Dictionary<string, T> dictionary) where T: IEquatable<T>
        {
            await this.PerformWithAllClientsAsync(async client =>
            {
                await client.SetDictionaryAsync(this.Key, dictionary);
                var resultObjects = await client.GetDictionaryAsync<T>(this.Key);
                Assert.NotNull(resultObjects);
                Assert.AreEqual(dictionaryLength, resultObjects.Count);
                for (int x = 1; x < dictionaryLength; x++)
                {
                    this.AssertEquitableNotNullAndEquals(dictionary[x.ToString()], resultObjects[x.ToString()]);
                }
            });
        }

        /// <summary>
        /// Tests the dictionary equitable elements.
        /// </summary>
        /// <typeparam name="T">The dictionary value type.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task TestDictionaryEquitableElements<T>(Dictionary<string, T> dictionary) where T : IEquatable<T>
        {
            await this.PerformWithAllClientsAsync(async client =>
            {
                await client.SetDictionaryAsync(this.Key, dictionary);
                for (int x = 1; x < dictionaryLength; x++)
                {
                    var result = await client.GetDictionaryValueAsync<T>(this.Key, x.ToString());
                    this.AssertEquitableNotNullAndEquals(dictionary[x.ToString()], result);
                }

                Assert.ThrowsAsync<CacheException>(async () => await client.GetDictionaryValueAsync<T>(
                    this.Key, (dictionaryLength + 1).ToString()));
            });
        }

        #endregion
    }
}
