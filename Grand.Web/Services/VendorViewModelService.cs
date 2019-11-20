using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Vendors;
using Grand.Framework.Security.Captcha;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Vendors;
using Grand.Web.Interfaces;
using Grand.Web.Models.Vendors;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class VendorViewModelService : IVendorViewModelService
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICacheManager _cacheManager;
        private readonly IVendorService _vendorService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IServiceProvider _serviceProvider;
        private readonly VendorSettings _vendorSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly LocalizationSettings _localizationSettings;

        public VendorViewModelService(
            IPermissionService permissionService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IVendorService vendorService,
            ICacheManager cacheManager,
            IDateTimeHelper dateTimeHelper,
            IWorkflowMessageService workflowMessageService,
            IServiceProvider serviceProvider,
            VendorSettings vendorSettings,
            CustomerSettings customerSettings,
            CaptchaSettings captchaSettings,
            LocalizationSettings localizationSettings)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _storeContext = storeContext;
            _vendorService = vendorService;
            _cacheManager = cacheManager;
            _vendorService = vendorService;
            _dateTimeHelper = dateTimeHelper;
            _workflowMessageService = workflowMessageService;
            _serviceProvider = serviceProvider;
            _vendorSettings = vendorSettings;
            _customerSettings = customerSettings;
            _captchaSettings = captchaSettings;
            _localizationSettings = localizationSettings;
        }

        public virtual VendorReviewOverviewModel PrepareVendorReviewOverviewModel(Vendor vendor)
        {
            VendorReviewOverviewModel VendorReview = new VendorReviewOverviewModel()
            {
                RatingSum = vendor.ApprovedRatingSum,
                TotalReviews = vendor.ApprovedTotalReviews
            };
            
            if (VendorReview != null)
            {
                VendorReview.VendorId = vendor.Id;
                VendorReview.AllowCustomerReviews = vendor.AllowCustomerReviews;
            }
            return VendorReview;
        }

        public virtual async Task PrepareVendorReviewsModel(VendorReviewsModel model, Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException("vendor");

            if (model == null)
                throw new ArgumentNullException("model");

            model.VendorId = vendor.Id;
            model.VendorName = vendor.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
            model.VendorSeName = vendor.GetSeName(_workContext.WorkingLanguage.Id);

            var vendorReviews = await _vendorService.GetAllVendorReviews("", true, null, null, "", vendor.Id);
            foreach (var pr in vendorReviews)
            {
                var customer = await _serviceProvider.GetRequiredService<ICustomerService>().GetCustomerById(pr.CustomerId);
                model.Items.Add(new VendorReviewModel
                {
                    Id = pr.Id,
                    CustomerId = pr.CustomerId,
                    CustomerName = customer.FormatUserName(_customerSettings.CustomerNameFormat),
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !customer.IsGuest(),
                    Title = pr.Title,
                    ReviewText = pr.ReviewText,
                    Rating = pr.Rating,
                    Helpfulness = new VendorReviewHelpfulnessModel
                    {
                        VendorId = vendor.Id,
                        VendorReviewId = pr.Id,
                        HelpfulYesTotal = pr.HelpfulYesTotal,
                        HelpfulNoTotal = pr.HelpfulNoTotal,
                    },
                    WrittenOnStr = _dateTimeHelper.ConvertToUserTime(pr.CreatedOnUtc, DateTimeKind.Utc).ToString("g"),
                });
            }

            model.AddVendorReview.CanCurrentCustomerLeaveReview = _vendorSettings.AllowAnonymousUsersToReviewVendor || !_workContext.CurrentCustomer.IsGuest();
            model.AddVendorReview.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnVendorReviewPage;
        }

        public virtual async Task<VendorReview> InsertVendorReview(Vendor vendor, VendorReviewsModel model)
        {
            //save review
            int rating = model.AddVendorReview.Rating;
            if (rating < 1 || rating > 5)
                rating = _vendorSettings.DefaultVendorRatingValue;
            bool isApproved = !_vendorSettings.VendorReviewsMustBeApproved;

            var vendorReview = new VendorReview
            {
                VendorId = vendor.Id,
                CustomerId = _workContext.CurrentCustomer.Id,
                Title = model.AddVendorReview.Title,
                ReviewText = model.AddVendorReview.ReviewText,
                Rating = rating,
                HelpfulYesTotal = 0,
                HelpfulNoTotal = 0,
                IsApproved = isApproved,
                CreatedOnUtc = DateTime.UtcNow,
            };
            await _vendorService.InsertVendorReview(vendorReview);

            if (!_workContext.CurrentCustomer.HasContributions)
            {
                await _serviceProvider.GetRequiredService<ICustomerService>().UpdateContributions(_workContext.CurrentCustomer);
            }

            //update vendor totals
            await _vendorService.UpdateVendorReviewTotals(vendor);

            //notify store owner
            if (_vendorSettings.NotifyVendorAboutNewVendorReviews)
                await _workflowMessageService.SendVendorReviewNotificationMessage(vendorReview, _localizationSettings.DefaultAdminLanguageId);

            return vendorReview;
        }
    }
}