using Marketplace.Services.Catalog.DataModel.Configuration;
using Marketplace.Services.Catalog.DataModel.Entities;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Services.Catalog.DataModel
{
    /// <summary>
    /// The catalog DB context
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    internal class CatalogContext : DbContext
    {
        /// <summary>
        /// Gets or sets the catalog items.
        /// </summary>
        /// <value>
        /// The catalog items.
        /// </value>
        public DbSet<CatalogItem> Items { get; set; }

        /// <summary>
        /// Gets or sets the catalog categories.
        /// </summary>
        /// <value>
        /// The catalog categories.
        /// </value>
        public DbSet<CatalogCategory> Categories { get; set; }

        /// <summary>
        /// Gets or sets the item producers.
        /// </summary>
        /// <value>
        /// The catalog item producers.
        /// </value>
        public DbSet<CatalogItemProducer> ItemProducers { get; set; }

        /// <summary>
        /// Gets or sets the item sellers.
        /// </summary>
        /// <value>
        /// The item sellers.
        /// </value>
        public DbSet<CatalogItemProducer> ItemSellers { get; set; }

        /// <summary>
        /// Called when [model is creating].
        /// </summary>
        /// <param name="builder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new CatalogCategoryItemPropertyConfiguration());
            builder.ApplyConfiguration(new CatalogItemPropertyValueConfiguration());
        }
    }
}
