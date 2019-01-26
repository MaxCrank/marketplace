// File: CatalogCategoryItemPropertyConfiguration.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using Marketplace.Services.Catalog.DataModel.Entities.Joins;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Services.Catalog.DataModel.Configuration
{
    /// <summary>
    /// <see cref="CatalogCategoryItemPropertyJoin"/> join configuration.
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.IEntityTypeConfiguration{Marketplace.Services.Catalog.DataModel.Entities.Joins.CatalogCategoryItemPropertyJoin}" />
    internal class CatalogCategoryItemPropertyConfiguration : IEntityTypeConfiguration<CatalogCategoryItemPropertyJoin>
    {
        /// <summary>
        /// Configures the CatalogCategoryItemPropertyJoin.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<CatalogCategoryItemPropertyJoin> builder)
        {
            builder.HasKey(t => new { t.CategoryId, t.ItemPropertyId });

            builder.HasOne(pt => pt.Category)
                .WithMany(p => p.CategoriesItemProperties)
                .HasForeignKey(pt => pt.CategoryId);

            builder.HasOne(pt => pt.ItemPoprerty)
                .WithMany(t => t.CategoriesItemProperties)
                .HasForeignKey(pt => pt.ItemPropertyId);
        }
    }
}
