// File: IEventBusMessage.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;
using Marketplace.Core.EventBus.Base;

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
        /// Gets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        MessageType MessageType { get; }

        /// <summary>
        /// Gets the message event ID.
        /// </summary>
        /// <value>
        /// The message event ID.
        /// </value>
        string MessageEventId { get; }

        /// <summary>
        /// Gets the unified message identifier (combined of message type and event ID).
        /// </summary>
        /// <value>
        /// The unified message identifier.
        /// </value>
        string UnifiedMessageTypeEventId { get; }

        /// <summary>
        /// Get JSON string representation.
        /// </summary>
        /// <returns>JSON string representation.</returns>
        string ToJson();

        /// <summary>
        /// Get JSON string representation in bytes.
        /// </summary>
        /// <returns>JSON string representation in bytes.</returns>
        byte[] ToJsonBytes();

        /// <summary>
        /// Returns true if this message is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this message is valid; otherwise, <c>false</c>.
        /// </returns>
        bool IsValid();
    }
}
