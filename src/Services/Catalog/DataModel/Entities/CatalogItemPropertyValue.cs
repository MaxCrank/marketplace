// File: CatalogItemPropertyValue.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Marketplace.Services.Catalog.DataModel.Entities.Base;
using Marketplace.Services.Catalog.DataModel.Entities.Joins;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Services.Catalog.DataModel.Entities
{
    /// <summary>
    /// Item property value class
    /// </summary>
    /// <seealso cref="CatalogEntity" />
    [Table("ItemPropertyValues")]
    internal class CatalogItemPropertyValue : CatalogRecord
    {
        /// <summary>
        /// Gets or sets the item property foreign key.
        /// </summary>
        /// <value>
        /// The item property foreign key.
        /// </value>
        public long ItemPropertyForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the item property.
        /// </summary>
        /// <value>
        /// The item property.
        /// </value>
        [ForeignKey("ItemPropertyForeignKey")]
        public CatalogItemProperty ItemProperty { get; set; }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        /// <value>
        /// The property value.
        /// </value>
        [Index]
        public string PropertyValue { get; set; }

        /// <summary>
        /// Gets or sets the catalog items.
        /// </summary>
        /// <value>
        /// The catalog items.
        /// </value>
        public IQueryable<CatalogItemPropertyValueJoin> CatalogPropertyValuesItems { get; set; }

        /// <summary>
        /// Gets or sets the associated images.
        /// </summary>
        /// <value>
        /// The associated images.
        /// </value>
        [ForeignKey("ItemPropertyValueForeignKey")]
        public override ICollection<CatalogImage> CatalogImages { get; set; }
    }
}
