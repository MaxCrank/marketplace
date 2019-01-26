// File: OrderingItem.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System.ComponentModel.DataAnnotations.Schema;
using Marketplace.Services.Ordering.DataModel.Entities.Base;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Services.Ordering.DataModel.Entities
{
    /// <summary>
    /// Ordering item class.
    /// </summary>
    /// <seealso cref="Marketplace.Services.Ordering.DataModel.Entities.Base.OrderingEntity" />
    [Table("Items")]
    internal class OrderingItem : OrderingEntity
    {
        /// <summary>
        /// Gets or sets the item identifier.
        /// </summary>
        /// <value>
        /// The item identifier.
        /// </value>
        [Index]
        public long ItemId { get; set; }

        /// <summary>
        /// Gets or sets the image foreign key.
        /// </summary>
        /// <value>
        /// The image foreign key.
        /// </value>
        public long ImageForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        /// <value>
        /// The image.
        /// </value>
        [ForeignKey("ImageForeignKey")]
        public OrderingImage Image { get; set; }

        /// <summary>
        /// Gets or sets the order foreign key.
        /// </summary>
        /// <value>
        /// The order foreign key.
        /// </value>
        public long OrderForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [Index(IsUnique = true)]
        [ForeignKey("OrderForeignKey")]
        public OrderingOrder Order { get; set; }

        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the items count.
        /// </summary>
        /// <value>
        /// The items count.
        /// </value>
        public int ItemsCount { get; set; } = 1;
    }
}
