using System;
using System.Collections.Generic;
using Marketplace.Core.EventBus.Base;
using Marketplace.Core.EventBus.Interfaces;

namespace Marketplace.Core.EventBus.Messages
{
    /// <summary>
    /// Event bus message info resolver.
    /// </summary>
    internal static class EventBusMessageInfoResolver
    {
        #region Fields

        /// <summary>
        /// The message tags information.
        /// </summary>
        private static readonly Dictionary<Type, IEventBusMessageTagInfo> MessageTagsInfo =
            new Dictionary<Type, IEventBusMessageTagInfo>()
            {
                { typeof(LogMessage), new EventBusMessageTagInfo(EventBusMessageTag.Log) }
            };

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the tag info for event bus message.
        /// </summary>
        /// <typeparam name="T">Type of the event bus message.</typeparam>
        /// <returns>Corresponding event bus message tag.</returns>
        public static IEventBusMessageTagInfo GetTagInfo<T>() where T : IEventBusMessage
        {
            return GetTagInfo(typeof(T));
        }

        /// <summary>
        /// Gets the tag info for event bus message.
        /// </summary>
        /// <param name="messageType">Type of the event bus message.</param>
        /// <returns>Corresponding event bus message tag.</returns>
        public static IEventBusMessageTagInfo GetTagInfo(Type messageType)
        {
            return MessageTagsInfo[messageType];
        }

        /// <summary>
        /// Gets the ID for event bus message.
        /// </summary>
        /// <typeparam name="T">Type of the event bus message.</typeparam>
        /// <returns>Corresponding event message ID.</returns>
        public static string GetEventMessageId<T>() where T : IEventBusMessage
        {
            return GetEventMessageId(typeof(T));
        }

        /// <summary>
        /// Gets the ID for event bus message.
        /// </summary>
        /// <param name="messageType">Type of the event bus message.</param>
        /// <returns>Corresponding event message ID.</returns>
        public static string GetEventMessageId(Type messageType)
        {
            return messageType.Name.ToLowerInvariant();
        }

        #endregion
    }
}
