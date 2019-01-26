// File: MemcachedCacheClient.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Enyim.Caching.Memcached.Results;
using Marketplace.Core.Cache.Exceptions;
using Marketplace.Core.Cache.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        /// Initializes a new instance of the <see cref="MemcachedCacheClient"/> class with overriding default DI container.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <param name="host">The host.</param>
        /// <param name="password">The password.</param>
        /// <param name="port">The port.</param>
        public MemcachedCacheClient(string applicationId, string host, string password = null, int port = 11211)
        {
            this.ApplicationId = applicationId;

            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(new LoggerFactory());
            services.AddEnyimMemcached(options =>
            {
                options.AddServer(host, port);
                options.Protocol = MemcachedProtocol.Binary;
                if (!string.IsNullOrEmpty(password))
                {
                    options.Authentication = new Authentication()
                    {
                        Type = "Enyim.Caching.Memcached.PlainTextAuthenticator",
                        Parameters = new Dictionary<string, string>()
                        {
                            { "password", password },
                            { "userName", "memcache" },
                            { "zone", "" }
                        }
                    };
                }
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            this.client = serviceProvider.GetService<IMemcachedClient>();
            this.client.NodeFailed += node =>
            {
                Console.WriteLine(node.ToString());
                Console.WriteLine(node.IsAlive);
                Console.WriteLine(node.EndPoint);
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemcachedCacheClient"/> class with default DI container.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <param name="memcachedClient">The memcached client (for default Dependency Injection).</param>
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
        /// <returns>If connection has been established.</returns>
        public bool Connect()
        {
            return this.IsConnected;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if cache value has been set.</returns>
        public async Task<bool> SetValueAsync(string key, object value)
        {
            if (!(value is string))
            {
                value = JsonConvert.SerializeObject(value);
            }
            
            return await this.client.StoreAsync(StoreMode.Set, key, value, TimeSpan.MaxValue);
        }

        /// <summary>
        /// Completely removes any underlying types of values specified by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if underlying cache values has been removed by key.</returns>
        public async Task<bool> RemoveUnderlyingValuesAsync(string key)
        {
            return await this.client.RemoveAsync(key);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Returns the cache value.</returns>
        public async Task<T> GetValueAsync<T>(string key)
        {
            return await this.GetValueInternalAsync<T>(key);
        }

        /// <summary>
        /// Sets the list.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if cache list has been set.</returns>
        public async Task<bool> SetListAsync(string key, IEnumerable<object> values)
        {
            return await this.SetValueAsync(key, values);
        }

        /// <summary>
        /// Gets the list values.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>A task that represents the asynchronous operation. 
        /// Returns cache list values.</returns>
        public async Task<IList<T>> GetListValuesAsync<T>(string key)
        {
            return await this.GetValueAsync<List<T>>(key);
        }

        /// <summary>
        /// Adds the list value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if cache list value has been added.</returns>
        public async Task<bool> AddListValueAsync(string key, object value)
        {
            var values = await this.GetListValuesInternalAsync<object>(key);
            List<object> listValues = values?.ToList() ?? new List<object>();

            listValues.Add(value);
            return await this.SetListAsync(key, listValues.ToArray());
        }

        /// <summary>
        /// Gets the list value.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="index">The index.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Returns cache list value.</returns>
        public async Task<T> GetListValueAsync<T>(string key, int index)
        {
            var values = await this.GetListValuesAsync<T>(key);
            if (values == null || values.Count == 0 || index >= values.Count)
            {
                throw new CacheException($"Couldn't get list value for key {key} at index {index}", 
                    nameof(MemcachedCacheClient));
            }

            return values[index];
        }

        /// <summary>
        /// Removes the list value occurences.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if cache list value has been removed.</returns>
        public async Task<bool> RemoveListValueAsync(string key, object value)
        {
            var values = await this.GetListValuesInternalAsync<object>(key);
            if (values == null || values.Length == 0)
            {
                return false;
            }

            bool removed = false;
            var listValues = values.ToList();
            for (int x = 0; x < listValues.Count; x++)
            {
                if (listValues[x].Equals(value) && listValues.Remove(value))
                {
                    removed = true;
                }
            }

            if (removed)
            {
                await this.SetListAsync(key, values.ToArray());
            }
            
            return removed;
        }

        /// <summary>
        /// Sets the dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if cache dictionary has been added.</returns>
        public async Task<bool> SetDictionaryAsync<T>(string key, Dictionary<string, T> values)
        {
            return await this.SetValueAsync(key, values);
        }

        /// <summary>
        /// Gets the dictionary values.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Returns cache dictionary values.</returns>
        public async Task<Dictionary<string, T>> GetDictionaryAsync<T>(string key)
        {
            return await this.GetValueAsync<Dictionary<string, T>>(key);
        }

        /// <summary>
        /// Sets the dictionary key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="dictionaryKey">The dictionary key.</param>
        /// <param name="dictionaryValue">The dictionary value.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if cache dictionary key has been set.</returns>
        public async Task<bool> SetDictionaryKeyAsync(string key, string dictionaryKey, object dictionaryValue)
        {
            Dictionary<string, object> values = await this.GetDictionaryInternalAsync<object>(key) ?? 
                new Dictionary<string, object>();

            values[dictionaryKey] = dictionaryValue;
            return await this.SetDictionaryAsync(key, values);
        }

        /// <summary>
        /// Gets the dictionary value.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="dictionaryKey">The dictionary key.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Returns cache dictionary value.</returns>
        public async Task<T> GetDictionaryValueAsync<T>(string key, string dictionaryKey)
        {
            var dictionary = await this.GetDictionaryAsync<T>(key);
            if (dictionary == null || !dictionary.ContainsKey(dictionaryKey))
            {
                throw new CacheException($"Can't get dictionary value from cache key {key} and dictionary key {dictionaryKey}", 
                    nameof(MemcachedCacheClient));
            }

            return dictionary[dictionaryKey];
        }

        /// <summary>
        /// Removes the dictionary key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="dictionaryKey">The dictionary key.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if cache dictionary entry has been removed.</returns>
        public async Task<bool> RemoveDictionaryKeyAsync(string key, string dictionaryKey)
        {
            Dictionary<string, object> values = await this.GetDictionaryInternalAsync<object>(key);
            if (values != null && values.Remove(dictionaryKey))
            {
                await this.SetDictionaryAsync(key, values);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Flushes the data.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task FlushAsync()
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

        #region Private methods

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="throwNonExistingException">If exception is to be thrown when value doesn't exist.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Returns the cache value.</returns>
        private async Task<T> GetValueInternalAsync<T>(string key, bool throwNonExistingException = true)
        {
            CacheClientResponse response = await this.GetCacheClientResponse<T>(key);
            
            if (response.Value != null)
            {
                return (T)response.Value;
            }
            else if (throwNonExistingException)
            {
                throw response.Exception;
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Gets the cache client response.
        /// </summary>
        /// <typeparam name="T">Return value type.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Returns the cache client response.</returns>
        private async Task<CacheClientResponse> GetCacheClientResponse<T>(string key)
        {
            CacheException exception = null;
            object result = null;

            GetOperationResult<string> valueResult = await this.client.GetAsync<string>(key) as GetOperationResult<string>;
            if (valueResult.Success)
            {
                result = typeof(T) == typeof(string) ? valueResult.Value : 
                    (object)JsonConvert.DeserializeObject<T>(valueResult.Value);
            }
            else
            {
                exception = new CacheException(valueResult.Message, valueResult.Exception);
            }

            return new CacheClientResponse(result, exception);
        }

        /// <summary>
        /// Gets the dictionary values.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Returns the cache dictionary entries.</returns>
        private async Task<Dictionary<string, T>> GetDictionaryInternalAsync<T>(string key)
        {
            return await this.GetValueInternalAsync<Dictionary<string, T>>(key, false);
        }

        /// <summary>
        /// Gets the list values.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Returns the cache list values.</returns>
        private async Task<T[]> GetListValuesInternalAsync<T>(string key)
        {
            return await this.GetValueInternalAsync<T[]>(key, false);
        }

        #endregion
    }
}
