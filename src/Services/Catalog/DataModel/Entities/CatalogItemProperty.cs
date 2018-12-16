using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Marketplace.Services.Catalog.DataModel.Entities.Base;
using Marketplace.Services.Catalog.DataModel.Entities.Joins;

namespace Marketplace.Services.Catalog.DataModel.Entities
{
    /// <summary>
    /// Item property class
    /// </summary>
    /// <seealso cref="CatalogEntity" />
    [Table("ItemProperties")]
    internal class CatalogItemProperty : CatalogEntityWithName
    {
        /// <summary>
        /// Gets or sets a value indicating whether this property is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this property is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use for filtering].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use for filtering]; otherwise, <c>false</c>.
        /// </value>
        public bool UseForFiltering { get; set; }

        /// <summary>
        /// Gets or sets the possible values.
        /// </summary>
        /// <value>
        /// The possible values.
        /// </value>
        public ICollection<string> PossibleValues { get; set; }

        /// <summary>
        /// Gets or sets the categories item properties.
        /// </summary>
        /// <value>
        /// The categories item properties.
        /// </value>
        public IQueryable<CatalogCategoryItemPropertyJoin> CategoriesItemProperties { get; set; }

        /// <summary>
        /// Gets or sets the property values.
        /// </summary>
        /// <value>
        /// The property values.
        /// </value>
        [ForeignKey("ItemPropertyForeignKey")]
        public IQueryable<CatalogItemPropertyValue> PropertyValues { get; set; }
    }
}
