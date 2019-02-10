using Marketplace.Core.EventBus.Interfaces;

namespace Marketplace.Core.EventBus.Base
{
    /// <summary>
    /// Event bus message tag info.
    /// </summary>
    /// <seealso cref="Marketplace.Core.EventBus.Interfaces.IEventBusMessageTagInfo" />
    public class EventBusMessageTagInfo : IEventBusMessageTagInfo
    {
        #region Properties

        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public EventBusMessageTag MessageTag { get; }

        /// <summary>
        /// Gets the message tag string.
        /// </summary>
        /// <value>
        /// The message tag string.
        /// </value>
        public string MessageTagString => this.MessageTag.ToString().ToLowerInvariant();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBusMessageTagInfo"/> class.
        /// </summary>
        /// <param name="messageTag">The message tag.</param>
        public EventBusMessageTagInfo(EventBusMessageTag messageTag)
        {
            this.MessageTag = messageTag;
        }

        #endregion
    }
}
