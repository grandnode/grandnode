using System;
using System.Collections.Generic;

namespace Grand.Domain.Vendors
{
    /// <summary>
    /// Represents a vendor review
    /// </summary>
    public partial class VendorReview : BaseEntity
    {
        private ICollection<VendorReviewHelpfulness> _vendorReviewHelpfulnessEntries;

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the vendor identifier
        /// </summary>
        public string VendorId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the content is approved
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets the title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the review text
        /// </summary>
        public string ReviewText { get; set; }

        /// <summary>
        /// Review rating
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// Review helpful votes total
        /// </summary>
        public int HelpfulYesTotal { get; set; }

        /// <summary>
        /// Review not helpful votes total
        /// </summary>
        public int HelpfulNoTotal { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets the entries of vendor review helpfulness
        /// </summary>
        public virtual ICollection<VendorReviewHelpfulness> VendorReviewHelpfulnessEntries
        {
            get { return _vendorReviewHelpfulnessEntries ?? (_vendorReviewHelpfulnessEntries = new List<VendorReviewHelpfulness>()); }
            protected set { _vendorReviewHelpfulnessEntries = value; }
        }
    }
}
