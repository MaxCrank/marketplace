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
        /// Gets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public virtual MessageType MessageType => MessageType.Unknown;

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
        /// Gets the message event ID.
        /// </summary>
        /// <value>
        /// The message event ID.
        /// </value>
        public string MessageEventId => this.GetType().Name.ToLowerInvariant();

        /// <summary>
        /// Gets the unified message identifier (combined of message type and event IDs).
        /// </summary>
        /// <value>
        /// The unified message identifier.
        /// </value>
        public string UnifiedMessageTypeEventId => $"{this.MessageType.ToString().ToLowerInvariant()}_{this.MessageEventId}";

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
        /// Gets the basic information.
        /// </summary>
        /// <returns></returns>
        public string GetBasicInfo()
        {
            return
                $"Type: {this.MessageType}; Event ID: {this.MessageEventId}; Date added: {this.CreationDate}; Message ID: {this.MessageId}";
        }

        #endregion
    }
}
