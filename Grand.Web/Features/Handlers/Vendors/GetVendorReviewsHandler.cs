using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Vendors;
using Grand.Framework.Security.Captcha;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Services.Vendors;
using Grand.Web.Features.Models.Vendors;
using Grand.Web.Models.Vendors;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Vendors
{
    public class GetVendorReviewsHandler : IRequestHandler<GetVendorReviews, VendorReviewsModel>
    {
        private readonly IWorkContext _workContext;
        private readonly IVendorService _vendorService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;

        private readonly CustomerSettings _customerSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly CaptchaSettings _captchaSettings;

        public GetVendorReviewsHandler(IWorkContext workContext, IVendorService vendorService, ICustomerService customerService, IDateTimeHelper dateTimeHelper,
            CustomerSettings customerSettings, VendorSettings vendorSettings, CaptchaSettings captchaSettings)
        {
            _workContext = workContext;
            _vendorService = vendorService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;

            _customerSettings = customerSettings;
            _vendorSettings = vendorSettings;
            _captchaSettings = captchaSettings;
        }

        public async Task<VendorReviewsModel> Handle(GetVendorReviews request, CancellationToken cancellationToken)
        {
            if (request.Vendor == null)
                throw new ArgumentNullException("vendor");

            var model = new VendorReviewsModel();
            model.VendorId = request.Vendor.Id;
            model.VendorName = request.Vendor.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
            model.VendorSeName = request.Vendor.GetSeName(_workContext.WorkingLanguage.Id);

            var vendorReviews = await _vendorService.GetAllVendorReviews("", true, null, null, "", request.Vendor.Id);
            foreach (var pr in vendorReviews)
            {
                var customer = await _customerService.GetCustomerById(pr.CustomerId);
                model.Items.Add(new VendorReviewModel {
                    Id = pr.Id,
                    CustomerId = pr.CustomerId,
                    CustomerName = customer.FormatUserName(_customerSettings.CustomerNameFormat),
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !customer.IsGuest(),
                    Title = pr.Title,
                    ReviewText = pr.ReviewText,
                    Rating = pr.Rating,
                    Helpfulness = new VendorReviewHelpfulnessModel {
                        VendorId = request.Vendor.Id,
                        VendorReviewId = pr.Id,
                        HelpfulYesTotal = pr.HelpfulYesTotal,
                        HelpfulNoTotal = pr.HelpfulNoTotal,
                    },
                    WrittenOnStr = _dateTimeHelper.ConvertToUserTime(pr.CreatedOnUtc, DateTimeKind.Utc).ToString("g"),
                });
            }

            model.AddVendorReview.CanCurrentCustomerLeaveReview = _vendorSettings.AllowAnonymousUsersToReviewVendor || !_workContext.CurrentCustomer.IsGuest();
            model.AddVendorReview.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnVendorReviewPage;

            return model;
        }
    }
}
