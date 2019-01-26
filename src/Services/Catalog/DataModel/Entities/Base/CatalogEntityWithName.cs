// File: CatalogEntityWithName.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Services.Catalog.DataModel.Entities.Base
{
    /// <summary>
    /// Catalog entity with name class
    /// </summary>
    /// <seealso cref="CatalogEntity" />
    internal abstract class CatalogEntityWithName : CatalogEntity
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [Index]
        [Column(Order = 1)]
        public virtual string Name { get; set; }
    }
}
