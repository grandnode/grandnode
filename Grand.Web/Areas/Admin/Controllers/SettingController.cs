using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Models.Settings;
using Grand.Core;
using Grand.Core.Domain;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Security;
using Grand.Core.Domain.Seo;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Tax;
using Grand.Core.Domain.Vendors;
using Grand.Services.Common;
using Grand.Services.Configuration;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Localization;
using Grand.Framework.Mvc;
using Grand.Framework.Security;
using Grand.Framework.Security.Captcha;
using Grand.Framework.Themes;
using Grand.Core.Data;
using MongoDB.Driver;
using Grand.Core.Caching;
using Grand.Web.Areas.Admin.Helpers;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Domain.PushNotifications;
using Grand.Web.Areas.Admin.Models.PushNotifications;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class SettingController : BaseAdminController
	{
		#region Fields

        private readonly ISettingService _settingService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressService _addressService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ICurrencyService _currencyService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IOrderService _orderService;
        private readonly IEncryptionService _encryptionService;
        private readonly IThemeProvider _themeProvider;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IRepository<Product> _productRepository;
        private readonly IReturnRequestService _returnRequestService;
        private readonly ILanguageService _languageService;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Constructors

        public SettingController(ISettingService settingService,
            ICountryService countryService, 
            IStateProvinceService stateProvinceService,
            IAddressService addressService, 
            ITaxCategoryService taxCategoryService,
            ICurrencyService currencyService,
            IPictureService pictureService, 
            ILocalizationService localizationService, 
            IDateTimeHelper dateTimeHelper,
            IOrderService orderService,
            IEncryptionService encryptionService,
            IThemeProvider themeProvider,
            ICustomerService customerService, 
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
            IStoreService storeService,
            IWorkContext workContext, 
            IGenericAttributeService genericAttributeService,
            IRepository<Product> productRepository,
            IReturnRequestService returnRequestService,
            ILanguageService languageService,
            ICacheManager cacheManager)
        {
            this._settingService = settingService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._addressService = addressService;
            this._taxCategoryService = taxCategoryService;
            this._currencyService = currencyService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._orderService = orderService;
            this._encryptionService = encryptionService;
            this._themeProvider = themeProvider;
            this._customerService = customerService;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._storeService = storeService;
            this._workContext = workContext;
            this._genericAttributeService = genericAttributeService;
            this._productRepository = productRepository;
            this._returnRequestService = returnRequestService;
            this._languageService = languageService;
            this._cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(ReturnRequestReason rrr, ReturnRequestReasonModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();
            foreach (var local in model.Locales)
            {

                localized.Add(new LocalizedProperty()
                {
                    LanguageId = local.LanguageId,
                    LocaleKey = "Name",
                    LocaleValue = local.Name
                });
            }
            return localized;
        }

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(ReturnRequestAction rrr, ReturnRequestActionModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();
            foreach (var local in model.Locales)
            {

                localized.Add(new LocalizedProperty()
                {
                    LanguageId = local.LanguageId,
                    LocaleKey = "Name",
                    LocaleValue = local.Name
                });
            }
            return localized;
        }

        public IActionResult ChangeStoreScopeConfiguration(string storeid, string returnUrl = "")
        {
            if (storeid != null)
                storeid = storeid.Trim();

            var store = _storeService.GetStoreById(storeid);
            if (store != null || storeid == "")
            {
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration, storeid);
            }
            else
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration, "");


            //home page
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = Url.Action("Index", "Home", new { area = "Admin" });
            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            return Redirect(returnUrl);
        }

        public IActionResult Blog()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var blogSettings = _settingService.LoadSetting<BlogSettings>(storeScope);
            var model = blogSettings.ToModel();
            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.Enabled_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.Enabled, storeScope);
                model.PostsPageSize_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.PostsPageSize, storeScope);
                model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.AllowNotRegisteredUsersToLeaveComments, storeScope);
                model.NotifyAboutNewBlogComments_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.NotifyAboutNewBlogComments, storeScope);
                model.NumberOfTags_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.NumberOfTags, storeScope);
                model.ShowHeaderRssUrl_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.ShowHeaderRssUrl, storeScope);
                model.ShowBlogOnHomePage_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.ShowBlogOnHomePage, storeScope);
                model.HomePageBlogCount_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.HomePageBlogCount, storeScope);
                model.MaxTextSizeHomePage_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.MaxTextSizeHomePage, storeScope);
            }

            return View(model);
        }
        [HttpPost]
        public IActionResult Blog(BlogSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var blogSettings = _settingService.LoadSetting<BlogSettings>(storeScope);
            blogSettings = model.ToEntity(blogSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.Enabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(blogSettings, x => x.Enabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(blogSettings, x => x.Enabled, storeScope);
            
            if (model.PostsPageSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(blogSettings, x => x.PostsPageSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(blogSettings, x => x.PostsPageSize, storeScope);
            
            if (model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(blogSettings, x => x.AllowNotRegisteredUsersToLeaveComments, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(blogSettings, x => x.AllowNotRegisteredUsersToLeaveComments, storeScope);
            
            if (model.NotifyAboutNewBlogComments_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(blogSettings, x => x.NotifyAboutNewBlogComments, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(blogSettings, x => x.NotifyAboutNewBlogComments, storeScope);
            
            if (model.NumberOfTags_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(blogSettings, x => x.NumberOfTags, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(blogSettings, x => x.NumberOfTags, storeScope);
            
            if (model.ShowHeaderRssUrl_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(blogSettings, x => x.ShowHeaderRssUrl, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(blogSettings, x => x.ShowHeaderRssUrl, storeScope);

            if (model.ShowBlogOnHomePage_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(blogSettings, x => x.ShowBlogOnHomePage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(blogSettings, x => x.ShowBlogOnHomePage, storeScope);

            if (model.HomePageBlogCount_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(blogSettings, x => x.HomePageBlogCount, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(blogSettings, x => x.HomePageBlogCount, storeScope);

            if (model.MaxTextSizeHomePage_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(blogSettings, x => x.MaxTextSizeHomePage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(blogSettings, x => x.MaxTextSizeHomePage, storeScope);

            //now clear cache
            _cacheManager.Clear();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Blog");
        }




        public IActionResult Vendor()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var vendorSettings = _settingService.LoadSetting<VendorSettings>(storeScope);
            var model = vendorSettings.ToModel();
            model.AddressSettings.CityEnabled = vendorSettings.CityEnabled;
            model.AddressSettings.CityRequired = vendorSettings.CityRequired;
            model.AddressSettings.CompanyEnabled = vendorSettings.CompanyEnabled;
            model.AddressSettings.CompanyRequired = vendorSettings.CompanyRequired;
            model.AddressSettings.CountryEnabled = vendorSettings.CountryEnabled;
            model.AddressSettings.FaxEnabled = vendorSettings.FaxEnabled;
            model.AddressSettings.FaxRequired = vendorSettings.FaxRequired;
            model.AddressSettings.PhoneEnabled = vendorSettings.PhoneEnabled;
            model.AddressSettings.PhoneRequired = vendorSettings.PhoneRequired;
            model.AddressSettings.StateProvinceEnabled = vendorSettings.StateProvinceEnabled;
            model.AddressSettings.StreetAddress2Enabled = vendorSettings.StreetAddress2Enabled;
            model.AddressSettings.StreetAddress2Required = vendorSettings.StreetAddress2Required;
            model.AddressSettings.StreetAddressEnabled = vendorSettings.StreetAddressEnabled;
            model.AddressSettings.StreetAddressRequired = vendorSettings.StreetAddressRequired;
            model.AddressSettings.ZipPostalCodeEnabled = vendorSettings.ZipPostalCodeEnabled;
            model.AddressSettings.ZipPostalCodeRequired = vendorSettings.ZipPostalCodeRequired;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.VendorsBlockItemsToDisplay_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.VendorsBlockItemsToDisplay, storeScope);
                model.ShowVendorOnProductDetailsPage_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.ShowVendorOnProductDetailsPage, storeScope);
                model.AllowCustomersToContactVendors_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.AllowCustomersToContactVendors, storeScope);
                model.AllowCustomersToApplyForVendorAccount_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.AllowCustomersToApplyForVendorAccount, storeScope);
                model.AllowSearchByVendor_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.AllowSearchByVendor, storeScope);

                model.AllowVendorsToEditInfo_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.AllowVendorsToEditInfo, storeScope);
                model.NotifyStoreOwnerAboutVendorInformationChange_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.NotifyStoreOwnerAboutVendorInformationChange, storeScope);
                model.TermsOfServiceEnabled_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.TermsOfServiceEnabled, storeScope);

                //vendor review tab
                model.VendorReviewsMustBeApproved_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.VendorReviewsMustBeApproved, storeScope);
                model.AllowAnonymousUsersToReviewVendor_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.AllowAnonymousUsersToReviewVendor, storeScope);
                model.VendorReviewPossibleOnlyAfterPurchasing_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.VendorReviewPossibleOnlyAfterPurchasing, storeScope);
                model.NotifyVendorAboutNewVendorReviews_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.NotifyVendorAboutNewVendorReviews, storeScope);
                
            }

            return View(model);
        }
        [HttpPost]
        public IActionResult Vendor(VendorSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var vendorSettings = _settingService.LoadSetting<VendorSettings>(storeScope);
            vendorSettings = model.ToEntity(vendorSettings);
            vendorSettings.CityEnabled = model.AddressSettings.CityEnabled;
            vendorSettings.CityRequired = model.AddressSettings.CityRequired;
            vendorSettings.CompanyEnabled = model.AddressSettings.CompanyEnabled;
            vendorSettings.CompanyRequired = model.AddressSettings.CompanyRequired;
            vendorSettings.CountryEnabled = model.AddressSettings.CountryEnabled;
            vendorSettings.FaxEnabled = model.AddressSettings.FaxEnabled;
            vendorSettings.FaxRequired = model.AddressSettings.FaxRequired;
            vendorSettings.PhoneEnabled = model.AddressSettings.PhoneEnabled;
            vendorSettings.PhoneRequired = model.AddressSettings.PhoneRequired;
            vendorSettings.StateProvinceEnabled = model.AddressSettings.StateProvinceEnabled;
            vendorSettings.StreetAddress2Enabled = model.AddressSettings.StreetAddress2Enabled;
            vendorSettings.StreetAddress2Required = model.AddressSettings.StreetAddress2Required;
            vendorSettings.StreetAddressEnabled = model.AddressSettings.StreetAddressEnabled;
            vendorSettings.StreetAddressRequired = model.AddressSettings.StreetAddressRequired;
            vendorSettings.ZipPostalCodeEnabled = model.AddressSettings.ZipPostalCodeEnabled;
            vendorSettings.ZipPostalCodeRequired = model.AddressSettings.ZipPostalCodeRequired;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            if (model.VendorsBlockItemsToDisplay_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(vendorSettings, x => x.VendorsBlockItemsToDisplay, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(vendorSettings, x => x.VendorsBlockItemsToDisplay, storeScope);

            if (model.ShowVendorOnProductDetailsPage_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(vendorSettings, x => x.ShowVendorOnProductDetailsPage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(vendorSettings, x => x.ShowVendorOnProductDetailsPage, storeScope);

            if (model.AllowCustomersToContactVendors_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(vendorSettings, x => x.AllowCustomersToContactVendors, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(vendorSettings, x => x.AllowCustomersToContactVendors, storeScope);

            if (model.AllowCustomersToApplyForVendorAccount_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(vendorSettings, x => x.AllowCustomersToApplyForVendorAccount, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(vendorSettings, x => x.AllowCustomersToApplyForVendorAccount, storeScope);

            if (model.AllowSearchByVendor_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(vendorSettings, x => x.AllowSearchByVendor, storeScope);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(vendorSettings, x => x.AllowSearchByVendor, storeScope);

            if (model.AllowVendorsToEditInfo_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(vendorSettings, x => x.AllowVendorsToEditInfo, storeScope);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(vendorSettings, x => x.AllowVendorsToEditInfo, storeScope);

            if (model.NotifyStoreOwnerAboutVendorInformationChange_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(vendorSettings, x => x.NotifyStoreOwnerAboutVendorInformationChange, storeScope);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(vendorSettings, x => x.NotifyStoreOwnerAboutVendorInformationChange, storeScope);

            if (model.TermsOfServiceEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(vendorSettings, x => x.TermsOfServiceEnabled, storeScope);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(vendorSettings, x => x.TermsOfServiceEnabled, storeScope);


            //review vendor
            if (model.VendorReviewsMustBeApproved_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(vendorSettings, x => x.VendorReviewsMustBeApproved, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(vendorSettings, x => x.VendorReviewsMustBeApproved, storeScope);

            if (model.AllowAnonymousUsersToReviewVendor_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(vendorSettings, x => x.AllowAnonymousUsersToReviewVendor, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(vendorSettings, x => x.AllowAnonymousUsersToReviewVendor, storeScope);
            
            if (model.VendorReviewPossibleOnlyAfterPurchasing_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(vendorSettings, x => x.VendorReviewPossibleOnlyAfterPurchasing, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(vendorSettings, x => x.VendorReviewPossibleOnlyAfterPurchasing, storeScope);
            
            if (model.NotifyVendorAboutNewVendorReviews_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(vendorSettings, x => x.NotifyVendorAboutNewVendorReviews, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(vendorSettings, x => x.NotifyVendorAboutNewVendorReviews, storeScope);

            _settingService.SaveSetting(vendorSettings);

            //now clear cache
            _cacheManager.Clear();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Vendor");
        }




        public IActionResult Forum()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var forumSettings = _settingService.LoadSetting<ForumSettings>(storeScope);
            var model = forumSettings.ToModel();
            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.ForumsEnabled_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ForumsEnabled, storeScope);
                model.RelativeDateTimeFormattingEnabled_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.RelativeDateTimeFormattingEnabled, storeScope);
                model.ShowCustomersPostCount_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ShowCustomersPostCount, storeScope);
                model.AllowGuestsToCreatePosts_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.AllowGuestsToCreatePosts, storeScope);
                model.AllowGuestsToCreateTopics_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.AllowGuestsToCreateTopics, storeScope);
                model.AllowCustomersToEditPosts_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.AllowCustomersToEditPosts, storeScope);
                model.AllowCustomersToDeletePosts_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.AllowCustomersToDeletePosts, storeScope);
                model.AllowCustomersToManageSubscriptions_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.AllowCustomersToManageSubscriptions, storeScope);
                model.TopicsPageSize_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.TopicsPageSize, storeScope);
                model.PostsPageSize_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.PostsPageSize, storeScope);
                model.ForumEditor_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ForumEditor, storeScope);
                model.SignaturesEnabled_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.SignaturesEnabled, storeScope);
                model.AllowPrivateMessages_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.AllowPrivateMessages, storeScope);
                model.ShowAlertForPM_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ShowAlertForPM, storeScope);
                model.NotifyAboutPrivateMessages_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.NotifyAboutPrivateMessages, storeScope);
                model.ActiveDiscussionsFeedEnabled_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ActiveDiscussionsFeedEnabled, storeScope);
                model.ActiveDiscussionsFeedCount_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ActiveDiscussionsFeedCount, storeScope);
                model.ForumFeedsEnabled_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ForumFeedsEnabled, storeScope);
                model.ForumFeedCount_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ForumFeedCount, storeScope);
                model.SearchResultsPageSize_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.SearchResultsPageSize, storeScope);
                model.ActiveDiscussionsPageSize_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ActiveDiscussionsPageSize, storeScope);
                model.AllowPostVoting_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.AllowPostVoting, storeScope);
                model.MaxVotesPerDay_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.MaxVotesPerDay, storeScope);
            }
            model.ForumEditorValues = forumSettings.ForumEditor.ToSelectList();

            return View(model);
        }
        [HttpPost]
        public IActionResult Forum(ForumSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();


            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var forumSettings = _settingService.LoadSetting<ForumSettings>(storeScope);
            forumSettings = model.ToEntity(forumSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.ForumsEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.ForumsEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.ForumsEnabled, storeScope);
            
            if (model.RelativeDateTimeFormattingEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.RelativeDateTimeFormattingEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.RelativeDateTimeFormattingEnabled, storeScope);
            
            if (model.ShowCustomersPostCount_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.ShowCustomersPostCount, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.ShowCustomersPostCount, storeScope);
            
            if (model.AllowGuestsToCreatePosts_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.AllowGuestsToCreatePosts, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.AllowGuestsToCreatePosts, storeScope);
            
            if (model.AllowGuestsToCreateTopics_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.AllowGuestsToCreateTopics, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.AllowGuestsToCreateTopics, storeScope);
            
            if (model.AllowCustomersToEditPosts_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.AllowCustomersToEditPosts, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.AllowCustomersToEditPosts, storeScope);
            
            if (model.AllowCustomersToDeletePosts_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.AllowCustomersToDeletePosts, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.AllowCustomersToDeletePosts, storeScope);
            
            if (model.AllowCustomersToManageSubscriptions_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.AllowCustomersToManageSubscriptions, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.AllowCustomersToManageSubscriptions, storeScope);
            
            if (model.TopicsPageSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.TopicsPageSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.TopicsPageSize, storeScope);
            
            if (model.PostsPageSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.PostsPageSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.PostsPageSize, storeScope);
            
            if (model.ForumEditor_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.ForumEditor, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.ForumEditor, storeScope);
            
            if (model.SignaturesEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.SignaturesEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.SignaturesEnabled, storeScope);
            
            if (model.AllowPrivateMessages_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.AllowPrivateMessages, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.AllowPrivateMessages, storeScope);
            
            if (model.ShowAlertForPM_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.ShowAlertForPM, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.ShowAlertForPM, storeScope);
            
            if (model.NotifyAboutPrivateMessages_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.NotifyAboutPrivateMessages, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.NotifyAboutPrivateMessages, storeScope);
            
            if (model.ActiveDiscussionsFeedEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.ActiveDiscussionsFeedEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.ActiveDiscussionsFeedEnabled, storeScope);
            
            if (model.ActiveDiscussionsFeedCount_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.ActiveDiscussionsFeedCount, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.ActiveDiscussionsFeedCount, storeScope);
            
            if (model.ForumFeedsEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.ForumFeedsEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.ForumFeedsEnabled, storeScope);
            
            if (model.ForumFeedCount_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.ForumFeedCount, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.ForumFeedCount, storeScope);
            
            if (model.SearchResultsPageSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.SearchResultsPageSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.SearchResultsPageSize, storeScope);

            if (model.ActiveDiscussionsPageSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.ActiveDiscussionsPageSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.ActiveDiscussionsPageSize, storeScope);


            if (model.AllowPostVoting_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.AllowPostVoting, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.AllowPostVoting, storeScope);
            
            if (model.MaxVotesPerDay_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(forumSettings, x => x.MaxVotesPerDay, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(forumSettings, x => x.MaxVotesPerDay, storeScope);

            //now clear cache
            _cacheManager.Clear();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Forum");
        }




        public IActionResult News()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var newsSettings = _settingService.LoadSetting<NewsSettings>(storeScope);
            var model = newsSettings.ToModel();
            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.Enabled_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.Enabled, storeScope);
                model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.AllowNotRegisteredUsersToLeaveComments, storeScope);
                model.NotifyAboutNewNewsComments_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.NotifyAboutNewNewsComments, storeScope);
                model.ShowNewsOnMainPage_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.ShowNewsOnMainPage, storeScope);
                model.MainPageNewsCount_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.MainPageNewsCount, storeScope);
                model.NewsArchivePageSize_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.NewsArchivePageSize, storeScope);
                model.ShowHeaderRssUrl_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.ShowHeaderRssUrl, storeScope);
            }
            return View(model);
        }
        [HttpPost]
        public IActionResult News(NewsSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var newsSettings = _settingService.LoadSetting<NewsSettings>(storeScope);
            newsSettings = model.ToEntity(newsSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.Enabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(newsSettings, x => x.Enabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(newsSettings, x => x.Enabled, storeScope);

            if (model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(newsSettings, x => x.AllowNotRegisteredUsersToLeaveComments, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(newsSettings, x => x.AllowNotRegisteredUsersToLeaveComments, storeScope);

            if (model.NotifyAboutNewNewsComments_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(newsSettings, x => x.NotifyAboutNewNewsComments, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(newsSettings, x => x.NotifyAboutNewNewsComments, storeScope);

            if (model.ShowNewsOnMainPage_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(newsSettings, x => x.ShowNewsOnMainPage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(newsSettings, x => x.ShowNewsOnMainPage, storeScope);

            if (model.MainPageNewsCount_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(newsSettings, x => x.MainPageNewsCount, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(newsSettings, x => x.MainPageNewsCount, storeScope);

            if (model.NewsArchivePageSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(newsSettings, x => x.NewsArchivePageSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(newsSettings, x => x.NewsArchivePageSize, storeScope);

            if (model.ShowHeaderRssUrl_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(newsSettings, x => x.ShowHeaderRssUrl, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(newsSettings, x => x.ShowHeaderRssUrl, storeScope);

            //now clear cache
            _cacheManager.Clear();
            

            //activity log
            _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("News");
        }




        public IActionResult Shipping()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var shippingSettings = _settingService.LoadSetting<ShippingSettings>(storeScope);
            var model = shippingSettings.ToModel();
            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.ShipToSameAddress_OverrideForStore = _settingService.SettingExists(shippingSettings, x => x.ShipToSameAddress, storeScope);
                model.AllowPickUpInStore_OverrideForStore = _settingService.SettingExists(shippingSettings, x => x.AllowPickUpInStore, storeScope);                
                model.UseWarehouseLocation_OverrideForStore = _settingService.SettingExists(shippingSettings, x => x.UseWarehouseLocation, storeScope);
                model.NotifyCustomerAboutShippingFromMultipleLocations_OverrideForStore = _settingService.SettingExists(shippingSettings, x => x.NotifyCustomerAboutShippingFromMultipleLocations, storeScope);
                model.FreeShippingOverXEnabled_OverrideForStore = _settingService.SettingExists(shippingSettings, x => x.FreeShippingOverXEnabled, storeScope);
                model.FreeShippingOverXValue_OverrideForStore = _settingService.SettingExists(shippingSettings, x => x.FreeShippingOverXValue, storeScope);
                model.FreeShippingOverXIncludingTax_OverrideForStore = _settingService.SettingExists(shippingSettings, x => x.FreeShippingOverXIncludingTax, storeScope);
                model.EstimateShippingEnabled_OverrideForStore = _settingService.SettingExists(shippingSettings, x => x.EstimateShippingEnabled, storeScope);
                model.DisplayShipmentEventsToCustomers_OverrideForStore = _settingService.SettingExists(shippingSettings, x => x.DisplayShipmentEventsToCustomers, storeScope);
                model.DisplayShipmentEventsToStoreOwner_OverrideForStore = _settingService.SettingExists(shippingSettings, x => x.DisplayShipmentEventsToStoreOwner, storeScope);
                model.BypassShippingMethodSelectionIfOnlyOne_OverrideForStore = _settingService.SettingExists(shippingSettings, x => x.BypassShippingMethodSelectionIfOnlyOne, storeScope);
                model.ShippingOriginAddress_OverrideForStore = _settingService.SettingExists(shippingSettings, x => x.ShippingOriginAddressId, storeScope);
            }
            //shipping origin
            var originAddress = !String.IsNullOrEmpty(shippingSettings.ShippingOriginAddressId)
                                     ? _addressService.GetAddressByIdSettings(shippingSettings.ShippingOriginAddressId)
                                     : null;
            if (originAddress != null)
                model.ShippingOriginAddress = originAddress.ToModel();
            else
                model.ShippingOriginAddress = new AddressModel();

            model.ShippingOriginAddress.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
            foreach (var c in _countryService.GetAllCountries(showHidden:true))
                model.ShippingOriginAddress.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (originAddress != null && c.Id == originAddress.CountryId) });

            var states = originAddress != null && !String.IsNullOrEmpty(originAddress.CountryId) ? _stateProvinceService.GetStateProvincesByCountryId(originAddress.CountryId, showHidden: true).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.ShippingOriginAddress.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == originAddress.StateProvinceId) });
            }
            else
                model.ShippingOriginAddress.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "" });
            model.ShippingOriginAddress.CountryEnabled = true;
            model.ShippingOriginAddress.StateProvinceEnabled = true;
            model.ShippingOriginAddress.CityEnabled = true;
            model.ShippingOriginAddress.StreetAddressEnabled = true;
            model.ShippingOriginAddress.ZipPostalCodeEnabled = true;
            model.ShippingOriginAddress.ZipPostalCodeRequired = true;
            
            return View(model);
        }
        [HttpPost]
        public IActionResult Shipping(ShippingSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();


            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var shippingSettings = _settingService.LoadSetting<ShippingSettings>(storeScope);
            shippingSettings = model.ToEntity(shippingSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.ShipToSameAddress_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shippingSettings, x => x.ShipToSameAddress, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shippingSettings, x => x.ShipToSameAddress, storeScope);

            if (model.AllowPickUpInStore_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shippingSettings, x => x.AllowPickUpInStore, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shippingSettings, x => x.AllowPickUpInStore, storeScope);

            if (model.UseWarehouseLocation_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shippingSettings, x => x.UseWarehouseLocation, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shippingSettings, x => x.UseWarehouseLocation, storeScope);

            if (model.NotifyCustomerAboutShippingFromMultipleLocations_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shippingSettings, x => x.NotifyCustomerAboutShippingFromMultipleLocations, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shippingSettings, x => x.NotifyCustomerAboutShippingFromMultipleLocations, storeScope);

            if (model.FreeShippingOverXEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shippingSettings, x => x.FreeShippingOverXEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shippingSettings, x => x.FreeShippingOverXEnabled, storeScope);

            if (model.FreeShippingOverXValue_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shippingSettings, x => x.FreeShippingOverXValue, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shippingSettings, x => x.FreeShippingOverXValue, storeScope);

            if (model.FreeShippingOverXIncludingTax_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shippingSettings, x => x.FreeShippingOverXIncludingTax, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shippingSettings, x => x.FreeShippingOverXIncludingTax, storeScope);

            if (model.EstimateShippingEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shippingSettings, x => x.EstimateShippingEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shippingSettings, x => x.EstimateShippingEnabled, storeScope);

            if (model.DisplayShipmentEventsToCustomers_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shippingSettings, x => x.DisplayShipmentEventsToCustomers, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shippingSettings, x => x.DisplayShipmentEventsToCustomers, storeScope);

            if (model.DisplayShipmentEventsToStoreOwner_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shippingSettings, x => x.DisplayShipmentEventsToStoreOwner, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shippingSettings, x => x.DisplayShipmentEventsToStoreOwner, storeScope);

            if (model.BypassShippingMethodSelectionIfOnlyOne_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shippingSettings, x => x.BypassShippingMethodSelectionIfOnlyOne, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shippingSettings, x => x.BypassShippingMethodSelectionIfOnlyOne, storeScope);

            if (model.ShippingOriginAddress_OverrideForStore || storeScope == "")
            {
                //update address
                var addressId = _settingService.SettingExists(shippingSettings, x => x.ShippingOriginAddressId, storeScope) ?
                    shippingSettings.ShippingOriginAddressId : "";
                var originAddress = _addressService.GetAddressByIdSettings(addressId) ??
                    new Address
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                //update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one)
                originAddress = model.ShippingOriginAddress.ToEntity(originAddress);
                if (_addressService.GetAddressByIdSettings(addressId)!=null)
                    _addressService.UpdateAddressSettings(originAddress);
                else
                    _addressService.InsertAddressSettings(originAddress);

                //model.ShippingOriginAddress.Id = addressId;
                shippingSettings.ShippingOriginAddressId = originAddress.Id;

                _settingService.SaveSetting(shippingSettings, x => x.ShippingOriginAddressId, storeScope, false);
            }
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shippingSettings, x => x.ShippingOriginAddressId, storeScope);


            //now clear cache
            _cacheManager.Clear();


            //activity log
            _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Shipping");
        }




        public IActionResult Tax()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();


            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var taxSettings = _settingService.LoadSetting<TaxSettings>(storeScope);
            var model = taxSettings.ToModel();
            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.PricesIncludeTax_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.PricesIncludeTax, storeScope);
                model.AllowCustomersToSelectTaxDisplayType_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.AllowCustomersToSelectTaxDisplayType, storeScope);
                model.TaxDisplayType_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.TaxDisplayType, storeScope);
                model.DisplayTaxSuffix_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.DisplayTaxSuffix, storeScope);
                model.DisplayTaxRates_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.DisplayTaxRates, storeScope);
                model.HideZeroTax_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.HideZeroTax, storeScope);
                model.HideTaxInOrderSummary_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.HideTaxInOrderSummary, storeScope);
                model.ForceTaxExclusionFromOrderSubtotal_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.ForceTaxExclusionFromOrderSubtotal, storeScope);
                model.TaxBasedOn_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.TaxBasedOn, storeScope);
                model.DefaultTaxAddress_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.DefaultTaxAddressId, storeScope);
                model.ShippingIsTaxable_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.ShippingIsTaxable, storeScope);
                model.DefaultTaxCategoryId_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.DefaultTaxCategoryId, storeScope);
                model.ShippingPriceIncludesTax_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.ShippingPriceIncludesTax, storeScope);
                model.ShippingTaxClassId_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.ShippingTaxClassId, storeScope);
                model.PaymentMethodAdditionalFeeIsTaxable_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.PaymentMethodAdditionalFeeIsTaxable, storeScope);
                model.PaymentMethodAdditionalFeeIncludesTax_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.PaymentMethodAdditionalFeeIncludesTax, storeScope);
                model.PaymentMethodAdditionalFeeTaxClassId_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.PaymentMethodAdditionalFeeTaxClassId, storeScope);
                model.EuVatEnabled_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.EuVatEnabled, storeScope);
                model.EuVatShopCountryId_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.EuVatShopCountryId, storeScope);
                model.EuVatAllowVatExemption_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.EuVatAllowVatExemption, storeScope);
                model.EuVatUseWebService_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.EuVatUseWebService, storeScope);
                model.EuVatAssumeValid_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.EuVatAssumeValid, storeScope);
                model.EuVatEmailAdminWhenNewVatSubmitted_OverrideForStore = _settingService.SettingExists(taxSettings, x => x.EuVatEmailAdminWhenNewVatSubmitted, storeScope);
            }

            model.TaxBasedOnValues = taxSettings.TaxBasedOn.ToSelectList();
            model.TaxDisplayTypeValues = taxSettings.TaxDisplayType.ToSelectList();

            //tax categories
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            model.TaxCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Configuration.Settings.Tax.TaxCategories.None"), Value = "" });
            foreach (var tc in taxCategories)
                model.TaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id.ToString(), Selected = tc.Id == taxSettings.ShippingTaxClassId });
            model.PaymentMethodAdditionalFeeTaxCategories.Add(new SelectListItem { Text = "---", Value = "" });
            foreach (var tc in taxCategories)
                model.PaymentMethodAdditionalFeeTaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id.ToString(), Selected = tc.Id == taxSettings.PaymentMethodAdditionalFeeTaxClassId });

            //EU VAT countries
            model.EuVatShopCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
            foreach (var c in _countryService.GetAllCountries(showHidden:true))
                model.EuVatShopCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = c.Id == taxSettings.EuVatShopCountryId });

            //default tax address
            var defaultAddress = !String.IsNullOrEmpty(taxSettings.DefaultTaxAddressId)
                                     ? _addressService.GetAddressByIdSettings(taxSettings.DefaultTaxAddressId)
                                     : null;
            if (defaultAddress != null)
                model.DefaultTaxAddress = defaultAddress.ToModel();
            else
                model.DefaultTaxAddress = new AddressModel();

            model.DefaultTaxAddress.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.DefaultTaxAddress.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (defaultAddress != null && c.Id == defaultAddress.CountryId) });

            var states = defaultAddress != null && !String.IsNullOrEmpty(defaultAddress.CountryId) ? _stateProvinceService.GetStateProvincesByCountryId(defaultAddress.CountryId, showHidden:true).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.DefaultTaxAddress.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == defaultAddress.StateProvinceId) });
            }
            else
                model.DefaultTaxAddress.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "" });
            model.DefaultTaxAddress.CountryEnabled = true;
            model.DefaultTaxAddress.StateProvinceEnabled = true;
            model.DefaultTaxAddress.ZipPostalCodeEnabled = true;
            model.DefaultTaxAddress.ZipPostalCodeRequired = true;

            return View(model);
        }
        [HttpPost]
        public IActionResult Tax(TaxSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();


            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var taxSettings = _settingService.LoadSetting<TaxSettings>(storeScope);
            taxSettings = model.ToEntity(taxSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.PricesIncludeTax_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.PricesIncludeTax, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.PricesIncludeTax, storeScope);
            
            if (model.AllowCustomersToSelectTaxDisplayType_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.AllowCustomersToSelectTaxDisplayType, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.AllowCustomersToSelectTaxDisplayType, storeScope);
            
            if (model.TaxDisplayType_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.TaxDisplayType, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.TaxDisplayType, storeScope);
            
            if (model.DisplayTaxSuffix_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.DisplayTaxSuffix, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.DisplayTaxSuffix, storeScope);
            
            if (model.DisplayTaxRates_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.DisplayTaxRates, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.DisplayTaxRates, storeScope);
            
            if (model.HideZeroTax_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.HideZeroTax, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.HideZeroTax, storeScope);
            
            if (model.HideTaxInOrderSummary_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.HideTaxInOrderSummary, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.HideTaxInOrderSummary, storeScope);

            if (model.ForceTaxExclusionFromOrderSubtotal_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.ForceTaxExclusionFromOrderSubtotal, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.ForceTaxExclusionFromOrderSubtotal, storeScope);
            
            if (model.TaxBasedOn_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.TaxBasedOn, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.TaxBasedOn, storeScope);



            if (model.DefaultTaxAddress_OverrideForStore || storeScope == "")
            {
                //update address
                var addressId = _settingService.SettingExists(taxSettings, x => x.DefaultTaxAddressId, storeScope) ?
                    taxSettings.DefaultTaxAddressId : "";
                var originAddress = _addressService.GetAddressByIdSettings(addressId) ??
                    new Address
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        Id = "",
                    };
                //update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one)
                model.DefaultTaxAddress.Id = addressId;
                originAddress = model.DefaultTaxAddress.ToEntity(originAddress);
                if (!String.IsNullOrEmpty(model.DefaultTaxAddress.Id))
                    _addressService.UpdateAddressSettings(originAddress);
                else
                    _addressService.InsertAddressSettings(originAddress);
                taxSettings.DefaultTaxAddressId = originAddress.Id;

                _settingService.SaveSetting(taxSettings, x => x.DefaultTaxAddressId, storeScope, false);
            }
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.DefaultTaxAddressId, storeScope);




            if (model.ShippingIsTaxable_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.ShippingIsTaxable, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.ShippingIsTaxable, storeScope);

            if (model.DefaultTaxCategoryId_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.DefaultTaxCategoryId, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.DefaultTaxCategoryId, storeScope);

            if (model.ShippingPriceIncludesTax_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.ShippingPriceIncludesTax, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.ShippingPriceIncludesTax, storeScope);
            
            if (model.ShippingTaxClassId_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.ShippingTaxClassId, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.ShippingTaxClassId, storeScope);
            
            if (model.PaymentMethodAdditionalFeeIsTaxable_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.PaymentMethodAdditionalFeeIsTaxable, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.PaymentMethodAdditionalFeeIsTaxable, storeScope);
            
            if (model.PaymentMethodAdditionalFeeIncludesTax_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.PaymentMethodAdditionalFeeIncludesTax, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.PaymentMethodAdditionalFeeIncludesTax, storeScope);
            
            if (model.PaymentMethodAdditionalFeeTaxClassId_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.PaymentMethodAdditionalFeeTaxClassId, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.PaymentMethodAdditionalFeeTaxClassId, storeScope);
            
            if (model.EuVatEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.EuVatEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.EuVatEnabled, storeScope);
            
            if (model.EuVatShopCountryId_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.EuVatShopCountryId, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.EuVatShopCountryId, storeScope);
            
            if (model.EuVatAllowVatExemption_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.EuVatAllowVatExemption, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.EuVatAllowVatExemption, storeScope);

            if (model.EuVatUseWebService_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.EuVatUseWebService, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.EuVatUseWebService, storeScope);

            if (model.EuVatAssumeValid_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.EuVatAssumeValid, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.EuVatAssumeValid, storeScope);

            if (model.EuVatEmailAdminWhenNewVatSubmitted_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(taxSettings, x => x.EuVatEmailAdminWhenNewVatSubmitted, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(taxSettings, x => x.EuVatEmailAdminWhenNewVatSubmitted, storeScope);

            //now clear cache
            _cacheManager.Clear();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Tax");
        }




        public IActionResult Catalog()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();


            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var catalogSettings = _settingService.LoadSetting<CatalogSettings>(storeScope);
            var model = catalogSettings.ToModel();
            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.AllowViewUnpublishedProductPage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowViewUnpublishedProductPage, storeScope);
                model.DisplayDiscontinuedMessageForUnpublishedProducts_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DisplayDiscontinuedMessageForUnpublishedProducts, storeScope);
                model.ShowSkuOnProductDetailsPage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowSkuOnProductDetailsPage, storeScope);
                model.ShowSkuOnCatalogPages_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowSkuOnCatalogPages, storeScope);
                model.ShowManufacturerPartNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowManufacturerPartNumber, storeScope);
                model.ShowGtin_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowGtin, storeScope);
                model.ShowFreeShippingNotification_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowFreeShippingNotification, storeScope);
                model.AllowProductSorting_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowProductSorting, storeScope);
                model.AllowProductViewModeChanging_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowProductViewModeChanging, storeScope);
                model.ShowProductsFromSubcategories_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowProductsFromSubcategories, storeScope);
                model.ShowCategoryProductNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowCategoryProductNumber, storeScope);
                model.ShowCategoryProductNumberIncludingSubcategories_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowCategoryProductNumberIncludingSubcategories, storeScope);
                model.CategoryBreadcrumbEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.CategoryBreadcrumbEnabled, storeScope);
                model.ShowShareButton_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowShareButton, storeScope);
                model.PageShareCode_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.PageShareCode, storeScope);
                model.ProductReviewsMustBeApproved_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductReviewsMustBeApproved, storeScope);
                model.AllowAnonymousUsersToReviewProduct_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowAnonymousUsersToReviewProduct, storeScope);
                model.ProductReviewPossibleOnlyAfterPurchasing_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductReviewPossibleOnlyAfterPurchasing, storeScope);
                model.NotifyStoreOwnerAboutNewProductReviews_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NotifyStoreOwnerAboutNewProductReviews, storeScope);
                model.EmailAFriendEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.EmailAFriendEnabled, storeScope);
                model.AskQuestionEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AskQuestionEnabled, storeScope);
                model.AllowAnonymousUsersToEmailAFriend_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowAnonymousUsersToEmailAFriend, storeScope);
                model.RecentlyViewedProductsNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.RecentlyViewedProductsNumber, storeScope);
                model.RecentlyViewedProductsEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.RecentlyViewedProductsEnabled, storeScope);
                model.RecommendedProductsEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.RecommendedProductsEnabled, storeScope);
                model.SuggestedProductsEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.SuggestedProductsEnabled, storeScope);
                model.SuggestedProductsNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.SuggestedProductsNumber, storeScope);
                model.NewProductsNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NewProductsNumber, storeScope);
                model.NewProductsEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NewProductsEnabled, storeScope);
                model.CompareProductsEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.CompareProductsEnabled, storeScope);
                model.ShowBestsellersOnHomepage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowBestsellersOnHomepage, storeScope);
                model.NumberOfBestsellersOnHomepage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NumberOfBestsellersOnHomepage, storeScope);
                model.SearchPageProductsPerPage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.SearchPageProductsPerPage, storeScope);
                model.SearchPageAllowCustomersToSelectPageSize_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.SearchPageAllowCustomersToSelectPageSize, storeScope);
                model.SearchPagePageSizeOptions_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.SearchPagePageSizeOptions, storeScope);
                model.ProductSearchAutoCompleteEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductSearchAutoCompleteEnabled, storeScope);
                model.ProductSearchAutoCompleteNumberOfProducts_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductSearchAutoCompleteNumberOfProducts, storeScope);
                model.ShowProductImagesInSearchAutoComplete_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowProductImagesInSearchAutoComplete, storeScope);
                model.ProductSearchTermMinimumLength_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductSearchTermMinimumLength, storeScope);
                model.ProductsAlsoPurchasedEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductsAlsoPurchasedEnabled, storeScope);
                model.ProductsAlsoPurchasedNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductsAlsoPurchasedNumber, storeScope);
                model.NumberOfProductTags_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NumberOfProductTags, storeScope);
                model.ProductsByTagPageSize_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductsByTagPageSize, storeScope);
                model.ProductsByTagAllowCustomersToSelectPageSize_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductsByTagAllowCustomersToSelectPageSize, storeScope);
                model.ProductsByTagPageSizeOptions_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductsByTagPageSizeOptions, storeScope);
                model.IncludeShortDescriptionInCompareProducts_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.IncludeShortDescriptionInCompareProducts, storeScope);
                model.IncludeFullDescriptionInCompareProducts_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.IncludeFullDescriptionInCompareProducts, storeScope);
                model.IgnoreDiscounts_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.IgnoreDiscounts, storeScope);
                model.IgnoreFeaturedProducts_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.IgnoreFeaturedProducts, storeScope);
                model.IgnoreAcl_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.IgnoreAcl, storeScope);
                model.IgnoreStoreLimitations_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.IgnoreStoreLimitations, storeScope);
                model.IgnoreFilterableSpecAttributeOption_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.IgnoreFilterableSpecAttributeOption, storeScope);
                model.IgnoreFilterableAvailableStartEndDateTime_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.IgnoreFilterableAvailableStartEndDateTime, storeScope);
                model.CacheProductPrices_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.CacheProductPrices, storeScope);
                model.ManufacturersBlockItemsToDisplay_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ManufacturersBlockItemsToDisplay, storeScope);
                model.DisplayTaxShippingInfoFooter_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DisplayTaxShippingInfoFooter, storeScope);
                model.DisplayTaxShippingInfoProductDetailsPage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DisplayTaxShippingInfoProductDetailsPage, storeScope);
                model.DisplayTaxShippingInfoProductBoxes_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DisplayTaxShippingInfoProductBoxes, storeScope);
                model.DisplayTaxShippingInfoShoppingCart_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DisplayTaxShippingInfoShoppingCart, storeScope);
                model.DisplayTaxShippingInfoWishlist_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DisplayTaxShippingInfoWishlist, storeScope);
                model.DisplayTaxShippingInfoOrderDetailsPage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DisplayTaxShippingInfoOrderDetailsPage, storeScope);
                model.ShowProductReviewsPerStore_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowProductReviewsPerStore, storeScope);
            }
            return View(model);
        }
        [HttpPost]
        public IActionResult Catalog(CatalogSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();


            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var catalogSettings = _settingService.LoadSetting<CatalogSettings>(storeScope);
            catalogSettings = model.ToEntity(catalogSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.AllowViewUnpublishedProductPage_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.AllowViewUnpublishedProductPage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.AllowViewUnpublishedProductPage, storeScope);

            if (model.DisplayDiscontinuedMessageForUnpublishedProducts_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.DisplayDiscontinuedMessageForUnpublishedProducts, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.DisplayDiscontinuedMessageForUnpublishedProducts, storeScope);

            if (model.ShowSkuOnProductDetailsPage_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ShowSkuOnProductDetailsPage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ShowSkuOnProductDetailsPage, storeScope);

            if (model.ShowSkuOnCatalogPages_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ShowSkuOnCatalogPages, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ShowSkuOnCatalogPages, storeScope);

            if (model.ShowManufacturerPartNumber_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ShowManufacturerPartNumber, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ShowManufacturerPartNumber, storeScope);
            
            if (model.ShowGtin_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ShowGtin, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ShowGtin, storeScope);

            if (model.ShowFreeShippingNotification_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ShowFreeShippingNotification, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ShowFreeShippingNotification, storeScope);
            
            if (model.AllowProductSorting_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.AllowProductSorting, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.AllowProductSorting, storeScope);
            
            if (model.AllowProductViewModeChanging_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.AllowProductViewModeChanging, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.AllowProductViewModeChanging, storeScope);
            
            if (model.ShowProductsFromSubcategories_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ShowProductsFromSubcategories, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ShowProductsFromSubcategories, storeScope);
            
            if (model.ShowCategoryProductNumber_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ShowCategoryProductNumber, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ShowCategoryProductNumber, storeScope);
            
            if (model.ShowCategoryProductNumberIncludingSubcategories_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ShowCategoryProductNumberIncludingSubcategories, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ShowCategoryProductNumberIncludingSubcategories, storeScope);
            
            if (model.CategoryBreadcrumbEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.CategoryBreadcrumbEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.CategoryBreadcrumbEnabled, storeScope);
            
            if (model.ShowShareButton_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ShowShareButton, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ShowShareButton, storeScope);

            if (model.PageShareCode_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.PageShareCode, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.PageShareCode, storeScope);

            if (model.ProductReviewsMustBeApproved_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ProductReviewsMustBeApproved, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ProductReviewsMustBeApproved, storeScope);
            
            if (model.AllowAnonymousUsersToReviewProduct_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.AllowAnonymousUsersToReviewProduct, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.AllowAnonymousUsersToReviewProduct, storeScope);

            if (model.ProductReviewPossibleOnlyAfterPurchasing_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ProductReviewPossibleOnlyAfterPurchasing, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ProductReviewPossibleOnlyAfterPurchasing, storeScope);

            if (model.NotifyStoreOwnerAboutNewProductReviews_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.NotifyStoreOwnerAboutNewProductReviews, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.NotifyStoreOwnerAboutNewProductReviews, storeScope);
            
            if (model.EmailAFriendEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.EmailAFriendEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.EmailAFriendEnabled, storeScope);

            if (model.AskQuestionEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.AskQuestionEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.AskQuestionEnabled, storeScope);

            if (model.AllowAnonymousUsersToEmailAFriend_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.AllowAnonymousUsersToEmailAFriend, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.AllowAnonymousUsersToEmailAFriend, storeScope);
            
            if (model.RecentlyViewedProductsNumber_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.RecentlyViewedProductsNumber, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.RecentlyViewedProductsNumber, storeScope);
            
            if (model.RecentlyViewedProductsEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.RecentlyViewedProductsEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.RecentlyViewedProductsEnabled, storeScope);

            if (model.RecommendedProductsEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.RecommendedProductsEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.RecommendedProductsEnabled, storeScope);

            if (model.SuggestedProductsEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.SuggestedProductsEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.SuggestedProductsEnabled, storeScope);

            if (model.SuggestedProductsNumber_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.SuggestedProductsNumber, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.SuggestedProductsNumber, storeScope);

            if (model.NewProductsNumber_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.NewProductsNumber, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.NewProductsNumber, storeScope);

            if (model.NewProductsEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.NewProductsEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.NewProductsEnabled, storeScope);

            if (model.CompareProductsEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.CompareProductsEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.CompareProductsEnabled, storeScope);
            
            if (model.ShowBestsellersOnHomepage_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ShowBestsellersOnHomepage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ShowBestsellersOnHomepage, storeScope);
            
            if (model.NumberOfBestsellersOnHomepage_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.NumberOfBestsellersOnHomepage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.NumberOfBestsellersOnHomepage, storeScope);
            
            if (model.SearchPageProductsPerPage_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.SearchPageProductsPerPage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.SearchPageProductsPerPage, storeScope);

            if (model.SearchPageAllowCustomersToSelectPageSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.SearchPageAllowCustomersToSelectPageSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.SearchPageAllowCustomersToSelectPageSize, storeScope);

            if (model.SearchPagePageSizeOptions_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.SearchPagePageSizeOptions, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.SearchPagePageSizeOptions, storeScope);
            
            if (model.ProductSearchAutoCompleteEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ProductSearchAutoCompleteEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ProductSearchAutoCompleteEnabled, storeScope);
            
            if (model.ProductSearchAutoCompleteNumberOfProducts_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ProductSearchAutoCompleteNumberOfProducts, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ProductSearchAutoCompleteNumberOfProducts, storeScope);
            
            if (model.ShowProductImagesInSearchAutoComplete_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ShowProductImagesInSearchAutoComplete, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ShowProductImagesInSearchAutoComplete, storeScope);

            if (model.ProductSearchTermMinimumLength_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ProductSearchTermMinimumLength, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ProductSearchTermMinimumLength, storeScope);
            
            if (model.ProductsAlsoPurchasedEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ProductsAlsoPurchasedEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ProductsAlsoPurchasedEnabled, storeScope);
            
            if (model.ProductsAlsoPurchasedNumber_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ProductsAlsoPurchasedNumber, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ProductsAlsoPurchasedNumber, storeScope);
                        
            if (model.NumberOfProductTags_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.NumberOfProductTags, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.NumberOfProductTags, storeScope);
            
            if (model.ProductsByTagPageSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ProductsByTagPageSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ProductsByTagPageSize, storeScope);
            
            if (model.ProductsByTagAllowCustomersToSelectPageSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ProductsByTagAllowCustomersToSelectPageSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ProductsByTagAllowCustomersToSelectPageSize, storeScope);
            
            if (model.ProductsByTagPageSizeOptions_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ProductsByTagPageSizeOptions, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ProductsByTagPageSizeOptions, storeScope);
            
            if (model.IncludeShortDescriptionInCompareProducts_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.IncludeShortDescriptionInCompareProducts, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.IncludeShortDescriptionInCompareProducts, storeScope);
            
            if (model.IncludeFullDescriptionInCompareProducts_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.IncludeFullDescriptionInCompareProducts, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.IncludeFullDescriptionInCompareProducts, storeScope);
            
            if (model.IgnoreDiscounts_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.IgnoreDiscounts, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.IgnoreDiscounts, storeScope);
            
            if (model.IgnoreFeaturedProducts_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.IgnoreFeaturedProducts, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.IgnoreFeaturedProducts, storeScope);

            if (model.IgnoreAcl_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.IgnoreAcl, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.IgnoreAcl, storeScope);

            if (model.IgnoreStoreLimitations_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.IgnoreStoreLimitations, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.IgnoreStoreLimitations, storeScope);

            if (model.IgnoreFilterableAvailableStartEndDateTime_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.IgnoreFilterableAvailableStartEndDateTime, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.IgnoreFilterableAvailableStartEndDateTime, storeScope);

            if (model.IgnoreFilterableSpecAttributeOption_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.IgnoreFilterableSpecAttributeOption, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.IgnoreFilterableSpecAttributeOption, storeScope);


            if (model.CacheProductPrices_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.CacheProductPrices, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.CacheProductPrices, storeScope);

            if (model.ManufacturersBlockItemsToDisplay_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ManufacturersBlockItemsToDisplay, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ManufacturersBlockItemsToDisplay, storeScope);

            if (model.DisplayTaxShippingInfoFooter_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.DisplayTaxShippingInfoFooter, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.DisplayTaxShippingInfoFooter, storeScope);

            if (model.DisplayTaxShippingInfoProductDetailsPage_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.DisplayTaxShippingInfoProductDetailsPage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.DisplayTaxShippingInfoProductDetailsPage, storeScope);

            if (model.DisplayTaxShippingInfoProductBoxes_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.DisplayTaxShippingInfoProductBoxes, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.DisplayTaxShippingInfoProductBoxes, storeScope);

            if (model.DisplayTaxShippingInfoShoppingCart_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.DisplayTaxShippingInfoShoppingCart, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.DisplayTaxShippingInfoShoppingCart, storeScope);

            if (model.DisplayTaxShippingInfoWishlist_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.DisplayTaxShippingInfoWishlist, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.DisplayTaxShippingInfoWishlist, storeScope);

            if (model.DisplayTaxShippingInfoOrderDetailsPage_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.DisplayTaxShippingInfoOrderDetailsPage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.DisplayTaxShippingInfoOrderDetailsPage, storeScope);

            if (model.ShowProductReviewsPerStore_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(catalogSettings, x => x.ShowProductReviewsPerStore, storeScope, false);
                       else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(catalogSettings, x => x.ShowProductReviewsPerStore, storeScope);

            //now clear cache
            _cacheManager.Clear();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));

            //selected tab
            SaveSelectedTabIndex();

            return RedirectToAction("Catalog");
        }

        #region Sort options

        [HttpPost]
        public IActionResult SortOptionsList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var catalogSettings = _settingService.LoadSetting<CatalogSettings>(storeScope);
            var model = new List<SortOptionModel>();
            foreach (int option in Enum.GetValues(typeof(ProductSortingEnum)))
            {
                int value;
                model.Add(new SortOptionModel()
                {
                    Id = option,
                    Name = ((ProductSortingEnum)option).GetLocalizedEnum(_localizationService, _workContext),
                    IsActive = !catalogSettings.ProductSortingEnumDisabled.Contains(option),
                    DisplayOrder = catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(option, out value) ? value : option
                });
            }
            var gridModel = new DataSourceResult
            {
                Data = model.OrderBy(option => option.DisplayOrder),
                Total = model.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult SortOptionUpdate(SortOptionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var catalogSettings = _settingService.LoadSetting<CatalogSettings>(storeScope);

            catalogSettings.ProductSortingEnumDisplayOrder[model.Id] = model.DisplayOrder;
            if (model.IsActive && catalogSettings.ProductSortingEnumDisabled.Contains(model.Id))
                catalogSettings.ProductSortingEnumDisabled.Remove(model.Id);
            if (!model.IsActive && !catalogSettings.ProductSortingEnumDisabled.Contains(model.Id))
                catalogSettings.ProductSortingEnumDisabled.Add(model.Id);

            _settingService.SaveSetting(catalogSettings, x => x.ProductSortingEnumDisabled, storeScope, false);
            _settingService.SaveSetting(catalogSettings, x => x.ProductSortingEnumDisplayOrder, storeScope, false);
            _cacheManager.Clear();

            return new NullJsonResult();
        }

        #endregion

        public IActionResult RewardPoints()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();


            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var rewardPointsSettings = _settingService.LoadSetting<RewardPointsSettings>(storeScope);
            var model = rewardPointsSettings.ToModel();
            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.Enabled_OverrideForStore = _settingService.SettingExists(rewardPointsSettings, x => x.Enabled, storeScope);
                model.ExchangeRate_OverrideForStore = _settingService.SettingExists(rewardPointsSettings, x => x.ExchangeRate, storeScope);
                model.MinimumRewardPointsToUse_OverrideForStore = _settingService.SettingExists(rewardPointsSettings, x => x.MinimumRewardPointsToUse, storeScope);
                model.PointsForRegistration_OverrideForStore = _settingService.SettingExists(rewardPointsSettings, x => x.PointsForRegistration, storeScope);
                model.PointsForPurchases_OverrideForStore = _settingService.SettingExists(rewardPointsSettings, x => x.PointsForPurchases_Amount, storeScope) ||
                    _settingService.SettingExists(rewardPointsSettings, x => x.PointsForPurchases_Points, storeScope);
                model.PointsForPurchases_Awarded_OverrideForStore = _settingService.SettingExists(rewardPointsSettings, x => x.PointsForPurchases_Awarded, storeScope);
                model.PointsForPurchases_Canceled_OverrideForStore = _settingService.SettingExists(rewardPointsSettings, x => x.PointsForPurchases_Canceled, storeScope);
                model.DisplayHowMuchWillBeEarned_OverrideForStore = _settingService.SettingExists(rewardPointsSettings, x => x.DisplayHowMuchWillBeEarned, storeScope); model.PointsForRegistration_OverrideForStore = _settingService.SettingExists(rewardPointsSettings, x => x.PointsForRegistration, storeScope);
            }
            var currencySettings = _settingService.LoadSetting<CurrencySettings>(storeScope); 
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            return View(model);
        }
        [HttpPost]
        public IActionResult RewardPoints(RewardPointsSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
                var rewardPointsSettings = _settingService.LoadSetting<RewardPointsSettings>(storeScope);
                rewardPointsSettings = model.ToEntity(rewardPointsSettings);

                /* We do not clear cache after each setting update.
                 * This behavior can increase performance because cached settings will not be cleared 
                 * and loaded from database after each update */
                if (model.Enabled_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(rewardPointsSettings, x => x.Enabled, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(rewardPointsSettings, x => x.Enabled, storeScope);
                
                if (model.ExchangeRate_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(rewardPointsSettings, x => x.ExchangeRate, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(rewardPointsSettings, x => x.ExchangeRate, storeScope);
                
                if (model.MinimumRewardPointsToUse_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(rewardPointsSettings, x => x.MinimumRewardPointsToUse, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(rewardPointsSettings, x => x.MinimumRewardPointsToUse, storeScope);
                
                if (model.PointsForRegistration_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(rewardPointsSettings, x => x.PointsForRegistration, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(rewardPointsSettings, x => x.PointsForRegistration, storeScope);
                
                if (model.PointsForPurchases_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(rewardPointsSettings, x => x.PointsForPurchases_Amount, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(rewardPointsSettings, x => x.PointsForPurchases_Amount, storeScope);

                if (model.PointsForPurchases_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(rewardPointsSettings, x => x.PointsForPurchases_Points, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(rewardPointsSettings, x => x.PointsForPurchases_Points, storeScope);
                
                if (model.PointsForPurchases_Awarded_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(rewardPointsSettings, x => x.PointsForPurchases_Awarded, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(rewardPointsSettings, x => x.PointsForPurchases_Awarded, storeScope);
                
                if (model.PointsForPurchases_Canceled_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(rewardPointsSettings, x => x.PointsForPurchases_Canceled, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(rewardPointsSettings, x => x.PointsForPurchases_Canceled, storeScope);
                
                if (model.DisplayHowMuchWillBeEarned_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(rewardPointsSettings, x => x.DisplayHowMuchWillBeEarned, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(rewardPointsSettings, x => x.DisplayHowMuchWillBeEarned, storeScope);

                _settingService.SaveSetting(rewardPointsSettings, x => x.PointsAccumulatedForAllStores, "", false);

                //now clear cache
                _cacheManager.Clear();

                //activity log
                _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            }
            else
            {
                //If we got this far, something failed, redisplay form
                foreach (var modelState in ModelState.Values)
                    foreach (var error in modelState.Errors)
                        ErrorNotification(error.ErrorMessage);
            }
            return RedirectToAction("RewardPoints");
        }




        public IActionResult Order()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var orderSettings = _settingService.LoadSetting<OrderSettings>(storeScope);
            var model = orderSettings.ToModel();
            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.IsReOrderAllowed_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.IsReOrderAllowed, storeScope);
                model.MinOrderSubtotalAmount_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.MinOrderSubtotalAmount, storeScope);
                model.MinOrderTotalAmount_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.MinOrderTotalAmount, storeScope);
                model.MinOrderSubtotalAmountIncludingTax_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.MinOrderSubtotalAmountIncludingTax, storeScope);
                model.AnonymousCheckoutAllowed_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.AnonymousCheckoutAllowed, storeScope);
                model.TermsOfServiceOnShoppingCartPage_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.TermsOfServiceOnShoppingCartPage, storeScope);
                model.TermsOfServiceOnOrderConfirmPage_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.TermsOfServiceOnOrderConfirmPage, storeScope);
                model.OnePageCheckoutEnabled_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.OnePageCheckoutEnabled, storeScope);
                model.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab, storeScope);
                model.DisableBillingAddressCheckoutStep_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.DisableBillingAddressCheckoutStep, storeScope);
                model.DisableOrderCompletedPage_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.DisableOrderCompletedPage, storeScope);
                model.AttachPdfInvoiceToOrderPlacedEmail_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.AttachPdfInvoiceToOrderPlacedEmail, storeScope);
                model.AttachPdfInvoiceToOrderPaidEmail_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.AttachPdfInvoiceToOrderPaidEmail, storeScope);
                model.AttachPdfInvoiceToOrderCompletedEmail_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.AttachPdfInvoiceToOrderCompletedEmail, storeScope);
                model.ReturnRequestsEnabled_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.ReturnRequestsEnabled, storeScope);
                model.NumberOfDaysReturnRequestAvailable_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.NumberOfDaysReturnRequestAvailable, storeScope);
            }

            var currencySettings = _settingService.LoadSetting<CurrencySettings>(storeScope);
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            //gift card activation/deactivation
            model.GiftCards_Activated_OrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.GiftCards_Activated_OrderStatuses.Insert(0, new SelectListItem { Text = "---", Value = "0" });
            model.GiftCards_Deactivated_OrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.GiftCards_Deactivated_OrderStatuses.Insert(0, new SelectListItem { Text = "---", Value = "0" });

            //order ident
            var orderRepository = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IRepository<Order>>();
            int maxOrderNumber = 0;
            if (orderRepository.Table.Count() > 0)
                maxOrderNumber = orderRepository.Table.Max(x => x.OrderNumber);
            model.OrderIdent = maxOrderNumber;

            return View(model);
        }
        [HttpPost]
        public IActionResult Order(OrderSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
                var orderSettings = _settingService.LoadSetting<OrderSettings>(storeScope);
                orderSettings = model.ToEntity(orderSettings);

                /* We do not clear cache after each setting update.
                 * This behavior can increase performance because cached settings will not be cleared 
                 * and loaded from database after each update */
                if (model.IsReOrderAllowed_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.IsReOrderAllowed, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.IsReOrderAllowed, storeScope);
                
                if (model.MinOrderSubtotalAmount_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.MinOrderSubtotalAmount, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.MinOrderSubtotalAmount, storeScope);

                if (model.MinOrderSubtotalAmountIncludingTax_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.MinOrderSubtotalAmountIncludingTax, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.MinOrderSubtotalAmountIncludingTax, storeScope);

                if (model.MinOrderTotalAmount_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.MinOrderTotalAmount, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.MinOrderTotalAmount, storeScope);
                
                if (model.AnonymousCheckoutAllowed_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.AnonymousCheckoutAllowed, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.AnonymousCheckoutAllowed, storeScope);

                if (model.TermsOfServiceOnShoppingCartPage_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.TermsOfServiceOnShoppingCartPage, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.TermsOfServiceOnShoppingCartPage, storeScope);

                if (model.TermsOfServiceOnOrderConfirmPage_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.TermsOfServiceOnOrderConfirmPage, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.TermsOfServiceOnOrderConfirmPage, storeScope);
                
                if (model.OnePageCheckoutEnabled_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.OnePageCheckoutEnabled, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.OnePageCheckoutEnabled, storeScope);

                if (model.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab, storeScope);

                if (model.DisableBillingAddressCheckoutStep_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.DisableBillingAddressCheckoutStep, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.DisableBillingAddressCheckoutStep, storeScope);

                if (model.DisableOrderCompletedPage_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.DisableOrderCompletedPage, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.DisableOrderCompletedPage, storeScope);

                if (model.AttachPdfInvoiceToOrderPlacedEmail_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.AttachPdfInvoiceToOrderPlacedEmail, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.AttachPdfInvoiceToOrderPlacedEmail, storeScope);

                if (model.AttachPdfInvoiceToOrderPaidEmail_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.AttachPdfInvoiceToOrderPaidEmail, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.AttachPdfInvoiceToOrderPaidEmail, storeScope);

                if (model.AttachPdfInvoiceToOrderCompletedEmail_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.AttachPdfInvoiceToOrderCompletedEmail, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.AttachPdfInvoiceToOrderCompletedEmail, storeScope);
                
                if (model.ReturnRequestsEnabled_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.ReturnRequestsEnabled, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.ReturnRequestsEnabled, storeScope);

                if (model.NumberOfDaysReturnRequestAvailable_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.NumberOfDaysReturnRequestAvailable, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.NumberOfDaysReturnRequestAvailable, storeScope);

                if (model.UserCanCancelUnpaidOrder_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.UserCanCancelUnpaidOrder, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.UserCanCancelUnpaidOrder, storeScope);

                _settingService.SaveSetting(orderSettings, x => x.DeactivateGiftCardsAfterDeletingOrder, "", false);
                _settingService.SaveSetting(orderSettings, x => x.CompleteOrderWhenDelivered, "", false);
                _settingService.SaveSetting(orderSettings, x => x.GiftCards_Activated_OrderStatusId, "", false);
                _settingService.SaveSetting(orderSettings, x => x.GiftCards_Deactivated_OrderStatusId, "", false);

                if (model.ReturnRequestsEnabled_OverrideForStore || storeScope == "")
                    _settingService.SaveSetting(orderSettings, x => x.ReturnRequestsEnabled, storeScope, false);
                else if (!String.IsNullOrEmpty(storeScope))
                    _settingService.DeleteSetting(orderSettings, x => x.ReturnRequestsEnabled, storeScope);

                //now clear cache
                _cacheManager.Clear();
                
                //order ident
                if (model.OrderIdent > 0)
                {
                    var orderRepository = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IRepository<Order>>();
                    int maxOrderNumber = 0;
                    if (orderRepository.Table.Count() > 0)
                        maxOrderNumber = orderRepository.Table.Max(x => x.OrderNumber);
                    if(model.OrderIdent > maxOrderNumber)
                    {
                        orderRepository.Insert(new Order() { OrderNumber = model.OrderIdent.Value, Deleted = true, CreatedOnUtc = DateTime.UtcNow });
                        /* another solution
                        string command = "{ insert: \"Order\", documents: [  { \"_id\": \"" + ObjectId.GenerateNewId().ToString() + "\", \"OrderNumber\" : NumberInt(" + model.OrderIdent + "), \"Deleted\" : true } ]}";
                        var result = orderRepository.Database.RunCommand<BsonDocument>(command);
                        */
                    }
                }

                //activity log
                _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            }
            else
            {
                //If we got this far, something failed, redisplay form
                foreach (var modelState in ModelState.Values)
                    foreach (var error in modelState.Errors)
                        ErrorNotification(error.ErrorMessage);
            }

            //selected tab
            SaveSelectedTabIndex();

            return RedirectToAction("Order");
        }




        public IActionResult ShoppingCart()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var shoppingCartSettings = _settingService.LoadSetting<ShoppingCartSettings>(storeScope);
            var model = shoppingCartSettings.ToModel();
            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.DisplayCartAfterAddingProduct_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.DisplayCartAfterAddingProduct, storeScope);
                model.DisplayWishlistAfterAddingProduct_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.DisplayWishlistAfterAddingProduct, storeScope);
                model.MaximumShoppingCartItems_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.MaximumShoppingCartItems, storeScope);
                model.MaximumWishlistItems_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.MaximumWishlistItems, storeScope);
                model.AllowOutOfStockItemsToBeAddedToWishlist_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.AllowOutOfStockItemsToBeAddedToWishlist, storeScope);
                model.MoveItemsFromWishlistToCart_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.MoveItemsFromWishlistToCart, storeScope);
                model.ShowProductImagesOnShoppingCart_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.ShowProductImagesOnShoppingCart, storeScope);
                model.ShowProductImagesOnWishList_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.ShowProductImagesOnWishList, storeScope);
                model.ShowDiscountBox_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.ShowDiscountBox, storeScope);
                model.ShowGiftCardBox_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.ShowGiftCardBox, storeScope);
                model.CrossSellsNumber_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.CrossSellsNumber, storeScope);
                model.EmailWishlistEnabled_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.EmailWishlistEnabled, storeScope);
                model.AllowAnonymousUsersToEmailWishlist_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.AllowAnonymousUsersToEmailWishlist, storeScope);
                model.MiniShoppingCartEnabled_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.MiniShoppingCartEnabled, storeScope);
                model.ShowProductImagesInMiniShoppingCart_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.ShowProductImagesInMiniShoppingCart, storeScope);
                model.MiniShoppingCartProductNumber_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.MiniShoppingCartProductNumber, storeScope);
                model.AllowCartItemEditing_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.AllowCartItemEditing, storeScope);
                model.CartsSharedBetweenStores_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.CartsSharedBetweenStores, storeScope);
            }
            return View(model);
        }
        [HttpPost]
        public IActionResult ShoppingCart(ShoppingCartSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var shoppingCartSettings = _settingService.LoadSetting<ShoppingCartSettings>(storeScope);
            shoppingCartSettings = model.ToEntity(shoppingCartSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.DisplayCartAfterAddingProduct_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.DisplayCartAfterAddingProduct, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.DisplayCartAfterAddingProduct, storeScope);
            
            if (model.DisplayWishlistAfterAddingProduct_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.DisplayWishlistAfterAddingProduct, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.DisplayWishlistAfterAddingProduct, storeScope);
            
            if (model.MaximumShoppingCartItems_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.MaximumShoppingCartItems, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.MaximumShoppingCartItems, storeScope);
            
            if (model.MaximumWishlistItems_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.MaximumWishlistItems, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.MaximumWishlistItems, storeScope);
            
            if (model.AllowOutOfStockItemsToBeAddedToWishlist_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.AllowOutOfStockItemsToBeAddedToWishlist, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.AllowOutOfStockItemsToBeAddedToWishlist, storeScope);
            
            if (model.MoveItemsFromWishlistToCart_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.MoveItemsFromWishlistToCart, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.MoveItemsFromWishlistToCart, storeScope);
            
            if (model.ShowProductImagesOnShoppingCart_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.ShowProductImagesOnShoppingCart, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.ShowProductImagesOnShoppingCart, storeScope);
            
            if (model.ShowProductImagesOnWishList_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.ShowProductImagesOnWishList, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.ShowProductImagesOnWishList, storeScope);
            
            if (model.ShowDiscountBox_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.ShowDiscountBox, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.ShowDiscountBox, storeScope);
            
            if (model.ShowGiftCardBox_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.ShowGiftCardBox, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.ShowGiftCardBox, storeScope);
            
            if (model.CrossSellsNumber_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.CrossSellsNumber, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.CrossSellsNumber, storeScope);
            
            if (model.EmailWishlistEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.EmailWishlistEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.EmailWishlistEnabled, storeScope);
            
            if (model.AllowAnonymousUsersToEmailWishlist_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.AllowAnonymousUsersToEmailWishlist, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.AllowAnonymousUsersToEmailWishlist, storeScope);
            
            if (model.MiniShoppingCartEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.MiniShoppingCartEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.MiniShoppingCartEnabled, storeScope);
            
            if (model.ShowProductImagesInMiniShoppingCart_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.ShowProductImagesInMiniShoppingCart, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.ShowProductImagesInMiniShoppingCart, storeScope);

            if (model.MiniShoppingCartProductNumber_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.MiniShoppingCartProductNumber, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.MiniShoppingCartProductNumber, storeScope);

            if (model.AllowCartItemEditing_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.AllowCartItemEditing, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.AllowCartItemEditing, storeScope);

            if (model.CartsSharedBetweenStores_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(shoppingCartSettings, x => x.CartsSharedBetweenStores, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(shoppingCartSettings, x => x.CartsSharedBetweenStores, storeScope);

            //now clear cache
            _cacheManager.Clear();
            

            //activity log
            _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("ShoppingCart");
        }


        #region Return request reasons

        public IActionResult ReturnRequestReasonList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //we just redirect a user to the order settings page

            //select second tab
            const int customerFormFieldIndex = 1;
            SaveSelectedTabIndex(customerFormFieldIndex);
            return RedirectToAction("Order", "Setting");
        }
        [HttpPost]
        public IActionResult ReturnRequestReasonList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var reasons = _returnRequestService.GetAllReturnRequestReasons();
            var gridModel = new DataSourceResult
            {
                Data = reasons.Select(x => x.ToModel()),
                Total = reasons.Count
            };
            return Json(gridModel);
        }
        //create
        public IActionResult ReturnRequestReasonCreate()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = new ReturnRequestReasonModel();
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult ReturnRequestReasonCreate(ReturnRequestReasonModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var rrr = model.ToEntity();
                _returnRequestService.InsertReturnRequestReason(rrr);
                //locales
                rrr.Locales = UpdateLocales(rrr, model);
                _returnRequestService.UpdateReturnRequestReason(rrr);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.Order.ReturnRequestReasons.Added"));
                return continueEditing ? RedirectToAction("ReturnRequestReasonEdit", new { id = rrr.Id }) : RedirectToAction("ReturnRequestReasonList");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }
        //edit
        public IActionResult ReturnRequestReasonEdit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var rrr = _returnRequestService.GetReturnRequestReasonById(id);
            if (rrr == null)
                //No reason found with the specified id
                return RedirectToAction("ReturnRequestReasonList");

            var model = rrr.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = rrr.GetLocalized(x => x.Name, languageId, false, false);
            });
            return View(model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult ReturnRequestReasonEdit(ReturnRequestReasonModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var rrr = _returnRequestService.GetReturnRequestReasonById(model.Id);
            if (rrr == null)
                //No reason found with the specified id
                return RedirectToAction("ReturnRequestReasonList");

            if (ModelState.IsValid)
            {
                rrr = model.ToEntity(rrr);
                _returnRequestService.UpdateReturnRequestReason(rrr);
                //locales
                rrr.Locales = UpdateLocales(rrr, model);
                _returnRequestService.UpdateReturnRequestReason(rrr);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.Order.ReturnRequestReasons.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("ReturnRequestReasonEdit", new { id = rrr.Id });
                }
                return RedirectToAction("ReturnRequestReasonList");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }
        //delete
        [HttpPost]
        public IActionResult ReturnRequestReasonDelete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var rrr = _returnRequestService.GetReturnRequestReasonById(id);
            _returnRequestService.DeleteReturnRequestReason(rrr);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.Order.ReturnRequestReasons.Deleted"));
            return RedirectToAction("ReturnRequestReasonList");
        }

        #endregion

        #region Return request actions

        public IActionResult ReturnRequestActionList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //we just redirect a user to the order settings page

            //select second tab
            const int customerFormFieldIndex = 1;
            SaveSelectedTabIndex(customerFormFieldIndex);
            return RedirectToAction("Order", "Setting");
        }
        [HttpPost]
        public IActionResult ReturnRequestActionList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var actions = _returnRequestService.GetAllReturnRequestActions();
            var gridModel = new DataSourceResult
            {
                Data = actions.Select(x => x.ToModel()),
                Total = actions.Count
            };
            return Json(gridModel);
        }
        //create
        public IActionResult ReturnRequestActionCreate()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = new ReturnRequestActionModel();
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult ReturnRequestActionCreate(ReturnRequestActionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var rra = model.ToEntity();
                
                _returnRequestService.InsertReturnRequestAction(rra);
                //locales
                rra.Locales = UpdateLocales(rra, model);
                _returnRequestService.UpdateReturnRequestAction(rra);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.Order.ReturnRequestActions.Added"));
                return continueEditing ? RedirectToAction("ReturnRequestActionEdit", new { id = rra.Id }) : RedirectToAction("ReturnRequestActionList");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }
        //edit
        public IActionResult ReturnRequestActionEdit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var rra = _returnRequestService.GetReturnRequestActionById(id);
            if (rra == null)
                //No action found with the specified id
                return RedirectToAction("ReturnRequestActionList");

            var model = rra.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = rra.GetLocalized(x => x.Name, languageId, false, false);
            });
            return View(model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult ReturnRequestActionEdit(ReturnRequestActionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var rra = _returnRequestService.GetReturnRequestActionById(model.Id);
            if (rra == null)
                //No action found with the specified id
                return RedirectToAction("ReturnRequestActionList");

            if (ModelState.IsValid)
            {
                rra = model.ToEntity(rra);
                rra.Locales = UpdateLocales(rra, model);
                _returnRequestService.UpdateReturnRequestAction(rra);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.Order.ReturnRequestActions.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("ReturnRequestActionEdit", new { id = rra.Id });
                }
                return RedirectToAction("ReturnRequestActionList");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }
        //delete
        [HttpPost]
        public IActionResult ReturnRequestActionDelete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var rra = _returnRequestService.GetReturnRequestActionById(id);
            _returnRequestService.DeleteReturnRequestAction(rra);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.Order.ReturnRequestActions.Deleted"));
            return RedirectToAction("ReturnRequestActionList");
        }

        #endregion



        public IActionResult Media()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var mediaSettings = _settingService.LoadSetting<MediaSettings>(storeScope);
            var model = mediaSettings.ToModel();
            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.AvatarPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.AvatarPictureSize, storeScope);
                model.ProductThumbPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.ProductThumbPictureSize, storeScope);
                model.ProductDetailsPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.ProductDetailsPictureSize, storeScope);
                model.ProductThumbPictureSizeOnProductDetailsPage_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.ProductThumbPictureSizeOnProductDetailsPage, storeScope);
                model.AssociatedProductPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.AssociatedProductPictureSize, storeScope);
                model.CategoryThumbPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.CategoryThumbPictureSize, storeScope);
                model.ManufacturerThumbPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.ManufacturerThumbPictureSize, storeScope);
                model.VendorThumbPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.VendorThumbPictureSize, storeScope);
                model.CartThumbPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.CartThumbPictureSize, storeScope);
                model.MiniCartThumbPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.MiniCartThumbPictureSize, storeScope);
                model.MaximumImageSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.MaximumImageSize, storeScope);
                model.MultipleThumbDirectories_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.MultipleThumbDirectories, storeScope);
                model.DefaultImageQuality_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.DefaultImageQuality, storeScope);

            }
            model.PicturesStoredIntoDatabase = _pictureService.StoreInDb;
            return View(model);
        }
        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult Media(MediaSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var mediaSettings = _settingService.LoadSetting<MediaSettings>(storeScope);
            mediaSettings = model.ToEntity(mediaSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.AvatarPictureSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(mediaSettings, x => x.AvatarPictureSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(mediaSettings, x => x.AvatarPictureSize, storeScope);
            
            if (model.ProductThumbPictureSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(mediaSettings, x => x.ProductThumbPictureSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(mediaSettings, x => x.ProductThumbPictureSize, storeScope);
            
            if (model.ProductDetailsPictureSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(mediaSettings, x => x.ProductDetailsPictureSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(mediaSettings, x => x.ProductDetailsPictureSize, storeScope);
            
            if (model.ProductThumbPictureSizeOnProductDetailsPage_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(mediaSettings, x => x.ProductThumbPictureSizeOnProductDetailsPage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(mediaSettings, x => x.ProductThumbPictureSizeOnProductDetailsPage, storeScope);

            if (model.AssociatedProductPictureSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(mediaSettings, x => x.AssociatedProductPictureSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(mediaSettings, x => x.AssociatedProductPictureSize, storeScope);
            
            if (model.CategoryThumbPictureSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(mediaSettings, x => x.CategoryThumbPictureSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(mediaSettings, x => x.CategoryThumbPictureSize, storeScope);
            
            if (model.ManufacturerThumbPictureSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(mediaSettings, x => x.ManufacturerThumbPictureSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(mediaSettings, x => x.ManufacturerThumbPictureSize, storeScope);

            if (model.VendorThumbPictureSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(mediaSettings, x => x.VendorThumbPictureSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(mediaSettings, x => x.VendorThumbPictureSize, storeScope);

            if (model.CartThumbPictureSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(mediaSettings, x => x.CartThumbPictureSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(mediaSettings, x => x.CartThumbPictureSize, storeScope);
            
            if (model.MiniCartThumbPictureSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(mediaSettings, x => x.MiniCartThumbPictureSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(mediaSettings, x => x.MiniCartThumbPictureSize, storeScope);

            if (model.MaximumImageSize_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(mediaSettings, x => x.MaximumImageSize, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(mediaSettings, x => x.MaximumImageSize, storeScope);

            if (model.MultipleThumbDirectories_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(mediaSettings, x => x.MultipleThumbDirectories, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(mediaSettings, x => x.MultipleThumbDirectories, storeScope);

            if (model.DefaultImageQuality_OverrideForStore || storeScope == "")
              _settingService.SaveSetting(mediaSettings, x => x.DefaultImageQuality, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
              _settingService.DeleteSetting(mediaSettings, x => x.DefaultImageQuality, storeScope);



            //now clear cache
            var cacheManager = Grand.Core.Infrastructure.EngineContext.Current.Resolve<Grand.Core.Caching.ICacheManager>();
            cacheManager.Clear();

            //clear old Thumbs
            _pictureService.ClearThumbs();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Media");
        }
        [HttpPost, ActionName("Media")]
        [FormValueRequired("change-picture-storage")]
        public IActionResult ChangePictureStorage()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            _pictureService.StoreInDb = !_pictureService.StoreInDb;

            //activity log
            _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Media");
        }



        public IActionResult CustomerUser()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var customerSettings = _settingService.LoadSetting<CustomerSettings>(storeScope);
            var addressSettings = _settingService.LoadSetting<AddressSettings>(storeScope);
            var dateTimeSettings = _settingService.LoadSetting<DateTimeSettings>(storeScope);
            var externalAuthenticationSettings = _settingService.LoadSetting<ExternalAuthenticationSettings>(storeScope);

            //merge settings
            var model = new CustomerUserSettingsModel();
            model.CustomerSettings = customerSettings.ToModel();
            model.AddressSettings = addressSettings.ToModel();

            model.DateTimeSettings.AllowCustomersToSetTimeZone = dateTimeSettings.AllowCustomersToSetTimeZone;
            model.DateTimeSettings.DefaultStoreTimeZoneId = _dateTimeHelper.DefaultStoreTimeZone.Id;
            foreach (TimeZoneInfo timeZone in _dateTimeHelper.GetSystemTimeZones())
            {
                model.DateTimeSettings.AvailableTimeZones.Add(new SelectListItem
                    {
                        Text = timeZone.DisplayName,
                        Value = timeZone.Id,
                        Selected = timeZone.Id.Equals(_dateTimeHelper.DefaultStoreTimeZone.Id, StringComparison.OrdinalIgnoreCase)
                    });
            }

            model.ExternalAuthenticationSettings.AutoRegisterEnabled = externalAuthenticationSettings.AutoRegisterEnabled;

            return View(model);
        }
        [HttpPost]
        public IActionResult CustomerUser(CustomerUserSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();


            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var customerSettings = _settingService.LoadSetting<CustomerSettings>(storeScope);
            var addressSettings = _settingService.LoadSetting<AddressSettings>(storeScope);
            var dateTimeSettings = _settingService.LoadSetting<DateTimeSettings>(storeScope);
            var externalAuthenticationSettings = _settingService.LoadSetting<ExternalAuthenticationSettings>(storeScope);

            customerSettings = model.CustomerSettings.ToEntity(customerSettings);
            _settingService.SaveSetting(customerSettings);

            addressSettings = model.AddressSettings.ToEntity(addressSettings);
            _settingService.SaveSetting(addressSettings);

            dateTimeSettings.DefaultStoreTimeZoneId = model.DateTimeSettings.DefaultStoreTimeZoneId;
            dateTimeSettings.AllowCustomersToSetTimeZone = model.DateTimeSettings.AllowCustomersToSetTimeZone;
            _settingService.SaveSetting(dateTimeSettings);

            externalAuthenticationSettings.AutoRegisterEnabled = model.ExternalAuthenticationSettings.AutoRegisterEnabled;
            _settingService.SaveSetting(externalAuthenticationSettings);

            //activity log
            _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));

            //selected tab
            SaveSelectedTabIndex();

            return RedirectToAction("CustomerUser");
        }


        public IActionResult GeneralCommon()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = new GeneralCommonSettingsModel();
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            model.ActiveStoreScopeConfiguration = storeScope;
            //store information
            var storeInformationSettings = _settingService.LoadSetting<StoreInformationSettings>(storeScope);
            var commonSettings = _settingService.LoadSetting<CommonSettings>(storeScope);
            var googleAnalyticsSettings = _settingService.LoadSetting<GoogleAnalyticsSettings>(storeScope);
            var adminareasettings = _settingService.LoadSetting<AdminAreaSettings>(storeScope);

            model.StoreInformationSettings.StoreClosed = storeInformationSettings.StoreClosed;
            model.Layout = string.IsNullOrEmpty(adminareasettings.AdminLayout) ? (AdminLayout)Enum.Parse(typeof(AdminLayout), "Default") : (AdminLayout)Enum.Parse(typeof(AdminLayout), adminareasettings.AdminLayout);
            model.GridLayout = string.IsNullOrEmpty(adminareasettings.KendoLayout) ? (KendoLayout)Enum.Parse(typeof(KendoLayout), "custom") : (KendoLayout)Enum.Parse(typeof(KendoLayout), adminareasettings.KendoLayout);

            //themes
            model.StoreInformationSettings.DefaultStoreTheme = storeInformationSettings.DefaultStoreTheme;
            model.StoreInformationSettings.AvailableStoreThemes = _themeProvider
                .GetThemeConfigurations()
                .Select(x => new GeneralCommonSettingsModel.StoreInformationSettingsModel.ThemeConfigurationModel
                {
                    ThemeTitle = x.ThemeTitle,
                    ThemeName = x.ThemeName,
                    PreviewImageUrl = x.PreviewImageUrl,
                    PreviewText = x.PreviewText,
                    SupportRtl = x.SupportRtl,
                    Selected = x.ThemeName.Equals(storeInformationSettings.DefaultStoreTheme, StringComparison.OrdinalIgnoreCase)
                })
                .ToList();
            model.StoreInformationSettings.AllowCustomerToSelectTheme = storeInformationSettings.AllowCustomerToSelectTheme;

            model.StoreInformationSettings.LogoPictureId = storeInformationSettings.LogoPictureId;
            //EU Cookie law
            model.StoreInformationSettings.DisplayEuCookieLawWarning = storeInformationSettings.DisplayEuCookieLawWarning;
            //social pages
            model.StoreInformationSettings.FacebookLink = storeInformationSettings.FacebookLink;
            model.StoreInformationSettings.TwitterLink = storeInformationSettings.TwitterLink;
            model.StoreInformationSettings.YoutubeLink = storeInformationSettings.YoutubeLink;
            model.StoreInformationSettings.GooglePlusLink = storeInformationSettings.GooglePlusLink;
            //contact us
            model.StoreInformationSettings.StoreInDatabaseContactUsForm = commonSettings.StoreInDatabaseContactUsForm;
            model.StoreInformationSettings.SubjectFieldOnContactUsForm = commonSettings.SubjectFieldOnContactUsForm;
            model.StoreInformationSettings.UseSystemEmailForContactUsForm = commonSettings.UseSystemEmailForContactUsForm;
            //override settings
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.StoreInformationSettings.StoreClosed_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.StoreClosed, storeScope);
                model.StoreInformationSettings.DefaultStoreTheme_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.DefaultStoreTheme, storeScope);
                model.StoreInformationSettings.AllowCustomerToSelectTheme_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.AllowCustomerToSelectTheme, storeScope);
                model.StoreInformationSettings.LogoPictureId_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.LogoPictureId, storeScope);
                model.StoreInformationSettings.DisplayEuCookieLawWarning_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.DisplayEuCookieLawWarning, storeScope);
                model.StoreInformationSettings.FacebookLink_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.FacebookLink, storeScope);
                model.StoreInformationSettings.TwitterLink_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.TwitterLink, storeScope);
                model.StoreInformationSettings.YoutubeLink_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.YoutubeLink, storeScope);
                model.StoreInformationSettings.GooglePlusLink_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.GooglePlusLink, storeScope);
                model.StoreInformationSettings.StoreInDatabaseContactUsForm_OverrideForStore = _settingService.SettingExists(commonSettings, x => x.StoreInDatabaseContactUsForm, storeScope);
                model.StoreInformationSettings.SubjectFieldOnContactUsForm_OverrideForStore = _settingService.SettingExists(commonSettings, x => x.SubjectFieldOnContactUsForm, storeScope);
                model.StoreInformationSettings.UseSystemEmailForContactUsForm_OverrideForStore = _settingService.SettingExists(commonSettings, x => x.UseSystemEmailForContactUsForm, storeScope);
            }

            //seo settings
            var seoSettings = _settingService.LoadSetting<SeoSettings>(storeScope);
            model.SeoSettings.PageTitleSeparator = seoSettings.PageTitleSeparator;
            model.SeoSettings.PageTitleSeoAdjustment = (int)seoSettings.PageTitleSeoAdjustment;
            model.SeoSettings.PageTitleSeoAdjustmentValues = seoSettings.PageTitleSeoAdjustment.ToSelectList();
            model.SeoSettings.DefaultTitle = seoSettings.DefaultTitle;
            model.SeoSettings.DefaultMetaKeywords = seoSettings.DefaultMetaKeywords;
            model.SeoSettings.DefaultMetaDescription = seoSettings.DefaultMetaDescription;
            model.SeoSettings.GenerateProductMetaDescription = seoSettings.GenerateProductMetaDescription;
            model.SeoSettings.ConvertNonWesternChars = seoSettings.ConvertNonWesternChars;
            model.SeoSettings.CanonicalUrlsEnabled = seoSettings.CanonicalUrlsEnabled;
            model.SeoSettings.WwwRequirement = (int)seoSettings.WwwRequirement;
            model.SeoSettings.WwwRequirementValues = seoSettings.WwwRequirement.ToSelectList();
            model.SeoSettings.EnableJsBundling = seoSettings.EnableJsBundling;
            model.SeoSettings.EnableCssBundling = seoSettings.EnableCssBundling;
            model.SeoSettings.TwitterMetaTags = seoSettings.TwitterMetaTags;
            model.SeoSettings.OpenGraphMetaTags = seoSettings.OpenGraphMetaTags;
            //override settings
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.SeoSettings.PageTitleSeparator_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.PageTitleSeparator, storeScope);
                model.SeoSettings.PageTitleSeoAdjustment_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.PageTitleSeoAdjustment, storeScope);
                model.SeoSettings.DefaultTitle_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.DefaultTitle, storeScope);
                model.SeoSettings.DefaultMetaKeywords_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.DefaultMetaKeywords, storeScope);
                model.SeoSettings.DefaultMetaDescription_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.DefaultMetaDescription, storeScope);
                model.SeoSettings.GenerateProductMetaDescription_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.GenerateProductMetaDescription, storeScope);
                model.SeoSettings.ConvertNonWesternChars_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.ConvertNonWesternChars, storeScope);
                model.SeoSettings.CanonicalUrlsEnabled_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.CanonicalUrlsEnabled, storeScope);
                model.SeoSettings.WwwRequirement_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.WwwRequirement, storeScope);
                model.SeoSettings.EnableJsBundling_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.EnableJsBundling, storeScope);
                model.SeoSettings.EnableCssBundling_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.EnableCssBundling, storeScope);
                model.SeoSettings.TwitterMetaTags_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.TwitterMetaTags, storeScope);
                model.SeoSettings.OpenGraphMetaTags_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.OpenGraphMetaTags, storeScope);
            }
            
            //security settings
            var securitySettings = _settingService.LoadSetting<SecuritySettings>(storeScope);
            var captchaSettings = _settingService.LoadSetting<CaptchaSettings>(storeScope);
            model.SecuritySettings.EncryptionKey = securitySettings.EncryptionKey;
            if (securitySettings.AdminAreaAllowedIpAddresses != null)
                for (int i = 0; i < securitySettings.AdminAreaAllowedIpAddresses.Count; i++)
                {
                    model.SecuritySettings.AdminAreaAllowedIpAddresses += securitySettings.AdminAreaAllowedIpAddresses[i];
                    if (i != securitySettings.AdminAreaAllowedIpAddresses.Count - 1)
                        model.SecuritySettings.AdminAreaAllowedIpAddresses += ",";
                }
            model.SecuritySettings.EnableXsrfProtectionForAdminArea = securitySettings.EnableXsrfProtectionForAdminArea;
            model.SecuritySettings.EnableXsrfProtectionForPublicStore = securitySettings.EnableXsrfProtectionForPublicStore;
            model.SecuritySettings.HoneypotEnabled = securitySettings.HoneypotEnabled;
            model.SecuritySettings.CaptchaEnabled = captchaSettings.Enabled;
            model.SecuritySettings.CaptchaShowOnLoginPage = captchaSettings.ShowOnLoginPage;
            model.SecuritySettings.CaptchaShowOnRegistrationPage = captchaSettings.ShowOnRegistrationPage;
            model.SecuritySettings.CaptchaShowOnContactUsPage = captchaSettings.ShowOnContactUsPage;
            model.SecuritySettings.CaptchaShowOnEmailWishlistToFriendPage = captchaSettings.ShowOnEmailWishlistToFriendPage;
            model.SecuritySettings.CaptchaShowOnEmailProductToFriendPage = captchaSettings.ShowOnEmailProductToFriendPage;
            model.SecuritySettings.CaptchaShowOnAskQuestionPage = captchaSettings.ShowOnAskQuestionPage;
            model.SecuritySettings.CaptchaShowOnBlogCommentPage = captchaSettings.ShowOnBlogCommentPage;
            model.SecuritySettings.CaptchaShowOnNewsCommentPage = captchaSettings.ShowOnNewsCommentPage;
            model.SecuritySettings.CaptchaShowOnProductReviewPage = captchaSettings.ShowOnProductReviewPage;
            model.SecuritySettings.CaptchaShowOnApplyVendorPage = captchaSettings.ShowOnApplyVendorPage;
            model.SecuritySettings.ReCaptchaVersion = captchaSettings.ReCaptchaVersion;
            model.SecuritySettings.AvailableReCaptchaVersions = ReCaptchaVersion.Version2.ToSelectList(false).ToList();
            model.SecuritySettings.ReCaptchaPublicKey = captchaSettings.ReCaptchaPublicKey;
            model.SecuritySettings.ReCaptchaPrivateKey = captchaSettings.ReCaptchaPrivateKey;

            //PDF settings
            var pdfSettings = _settingService.LoadSetting<PdfSettings>(storeScope);
            model.PdfSettings.LetterPageSizeEnabled = pdfSettings.LetterPageSizeEnabled;
            model.PdfSettings.LogoPictureId = pdfSettings.LogoPictureId;
            model.PdfSettings.DisablePdfInvoicesForPendingOrders = pdfSettings.DisablePdfInvoicesForPendingOrders;
            model.PdfSettings.InvoiceFooterTextColumn1 = pdfSettings.InvoiceFooterTextColumn1;
            model.PdfSettings.InvoiceFooterTextColumn2 = pdfSettings.InvoiceFooterTextColumn2;
            //override settings
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.PdfSettings.LetterPageSizeEnabled_OverrideForStore = _settingService.SettingExists(pdfSettings, x => x.LetterPageSizeEnabled, storeScope);
                model.PdfSettings.LogoPictureId_OverrideForStore = _settingService.SettingExists(pdfSettings, x => x.LogoPictureId, storeScope);
                model.PdfSettings.DisablePdfInvoicesForPendingOrders_OverrideForStore = _settingService.SettingExists(pdfSettings, x => x.DisablePdfInvoicesForPendingOrders, storeScope);
                model.PdfSettings.InvoiceFooterTextColumn1_OverrideForStore = _settingService.SettingExists(pdfSettings, x => x.InvoiceFooterTextColumn1, storeScope);
                model.PdfSettings.InvoiceFooterTextColumn2_OverrideForStore = _settingService.SettingExists(pdfSettings, x => x.InvoiceFooterTextColumn2, storeScope);
            }

            //localization
            var localizationSettings = _settingService.LoadSetting<LocalizationSettings>(storeScope);
            model.LocalizationSettings.UseImagesForLanguageSelection = localizationSettings.UseImagesForLanguageSelection;
            model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled = localizationSettings.SeoFriendlyUrlsForLanguagesEnabled;
            model.LocalizationSettings.AutomaticallyDetectLanguage = localizationSettings.AutomaticallyDetectLanguage;
            model.LocalizationSettings.LoadAllLocaleRecordsOnStartup = localizationSettings.LoadAllLocaleRecordsOnStartup;
            model.LocalizationSettings.LoadAllLocalizedPropertiesOnStartup = localizationSettings.LoadAllLocalizedPropertiesOnStartup;
            model.LocalizationSettings.LoadAllUrlRecordsOnStartup = localizationSettings.LoadAllUrlRecordsOnStartup;

            //full-text support
            model.FullTextSettings.Supported = true;
            model.FullTextSettings.Enabled = commonSettings.UseFullTextSearch;
            model.FullTextSettings.SearchMode = (int)commonSettings.FullTextMode;
            model.FullTextSettings.SearchModeValues = commonSettings.FullTextMode.ToSelectList();

            //google analytics
            model.GoogleAnalyticsSettings.gaprivateKey = googleAnalyticsSettings.gaprivateKey;
            model.GoogleAnalyticsSettings.gaserviceAccountEmail = googleAnalyticsSettings.gaserviceAccountEmail;
            model.GoogleAnalyticsSettings.gaviewID = googleAnalyticsSettings.gaviewID;

            if (!String.IsNullOrEmpty(storeScope))
            {
                model.GoogleAnalyticsSettings.gaprivateKey_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.gaprivateKey, storeScope);
                model.GoogleAnalyticsSettings.gaserviceAccountEmail_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.gaserviceAccountEmail, storeScope);
                model.GoogleAnalyticsSettings.gaviewID_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.gaviewID, storeScope);
            }

            //display menu settings
            var displayMenuItemSettings = _settingService.LoadSetting<MenuItemSettings>(storeScope);
            model.DisplayMenuSettings.DisplayHomePageMenu = displayMenuItemSettings.DisplayHomePageMenu;
            model.DisplayMenuSettings.DisplayNewProductsMenu = displayMenuItemSettings.DisplayNewProductsMenu;
            model.DisplayMenuSettings.DisplaySearchMenu = displayMenuItemSettings.DisplaySearchMenu;
            model.DisplayMenuSettings.DisplayCustomerMenu = displayMenuItemSettings.DisplayCustomerMenu;
            model.DisplayMenuSettings.DisplayBlogMenu = displayMenuItemSettings.DisplayBlogMenu;
            model.DisplayMenuSettings.DisplayForumsMenu = displayMenuItemSettings.DisplayForumsMenu;
            model.DisplayMenuSettings.DisplayContactUsMenu = displayMenuItemSettings.DisplayContactUsMenu;
            //override settings
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.DisplayMenuSettings.DisplayHomePageMenu_OverrideForStore = _settingService.SettingExists(displayMenuItemSettings, x => x.DisplayHomePageMenu, storeScope);
                model.DisplayMenuSettings.DisplayNewProductsMenu_OverrideForStore = _settingService.SettingExists(displayMenuItemSettings, x => x.DisplayNewProductsMenu, storeScope);
                model.DisplayMenuSettings.DisplaySearchMenu_OverrideForStore = _settingService.SettingExists(displayMenuItemSettings, x => x.DisplaySearchMenu, storeScope);
                model.DisplayMenuSettings.DisplayCustomerMenu_OverrideForStore = _settingService.SettingExists(displayMenuItemSettings, x => x.DisplayCustomerMenu, storeScope);
                model.DisplayMenuSettings.DisplayBlogMenu_OverrideForStore = _settingService.SettingExists(displayMenuItemSettings, x => x.DisplayBlogMenu, storeScope);
                model.DisplayMenuSettings.DisplayForumsMenu_OverrideForStore = _settingService.SettingExists(displayMenuItemSettings, x => x.DisplayForumsMenu, storeScope);
                model.DisplayMenuSettings.DisplayContactUsMenu_OverrideForStore = _settingService.SettingExists(displayMenuItemSettings, x => x.DisplayContactUsMenu, storeScope);
            }

            //knowledgebase
            var knowledgebaseSettings = _settingService.LoadSetting<KnowledgebaseSettings>(storeScope);
            model.KnowledgebaseSettings.Enabled = knowledgebaseSettings.Enabled;


            return View(model);
        }
        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult GeneralCommon(GeneralCommonSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();


            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

            //store information settings
            var storeInformationSettings = _settingService.LoadSetting<StoreInformationSettings>(storeScope);
            var commonSettings = _settingService.LoadSetting<CommonSettings>(storeScope);

            storeInformationSettings.StoreClosed = model.StoreInformationSettings.StoreClosed;
            storeInformationSettings.DefaultStoreTheme = model.StoreInformationSettings.DefaultStoreTheme;
            storeInformationSettings.AllowCustomerToSelectTheme = model.StoreInformationSettings.AllowCustomerToSelectTheme;
            storeInformationSettings.LogoPictureId = model.StoreInformationSettings.LogoPictureId;
            //EU Cookie law
            storeInformationSettings.DisplayEuCookieLawWarning = model.StoreInformationSettings.DisplayEuCookieLawWarning;
            //social pages
            storeInformationSettings.FacebookLink = model.StoreInformationSettings.FacebookLink;
            storeInformationSettings.TwitterLink = model.StoreInformationSettings.TwitterLink;
            storeInformationSettings.YoutubeLink = model.StoreInformationSettings.YoutubeLink;
            storeInformationSettings.GooglePlusLink = model.StoreInformationSettings.GooglePlusLink;
            //contact us
            commonSettings.StoreInDatabaseContactUsForm = model.StoreInformationSettings.StoreInDatabaseContactUsForm;
            commonSettings.SubjectFieldOnContactUsForm = model.StoreInformationSettings.SubjectFieldOnContactUsForm;
            commonSettings.UseSystemEmailForContactUsForm = model.StoreInformationSettings.UseSystemEmailForContactUsForm;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            if (model.StoreInformationSettings.StoreClosed_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(storeInformationSettings, x => x.StoreClosed, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(storeInformationSettings, x => x.StoreClosed, storeScope);

            if (model.StoreInformationSettings.DefaultStoreTheme_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(storeInformationSettings, x => x.DefaultStoreTheme, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(storeInformationSettings, x => x.DefaultStoreTheme, storeScope);

            if (model.StoreInformationSettings.AllowCustomerToSelectTheme_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(storeInformationSettings, x => x.AllowCustomerToSelectTheme, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(storeInformationSettings, x => x.AllowCustomerToSelectTheme, storeScope);

            if (model.StoreInformationSettings.LogoPictureId_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(storeInformationSettings, x => x.LogoPictureId, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(storeInformationSettings, x => x.LogoPictureId, storeScope);

            if (model.StoreInformationSettings.DisplayEuCookieLawWarning_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(storeInformationSettings, x => x.DisplayEuCookieLawWarning, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(storeInformationSettings, x => x.DisplayEuCookieLawWarning, storeScope);

            if (model.StoreInformationSettings.FacebookLink_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(storeInformationSettings, x => x.FacebookLink, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(storeInformationSettings, x => x.FacebookLink, storeScope);

            if (model.StoreInformationSettings.TwitterLink_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(storeInformationSettings, x => x.TwitterLink, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(storeInformationSettings, x => x.TwitterLink, storeScope);

            if (model.StoreInformationSettings.YoutubeLink_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(storeInformationSettings, x => x.YoutubeLink, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(storeInformationSettings, x => x.YoutubeLink, storeScope);

            if (model.StoreInformationSettings.GooglePlusLink_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(storeInformationSettings, x => x.GooglePlusLink, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(storeInformationSettings, x => x.GooglePlusLink, storeScope);

            if (model.StoreInformationSettings.StoreInDatabaseContactUsForm_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(commonSettings, x => x.StoreInDatabaseContactUsForm, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(commonSettings, x => x.StoreInDatabaseContactUsForm, storeScope);

            if (model.StoreInformationSettings.SubjectFieldOnContactUsForm_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(commonSettings, x => x.SubjectFieldOnContactUsForm, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(commonSettings, x => x.SubjectFieldOnContactUsForm, storeScope);
            
            if (model.StoreInformationSettings.UseSystemEmailForContactUsForm_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(commonSettings, x => x.UseSystemEmailForContactUsForm, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(commonSettings, x => x.UseSystemEmailForContactUsForm, storeScope);
            

            //seo settings
            var seoSettings = _settingService.LoadSetting<SeoSettings>(storeScope);
            seoSettings.PageTitleSeparator = model.SeoSettings.PageTitleSeparator;
            seoSettings.PageTitleSeoAdjustment = (PageTitleSeoAdjustment)model.SeoSettings.PageTitleSeoAdjustment;
            seoSettings.DefaultTitle = model.SeoSettings.DefaultTitle;
            seoSettings.DefaultMetaKeywords = model.SeoSettings.DefaultMetaKeywords;
            seoSettings.DefaultMetaDescription = model.SeoSettings.DefaultMetaDescription;
            seoSettings.GenerateProductMetaDescription = model.SeoSettings.GenerateProductMetaDescription;
            seoSettings.ConvertNonWesternChars = model.SeoSettings.ConvertNonWesternChars;
            seoSettings.CanonicalUrlsEnabled = model.SeoSettings.CanonicalUrlsEnabled;
            seoSettings.WwwRequirement = (WwwRequirement)model.SeoSettings.WwwRequirement;
            seoSettings.EnableJsBundling = model.SeoSettings.EnableJsBundling;
            seoSettings.EnableCssBundling = model.SeoSettings.EnableCssBundling;
            seoSettings.TwitterMetaTags = model.SeoSettings.TwitterMetaTags;
            seoSettings.OpenGraphMetaTags = model.SeoSettings.OpenGraphMetaTags;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.SeoSettings.PageTitleSeparator_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(seoSettings, x => x.PageTitleSeparator, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(seoSettings, x => x.PageTitleSeparator, storeScope);
            
            if (model.SeoSettings.PageTitleSeoAdjustment_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(seoSettings, x => x.PageTitleSeoAdjustment, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(seoSettings, x => x.PageTitleSeoAdjustment, storeScope);
            
            if (model.SeoSettings.DefaultTitle_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(seoSettings, x => x.DefaultTitle, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(seoSettings, x => x.DefaultTitle, storeScope);
            
            if (model.SeoSettings.DefaultMetaKeywords_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(seoSettings, x => x.DefaultMetaKeywords, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(seoSettings, x => x.DefaultMetaKeywords, storeScope);
            
            if (model.SeoSettings.DefaultMetaDescription_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(seoSettings, x => x.DefaultMetaDescription, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(seoSettings, x => x.DefaultMetaDescription, storeScope);

            if (model.SeoSettings.GenerateProductMetaDescription_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(seoSettings, x => x.GenerateProductMetaDescription, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(seoSettings, x => x.GenerateProductMetaDescription, storeScope);
            
            if (model.SeoSettings.ConvertNonWesternChars_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(seoSettings, x => x.ConvertNonWesternChars, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(seoSettings, x => x.ConvertNonWesternChars, storeScope);
            
            if (model.SeoSettings.CanonicalUrlsEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(seoSettings, x => x.CanonicalUrlsEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(seoSettings, x => x.CanonicalUrlsEnabled, storeScope);

            if (model.SeoSettings.WwwRequirement_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(seoSettings, x => x.WwwRequirement, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(seoSettings, x => x.WwwRequirement, storeScope);
            
            if (model.SeoSettings.EnableJsBundling_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(seoSettings, x => x.EnableJsBundling, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(seoSettings, x => x.EnableJsBundling, storeScope);

            if (model.SeoSettings.EnableCssBundling_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(seoSettings, x => x.EnableCssBundling, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(seoSettings, x => x.EnableCssBundling, storeScope);

            if (model.SeoSettings.TwitterMetaTags_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(seoSettings, x => x.TwitterMetaTags, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(seoSettings, x => x.TwitterMetaTags, storeScope);

            if (model.SeoSettings.OpenGraphMetaTags_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(seoSettings, x => x.OpenGraphMetaTags, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(seoSettings, x => x.OpenGraphMetaTags, storeScope);

            //security settings
            var securitySettings = _settingService.LoadSetting<SecuritySettings>(storeScope);
            var captchaSettings = _settingService.LoadSetting<CaptchaSettings>(storeScope);
            if (securitySettings.AdminAreaAllowedIpAddresses == null)
                securitySettings.AdminAreaAllowedIpAddresses = new List<string>();
            securitySettings.AdminAreaAllowedIpAddresses.Clear();
            if (!String.IsNullOrEmpty(model.SecuritySettings.AdminAreaAllowedIpAddresses))
                foreach (string s in model.SecuritySettings.AdminAreaAllowedIpAddresses.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    if (!String.IsNullOrWhiteSpace(s))
                        securitySettings.AdminAreaAllowedIpAddresses.Add(s.Trim());
            securitySettings.EnableXsrfProtectionForAdminArea = model.SecuritySettings.EnableXsrfProtectionForAdminArea;
            securitySettings.EnableXsrfProtectionForPublicStore = model.SecuritySettings.EnableXsrfProtectionForPublicStore;
            securitySettings.HoneypotEnabled = model.SecuritySettings.HoneypotEnabled;
            _settingService.SaveSetting(securitySettings);
            captchaSettings.Enabled = model.SecuritySettings.CaptchaEnabled;
            captchaSettings.ShowOnLoginPage = model.SecuritySettings.CaptchaShowOnLoginPage;
            captchaSettings.ShowOnRegistrationPage = model.SecuritySettings.CaptchaShowOnRegistrationPage;
            captchaSettings.ShowOnContactUsPage = model.SecuritySettings.CaptchaShowOnContactUsPage;
            captchaSettings.ShowOnEmailWishlistToFriendPage = model.SecuritySettings.CaptchaShowOnEmailWishlistToFriendPage;
            captchaSettings.ShowOnAskQuestionPage = model.SecuritySettings.CaptchaShowOnAskQuestionPage;
            captchaSettings.ShowOnEmailProductToFriendPage = model.SecuritySettings.CaptchaShowOnEmailProductToFriendPage;
            captchaSettings.ShowOnBlogCommentPage = model.SecuritySettings.CaptchaShowOnBlogCommentPage;
            captchaSettings.ShowOnNewsCommentPage = model.SecuritySettings.CaptchaShowOnNewsCommentPage;
            captchaSettings.ShowOnProductReviewPage = model.SecuritySettings.CaptchaShowOnProductReviewPage;
            captchaSettings.ShowOnApplyVendorPage = model.SecuritySettings.CaptchaShowOnApplyVendorPage;
            captchaSettings.ReCaptchaVersion = model.SecuritySettings.ReCaptchaVersion;
            captchaSettings.ReCaptchaPublicKey = model.SecuritySettings.ReCaptchaPublicKey;
            captchaSettings.ReCaptchaPrivateKey = model.SecuritySettings.ReCaptchaPrivateKey;
            _settingService.SaveSetting(captchaSettings);
            if (captchaSettings.Enabled &&
                (String.IsNullOrWhiteSpace(captchaSettings.ReCaptchaPublicKey) || String.IsNullOrWhiteSpace(captchaSettings.ReCaptchaPrivateKey)))
            {
                //captcha is enabled but the keys are not entered
                ErrorNotification("Captcha is enabled but the appropriate keys are not entered");
            }

            //PDF settings
            var pdfSettings = _settingService.LoadSetting<PdfSettings>(storeScope);
            pdfSettings.LetterPageSizeEnabled = model.PdfSettings.LetterPageSizeEnabled;
            pdfSettings.LogoPictureId = model.PdfSettings.LogoPictureId;
            pdfSettings.DisablePdfInvoicesForPendingOrders = model.PdfSettings.DisablePdfInvoicesForPendingOrders;
            pdfSettings.InvoiceFooterTextColumn1 = model.PdfSettings.InvoiceFooterTextColumn1;
            pdfSettings.InvoiceFooterTextColumn2 = model.PdfSettings.InvoiceFooterTextColumn2;
            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            
            if (model.PdfSettings.LetterPageSizeEnabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(pdfSettings, x => x.LetterPageSizeEnabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(pdfSettings, x => x.LetterPageSizeEnabled, storeScope);

            if (model.PdfSettings.LogoPictureId_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(pdfSettings, x => x.LogoPictureId, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(pdfSettings, x => x.LogoPictureId, storeScope);

            if (model.PdfSettings.DisablePdfInvoicesForPendingOrders_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(pdfSettings, x => x.DisablePdfInvoicesForPendingOrders, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(pdfSettings, x => x.DisablePdfInvoicesForPendingOrders, storeScope);

            if (model.PdfSettings.InvoiceFooterTextColumn1_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(pdfSettings, x => x.InvoiceFooterTextColumn1, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(pdfSettings, x => x.InvoiceFooterTextColumn1, storeScope);

            if (model.PdfSettings.InvoiceFooterTextColumn2_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(pdfSettings, x => x.InvoiceFooterTextColumn2, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(pdfSettings, x => x.InvoiceFooterTextColumn2, storeScope);

            //localization settings
            var localizationSettings = _settingService.LoadSetting<LocalizationSettings>(storeScope);
            localizationSettings.UseImagesForLanguageSelection = model.LocalizationSettings.UseImagesForLanguageSelection;
            if (localizationSettings.SeoFriendlyUrlsForLanguagesEnabled != model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                localizationSettings.SeoFriendlyUrlsForLanguagesEnabled = model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled;
                //clear cached values of routes
                this.RouteData.Routers.ClearSeoFriendlyUrlsCachedValueForRoutes();
            }
            localizationSettings.AutomaticallyDetectLanguage = model.LocalizationSettings.AutomaticallyDetectLanguage;
            localizationSettings.LoadAllLocaleRecordsOnStartup = model.LocalizationSettings.LoadAllLocaleRecordsOnStartup;
            localizationSettings.LoadAllLocalizedPropertiesOnStartup = model.LocalizationSettings.LoadAllLocalizedPropertiesOnStartup;
            localizationSettings.LoadAllUrlRecordsOnStartup = model.LocalizationSettings.LoadAllUrlRecordsOnStartup;
            _settingService.SaveSetting(localizationSettings);

            //full-text
            commonSettings.FullTextMode = (FulltextSearchMode)model.FullTextSettings.SearchMode;
            _settingService.SaveSetting(commonSettings);

            //admin settings
            var adminareasettings = _settingService.LoadSetting<AdminAreaSettings>(storeScope);
            adminareasettings.AdminLayout = model.Layout.ToString();
            adminareasettings.KendoLayout = model.GridLayout.ToString();

            _settingService.SaveSetting(adminareasettings);

            //googleanalytics settings
            var googleAnalyticsSettings = _settingService.LoadSetting<GoogleAnalyticsSettings>(storeScope);
            googleAnalyticsSettings.gaprivateKey = model.GoogleAnalyticsSettings.gaprivateKey;
            googleAnalyticsSettings.gaserviceAccountEmail = model.GoogleAnalyticsSettings.gaserviceAccountEmail;
            googleAnalyticsSettings.gaviewID = model.GoogleAnalyticsSettings.gaviewID;

            if (model.GoogleAnalyticsSettings.gaprivateKey_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(googleAnalyticsSettings, x => x.gaprivateKey, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(googleAnalyticsSettings, x => x.gaprivateKey, storeScope);

            if (model.GoogleAnalyticsSettings.gaserviceAccountEmail_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(googleAnalyticsSettings, x => x.gaserviceAccountEmail, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(googleAnalyticsSettings, x => x.gaserviceAccountEmail, storeScope);

            if (model.GoogleAnalyticsSettings.gaviewID_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(googleAnalyticsSettings, x => x.gaviewID, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(googleAnalyticsSettings, x => x.gaviewID, storeScope);


            //Menu item settings
            var displayMenuItemSettings = _settingService.LoadSetting<MenuItemSettings>(storeScope);
            displayMenuItemSettings.DisplayHomePageMenu = model.DisplayMenuSettings.DisplayHomePageMenu;
            displayMenuItemSettings.DisplayNewProductsMenu = model.DisplayMenuSettings.DisplayNewProductsMenu;
            displayMenuItemSettings.DisplaySearchMenu = model.DisplayMenuSettings.DisplaySearchMenu;
            displayMenuItemSettings.DisplayCustomerMenu = model.DisplayMenuSettings.DisplayCustomerMenu;
            displayMenuItemSettings.DisplayBlogMenu = model.DisplayMenuSettings.DisplayBlogMenu;
            displayMenuItemSettings.DisplayForumsMenu = model.DisplayMenuSettings.DisplayForumsMenu;
            displayMenuItemSettings.DisplayContactUsMenu = model.DisplayMenuSettings.DisplayContactUsMenu;

            if (model.DisplayMenuSettings.DisplayHomePageMenu_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(displayMenuItemSettings, x => x.DisplayHomePageMenu, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(displayMenuItemSettings, x => x.DisplayHomePageMenu, storeScope);

            if (model.DisplayMenuSettings.DisplayNewProductsMenu_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(displayMenuItemSettings, x => x.DisplayNewProductsMenu, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(displayMenuItemSettings, x => x.DisplayNewProductsMenu, storeScope);

            if (model.DisplayMenuSettings.DisplaySearchMenu_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(displayMenuItemSettings, x => x.DisplaySearchMenu, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(displayMenuItemSettings, x => x.DisplaySearchMenu, storeScope);

            if (model.DisplayMenuSettings.DisplayCustomerMenu_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(displayMenuItemSettings, x => x.DisplayCustomerMenu, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(displayMenuItemSettings, x => x.DisplayCustomerMenu, storeScope);

            if (model.DisplayMenuSettings.DisplayBlogMenu_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(displayMenuItemSettings, x => x.DisplayBlogMenu, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(displayMenuItemSettings, x => x.DisplayBlogMenu, storeScope);

            if (model.DisplayMenuSettings.DisplayForumsMenu_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(displayMenuItemSettings, x => x.DisplayForumsMenu, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(displayMenuItemSettings, x => x.DisplayForumsMenu, storeScope);

            if (model.DisplayMenuSettings.DisplayContactUsMenu_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(displayMenuItemSettings, x => x.DisplayContactUsMenu, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(displayMenuItemSettings, x => x.DisplayContactUsMenu, storeScope);

            //Knowledgebase
            var knowledgebaseSettings = _settingService.LoadSetting<KnowledgebaseSettings>(storeScope);
            knowledgebaseSettings.Enabled = model.KnowledgebaseSettings.Enabled;
            if (model.KnowledgebaseSettings.Enabled_OverrideForStore || storeScope == "")
                _settingService.SaveSetting(knowledgebaseSettings, x => x.Enabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(knowledgebaseSettings, x => x.Enabled, storeScope);

            //now clear cache
            _cacheManager.Clear();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));

            //selected tab
            SaveSelectedTabIndex();

            return RedirectToAction("GeneralCommon");
        }
        [HttpPost, ActionName("GeneralCommon")]
        [FormValueRequired("changeencryptionkey")]
        public IActionResult ChangeEncryptionKey(GeneralCommonSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var securitySettings = _settingService.LoadSetting<SecuritySettings>(storeScope);

            try
            {
                if (model.SecuritySettings.EncryptionKey == null)
                    model.SecuritySettings.EncryptionKey = "";

                model.SecuritySettings.EncryptionKey = model.SecuritySettings.EncryptionKey.Trim();

                var newEncryptionPrivateKey = model.SecuritySettings.EncryptionKey;
                if (String.IsNullOrEmpty(newEncryptionPrivateKey) || newEncryptionPrivateKey.Length != 16)
                    throw new GrandException(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TooShort"));

                string oldEncryptionPrivateKey = securitySettings.EncryptionKey;
                if (oldEncryptionPrivateKey == newEncryptionPrivateKey)
                    throw new GrandException(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TheSame"));

                //update encrypted order info
                var orders = _orderService.SearchOrders();
                foreach (var order in orders)
                {
                    string decryptedCardType = _encryptionService.DecryptText(order.CardType, oldEncryptionPrivateKey);
                    string decryptedCardName = _encryptionService.DecryptText(order.CardName, oldEncryptionPrivateKey);
                    string decryptedCardNumber = _encryptionService.DecryptText(order.CardNumber, oldEncryptionPrivateKey);
                    string decryptedMaskedCreditCardNumber = _encryptionService.DecryptText(order.MaskedCreditCardNumber, oldEncryptionPrivateKey);
                    string decryptedCardCvv2 = _encryptionService.DecryptText(order.CardCvv2, oldEncryptionPrivateKey);
                    string decryptedCardExpirationMonth = _encryptionService.DecryptText(order.CardExpirationMonth, oldEncryptionPrivateKey);
                    string decryptedCardExpirationYear = _encryptionService.DecryptText(order.CardExpirationYear, oldEncryptionPrivateKey);

                    string encryptedCardType = _encryptionService.EncryptText(decryptedCardType, newEncryptionPrivateKey);
                    string encryptedCardName = _encryptionService.EncryptText(decryptedCardName, newEncryptionPrivateKey);
                    string encryptedCardNumber = _encryptionService.EncryptText(decryptedCardNumber, newEncryptionPrivateKey);
                    string encryptedMaskedCreditCardNumber = _encryptionService.EncryptText(decryptedMaskedCreditCardNumber, newEncryptionPrivateKey);
                    string encryptedCardCvv2 = _encryptionService.EncryptText(decryptedCardCvv2, newEncryptionPrivateKey);
                    string encryptedCardExpirationMonth = _encryptionService.EncryptText(decryptedCardExpirationMonth, newEncryptionPrivateKey);
                    string encryptedCardExpirationYear = _encryptionService.EncryptText(decryptedCardExpirationYear, newEncryptionPrivateKey);

                    order.CardType = encryptedCardType;
                    order.CardName = encryptedCardName;
                    order.CardNumber = encryptedCardNumber;
                    order.MaskedCreditCardNumber = encryptedMaskedCreditCardNumber;
                    order.CardCvv2 = encryptedCardCvv2;
                    order.CardExpirationMonth = encryptedCardExpirationMonth;
                    order.CardExpirationYear = encryptedCardExpirationYear;
                    _orderService.UpdateOrder(order);
                }

                //update user information
                //optimization - load only users with PasswordFormat.Encrypted
                var customers = _customerService.GetAllCustomersByPasswordFormat(PasswordFormat.Encrypted);
                foreach (var customer in customers)
                {
                    string decryptedPassword = _encryptionService.DecryptText(customer.Password, oldEncryptionPrivateKey);
                    string encryptedPassword = _encryptionService.EncryptText(decryptedPassword, newEncryptionPrivateKey);

                    customer.Password = encryptedPassword;
                    _customerService.UpdateCustomerPassword(customer);
                }

                securitySettings.EncryptionKey = newEncryptionPrivateKey;
                _settingService.SaveSetting(securitySettings);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.Changed"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
            }

            //selected tab
            SaveSelectedTabIndex();

            return RedirectToAction("GeneralCommon");
        }
        [HttpPost, ActionName("GeneralCommon")]
        [FormValueRequired("togglefulltext")]
        public IActionResult ToggleFullText(GeneralCommonSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var commonSettings = _settingService.LoadSetting<CommonSettings>(storeScope);
            try
            {
                if (commonSettings.UseFullTextSearch)
                {
                    commonSettings.UseFullTextSearch = false;
                    _settingService.SaveSetting(commonSettings);
                    _productRepository.Collection.Indexes.DropOneAsync("ProductText");
                    SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.FullTextSettings.Disabled"));
                }
                else
                {
                    commonSettings.UseFullTextSearch = true;
                    _settingService.SaveSetting(commonSettings);
                    _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Text("$**")), new CreateIndexOptions() { Name = "ProductText" }));
                    SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.FullTextSettings.Enabled"));
                }
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
            }

            //selected tab
            SaveSelectedTabIndex();

            return RedirectToAction("GeneralCommon");
        }
        //all settings
        public IActionResult AllSettings()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();
            return View();
        }

        [HttpPost]
        [AdminAntiForgery(true)]
        public IActionResult AllSettings(DataSourceRequest command, SettingFilterModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var settings = _settingService
                .GetAllSettings()
                .Select(x =>
                {
                    string storeName;
                    if (String.IsNullOrEmpty(x.StoreId))
                    {
                        storeName = _localizationService.GetResource("Admin.Configuration.Settings.AllSettings.Fields.StoreName.AllStores");
                    }
                    else
                    {
                        var store = _storeService.GetStoreById(x.StoreId);
                        storeName = store != null ? store.Name : "Unknown";
                    }
                    var settingModel = new SettingModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Value = x.Value,
                        Store = storeName,
                        StoreId = string.IsNullOrEmpty(x.StoreId)? " ": x.StoreId
                    };
                    return settingModel;
                });
            if (model != null)
            {
                if (!string.IsNullOrEmpty(model.SettingFilterName))
                    settings = settings.Where(x => x.Name.ToLowerInvariant().Contains(model.SettingFilterName.ToLowerInvariant()));
                if (!string.IsNullOrEmpty(model.SettingFilterValue))
                    settings = settings.Where(x => x.Value.ToLowerInvariant().Contains(model.SettingFilterValue.ToLowerInvariant()));
            }
            settings = settings.AsQueryable();

            var gridModel = new DataSourceResult
            {
                Data = settings.PagedForCommand(command).ToList(),
                Total = settings.Count()
            };

            return Json(gridModel);
        }

        public IActionResult PushNotifications()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<PushNotificationsSettings>(storeScope);

            var model = new ConfigurationModel();
            model.AllowGuestNotifications = settings.AllowGuestNotifications;
            model.AuthDomain = settings.AuthDomain;
            model.DatabaseUrl = settings.DatabaseUrl;
            model.ProjectId = settings.ProjectId;
            model.PushApiKey = settings.PublicApiKey;
            model.SenderId = settings.SenderId;
            model.StorageBucket = settings.StorageBucket;
            model.PrivateApiKey = settings.PrivateApiKey;

            return View(model);
        }

        [HttpPost]
        [AdminAntiForgery(true)]
        public IActionResult PushNotifications(DataSourceRequest command, ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<PushNotificationsSettings>(storeScope);
            settings.AllowGuestNotifications = model.AllowGuestNotifications;
            settings.AuthDomain = model.AuthDomain;
            settings.DatabaseUrl = model.DatabaseUrl;
            settings.ProjectId = model.ProjectId;
            settings.PublicApiKey = model.PushApiKey;
            settings.SenderId = model.SenderId;
            settings.StorageBucket = model.StorageBucket;
            settings.PrivateApiKey = model.PrivateApiKey;
            settings.Enabled = model.Enabled;
            _settingService.SaveSetting(settings);

            //edit js file needed by firebase
            var jsFilePath = CommonHelper.MapPath("~/wwwroot/firebase-messaging-sw.js");
            if (System.IO.File.Exists(jsFilePath))
            {
                string[] lines = System.IO.File.ReadAllLines(jsFilePath);

                int i = 0;
                foreach (var line in lines)
                {
                    if (line.Contains("apiKey"))
                    {
                        lines[i] = "apiKey: \"" + model.PushApiKey + "\",";
                    }

                    if (line.Contains("authDomain"))
                    {
                        lines[i] = "authDomain: \"" + model.AuthDomain + "\",";
                    }

                    if (line.Contains("databaseURL"))
                    {
                        lines[i] = "databaseURL: \"" + model.DatabaseUrl + "\",";
                    }

                    if (line.Contains("projectId"))
                    {
                        lines[i] = "projectId: \"" + model.ProjectId + "\",";
                    }

                    if (line.Contains("storageBucket"))
                    {
                        lines[i] = "storageBucket: \"" + model.StorageBucket + "\",";
                    }

                    if (line.Contains("messagingSenderId"))
                    {
                        lines[i] = "messagingSenderId: \"" + model.SenderId + "\",";
                    }

                    i++;
                }

                System.IO.File.WriteAllLines(jsFilePath, lines);
            }

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return PushNotifications();
        }

        [HttpPost]
        [AdminAntiForgery(true)]
        public IActionResult SettingUpdate(SettingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();
            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (model.StoreId != null)
                model.StoreId = model.StoreId.Trim();
            else
                model.StoreId = "";

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var setting = _settingService.GetSettingById(model.Id);
            if (setting == null)
                return Content("No setting could be loaded with the specified ID");

            var storeId = model.StoreId;

            if (!setting.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase) ||
                setting.StoreId != storeId)
            {
                //setting name or store has been changed
                _settingService.DeleteSetting(setting);
            }

            _settingService.SetSetting(model.Name, model.Value, storeId);

            //activity log
            _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            //now clear cache
            _cacheManager.Clear();

            return new NullJsonResult();
        }
        [HttpPost]
        [AdminAntiForgery(true)]
        public IActionResult SettingAdd( SettingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();
            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (model.StoreId != null)
                model.StoreId = model.StoreId.Trim();
            else
                model.StoreId = "";

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }
            var storeId = model.StoreId;
            _settingService.SetSetting(model.Name, model.Value, storeId);

            //activity log
            _customerActivityService.InsertActivity("AddNewSetting", "", _localizationService.GetResource("ActivityLog.AddNewSetting"), model.Name);

            //now clear cache
            _cacheManager.Clear();

            return new NullJsonResult();
        }
        [HttpPost]
        [AdminAntiForgery(true)]
        public IActionResult SettingDelete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var setting = _settingService.GetSettingById(id);
            if (setting == null)
                throw new ArgumentException("No setting found with the specified id");
            _settingService.DeleteSetting(setting);

            //activity log
            _customerActivityService.InsertActivity("DeleteSetting", "", _localizationService.GetResource("ActivityLog.DeleteSetting"), setting.Name);

            //now clear cache
            _cacheManager.Clear();

            return new NullJsonResult();
        }

        #endregion
    }
}
