// File: EventBusMessage.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;
using Marketplace.Core.EventBus.Interfaces;
using Marketplace.Core.EventBus.Messages;

namespace Marketplace.Core.EventBus.Base
{
    /// <summary>
    /// Event bus message class.
    /// </summary>
    /// <seealso cref="IEventBusMessage" />
    [Serializable]
    public abstract class EventBusMessage : IEventBusMessage
    {
        #region Properties

        /// <summary>
        /// Gets the message identifier.
        /// </summary>
        /// <value>
        /// The message identifier.
        /// </value>
        public Guid MessageId { get; } = new Guid();

        /// <summary>
        /// Gets the creation date.
        /// </summary>
        /// <value>
        /// The creation date.
        /// </value>
        public DateTime CreationDate { get; } = DateTime.UtcNow;

        /// <summary>
        /// Gets the message tag information.
        /// </summary>
        /// <value>
        /// The message tag information.
        /// </value>
        public IEventBusMessageTagInfo MessageTagInfo { get; }

        /// <summary>
        /// Gets the message event ID.
        /// </summary>
        /// <value>
        /// The message event ID.
        /// </value>
        public string MessageEventId { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBusMessage"/> class.
        /// </summary>
        protected EventBusMessage()
        {
            this.MessageTagInfo = EventBusMessageInfoResolver.GetTagInfo(this.GetType());
            this.MessageEventId = EventBusMessageInfoResolver.GetEventMessageId(this.GetType());
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns true if this message is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this message is valid; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsValid()
        {
            return this.MessageTagInfo.MessageTag != EventBusMessageTag.Unknown && !string.IsNullOrEmpty(this.MessageEventId);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return
                $"Tag: {this.MessageTagInfo.MessageTagString}; Event ID: {this.MessageEventId}; Date added: {this.CreationDate}; " +
                $"Message ID: {this.MessageId}";
        }

        #endregion
    }
}
