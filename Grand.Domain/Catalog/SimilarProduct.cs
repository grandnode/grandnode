namespace Grand.Domain.Catalog
{
    /// <summary>
    /// Represents a similar product
    /// </summary>
    public partial class SimilarProduct : SubBaseEntity
    {
        /// <summary>
        /// Gets or sets the first product identifier
        /// </summary>        
        public string ProductId1 { get; set; }

        /// <summary>
        /// Gets or sets the second product identifier
        /// </summary>
        public string ProductId2 { get; set; }
        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }

}
