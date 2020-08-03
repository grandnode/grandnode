namespace Grand.Domain.Catalog
{
    /// <summary>
    /// Represents a cross-sell product
    /// </summary>
    public partial class CrossSellProduct : BaseEntity
    {
        /// <summary>
        /// Gets or sets the first product identifier
        /// </summary>
        public string ProductId1 { get; set; }

        /// <summary>
        /// Gets or sets the second product identifier
        /// </summary>
        public string ProductId2 { get; set; }
    }

}
