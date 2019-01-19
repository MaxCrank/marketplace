using System;
using System.Threading.Tasks;
using Marketplace.Core.Cache.Exceptions;
using NUnit.Framework;

namespace Marketplace.Core.Tests.Cache
{
    /// <summary>
    /// Cache client value processing tests.
    /// </summary>
    /// <seealso cref="Marketplace.Core.Tests.Cache.CacheClientTester" />
    public class ValueProcessingTests : CacheClientTester
    {
        #region Fields

        /// <summary>
        /// The dummy object
        /// </summary>
        private readonly DummyCacheObject dummyObject = DummyCacheObject.GetTestObject();

        /// <summary>
        /// The string value
        /// </summary>
        private readonly string stringValue = "dummyString";

        /// <summary>
        /// The double value
        /// </summary>
        private readonly double doubleValue = 1.01;

        /// <summary>
        /// The int value
        /// </summary>
        private readonly int intValue = 1;

        /// <summary>
        /// The bytes
        /// </summary>
        private readonly byte[] bytes = new byte[] {0, 1, 2, 3, 4};

        /// <summary>
        /// The ints
        /// </summary>
        private readonly int[] ints = new int[] { 0, 1, 2, 3, 4 };

        /// <summary>
        /// The strings
        /// </summary>
        private readonly string[] strings = new string[] { "0", "1", "2", "3", "4" };

        #endregion

        #region Properties

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        protected override string Key => $"Value{base.Key}";

        #endregion

        #region Tests

        /// <summary>
        /// Tests the dummy object.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestDummyObject()
        {
            await this.TestEquitableValue(this.dummyObject);
        }

        /// <summary>
        /// Tests the string.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestString()
        {
            await this.TestEquitableValue(this.stringValue);
        }

        /// <summary>
        /// Tests the double.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestDouble()
        {
            await this.TestEquitableValue(this.doubleValue);
        }

        /// <summary>
        /// Tests the int.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestInt()
        {
            await this.TestEquitableValue(this.intValue);
        }

        /// <summary>
        /// Tests the bytes.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestBytes()
        {
            await this.TestValue(this.bytes, true);
        }

        /// <summary>
        /// Tests the ints.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestInts()
        {
            await this.TestValue(this.ints, true);
        }

        /// <summary>
        /// Tests the strings.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestStrings()
        {
            await this.TestValue(this.strings, true);
        }

        /// <summary>
        /// Tests the non existing key.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestNonExistingKey()
        {
            await this.PerformWithAllClientsAsync(async client =>
            {
                Assert.ThrowsAsync<CacheException>(async () => await client.GetValueAsync<DummyCacheObject>(this.Key));

                Assert.IsTrue(await client.SetValueAsync(this.Key, this.dummyObject));
                var resultObject = await client.GetValueAsync<DummyCacheObject>(this.Key);
                Assert.NotNull(resultObject);
                Assert.IsTrue(resultObject.Equals(this.dummyObject));

                Assert.ThrowsAsync<CacheException>(async () => await client.GetValueAsync<DummyCacheObject>(this.NonExistingKey));
            });
        }

        /// <summary>
        /// Tests the key removal.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Test]
        public async Task TestKeyRemoval()
        {
            await this.PerformWithAllClientsAsync(async client =>
            {
                Assert.IsFalse(await client.RemoveUnderlyingValuesAsync(this.Key));

                Assert.IsTrue(await client.SetValueAsync(this.Key, this.dummyObject));
                Assert.IsTrue(await client.RemoveUnderlyingValuesAsync(this.Key));
                Assert.IsFalse(await client.RemoveUnderlyingValuesAsync(this.Key));

                Assert.ThrowsAsync<CacheException>(async () => await client.GetValueAsync<DummyCacheObject>(this.Key));
            });
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Tests the value.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="isEnumerable">if set to <c>true</c> [is enumerable].</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task TestValue<T>(T value, bool isEnumerable = false)
        {
            await this.PerformWithAllClientsAsync(async client =>
            {
                Assert.IsTrue(await client.SetValueAsync(this.Key, value));
                T resultObject = await client.GetValueAsync<T>(this.Key);
                this.AssertNotNullAndEquals(value, resultObject, isEnumerable);
            });
        }

        /// <summary>
        /// Tests the equitable value.
        /// </summary>
        /// <typeparam name="T">Type that implements IEquatable</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task TestEquitableValue<T>(T value) where T: IEquatable<T>
        {
            await this.PerformWithAllClientsAsync(async client =>
            {
                Assert.IsTrue(await client.SetValueAsync(this.Key, value));
                T resultObject = await client.GetValueAsync<T>(this.Key);
                this.AssertEquitableNotNullAndEquals(value, resultObject);
            });
        }

        #endregion
    }
}
