// File: IEventBusMessageHandler.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;
using System.Threading.Tasks;

namespace Marketplace.Core.EventBus.Interfaces
{
    /// <summary>
    /// Event bus message handler interface
    /// </summary>
    public interface IEventBusMessageHandler<in T> where T : IEventBusMessage
    {
        /// <summary>
        /// Gets the creator identifier.
        /// </summary>
        /// <value>
        /// The creator identifier.
        /// </value>
        string CreatorId { get; }

        /// <summary>
        /// Gets the message event identifier.
        /// </summary>
        /// <value>
        /// The message event identifier.
        /// </value>
        string MessageEventId { get; }

        /// <summary>
        /// Gets the internal type to handle.
        /// </summary>
        /// <value>
        /// The internal type to handle.
        /// </value>
        Type InternalTypeToHandle { get; }

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <value>
        /// The handler.
        /// </value>
        Func<T, Task> Handler { get; }

        /// <summary>
        /// Gets the message tag information.
        /// </summary>
        /// <value>
        /// The message tag information.
        /// </value>
        IEventBusMessageTagInfo MessageTagInfo { get; }

        /// <summary>
        /// Returns true if handler is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this handler is valid; otherwise, <c>false</c>.
        /// </returns>
        bool IsValid();
    }
}
