using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Core.Cache.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Marketplace.Core.Cache
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
        /// The host
        /// </summary>
        private readonly string host;

        /// <summary>
        /// The default database
        /// </summary>
        private readonly int? defaultDatabase;

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
        /// <param name="host">The host.</param>
        /// <param name="defaultDb">The default database.</param>
        public RedisCacheClient(string applicationId, string host = "localhost", int? defaultDb = null)
        {
            this.ApplicationId = applicationId;
            this.host = host;
            this.defaultDatabase = defaultDb;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Connects this instance if not already connected.
        /// </summary>
        /// <returns>Indicates if connection is established.</returns>
        public bool Connect()
        {
            if (this.connection == null)
            {
                var options = ConfigurationOptions.Parse(this.host);
                options.ClientName = this.ApplicationId;
                options.DefaultDatabase = this.defaultDatabase;
                this.connection = ConnectionMultiplexer.Connect(options);
            }

            return this.connection.IsConnected;
        }

        /// <summary>
        /// Adds the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="asJson">Store object as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task AddValue(string key, object value, bool asJson = false, int? collectionId = null)
        {
            if (asJson)
            {
                value = JsonConvert.SerializeObject(value);
            }

            await this.GetDatabase(collectionId).SetAddAsync(key, (RedisValue)value);
        }

        /// <summary>
        /// Completely removes any underlying types of values specified by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task RemoveUnderlyingValues(string key, int? collectionId = null)
        {
            await this.GetDatabase(collectionId).KeyDeleteAsync(key);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T">T may be string, bool, int, long, double, byte[] without JSON conversion.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="fromJson">If object is stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task<T> GetValue<T>(string key, bool fromJson = false, int? collectionId = null)
        {
            var redisValue = await this.GetDatabase(collectionId).StringGetAsync(key);
            if (fromJson)
            {
                return JsonConvert.DeserializeObject<T>(redisValue.ToString());
            }
            else
            {
                return (T)Convert.ChangeType(redisValue, typeof(T));
            }
        }

        /// <summary>
        /// Adds the list.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <param name="asJson">Store objects as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task AddList(string key, object[] values, bool asJson = false, int? collectionId = null)
        {
            var db = this.GetDatabase(collectionId);
            var transaction = db.CreateTransaction();
            foreach (var value in values)
            {
                object valueToStore = value;
                if (asJson)
                {
                    valueToStore = JsonConvert.SerializeObject(value);
                }

                await transaction.ListRightPushAsync(key, (RedisValue)valueToStore);
            }

            await transaction.ExecuteAsync();
        }

        /// <summary>
        /// Gets the list values.
        /// </summary>
        /// <typeparam name="T">T may be string, bool, int, long, double, byte[] without JSON conversion.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="fromJson">If objects are stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task<T[]> GetListValues<T>(string key, bool fromJson = false, int? collectionId = null)
        {
            var list = await this.GetDatabase(collectionId).ListRangeAsync(key);
            return list.Select(v => fromJson ? JsonConvert.DeserializeObject<T>(v) : (T)Convert.ChangeType(v, typeof(T))).ToArray();
        }

        /// <summary>
        /// Adds the list value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        ///  <param name="asJson">Store object as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task AddListValue(string key, object value, bool asJson = false, int? collectionId = null)
        {
            if (asJson)
            {
                value = JsonConvert.SerializeObject(value);
            }

            await this.GetDatabase(collectionId).ListRightPushAsync(key, (RedisValue) value);
        }

        /// <summary>
        /// Gets the list value.
        /// </summary>
        /// <typeparam name="T">T may be string, bool, int, long, double, byte[] without JSON conversion.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="index">The index.</param>
        /// <param name="fromJson">If object is stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task<T> GetListValue<T>(string key, int index, bool fromJson = false, int? collectionId = null)
        {
            var redisValue = await this.GetDatabase(collectionId).ListGetByIndexAsync(key, index);
            return fromJson ? JsonConvert.DeserializeObject<T>(redisValue) : (T)Convert.ChangeType(redisValue, typeof(T));
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
            if (asJson)
            {
                value = JsonConvert.SerializeObject(value);
            }

            await this.GetDatabase(collectionId).ListRemoveAsync(key, (RedisValue) value);
        }

        /// <summary>
        /// Adds the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <param name="asJson">Store objects as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task AddDictionary(string key, Dictionary<string, object> values, bool asJson = false, int? collectionId = null)
        {
            await this.GetDatabase(collectionId)
                .HashSetAsync(key, values.Select(kvp =>
                {
                    object value = asJson ? JsonConvert.SerializeObject(kvp.Value) : kvp.Value;
                    return new HashEntry(kvp.Key, (RedisValue)value);
                }).ToArray());
        }

        /// <summary>
        /// Gets the dictionary values.
        /// </summary>
        /// <typeparam name="T">T may be string, bool, int, long, double, byte[] without JSON conversion.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="fromJson">If objects are stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task<Dictionary<string, T>> GetDictionaryValues<T>(string key, bool fromJson = false, int? collectionId = null)
        {
            var set = await this.GetDatabase(collectionId).HashGetAllAsync(key);
            var kvps = set.Select(s => new KeyValuePair<string, T>(s.Name, fromJson ? 
                JsonConvert.DeserializeObject<T>(s.Value) : (T)Convert.ChangeType(s.Value, typeof(T))));
            return new Dictionary<string, T>(kvps);
        }

        /// <summary>
        /// Adds the dictionary value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="dictionaryKey">The dictionary key.</param>
        /// <param name="dictionaryValue">The dictionary value.</param>
        /// <param name="asJson">Store object as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task AddDictionaryValue(string key, string dictionaryKey, object dictionaryValue, bool asJson = false, int? collectionId = null)
        {
            if (asJson)
            {
                dictionaryValue = JsonConvert.SerializeObject(dictionaryValue);
            }

            await this.GetDatabase(collectionId).HashSetAsync(key, new[] { new HashEntry(dictionaryKey, (RedisValue) dictionaryValue) });
        }

        /// <summary>
        /// Gets the dictionary value.
        /// </summary>
        /// <typeparam name="T">T may be string, bool, int, long, double, byte[] without JSON conversion.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="dictionaryKey">The dictionary key.</param>
        /// <param name="fromJson">If object is stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task<T> GetDictionaryValue<T>(string key, string dictionaryKey, bool fromJson = false, int? collectionId = null)
        {
            var redisValue = await this.GetDatabase(collectionId).HashGetAsync(key, dictionaryKey);
            return fromJson ? JsonConvert.DeserializeObject<T>(redisValue) : (T) Convert.ChangeType(redisValue, typeof(T));
        }

        /// <summary>
        /// Removes the dictionary key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="dictionaryKey">The dictionary key.</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        public async Task RemoveDictionaryKey(string key, string dictionaryKey, int? collectionId = null)
        {
            await this.GetDatabase(collectionId).HashDeleteAsync(key, dictionaryKey);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.connection?.Dispose();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>Redis database.</returns>
        private IDatabase GetDatabase(int? database)
        {
            return database != null ? this.connection.GetDatabase(database.Value) : this.connection.GetDatabase();
        }

        #endregion
    }
}
