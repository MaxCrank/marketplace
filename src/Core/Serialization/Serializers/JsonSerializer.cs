using System;
using System.Text;
using Newtonsoft.Json;

namespace Marketplace.Core.Serialization.Serializers
{
    /// <summary>
    /// The JSON serializer.
    /// </summary>
    /// <seealso cref="Marketplace.Core.Serialization.ISerializer" />
    public class JsonSerializer : ISerializer
    {
        #region Methods

        /// <summary>
        /// Serializes to string.
        /// </summary>
        /// <param name="serializableObject">The serializable object.</param>
        /// <returns>Serialized object as string.</returns>
        public string SerializeToString(object serializableObject)
        {
            return JsonConvert.SerializeObject(serializableObject);
        }

        /// <summary>
        /// Serializes to bytes.
        /// </summary>
        /// <param name="serializableObject">The serializable object.</param>
        /// <returns>Serialized object as byte array.</returns>
        public byte[] SerializeToBytes(object serializableObject)
        {
            return Encoding.Unicode.GetBytes(this.SerializeToString(serializableObject));
        }

        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns>Deserialized data.</returns>
        public T Deserialize<T>(byte[] data)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.Unicode.GetString(data));
        }

        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="type">Type to deserialize to.</param>
        /// <returns>Deserialized data.</returns>
        public object Deserialize(byte[] data, Type type)
        {
            return JsonConvert.DeserializeObject(Encoding.Unicode.GetString(data), type);
        }

        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns>Deserialized data.</returns>
        public T Deserialize<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="type">Type to deserialize to.</param>
        /// <returns>Deserialized data.</returns>
        public object Deserialize(string data, Type type)
        {
            return JsonConvert.DeserializeObject(data, type);
        }

        #endregion
    }
}
