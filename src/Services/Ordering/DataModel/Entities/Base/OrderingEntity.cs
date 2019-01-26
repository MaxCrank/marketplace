// File: OrderingEntity.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Services.Ordering.DataModel.Entities.Base
{
    /// <summary>
    /// Base ordering entity class
    /// </summary>
    internal abstract class OrderingEntity
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column(Order = 0)]
        public long Id { get; set; }
    }
}
