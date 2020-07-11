using Grand.Domain.Vendors;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Vendors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IVendorViewModelService
    {
        Task PrepareDiscountModel(VendorModel model, Vendor vendor, bool excludeProperties);
        Task PrepareVendorReviewModel(VendorReviewModel model,
            VendorReview vendorReview, bool excludeProperties, bool formatReviewText);
        Task PrepareVendorAddressModel(VendorModel model, Vendor vendor);
        Task PrepareStore(VendorModel model);
        Task<IList<VendorModel.AssociatedCustomerInfo>> AssociatedCustomers(string vendorId);
        Task<VendorModel> PrepareVendorModel();
        Task<Vendor> InsertVendorModel(VendorModel model);
        Task<Vendor> UpdateVendorModel(Vendor vendor, VendorModel model);
        Task DeleteVendor(Vendor vendor);
        IList<VendorModel.VendorNote> PrepareVendorNote(Vendor vendor);
        Task<bool> InsertVendorNote(string vendorId, string message);
        Task DeleteVendorNote(string id, string vendorId);
        Task<(IEnumerable<VendorReviewModel> vendorReviewModels, int totalCount)> PrepareVendorReviewModel(VendorReviewListModel model, int pageIndex, int pageSize);
        Task<VendorReview> UpdateVendorReviewModel(VendorReview vendorReview, VendorReviewModel model);
        Task DeleteVendorReview(VendorReview vendorReview);
        Task ApproveVendorReviews(IList<string> selectedIds);
        Task DisapproveVendorReviews(IList<string> selectedIds);

    }
}
