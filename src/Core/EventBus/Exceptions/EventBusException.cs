// File: EventBusException.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;

namespace Marketplace.Core.EventBus.Exceptions
{
    /// <summary>
    /// Event bus exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class EventBusException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventBusException"/> class.
        /// </summary>
        public EventBusException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBusException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public EventBusException(string message) :
            base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBusException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public EventBusException(string message, Exception innerException) :
            base(message, innerException)
        {

        }
    }
}
