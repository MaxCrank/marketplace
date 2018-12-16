using System;
using System.Text;
using Marketplace.Core.EventBus.Interfaces;
using Newtonsoft.Json;

namespace Marketplace.Core.EventBus.Base
{
    /// <summary>
    /// Event bus message class.
    /// </summary>
    /// <seealso cref="IEventBusMessage" />
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
        /// Gets the date added.
        /// </summary>
        /// <value>
        /// The date added.
        /// </value>
        public DateTime DateAdded { get; } = DateTime.UtcNow;

        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public virtual MessageType MessageType => MessageType.Unknown;

        #endregion

        #region Public methods

        /// <summary>
        /// Get JSON string representation.
        /// </summary>
        /// <returns>JSON string representation.</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Get JSON string representation in bytes.
        /// </summary>
        /// <returns>JSON string representation in bytes.</returns>
        public byte[] ToJsonBytes()
        {
            return Encoding.UTF8.GetBytes(this.ToJson());
        }

        /// <summary>
        /// Returns true if this message is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this message is valid; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsValid()
        {
            return this.MessageType != MessageType.Unknown && !string.IsNullOrEmpty(this.MessageEventId);
        }

        /// <summary>
        /// Gets the message event ID.
        /// </summary>
        /// <value>
        /// The message event ID.
        /// </value>
        public string MessageEventId => this.GetType().Name;

        #endregion
    }
}
