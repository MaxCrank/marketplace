namespace Marketplace.Services.Catalog.DataModel.Entities.Joins
{
    /// <summary>
    /// Join table for catalog categories and item properties
    /// </summary>
    internal class CatalogCategoryItemPropertyJoin
    {
        /// <summary>
        /// The category identifier
        /// </summary>
        public long CategoryId { get; set; }

        /// <summary>
        /// The category
        /// </summary>
        public CatalogCategory Category { get; set; }

        /// <summary>
        /// The item property identifier
        /// </summary>
        public long ItemPropertyId { get; set; }

        /// <summary>
        /// The item poprerty
        /// </summary>
        public CatalogItemProperty ItemPoprerty { get; set; }
    }
}
