using Grand.Core;
using Grand.Core.Domain.Vendors;
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
    }
}