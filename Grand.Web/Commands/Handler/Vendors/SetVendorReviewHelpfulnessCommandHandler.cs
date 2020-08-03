using Grand.Domain.Vendors;
using Grand.Services.Customers;
using Grand.Services.Vendors;
using Grand.Web.Commands.Models.Vendors;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Vendors
{
    public class SetVendorReviewHelpfulnessCommandHandler : IRequestHandler<SetVendorReviewHelpfulnessCommand, VendorReview>
    {
        private readonly ICustomerService _customerService;
        private readonly IVendorService _vendorService;

        public SetVendorReviewHelpfulnessCommandHandler(ICustomerService customerService, IVendorService vendorService)
        {
            _customerService = customerService;
            _vendorService = vendorService;
        }

        public async Task<VendorReview> Handle(SetVendorReviewHelpfulnessCommand request, CancellationToken cancellationToken)
        {
            //delete previous helpfulness
            var prh = request.Review.VendorReviewHelpfulnessEntries
                .FirstOrDefault(x => x.CustomerId == request.Customer.Id);
            if (prh != null)
            {
                //existing one
                prh.WasHelpful = request.Washelpful;
            }
            else
            {
                //insert new helpfulness
                prh = new VendorReviewHelpfulness {
                    VendorReviewId = request.Review.Id,
                    CustomerId = request.Customer.Id,
                    WasHelpful = request.Washelpful,
                };
                request.Review.VendorReviewHelpfulnessEntries.Add(prh);
                if (!request.Customer.HasContributions)
                {
                    await _customerService.UpdateContributions(request.Customer);
                }
            }

            //new totals
            request.Review.HelpfulYesTotal = request.Review.VendorReviewHelpfulnessEntries.Count(x => x.WasHelpful);
            request.Review.HelpfulNoTotal = request.Review.VendorReviewHelpfulnessEntries.Count(x => !x.WasHelpful);
            await _vendorService.UpdateVendorReview(request.Review);

            return request.Review;
        }
    }
}
