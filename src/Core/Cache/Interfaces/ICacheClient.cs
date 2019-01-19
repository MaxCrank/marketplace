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
        /// <returns>If connection has been established.</returns>
        bool Connect();

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if cache value has been set.</returns>
        Task<bool> SetValueAsync(string key, object value);

        /// <summary>
        /// Completely removes any underlying types of values specified by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if underlying cache values has been removed by key.</returns>
        Task<bool> RemoveUnderlyingValuesAsync(string key);

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Returns the cache value.</returns>
        Task<T> GetValueAsync<T>(string key);

        /// <summary>
        /// Sets the list.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if cache list has been set.</returns>
        Task<bool> SetListAsync(string key, IEnumerable<object> values);

        /// <summary>
        /// Gets the list values.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>A task that represents the asynchronous operation. 
        /// Returns cache list values.</returns>
        Task<IList<T>> GetListValuesAsync<T>(string key);

        /// <summary>
        /// Adds the list value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if cache list value has been added.</returns>
        Task<bool> AddListValueAsync(string key, object value);

        /// <summary>
        /// Gets the list value.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="index">The index.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Returns cache list value.</returns>
        Task<T> GetListValueAsync<T>(string key, int index);

        /// <summary>
        /// Removes the list value occurences.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if cache list value has been removed.</returns>
        Task<bool> RemoveListValueAsync(string key, object value);

        /// <summary>
        /// Sets the dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if cache dictionary has been added.</returns>
        Task<bool> SetDictionaryAsync<T>(string key, Dictionary<string, T> values);

        /// <summary>
        /// Gets the dictionary values.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Returns cache dictionary values.</returns>
        Task<Dictionary<string, T>> GetDictionaryAsync<T>(string key);

        /// <summary>
        /// Sets the dictionary key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="dictionaryKey">The dictionary key.</param>
        /// <param name="dictionaryValue">The dictionary value.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if cache dictionary key has been set.</returns>
        Task<bool> SetDictionaryKeyAsync(string key, string dictionaryKey, object dictionaryValue);

        /// <summary>
        /// Gets the dictionary value.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="dictionaryKey">The dictionary key.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Returns cache dictionary value.</returns>
        Task<T> GetDictionaryValueAsync<T>(string key, string dictionaryKey);

        /// <summary>
        /// Removes the dictionary key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="dictionaryKey">The dictionary key.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// Return value indicates if cache dictionary entry has been removed.</returns>
        Task<bool> RemoveDictionaryKeyAsync(string key, string dictionaryKey);

        /// <summary>
        /// Flushes the data.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task FlushAsync();
    }
}
