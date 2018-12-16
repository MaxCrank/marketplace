using Marketplace.Services.ContentDelivery.DataModel.Entities.Base;

namespace Marketplace.Services.ContentDelivery.DataModel.Entities
{
    /// <summary>
    /// Content image class
    /// </summary>
    /// <seealso cref="ContentRecord" />
    internal class Image : ContentRecord
    {
        /// <summary>
        /// Gets or sets the image data.
        /// </summary>
        /// <value>
        /// The image data.
        /// </value>
        public byte[] ImageData { get; set; }
    }
}
