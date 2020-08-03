namespace Grand.Domain.Vendors
{
    /// <summary>
    /// Represents a vendor review helpfulness
    /// </summary>
    public partial class VendorReviewHelpfulness : SubBaseEntity
    {
        /// <summary>
        /// Gets or sets the vendor review identifier
        /// </summary>
        public string VendorReviewId { get; set; }

        /// <summary>
        /// A value indicating whether a review a helpful
        /// </summary>
        public bool WasHelpful { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public string CustomerId { get; set; }

    }
}
