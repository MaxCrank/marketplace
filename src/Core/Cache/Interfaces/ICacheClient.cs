using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marketplace.Core.Cache.Interfaces
{
    /// <summary>
    /// Cache client interface
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface ICacheClient : IDisposable
    {
        /// <summary>
        /// Gets the application identifier.
        /// </summary>
        /// <value>
        /// The application identifier.
        /// </value>
        string ApplicationId { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        bool IsConnected { get; }

        /// <summary>
        /// Connects this instance if not already connected.
        /// </summary>
        /// <returns>Indicates if connection is established.</returns>
        bool Connect();

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="asJson">Store object as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        Task<bool> SetValue(string key, object value, bool asJson = false, int? collectionId = null);

        /// <summary>
        /// Completely removes any underlying types of values specified by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        Task<bool> RemoveUnderlyingValues(string key, int? collectionId = null);

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T">Return type (variety without JSON conversion depends on client implementation).</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="fromJson">If object is stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        Task<T> GetValue<T>(string key, bool fromJson = false, int? collectionId = null);

        /// <summary>
        /// Sets the list.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <param name="asJson">Store objects as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        Task<bool> SetList(string key, object[] values, bool asJson = false, int? collectionId = null);

        /// <summary>
        /// Gets the list values.
        /// </summary>
        /// <typeparam name="T">Return type (variety without JSON conversion depends on client implementation).</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="fromJson">If objects are stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        Task<T[]> GetListValues<T>(string key, bool fromJson = false, int ? collectionId = null);

        /// <summary>
        /// Adds the list value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="asJson">Store object as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        Task AddListValue(string key, object value, bool asJson = false, int? collectionId = null);

        /// <summary>
        /// Gets the list value.
        /// </summary>
        /// <typeparam name="T">Return type (variety without JSON conversion depends on client implementation).</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="index">The index.</param>
        /// <param name="fromJson">If object is stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        Task<T> GetListValue<T>(string key, int index, bool fromJson = false, int? collectionId = null);

        /// <summary>
        /// Removes the list value occurences.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="asJson">If object is stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        Task RemoveListValue(string key, object value, bool asJson = false, int? collectionId = null);

        /// <summary>
        /// Sets the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <param name="asJson">Store objects as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        Task SetDictionary(string key, Dictionary<string, object> values, bool asJson = false, int? collectionId = null);

        /// <summary>
        /// Gets the dictionary values.
        /// </summary>
        /// <typeparam name="T">Return type (variety without JSON conversion depends on client implementation).</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="fromJson">If objects are stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        Task<Dictionary<string, T>> GetDictionaryValues<T>(string key, bool fromJson = false, int? collectionId = null);

        /// <summary>
        /// Sets the dictionary value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="dictionaryKey">The dictionary key.</param>
        /// <param name="dictionaryValue">The dictionary value.</param>
        /// <param name="asJson">Store object as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        Task SetDictionaryValue(string key, string dictionaryKey, object dictionaryValue, bool asJson = false, int? collectionId = null);

        /// <summary>
        /// Gets the dictionary value.
        /// </summary>
        /// <typeparam name="T">Return type (variety without JSON conversion depends on client implementation).</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="dictionaryKey">The dictionary key.</param>
        /// <param name="fromJson">If object is stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        Task<T> GetDictionaryValue<T>(string key, string dictionaryKey, bool fromJson = false, int? collectionId = null);

        /// <summary>
        /// Removes the dictionary key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="dictionaryKey">The dictionary key.</param>
        /// <param name="fromJson">If object is stored as JSON (good for complex types depending on client implementation).</param>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns></returns>
        Task RemoveDictionaryKey(string key, string dictionaryKey, bool fromJson = false, int? collectionId = null);

        /// <summary>
        /// Flushes all.
        /// </summary>
        /// <returns></returns>
        Task FlushAll();
    }
}
