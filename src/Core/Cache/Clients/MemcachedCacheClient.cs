using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enyim.Caching;
using Enyim.Caching.Memcached;
using Marketplace.Core.Cache.Interfaces;
using Newtonsoft.Json;

namespace Marketplace.Core.Cache.Clients
{
    /// <summary>
    /// Memcached cache client
    /// </summary>
    /// <seealso cref="Marketplace.Core.Cache.Interfaces.ICacheClient" />
    public class MemcachedCacheClient : ICacheClient
    {
        #region Fields

        /// <summary>
        /// The client
        /// </summary>
        private readonly IMemcachedClient client;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the application identifier.
        /// </summary>
        /// <value>
        /// The application identifier.
        /// </value>
        public string ApplicationId { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected => this.client != null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MemcachedCacheClient"/> class.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <param name="memcachedClient">The memcached client.</param>
        public MemcachedCacheClient(string applicationId, IMemcachedClient memcachedClient)
        {
            this.ApplicationId = applicationId;
            this.client = memcachedClient;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Connects this instance if not already connected.
        /// </summary>
        /// <returns>Indicates if connection is established.</returns>
        public bool Connect()
        {
            return this.IsConnected;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="asJson">Store object as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task<bool> SetValue(string key, object value, bool asJson = false, int? collectionId = null)
        {
            if (asJson)
            {
                value = JsonConvert.SerializeObject(value);
            }
            
            return await this.client.StoreAsync(StoreMode.Set, key, value, TimeSpan.MaxValue);
        }

        /// <summary>
        /// Completely removes any underlying types of values specified by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task<bool> RemoveUnderlyingValues(string key, int? collectionId = null)
        {
            return await this.client.RemoveAsync(key);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T">Return type (variety without JSON conversion depends on client implementation).</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="fromJson">If object is stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task<T> GetValue<T>(string key, bool fromJson = false, int? collectionId = null)
        {
            if (fromJson)
            {
                return await Task.Run(() =>
                {
                    object value = this.client.Get(key);
                    return JsonConvert.DeserializeObject<T>((string)value);
                });
            }
            else
            {
                return (await this.client.GetAsync<T>(key)).Value;
            }

        }

        /// <summary>
        /// Sets the list.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <param name="asJson">Store objects as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task<bool> SetList(string key, object[] values, bool asJson = false, int? collectionId = null)
        {
            return await this.SetValue(key, values, asJson);
        }

        /// <summary>
        /// Gets the list values.
        /// </summary>
        /// <typeparam name="T">Return type (variety without JSON conversion depends on client implementation).</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="fromJson">If objects are stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task<T[]> GetListValues<T>(string key, bool fromJson = false, int? collectionId = null)
        {
            return await this.GetValue<T[]>(key, fromJson);
        }

        /// <summary>
        /// Adds the list value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="asJson">Store object as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task AddListValue(string key, object value, bool asJson = false, int? collectionId = null)
        {
            var values = (await this.GetListValues<object>(key, asJson)).ToList();
            values.Add(value);
            await this.SetList(key, values.ToArray(), asJson);
        }

        /// <summary>
        /// Gets the list value.
        /// </summary>
        /// <typeparam name="T">Return type (variety without JSON conversion depends on client implementation).</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="index">The index.</param>
        /// <param name="fromJson">If object is stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task<T> GetListValue<T>(string key, int index, bool fromJson = false, int? collectionId = null)
        {
            var values = await this.GetListValues<T>(key, fromJson);
            return values[index];
        }

        /// <summary>
        /// Removes the list value occurences.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="asJson">If object is stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task RemoveListValue(string key, object value, bool asJson = false, int? collectionId = null)
        {
            var values = (await this.GetListValues<object>(key, asJson)).ToList();
            for (int x = 0; x < values.Count; x++)
            {
                if (values[x].Equals(value))
                {
                    values.Remove(value);
                }
            }

            await this.SetList(key, values.ToArray(), asJson);
        }

        /// <summary>
        /// Sets the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <param name="asJson">Store objects as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task SetDictionary(string key, Dictionary<string, object> values, bool asJson = false, int? collectionId = null)
        {
            await this.SetValue(key, values, asJson);
        }

        /// <summary>
        /// Gets the dictionary values.
        /// </summary>
        /// <typeparam name="T">Return type (variety without JSON conversion depends on client implementation).</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="fromJson">If objects are stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task<Dictionary<string, T>> GetDictionaryValues<T>(string key, bool fromJson = false, int? collectionId = null)
        {
            return await this.GetValue<Dictionary<string, T>>(key, fromJson);
        }

        /// <summary>
        /// Sets the dictionary value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="dictionaryKey">The dictionary key.</param>
        /// <param name="dictionaryValue">The dictionary value.</param>
        /// <param name="asJson">Store object as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task SetDictionaryValue(string key, string dictionaryKey, object dictionaryValue, bool asJson = false,
            int? collectionId = null)
        {
            Dictionary<string, object> values = await this.GetDictionaryValues<object>(key, asJson);
            values[dictionaryKey] = dictionaryValue;
            await this.SetDictionary(key, values, asJson);
        }

        /// <summary>
        /// Gets the dictionary value.
        /// </summary>
        /// <typeparam name="T">Return type (variety without JSON conversion depends on client implementation).</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="dictionaryKey">The dictionary key.</param>
        /// <param name="fromJson">If object is stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task<T> GetDictionaryValue<T>(string key, string dictionaryKey, bool fromJson = false, int? collectionId = null)
        {
            return (await this.GetDictionaryValues<T>(key, fromJson))[dictionaryKey];
        }

        /// <summary>
        /// Removes the dictionary key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="dictionaryKey">The dictionary key.</param>
        /// <param name="fromJson">If object is stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task RemoveDictionaryKey(string key, string dictionaryKey, bool fromJson, int? collectionId = null)
        {
            Dictionary<string, object> values = await this.GetDictionaryValues<object>(key, fromJson);
            values.Remove(dictionaryKey);
            await this.SetDictionary(key, values, fromJson);
        }

        /// <summary>
        /// Flushes all.
        /// </summary>
        /// <returns></returns>
        public async Task FlushAll()
        {
            await this.client.FlushAllAsync();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.client?.Dispose();
        }

        #endregion
    }
}
