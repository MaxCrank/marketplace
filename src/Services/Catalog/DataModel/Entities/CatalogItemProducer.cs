using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Marketplace.Services.Catalog.DataModel.Entities.Base;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Services.Catalog.DataModel.Entities
{
    /// <summary>
    /// Item producer class
    /// </summary>
    /// <seealso cref="CatalogRecord" />
    [Table("ItemProducers")]
    internal class CatalogItemProducer : CatalogRecord
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [Index(IsUnique = true)]
        [Column(Order = 1)]
        public override string Name { get; set; }

        /// <summary>
        /// Gets or sets the catalog items.
        /// </summary>
        /// <value>
        /// The catalog items.
        /// </value>
        [ForeignKey("ProducerForeignKey")]
        public IQueryable<CatalogItem> CatalogItems { get; set; }

        /// <summary>
        /// Gets or sets the associated images.
        /// </summary>
        /// <value>
        /// The associated images.
        /// </value>
        [ForeignKey("ItemProducerForeignKey")]
        public override ICollection<CatalogImage> CatalogImages { get; set; }
    }
}
