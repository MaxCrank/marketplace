using Marketplace.Services.Catalog.DataModel.Entities.Joins;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Services.Catalog.DataModel.Configuration
{
    /// <summary>
    /// <see cref="CatalogItemPropertyValueJoin"/> join configuration.
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.IEntityTypeConfiguration{Marketplace.Services.Catalog.DataModel.Entities.Joins.CatalogItemPropertyValueJoin}" />
    internal class CatalogItemPropertyValueConfiguration : IEntityTypeConfiguration<CatalogItemPropertyValueJoin>
    {
        /// <summary>
        /// Configures the CatalogItemPropertyValueJoin.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<CatalogItemPropertyValueJoin> builder)
        {
            builder.HasKey(t => new { t.ItemId, t.PropertyValueId });

            builder.HasOne(pt => pt.Item)
                .WithMany(p => p.CatalogPropertyValuesItems)
                .HasForeignKey(pt => pt.ItemId);

            builder.HasOne(pt => pt.PropertyValue)
                .WithMany(t => t.CatalogPropertyValuesItems)
                .HasForeignKey(pt => pt.PropertyValueId);
        }
    }
}
