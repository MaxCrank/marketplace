using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Marketplace.Services.Catalog.DataModel.Entities.Base;
using Marketplace.Services.Catalog.DataModel.Entities.Joins;
using Marketplace.Services.Catalog.Exceptions;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Services.Catalog.DataModel.Entities
{
    /// <summary>
    /// Catalog item class
    /// </summary>
    /// <seealso cref="CatalogRecord" />
    [Table("Items")]
    internal class CatalogItem : CatalogRecord
    {
        /// <summary>
        /// Gets or sets the catalog category foreign key.
        /// </summary>
        /// <value>
        /// The catalog category foreign key.
        /// </value>
        public long CatalogCategoryForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the catalog category.
        /// </summary>
        /// <value>
        /// The catalog category.
        /// </value>
        [ForeignKey("CatalogCategoryForeignKey")]
        public CatalogCategory CatalogCategory { get; set; }

        /// <summary>
        /// Gets or sets the producer foreign key.
        /// </summary>
        /// <value>
        /// The producer foreign key.
        /// </value>
        public long ProducerForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the producer.
        /// </summary>
        /// <value>
        /// The producer.
        /// </value>
        [ForeignKey("ProducerForeignKey")]
        public CatalogItemProducer Producer { get; set; }

        /// <summary>
        /// Gets or sets the seller foreign key.
        /// </summary>
        /// <value>
        /// The seller foreign key.
        /// </value>
        public long SellerForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the seller.
        /// </summary>
        /// <value>
        /// The seller.
        /// </value>
        [ForeignKey("SellerForeignKey")]
        public CatalogItemProducer Seller { get; set; }

        /// <summary>
        /// Gets or sets the date added.
        /// </summary>
        /// <value>
        /// The date added.
        /// </value>
        [Index]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DateAdded { get; set; }

        /// <summary>
        /// Gets or sets the basic price.
        /// </summary>
        /// <value>
        /// The basic price.
        /// </value>
        [Index]
        public decimal BasicPrice { get; set; }

        /// <summary>
        /// Gets or sets the discount.
        /// </summary>
        /// <value>
        /// The discount.
        /// </value>
        [Index]
        public int Discount { get; set; }

        /// <summary>
        /// Gets or sets the available quantity.
        /// </summary>
        /// <value>
        /// The available quantity.
        /// </value>
        public int AvailableQuantity { get; set; }

        /// <summary>
        /// Gets or sets the maximum quantity.
        /// </summary>
        /// <value>
        /// The maximum quantity.
        /// </value>
        public int MaxQuantity { get; set; }

        /// <summary>
        /// Gets or sets the re-stock quantity (when item runs low and should be re-stocked).
        /// </summary>
        /// <value>
        /// The re-stock quantity.
        /// </value>
        public int RestockQuantity { get; set; }

        /// <summary>
        /// Gets or sets the item property values.
        /// </summary>
        /// <value>
        /// The item property values.
        /// </value>
        public ICollection<CatalogItemPropertyValueJoin> CatalogPropertyValuesItems { get; set; }

        /// <summary>
        /// Gets or sets the associated images.
        /// </summary>
        /// <value>
        /// The associated images.
        /// </value>
        [ForeignKey("ItemForeignKey")]
        public override ICollection<CatalogImage> CatalogImages { get; set; }

        /// <summary>
        /// Gets or sets the rating.
        /// </summary>
        /// <value>
        /// The rating.
        /// </value>
        [Index]
        public decimal Rating { get; set; }

        /// <summary>
        /// Gets or sets the ratings count.
        /// </summary>
        /// <value>
        /// The ratings count.
        /// </value>
        public long RatingsCount { get; set; }

        /// <summary>
        /// Gets or sets the order count.
        /// </summary>
        /// <value>
        /// The order count.
        /// </value>
        [Index]
        public long OrderCount { get; set; }

        /// <summary>
        /// Adds the specified quantity.
        /// </summary>
        /// <param name="quantity">The quantity.</param>
        /// <returns>Added quantity.</returns>
        public int Add(int quantity)
        {
            if (quantity < 0)
            {
                throw new CatalogException($"{this.Name} addition requires positive number of items instead of {quantity}");
            }

            if (this.AvailableQuantity == this.MaxQuantity)
            {
                return 0;
            }

            int addedQuantity;
            int possibleQuantity = this.AvailableQuantity + quantity;
            if (possibleQuantity > this.MaxQuantity)
            {
                addedQuantity = possibleQuantity - this.MaxQuantity;
                this.AvailableQuantity = this.MaxQuantity;
            }
            else
            {
                addedQuantity = quantity;
                this.AvailableQuantity = possibleQuantity;
            }

            return addedQuantity;
        }

        /// <summary>
        /// Removes the specified quantity.
        /// </summary>
        /// <param name="quantity">The quantity.</param>
        /// <returns>Removed quantity.</returns>
        public int Remove(int quantity)
        {
            if (quantity < 0)
            {
                throw new CatalogException($"{this.Name} removal requires positive number of items instead of {quantity}");
            }

            if (this.AvailableQuantity == 0)
            {
                throw new CatalogException($"{this.Name} is out of stock and is not available to remove {quantity} items!");
            }

            int removedQuantity;
            int possibleQuantity = this.AvailableQuantity - quantity;
            if (possibleQuantity < 0)
            {
                removedQuantity = quantity - possibleQuantity;
                this.AvailableQuantity = 0;
            }
            else
            {
                removedQuantity = quantity;
            }

            return removedQuantity;
        }

        /// <summary>
        /// Adds the rating.
        /// </summary>
        /// <param name="rating">The rating.</param>
        public void AddRating(int rating)
        {
            if (rating < 1 || rating > 5)
            {
                throw new CatalogException($"Catalog item rating should be 1 to 5 to add, but was {rating} instead");
            }

            decimal delta = rating - this.Rating;
            this.Rating += delta / ++this.RatingsCount;
        }
    }
}
