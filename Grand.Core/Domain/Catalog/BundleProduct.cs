namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a bundle product
    /// </summary>
    public partial class BundleProduct : SubBaseEntity
    {
        /// <summary>
        /// Gets or sets the product bundle identifier
        /// </summary>
        public string ProductBundleId { get; set; }

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }

}
