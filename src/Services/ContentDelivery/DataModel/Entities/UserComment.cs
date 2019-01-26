// File: UserComment.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using Marketplace.Services.ContentDelivery.DataModel.Entities.Base;
using System;

namespace Marketplace.Services.ContentDelivery.DataModel.Entities
{
    /// <summary>
    /// User comment class
    /// </summary>
    /// <seealso cref="ContentRecord" />
    internal class UserComment : ContentRecord
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the catalog item identifier.
        /// </summary>
        /// <value>
        /// The catalog item identifier.
        /// </value>
        public long CatalogItemId { get; set; }

        /// <summary>
        /// Gets or sets the date added.
        /// </summary>
        /// <value>
        /// The date added.
        /// </value>
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the review.
        /// </summary>
        /// <value>
        /// The review.
        /// </value>
        public UserReview Review { get; set; }
    }
}
