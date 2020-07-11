using Grand.Domain.Vendors;
using Grand.Web.Models.Vendors;
using MediatR;

namespace Grand.Web.Features.Models.Vendors
{
    public class GetVendorReviews : IRequest<VendorReviewsModel>
    {
        public Vendor Vendor { get; set; }
    }
}
