using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marketplace.Core.Cache.Exceptions;
using Marketplace.Core.Cache.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Marketplace.Core.Cache.Clients
{
    /// <summary>
    /// Redis cache client
    /// </summary>
    /// <seealso cref="Marketplace.Core.Cache.Interfaces.ICacheClient" />
    public class RedisCacheClient : ICacheClient
    {
        #region Fields

        /// <summary>
        /// The connection
        /// </summary>
        private ConnectionMultiplexer connection;

        /// <summary>
        /// The host and port
        /// </summary>
        private readonly string hostAndPort;

        /// <summary>
        /// The default database
        /// </summary>
        private readonly int defaultDatabase;

        /// <summary>
        /// The password
        /// </summary>
        private readonly string password;

        /// <summary>
        /// The admin mode
        /// </summary>
        private readonly bool adminMode;

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
        public bool IsConnected => this.connection != null && this.connection.IsConnected;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheClient"/> class.
        /// </summary>
        /// <param name="applicationId">The application identifier.</param>
        /// <param name="host">The host (please, do not specify port in this parameter - use <paramref name="port"/> parameter instead).</param>
        /// <param name="password">The password.</param>
        /// <param name="defaultDb">The default database.</param>
        /// <param name="adminMode">Allows admin mode operations.</param>
        /// <param name="port">The port.</param>
        public RedisCacheClient(string applicationId, string host, string password = null, int defaultDb = 0,
            bool adminMode = false, int port = 6379)
        {
            this.ApplicationId = applicationId;
            this.hostAndPort = $"{host.TrimEnd('/')}:{port}";
            this.defaultDatabase = defaultDb;
            this.password = password;
            this.adminMode = adminMode;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Connects this instance if not already connected.
        /// </summary>
        /// <returns>If connection has been established.</returns>
        public bool Connect()
        {
            if (this.connection == null)
            {
                var options = ConfigurationOptions.Parse(this.hostAndPort);
                options.ClientName = this.ApplicationId;
                options.DefaultDatabase = this.defaultDatabase;
                options.ResolveDns = false;
                options.AllowAdmin = this.adminMode;
                if (!string.IsNullOrEmpty(this.password))
                {
                    options.Password = this.password;
                }

                this.connection = ConnectionMultiplexer.Connect(options);
            }

            return this.connection.IsConnected;
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
            return await this.GetDefaultDatabase().StringSetAsync(key, this.Serialize(value));
        }

        /// <summary>
        /// Completely removes any underlying types of values specified by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if underlying cache values has been removed by key.</returns>
        public async Task<bool> RemoveUnderlyingValuesAsync(string key)
        {
            return await this.GetDefaultDatabase().KeyDeleteAsync(key);
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
            if (!this.GetDefaultDatabase().KeyExists(key))
            {
                throw new CacheException($"Can't get value from key {key} - it doesn't exist", 
                    nameof(RedisCacheClient));
            }

            var redisValue = await this.GetDefaultDatabase().StringGetAsync(key);
            return this.Deserialize<T>(redisValue);
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
            var db = this.GetDefaultDatabase();
            var transaction = db.CreateTransaction();
            foreach (var value in values)
            {
                // async methods are NOT to be awaited for transactions; otherwise, the first call will just block the thread
                // see https://github.com/StackExchange/StackExchange.Redis/blob/master/docs/Transactions.md#and-in-stackexchangeredis
#pragma warning disable 4014
                transaction.ListRightPushAsync(key, this.Serialize(value));
#pragma warning restore 4014
            }

            return await transaction.ExecuteAsync();
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
            if (!this.GetDefaultDatabase().KeyExists(key))
            {
                throw new CacheException($"Can't get list values from key {key} - it doesn't exist", 
                    nameof(RedisCacheClient));
            }

            var list = await this.GetDefaultDatabase().ListRangeAsync(key);
            return list.Select(Deserialize<T>).ToList();
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
            long initLength = this.GetDefaultDatabase().ListLength(key);
            return await this.GetDefaultDatabase().ListRightPushAsync(key, this.Serialize(value)) > initLength;
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
            if (!this.GetDefaultDatabase().KeyExists(key))
            {
                throw new CacheException($"Can't get list value from key {key} at index {index} - key doesn't exist",
                    nameof(RedisCacheClient));
            }

            var redisValue = await this.GetDefaultDatabase().ListGetByIndexAsync(key, index);
            if (string.IsNullOrEmpty(redisValue))
            {
                throw new CacheException($"Can't get list value from key {key} at index {index}",
                    nameof(RedisCacheClient));
            }

            return this.Deserialize<T>(redisValue);
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
            return await this.GetDefaultDatabase().ListRemoveAsync(key, this.Serialize(value)) > 0;
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
            await this.GetDefaultDatabase()
                .HashSetAsync(key, values.Select(kvp => new HashEntry(kvp.Key, this.Serialize(kvp.Value))).ToArray());
            return await this.GetDefaultDatabase().KeyExistsAsync(key);
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
            if (!this.GetDefaultDatabase().KeyExists(key))
            {
                throw new CacheException($"Can't get dictionary from key {key} - it doesn't exist",
                    nameof(RedisCacheClient));
            }

            var set = await this.GetDefaultDatabase().HashGetAllAsync(key);
            var kvps = set.Select(s => new KeyValuePair<string, T>(s.Name, this.Deserialize<T>(s.Value)));
            return new Dictionary<string, T>(kvps);
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
            await this.GetDefaultDatabase().HashSetAsync(
                key, new[] { new HashEntry(dictionaryKey, this.Serialize(dictionaryValue)) });
            return await this.GetDefaultDatabase().HashExistsAsync(key, dictionaryKey);
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
            if (!this.GetDefaultDatabase().KeyExists(key))
            {
                throw new CacheException($"Can't get dictionary value from cache key {key} " +
                                         $"and dictionary key {dictionaryKey} - key doesn't exist",
                                         nameof(RedisCacheClient));
            }

            var redisValue = await this.GetDefaultDatabase().HashGetAsync(key, dictionaryKey);
            if (string.IsNullOrEmpty(redisValue))
            {
                throw new CacheException($"Can't get dictionary value from cache key {key} and dictionary key {dictionaryKey}",
                    nameof(RedisCacheClient));
            }

            return this.Deserialize<T>(redisValue);
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
            return await this.GetDefaultDatabase().HashDeleteAsync(key, dictionaryKey);
        }

        /// <summary>
        /// Flushes the data.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task FlushAsync()
        {
            //throw new Exception(this.connection.GetEndPoints().Select(e => e.ToString()).Aggregate((s1, s2) => s1 + " ; " + s2));
            var server = this.connection.GetServer(this.hostAndPort);
            await server.FlushDatabaseAsync(this.defaultDatabase);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.connection?.Dispose();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="serializableObject">The object to serialize.</param>
        /// <returns>Redis value.</returns>
        private RedisValue Serialize(object serializableObject)
        {
            if (serializableObject is string stringObject)
            {
                serializableObject = Encoding.Unicode.GetBytes(stringObject);
            }

            if (serializableObject is byte[] byteArray)
            {
                return byteArray;
            }

            return Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(serializableObject));
        }

        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <typeparam name="T">Type to deserialize to.</typeparam>
        /// <param name="data">The data.</param>
        /// <returns>Deserialized object.</returns>
        private T Deserialize<T>(RedisValue data)
        {
            if (typeof(T) == typeof(byte[]))
            {
                return (T)Convert.ChangeType(data, typeof(T));
            }

            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(Encoding.Unicode.GetString(data), typeof(T));
            }

            return JsonConvert.DeserializeObject<T>(Encoding.Unicode.GetString(data));
        }

        /// <summary>
        /// Gets the default database.
        /// </summary>
        /// <returns>The default database.</returns>
        private IDatabase GetDefaultDatabase()
        {
            return this.connection.GetDatabase(this.defaultDatabase);
        }


        #endregion
    }
}
