using Grand.Core;
using Grand.Core.Domain.Vendors;
using System;
using System.Collections.Generic;

namespace Grand.Services.Vendors
{
    /// <summary>
    /// Vendor service interface
    /// </summary>
    public partial interface IVendorService
    {
        /// <summary>
        /// Gets a vendor by vendor identifier
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>Vendor</returns>
        Vendor GetVendorById(string vendorId);

        /// <summary>
        /// Delete a vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        void DeleteVendor(Vendor vendor);

        /// <summary>
        /// Gets all vendors
        /// </summary>
        /// <param name="name">Vendor name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Vendors</returns>
        IPagedList<Vendor> GetAllVendors(string name = "", 
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        /// <summary>
        /// Inserts a vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        void InsertVendor(Vendor vendor);

        /// <summary>
        /// Updates the vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        void UpdateVendor(Vendor vendor);

        /// <summary>
        /// Gets a vendor note note
        /// </summary>
        /// <param name="vendorId">The vendor identifier</param>
        /// <param name="vendorNoteId">The vendor note identifier</param>
        /// <returns>Vendor note</returns>
        VendorNote GetVendorNoteById(string vendorId,string vendorNoteId);

        /// <summary>
        /// Insert a vendor note
        /// </summary>
        /// <param name="vendorId">The vendor identifier</param>
        /// <param name="vendorNote">The vendor note</param>
        void InsertVendorNote(VendorNote vendorNote);
        /// <summary>
        /// Deletes a vendor note
        /// </summary>
        /// <param name="vendorId">The vendor identifier</param>
        /// <param name="vendorNote">The vendor note</param>
        void DeleteVendorNote(VendorNote vendorNote);

        /// <summary>
        /// Gets a vendor mapping 
        /// </summary>
        /// <param name="discountId">Discount id mapping identifier</param>
        /// <returns>vendor mapping</returns>
        IList<Vendor> GetAllVendorsByDiscount(string discountId);

        #region Vendor reviews

        /// <summary>
        /// Update Vendor review totals
        /// </summary>
        /// <param name="Vendor">Vendor</param>
        void UpdateVendorReviewTotals(Vendor Vendor);

        /// <summary>
        /// Update Vendor review 
        /// </summary>
        /// <param name="Vendor">Vendor</param>
        void UpdateVendorReview(VendorReview Vendorreview);

        /// <summary>
        /// Insert Vendor review 
        /// </summary>
        /// <param name="Vendor">Vendor</param>
        void InsertVendorReview(VendorReview Vendorreview);

        /// <summary>
        /// Gets all vendor reviews
        /// </summary>
        /// <param name="customerId">Customer identifier; 0 to load all records</param>
        /// <param name="approved">A value indicating whether to content is approved; null to load all records</param> 
        /// <param name="fromUtc">Item creation from; null to load all records</param>
        /// <param name="toUtc">Item item creation to; null to load all records</param>
        /// <param name="message">Search title or review text; null to load all records</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <returns>Reviews</returns>
        IPagedList<VendorReview> GetAllVendorReviews(string customerId, bool? approved,
            DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = null, string vendorId = "", int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get rating sum for vendor
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="storeId">Store identifier, 0 to load all records</param> 
        /// <returns>Sum</returns>
        int RatingSumVendor(string vendorId, string storeId);

        /// <summary>
        /// Total reviews for vendor
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="storeId">Store identifier, 0 to load all records</param> 
        /// <returns>Sum</returns>
        int TotalReviewsVendor(string vendorId, string storeId);

        /// <summary>
        /// Gets vendor review
        /// </summary>
        /// <param name="vendorReviewId">Vendor review identifier</param>
        /// <returns>Vendor review</returns>
        VendorReview GetVendorReviewById(string vendorReviewId);

        /// <summary>
        /// Deletes a vendor review
        /// </summary>
        /// <param name="vendorReview">Vendor review</param>
        void DeleteVendorReview(VendorReview vendorReview);

        /// <summary>
        /// Search vendors
        /// </summary>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <returns>Vendors</returns>
        IList<Vendor> SearchVendors(string vendorId = "", string keywords = null);

        #endregion
    }
}