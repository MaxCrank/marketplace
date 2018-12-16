namespace Marketplace.Services.Catalog.DataModel.Entities.Joins
{
    /// <summary>
    /// Join table for catalog items and property values
    /// </summary>
    internal class CatalogItemPropertyValueJoin
    {
        /// <summary>
        /// The item identifier
        /// </summary>
        public long ItemId { get; set; }

        /// <summary>
        /// The item
        /// </summary>
        public CatalogItem Item { get; set; }

        /// <summary>
        /// The property value identifier
        /// </summary>
        public long PropertyValueId { get; set; }

        /// <summary>
        /// The property value
        /// </summary>
        public CatalogItemPropertyValue PropertyValue { get; set; }
    }
}
