using Marketplace.Core.EventBus.Base;

namespace Marketplace.Core.EventBus.Interfaces
{
    /// <summary>
    /// Event bus message tag info.
    /// </summary>
    public interface IEventBusMessageTagInfo
    {
        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        EventBusMessageTag MessageTag { get; }

        /// <summary>
        /// Gets the lowercase message tag string.
        /// </summary>
        /// <value>
        /// The message tag string.
        /// </value>
        string MessageTagString { get; }
    }
}
