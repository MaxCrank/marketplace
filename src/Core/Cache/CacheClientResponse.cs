using Marketplace.Core.Cache.Exceptions;

namespace Marketplace.Core.Cache
{
    /// <summary>
    /// Cache client response.
    /// </summary>
    internal class CacheClientResponse
    {
        #region Fields

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public readonly object Value;

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public readonly CacheException Exception;

        #endregion

        #region Constructors

        public CacheClientResponse(object value, CacheException exception)
        {
            this.Value = value;
            this.Exception = exception;
        }

        #endregion
    }
}
