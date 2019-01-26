// File: CacheException.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;

namespace Marketplace.Core.Cache.Exceptions
{
    /// <summary>
    /// Event bus exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class CacheException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheException"/> class.
        /// </summary>
        public CacheException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="client">Cache client name.</param>
        public CacheException(string message, string client = null) :
            base($"{client}: {message}")
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        /// <param name="client">Cache client name.</param>
        public CacheException(string message, Exception innerException, string client = null) :
            base($"{client}: {message}", innerException)
        {

        }
    }
}
