// File: OrderingOrder.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Marketplace.Services.Ordering.DataModel.Entities.Base;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Services.Ordering.DataModel.Entities
{
    /// <summary>
    /// Ordering order class
    /// </summary>
    /// <seealso cref="Marketplace.Services.Ordering.DataModel.Entities.Base.OrderingEntity" />
    [Table("Orders")]
    internal class OrderingOrder : OrderingEntity
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        [Index]
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets the ordered items.
        /// </summary>
        /// <value>
        /// The ordered items.
        /// </value>
        [ForeignKey("OrderForeignKey")]
        public IQueryable<OrderingItem> OrderedItems { get; set; }

        /// <summary>
        /// Gets or sets the order date.
        /// </summary>
        /// <value>
        /// The order date.
        /// </value>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime OrderDate { get; set; }
    }
}
