using System;
using System.Threading.Tasks;
using Marketplace.Core.EventBus.Base;

namespace Marketplace.Core.EventBus.Interfaces
{
    /// <summary>
    /// Event bus message handler interface
    /// </summary>
    public interface IEventBusMessageHandler
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
        /// Gets the handler.
        /// </summary>
        /// <value>
        /// The handler.
        /// </value>
        Func<string, Task> Handler { get; }

        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        MessageType MessageType { get; }

        /// <summary>
        /// Returns true if handler is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this handler is valid; otherwise, <c>false</c>.
        /// </returns>
        bool IsValid();
    }
}
