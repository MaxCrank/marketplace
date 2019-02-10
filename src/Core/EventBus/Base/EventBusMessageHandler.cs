// File: EventBusMessageHandler.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;
using System.Threading.Tasks;
using Marketplace.Core.EventBus.Interfaces;
using Marketplace.Core.EventBus.Messages;

namespace Marketplace.Core.EventBus.Base
{
    /// <summary>
    /// Base event bus message handler class.
    /// </summary>
    /// <seealso cref="IEventBusMessageHandler{T}" />
    public class EventBusMessageHandler<T> : IEventBusMessageHandler<T> where T: IEventBusMessage
    {
        #region Properties

        /// <summary>
        /// Gets the creator identifier.
        /// </summary>
        /// <value>
        /// The creator identifier.
        /// </value>
        public string CreatorId { get; protected set; }

        /// <summary>
        /// Gets the message event identifier.
        /// </summary>
        /// <value>
        /// The message event identifier.
        /// </value>
        public string MessageEventId { get; }

        /// <summary>
        /// Gets the internal type to handle.
        /// </summary>
        /// <value>
        /// The internal type to handle.
        /// </value>
        public Type InternalTypeToHandle => typeof(T);

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <value>
        /// The handler.
        /// </value>
        public Func<T, Task> Handler { get; protected set; }

        /// <summary>
        /// Gets the message tag information.
        /// </summary>
        /// <value>
        /// The message tag information.
        /// </value>
        public IEventBusMessageTagInfo MessageTagInfo { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBusMessageHandler{T}"/> class.
        /// </summary>
        /// <param name="creatorId">The creator identifier.</param>
        /// <param name="handler">The event handler.</param>
        public EventBusMessageHandler(string creatorId, Func<T, Task> handler)
        {
            this.CreatorId = creatorId;
            this.Handler = handler;
            this.MessageTagInfo = EventBusMessageInfoResolver.GetTagInfo<T>();
            this.MessageEventId = EventBusMessageInfoResolver.GetEventMessageId<T>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns true if handler is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this handler is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(this.CreatorId) &&
                   !string.IsNullOrEmpty(this.MessageEventId) &&
                   this.MessageTagInfo.MessageTag != EventBusMessageTag.Unknown &&
                   this.Handler != null;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Tag: {this.MessageTagInfo.MessageTag}; Event ID; {this.MessageEventId}; Creator: {this.CreatorId}";
        }

        #endregion
    }
}
