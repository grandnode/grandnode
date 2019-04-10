using Grand.Core.Domain.Vendors;
using Grand.Web.Models.Vendors;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface IVendorViewModelService
    {
        VendorReviewOverviewModel PrepareVendorReviewOverviewModel(Vendor vendor);

        Task PrepareVendorReviewsModel(VendorReviewsModel model, Vendor vendor);

        Task<VendorReview> InsertVendorReview(Vendor vendor, VendorReviewsModel model);
    }
}