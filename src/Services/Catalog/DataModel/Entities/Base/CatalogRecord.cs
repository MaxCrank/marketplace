using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Services.Catalog.DataModel.Entities.Base
{
    /// <summary>
    /// Base catalog record class
    /// </summary>
    internal abstract class CatalogRecord : CatalogEntityWithName
    {
        /// <summary>
        /// Gets or sets the associated images.
        /// </summary>
        /// <value>
        /// The associated images.
        /// </value>
        public virtual ICollection<CatalogImage> CatalogImages { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [Required]
        public string Description { get; set; }
    }
}
