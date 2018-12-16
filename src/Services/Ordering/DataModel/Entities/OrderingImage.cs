using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Marketplace.Services.Ordering.DataModel.Entities.Base;

namespace Marketplace.Services.Ordering.DataModel.Entities
{
    /// <summary>
    /// Ordering image class
    /// </summary>
    /// <seealso cref="OrderingEntity" />
    [Table("Images")]
    internal class OrderingImage : OrderingEntity
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
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        [ForeignKey("ImageForeignKey")]
        public IQueryable<OrderingItem> Items { get; set; }
    }
}
