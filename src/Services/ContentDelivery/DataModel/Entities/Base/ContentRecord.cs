// File: ContentRecord.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Marketplace.Services.ContentDelivery.DataModel.Entities.Base
{
    /// <summary>
    /// Content record base class
    /// </summary>
    internal abstract class ContentRecord
    {
        /// <summary>
        /// The identifier
        /// </summary>
        [BsonId]
        public ObjectId Id;
    }
}
