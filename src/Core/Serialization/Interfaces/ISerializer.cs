using System;

namespace Marketplace.Core.Serialization
{
    /// <summary>
    /// Serialier interface.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serializes to string.
        /// </summary>
        /// <param name="serializableObject">The serializable object.</param>
        /// <returns>Serialized object as string.</returns>
        string SerializeToString(object serializableObject);

        /// <summary>
        /// Serializes to bytes.
        /// </summary>
        /// <param name="serializableObject">The serializable object.</param>
        /// <returns>Serialized object as byte array.</returns>
        byte[] SerializeToBytes(object serializableObject);

        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <typeparam name="T">Type to deserialize to.</typeparam>
        /// <param name="data">The data.</param>
        /// <returns>Deserialized data.</returns>
        T Deserialize<T>(byte[] data);

        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="type">Type to deserialize to.</param>
        /// <returns>Deserialized data.</returns>
        object Deserialize(byte[] data, Type type);

        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <typeparam name="T">Type to deserialize to.</typeparam>
        /// <param name="data">The data.</param>
        /// <returns>Deserialized data.</returns>
        T Deserialize<T>(string data);

        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="type">Type to deserialize to.</param>
        /// <returns>Deserialized data.</returns>
        object Deserialize(string data, Type type);
    }
}
