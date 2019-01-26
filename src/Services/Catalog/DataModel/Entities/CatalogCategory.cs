// File: CatalogCategory.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Marketplace.Services.Catalog.DataModel.Entities.Base;
using Marketplace.Services.Catalog.DataModel.Entities.Joins;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Services.Catalog.DataModel.Entities
{
    /// <summary>
    /// Catalog category class
    /// </summary>
    /// <seealso cref="CatalogRecord" />
    [Table("CatalogCategories")]
    internal class CatalogCategory : CatalogRecord
    {
        /// <summary>
        /// Gets or sets the parent category foreign key.
        /// </summary>
        /// <value>
        /// The parent category foreign key.
        /// </value>
        public long ParentCategoryForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the parent category.
        /// </summary>
        /// <value>
        /// The parent category.
        /// </value>
        [ForeignKey("ParentCategoryForeignKey")]
        public CatalogCategory ParentCategory { get; set; }

        /// <summary>
        /// Gets or sets the sub categories.
        /// </summary>
        /// <value>
        /// The sub categories.
        /// </value>
        [ForeignKey("ParentCategoryForeignKey")]
        public ICollection<CatalogCategory> SubCategories { get; set; }

        /// <summary>
        /// Gets or sets the item producer alias (i.e. name of the item producer).
        /// For example, it's "Author" for books, "Artist" for music and so on. 
        /// </summary>
        /// <value>
        /// The item producer alias.
        /// </value>
        [Required]
        [Index]
        public string ItemProducerAlias { get; set; }

        /// <summary>
        /// Gets or sets the categories item properties.
        /// </summary>
        /// <value>
        /// The categories item properties.
        /// </value>
        public ICollection<CatalogCategoryItemPropertyJoin> CategoriesItemProperties { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        [ForeignKey("CatalogCategoryForeignKey")]
        public IQueryable<CatalogItem> Items { get; set; }

        /// <summary>
        /// Gets or sets the associated images.
        /// </summary>
        /// <value>
        /// The associated images.
        /// </value>
        [ForeignKey("CategoryForeignKey")]
        public override ICollection<CatalogImage> CatalogImages { get; set; }
    }
}
