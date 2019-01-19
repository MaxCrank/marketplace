using System;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Core.Cache.Exceptions;
using NUnit.Framework;

namespace Marketplace.Core.Tests.Cache
{
    /// <summary>
    /// Cache client list processing tests
    /// </summary>
    /// <seealso cref="Marketplace.Core.Tests.Cache.CacheClientTester" />
    public class ListProcessingTests : CacheClientTester
    {
        #region Fields

        /// <summary>
        /// The list length
        /// </summary>
        private static readonly int ListLength = 5;

        /// <summary>
        /// The dummy object list
        /// </summary>
        private readonly DummyCacheObject[] dummyObjectList = Enumerable.Range(1, ListLength).Select(i =>
        {
            var dummyObject = DummyCacheObject.GetTestObject();
            dummyObject.DummyInt = i;
            return dummyObject;
        }).ToArray();

        /// <summary>
        /// The string list
        /// </summary>
        private readonly string[] stringList = Enumerable.Range(1, ListLength).Select(i => i.ToString()).ToArray();

        /// <summary>
        /// The int list
        /// </summary>
        private readonly int[] intList = Enumerable.Range(1, ListLength).ToArray();

        /// <summary>
        /// The double list
        /// </summary>
        private readonly double[] doubleList = Enumerable.Range(1, ListLength).Select(i => i + 0.01).ToArray();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Key.
        /// </summary>
        /// <value>
        /// The Key.
        /// </value>
        protected override string Key => $"List{base.Key}";

        #endregion

        #region Tests

        /// <summary>
        /// Tests the dummy object list.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestDummyObjectList()
        {
            await this.TestEquiatbleList(this.dummyObjectList);
        }

        /// <summary>
        /// Tests the string list.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestStringList()
        {
            await this.TestEquiatbleList(this.stringList);
        }

        /// <summary>
        /// Tests the int list.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestIntList()
        {
            await this.TestEquiatbleList(this.intList);
        }

        /// <summary>
        /// Tests the double list.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestDoubleList()
        {
            await this.TestEquiatbleList(this.doubleList);
        }

        /// <summary>
        /// Tests the dummy object list elements.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestDummyObjectListElements()
        {
            await this.TestListEquitableElements(this.dummyObjectList);
        }

        /// <summary>
        /// Tests the double list elements.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestDoubleListElements()
        {
            await this.TestListEquitableElements(this.doubleList);
        }

        /// <summary>
        /// Tests the int list elements.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestIntListElements()
        {
            await this.TestListEquitableElements(this.intList);
        }

        /// <summary>
        /// Tests the string list elements.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestStringListElements()
        {
            await this.TestListEquitableElements(this.stringList);
        }

        /// <summary>
        /// Tests the non existing list.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestNonExistingList()
        {
            await this.PerformWithAllClientsAsync(async client =>
            {
                Assert.ThrowsAsync<CacheException>(async () => await client.GetListValuesAsync<DummyCacheObject>(
                    this.Key));
                Assert.ThrowsAsync<CacheException>(async () => await client.GetListValueAsync<DummyCacheObject>(
                    this.Key, 0));

                Assert.IsTrue(await client.SetListAsync(this.Key, this.dummyObjectList));
                var resultObjects = await client.GetListValuesAsync<DummyCacheObject>(this.Key);
                Assert.NotNull(resultObjects);
                Assert.AreEqual(ListLength, resultObjects.Count);
                for (int x = 0; x < ListLength; x++)
                {
                    Assert.IsTrue(this.dummyObjectList[x].Equals(resultObjects[x]));
                }

                Assert.ThrowsAsync<CacheException>(async () => await client.GetListValuesAsync<DummyCacheObject>(
                    this.NonExistingKey));
                Assert.ThrowsAsync<CacheException>(async () => await client.GetListValueAsync<DummyCacheObject>(
                    this.NonExistingKey, 0));
            });
        }

        /// <summary>
        /// Tests the list removal.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestListRemoval()
        {
            await this.PerformWithAllClientsAsync(async client =>
            {
                Assert.IsFalse(await client.RemoveUnderlyingValuesAsync(this.Key));

                Assert.IsTrue(await client.SetListAsync(this.Key, this.dummyObjectList));
                Assert.IsTrue(await client.RemoveUnderlyingValuesAsync(this.Key));
                Assert.IsFalse(await client.RemoveUnderlyingValuesAsync(this.Key));

                Assert.ThrowsAsync<CacheException>(async () => await client.GetListValuesAsync<DummyCacheObject>(this.Key));
                Assert.ThrowsAsync<CacheException>(async () => await client.GetListValueAsync<DummyCacheObject>(this.Key, 0));
            });
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Tests the equiatble list.
        /// </summary>
        /// <typeparam name="T">The list element type.</typeparam>
        /// <param name="list">The list.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task TestEquiatbleList<T>(T[] list) where T: IEquatable<T>
        {
            await this.PerformWithAllClientsAsync(async client =>
            {
                object[] passObject = list.Cast<object>().ToArray();
                Assert.IsTrue(await client.SetListAsync(this.Key, passObject));
                var resultObjects = await client.GetListValuesAsync<T>(this.Key);
                Assert.NotNull(resultObjects);
                Assert.AreEqual(ListLength, resultObjects.Count);
                for (int x = 0; x < ListLength; x++)
                {
                    this.AssertEquitableNotNullAndEquals(list[x], resultObjects[x]);
                }
            });
        }

        /// <summary>
        /// Tests the list equitable elements.
        /// </summary>
        /// <typeparam name="T">The list element type.</typeparam>
        /// <param name="list">The list.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task TestListEquitableElements<T>(T[] list) where T: IEquatable<T>
        {
            await this.PerformWithAllClientsAsync(async client =>
            {
                object[] passObject = list.Cast<object>().ToArray();
                Assert.IsTrue(await client.SetListAsync(this.Key, passObject));
                for (int x = 0; x < ListLength; x++)
                {
                    var result = await client.GetListValueAsync<T>(this.Key, x);
                    this.AssertEquitableNotNullAndEquals(list[x], result);
                }

                Assert.ThrowsAsync<CacheException>(async () => await client.GetListValueAsync<T>(
                    this.Key, ListLength + 1));
            });
        }

        #endregion
    }
}
