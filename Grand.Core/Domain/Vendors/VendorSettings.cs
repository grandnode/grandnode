using Grand.Core.Configuration;

namespace Grand.Core.Domain.Vendors
{
    /// <summary>
    /// Vendor settings
    /// </summary>
    public class VendorSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the default value to use for Vendor page size options (for new vendors)
        /// </summary>
        public string DefaultVendorPageSizeOptions { get; set; }

        /// <summary>
        /// Gets or sets the value indicating how many vendors to display in vendors block
        /// </summary>
        public int VendorsBlockItemsToDisplay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display vendor name on the product details page
        /// </summary>
        public bool ShowVendorOnProductDetailsPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers can contact vendors
        /// </summary>
        public bool AllowCustomersToContactVendors { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether users can fill a form to become a new vendor
        /// </summary>
        public bool AllowCustomersToApplyForVendorAccount { get; set; }
        /// <summary>
        /// Gets or sets a value that indicates whether it is possible to carry out advanced search in the store by vendor
        /// </summary>
        public bool AllowSearchByVendor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether vendors have to accept terms of service during registration
        /// </summary>
        public bool TermsOfServiceEnabled { get; set; }

        /// <summary>
        /// Get or sets a value indicating whether vendor can edit information about itself (public store)
        /// </summary>
        public bool AllowVendorsToEditInfo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the store owner is notified that the vendor information has been changed
        /// </summary>
        public bool NotifyStoreOwnerAboutVendorInformationChange { get; set; }

        /// <summary>
        /// Gets or sets a value indicating vendor reviews must be approved
        /// </summary>
        public bool VendorReviewsMustBeApproved { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow anonymous users write vendor reviews.
        /// </summary>
        public bool AllowAnonymousUsersToReviewVendor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether vendor can be reviewed only by customer who have already ordered a product from this vendor
        /// </summary>
        public bool VendorReviewPossibleOnlyAfterPurchasing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether notification of a store owner about new vendor reviews is enabled
        /// </summary>
        public bool NotifyVendorAboutNewVendorReviews { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the default rating value of the vendor reviews
        /// </summary>
        public int DefaultVendorRatingValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the default allow customer review value of the vendor
        /// </summary>
        public bool DefaultAllowCustomerReview { get; set; }

    }
}
