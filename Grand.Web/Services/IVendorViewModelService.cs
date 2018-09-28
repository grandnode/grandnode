using Grand.Core.Domain.Vendors;
using Grand.Web.Models.Vendors;

namespace Grand.Web.Services
{
    public partial interface IVendorViewModelService
    {
        VendorReviewOverviewModel PrepareVendorReviewOverviewModel(Vendor vendor);

        void PrepareVendorReviewsModel(VendorReviewsModel model, Vendor vendor);

        VendorReview InsertVendorReview(Vendor vendor, VendorReviewsModel model);
    }
}