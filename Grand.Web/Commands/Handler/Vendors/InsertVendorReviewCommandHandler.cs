using Grand.Core;
using Grand.Domain.Localization;
using Grand.Domain.Vendors;
using Grand.Services.Customers;
using Grand.Services.Messages;
using Grand.Services.Vendors;
using Grand.Web.Commands.Models.Vendors;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Vendors
{
    public class InsertVendorReviewCommandHandler : IRequestHandler<InsertVendorReviewCommand, VendorReview>
    {
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IWorkflowMessageService _workflowMessageService;

        private readonly LocalizationSettings _localizationSettings;
        private readonly VendorSettings _vendorSettings;


        public InsertVendorReviewCommandHandler(IVendorService vendorService, IWorkContext workContext,
            ICustomerService customerService, IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings, VendorSettings vendorSettings)
        {
            _vendorService = vendorService;
            _workContext = workContext;
            _customerService = customerService;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _vendorSettings = vendorSettings;
        }

        public async Task<VendorReview> Handle(InsertVendorReviewCommand request, CancellationToken cancellationToken)
        {
            //save review
            int rating = request.Model.AddVendorReview.Rating;
            if (rating < 1 || rating > 5)
                rating = _vendorSettings.DefaultVendorRatingValue;
            bool isApproved = !_vendorSettings.VendorReviewsMustBeApproved;

            var vendorReview = new VendorReview {
                VendorId = request.Vendor.Id,
                CustomerId = _workContext.CurrentCustomer.Id,
                Title = request.Model.AddVendorReview.Title,
                ReviewText = request.Model.AddVendorReview.ReviewText,
                Rating = rating,
                HelpfulYesTotal = 0,
                HelpfulNoTotal = 0,
                IsApproved = isApproved,
                CreatedOnUtc = DateTime.UtcNow,
            };
            await _vendorService.InsertVendorReview(vendorReview);

            if (!_workContext.CurrentCustomer.HasContributions)
            {
                await _customerService.UpdateContributions(_workContext.CurrentCustomer);
            }

            //update vendor totals
            await _vendorService.UpdateVendorReviewTotals(request.Vendor);

            //notify store owner
            if (_vendorSettings.NotifyVendorAboutNewVendorReviews)
                await _workflowMessageService.SendVendorReviewNotificationMessage(vendorReview, request.Store, _localizationSettings.DefaultAdminLanguageId);

            return vendorReview;
        }
    }
}
