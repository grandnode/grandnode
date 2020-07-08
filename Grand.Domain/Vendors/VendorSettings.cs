using Grand.Domain.Configuration;
using Grand.Domain.Common;

namespace Grand.Domain.Vendors
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

        #region Address settings

        /// <summary>
        /// Gets or sets a value indicating whether 'Company' is enabled
        /// </summary>
        public bool CompanyEnabled { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether 'Company' is required
        /// </summary>
        public bool CompanyRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Street address' is enabled
        /// </summary>
        public bool StreetAddressEnabled { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether 'Street address' is required
        /// </summary>
        public bool StreetAddressRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Street address 2' is enabled
        /// </summary>
        public bool StreetAddress2Enabled { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether 'Street address 2' is required
        /// </summary>
        public bool StreetAddress2Required { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Zip / postal code' is enabled
        /// </summary>
        public bool ZipPostalCodeEnabled { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether 'Zip / postal code' is required
        /// </summary>
        public bool ZipPostalCodeRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'City' is enabled
        /// </summary>
        public bool CityEnabled { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether 'City' is required
        /// </summary>
        public bool CityRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Country' is enabled
        /// </summary>
        public bool CountryEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'State / province' is enabled
        /// </summary>
        public bool StateProvinceEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Phone number' is enabled
        /// </summary>
        public bool PhoneEnabled { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether 'Phone number' is required
        /// </summary>
        public bool PhoneRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Fax number' is enabled
        /// </summary>
        public bool FaxEnabled { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether 'Fax number' is required
        /// </summary>
        public bool FaxRequired { get; set; }

        public AddressSettings AddressSettings => new AddressSettings()
        {
            CityEnabled = this.CityEnabled,
            CityRequired = this.CityRequired,
            CompanyEnabled = this.CompanyEnabled,
            CompanyRequired = this.CompanyRequired,
            CountryEnabled = this.CountryEnabled,
            FaxEnabled = this.FaxEnabled,
            FaxRequired = this.FaxRequired,
            PhoneEnabled = this.PhoneEnabled,
            PhoneRequired = this.PhoneRequired,
            StateProvinceEnabled = this.StateProvinceEnabled,
            StreetAddress2Enabled = this.StreetAddress2Enabled,
            StreetAddress2Required = this.StreetAddress2Required,
            StreetAddressEnabled = this.StreetAddressEnabled,
            StreetAddressRequired = this.StreetAddressRequired,
            ZipPostalCodeEnabled = this.ZipPostalCodeEnabled,
            ZipPostalCodeRequired = this.ZipPostalCodeRequired,
        };

        #endregion

    }
}
