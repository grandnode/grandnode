using Grand.Domain.Stores;
using Grand.Domain.Vendors;
using Grand.Web.Models.Vendors;
using MediatR;

namespace Grand.Web.Commands.Models.Vendors
{
    public class InsertVendorReviewCommand : IRequest<VendorReview>
    {
        public Vendor Vendor { get; set; }
        public Store Store { get; set; }
        public VendorReviewsModel Model { get; set; }
    }
}
