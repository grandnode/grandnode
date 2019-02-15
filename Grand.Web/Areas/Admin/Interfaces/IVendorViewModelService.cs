using Grand.Core.Domain.Vendors;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Vendors;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IVendorViewModelService
    {
        void PrepareDiscountModel(VendorModel model, Vendor vendor, bool excludeProperties);
        void PrepareVendorReviewModel(VendorReviewModel model,
            VendorReview vendorReview, bool excludeProperties, bool formatReviewText);
        void PrepareVendorAddressModel(VendorModel model, Vendor vendor);
        void PrepareStore(VendorModel model);
        IList<VendorModel.AssociatedCustomerInfo> AssociatedCustomers(string vendorId);
        VendorModel PrepareVendorModel();
        Vendor InsertVendorModel(VendorModel model);
        Vendor UpdateVendorModel(Vendor vendor, VendorModel model);
        void DeleteVendor(Vendor vendor);
        IList<VendorModel.VendorNote> PrepareVendorNote(Vendor vendor);
        bool InsertVendorNote(string vendorId, string message);
        void DeleteVendorNote(string id, string vendorId);
        (IEnumerable<VendorReviewModel> vendorReviewModels, int totalCount) PrepareVendorReviewModel(VendorReviewListModel model, int pageIndex, int pageSize);
        VendorReview UpdateVendorReviewModel(VendorReview vendorReview, VendorReviewModel model);
        void DeleteVendorReview(VendorReview vendorReview);
        void ApproveVendorReviews(IList<string> selectedIds);
        void DisapproveVendorReviews(IList<string> selectedIds);

    }
}
