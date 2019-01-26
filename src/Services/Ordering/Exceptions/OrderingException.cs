// File: OrderingException.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;

namespace Marketplace.Services.Ordering.Exceptions
{
    /// <summary>
    /// Ordering exception class
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class OrderingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderingException"/> class.
        /// </summary>
        public OrderingException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderingException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public OrderingException(string message) :
            base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderingException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public OrderingException(string message, Exception innerException) :
            base(message, innerException)
        {

        }
    }
}
