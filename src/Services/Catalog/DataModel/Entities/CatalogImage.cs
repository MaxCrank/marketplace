using System.ComponentModel.DataAnnotations.Schema;
using Marketplace.Services.Catalog.DataModel.Entities.Base;

namespace Marketplace.Services.Catalog.DataModel.Entities
{
    /// <summary>
    /// Catalog image class
    /// </summary>
    /// <seealso cref="CatalogEntityWithName" />
    [Table("Images")]
    internal class CatalogImage : CatalogEntityWithName
    {
        /// <summary>
        /// The thumbnail image identifier
        /// </summary>
        public string ThumbnailImageId { get; set; }

        /// <summary>
        /// The mid-size image identifier
        /// </summary>
        public string MidSizeImageId { get; set; }

        /// <summary>
        /// The full-size image identifier
        /// </summary>
        public string FullSizeImageId { get; set; }

        /// <summary>
        /// Gets or sets the item producer foreign key.
        /// </summary>
        /// <value>
        /// The item producer foreign key.
        /// </value>
        public long ItemProducerForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the item producer.
        /// </summary>
        /// <value>
        /// The item producer.
        /// </value>
        [ForeignKey("ItemProducerForeignKey")]
        public CatalogItemProducer ItemProducer { get; set; }

        /// <summary>
        /// Gets or sets the item foreign key.
        /// </summary>
        /// <value>
        /// The item foreign key.
        /// </value>
        public long ItemForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        [ForeignKey("ItemForeignKey")]
        public CatalogItem Item { get; set; }

        /// <summary>
        /// Gets or sets the category foreign key.
        /// </summary>
        /// <value>
        /// The category foreign key.
        /// </value>
        public long CategoryForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [ForeignKey("CategoryForeignKey")]
        public CatalogCategory Category { get; set; }

        /// <summary>
        /// Gets or sets the item property value foreign key.
        /// </summary>
        /// <value>
        /// The item property value foreign key.
        /// </value>
        public long ItemPropertyValueForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the item property value.
        /// </summary>
        /// <value>
        /// The item property value.
        /// </value>
        [ForeignKey("ItemPropertyValueForeignKey")]
        public CatalogItemPropertyValue ItemPropertyValue { get; set; }

        /// <summary>
        /// Gets or sets the item seller foreign key.
        /// </summary>
        /// <value>
        /// The item seller foreign key.
        /// </value>
        public long ItemSellerForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the item seller.
        /// </summary>
        /// <value>
        /// The item seller.
        /// </value>
        [ForeignKey("ItemSellerForeignKey")]
        public CatalogItemPropertyValue ItemSeller { get; set; }
    }
}
