// File: IEventBusMessage.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;

namespace Marketplace.Core.EventBus.Interfaces
{
    /// <summary>
    /// Event bus message interface
    /// </summary>
    public interface IEventBusMessage
    {
        /// <summary>
        /// Gets the message identifier.
        /// </summary>
        /// <value>
        /// The message identifier.
        /// </value>
        Guid MessageId { get; }

        /// <summary>
        /// Gets the creation date.
        /// </summary>
        /// <value>
        /// The creation date.
        /// </value>
        DateTime CreationDate { get; }

        /// <summary>
        /// Gets the message tag information.
        /// </summary>
        /// <value>
        /// The message tag information.
        /// </value>
        IEventBusMessageTagInfo MessageTagInfo { get; }

        /// <summary>
        /// Gets the message event ID.
        /// </summary>
        /// <value>
        /// The message event ID.
        /// </value>
        string MessageEventId { get; }

        /// <summary>
        /// Returns true if this message is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this message is valid; otherwise, <c>false</c>.
        /// </returns>
        bool IsValid();
    }
}
