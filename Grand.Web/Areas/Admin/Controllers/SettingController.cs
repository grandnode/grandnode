using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.AdminSearch;
using Grand.Domain.Blogs;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Configuration;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Forums;
using Grand.Domain.Knowledgebase;
using Grand.Domain.Localization;
using Grand.Domain.Media;
using Grand.Domain.News;
using Grand.Domain.Orders;
using Grand.Domain.PushNotifications;
using Grand.Domain.Security;
using Grand.Domain.Seo;
using Grand.Domain.Shipping;
using Grand.Domain.Stores;
using Grand.Domain.Tax;
using Grand.Domain.Vendors;
using Grand.Framework.Controllers;
using Grand.Framework.Extensions;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Framework.Security.Captcha;
using Grand.Framework.Themes;
using Grand.Services.Commands.Models.Common;
using Grand.Services.Commands.Models.Orders;
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
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Models.PushNotifications;
using Grand.Web.Areas.Admin.Models.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Settings)]
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
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IMediator _mediator;
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
            IStoreService storeService,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            IMediator mediator,
            IReturnRequestService returnRequestService,
            ILanguageService languageService,
            ICacheManager cacheManager)
        {
            _settingService = settingService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _addressService = addressService;
            _taxCategoryService = taxCategoryService;
            _currencyService = currencyService;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _orderService = orderService;
            _encryptionService = encryptionService;
            _themeProvider = themeProvider;
            _customerService = customerService;
            _customerActivityService = customerActivityService;
            _storeService = storeService;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _mediator = mediator;
            _returnRequestService = returnRequestService;
            _languageService = languageService;
            _cacheManager = cacheManager;
        }

        #endregion

        #region Utilities

        private async Task UpdateOverrideForStore<T, TPropType>(string storeScope, bool overrideForStore, T settings,
            Expression<Func<T, TPropType>> keySelector) where T : ISettings, new()
        {
            if (overrideForStore || storeScope == "")
                await _settingService.SaveSetting(settings, keySelector, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(settings, keySelector, storeScope);
        }

        #endregion

        #region Methods

        protected async Task ClearCache()
        {
            await _cacheManager.Clear();
        }

        public async Task<IActionResult> ChangeStoreScopeConfiguration(string storeid, string returnUrl = "")
        {
            if (storeid != null)
                storeid = storeid.Trim();

            var store = await _storeService.GetStoreById(storeid);
            if (store != null || storeid == "")
            {
                await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration, storeid);
            }
            else
                await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration, "");


            //home page
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = Url.Action("Index", "Home", new { area = "Admin" });
            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            return Redirect(returnUrl);
        }

        public async Task<IActionResult> Blog()
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
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
        public async Task<IActionResult> Blog(BlogSettingsModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var blogSettings = _settingService.LoadSetting<BlogSettings>(storeScope);
            blogSettings = model.ToEntity(blogSettings);

            await UpdateOverrideForStore(storeScope, model.Enabled_OverrideForStore, blogSettings, x => x.Enabled);
            await UpdateOverrideForStore(storeScope, model.PostsPageSize_OverrideForStore, blogSettings, x => x.PostsPageSize);
            await UpdateOverrideForStore(storeScope, model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, blogSettings, x => x.AllowNotRegisteredUsersToLeaveComments);
            await UpdateOverrideForStore(storeScope, model.NotifyAboutNewBlogComments_OverrideForStore, blogSettings, x => x.NotifyAboutNewBlogComments);
            await UpdateOverrideForStore(storeScope, model.NumberOfTags_OverrideForStore, blogSettings, x => x.NumberOfTags);
            await UpdateOverrideForStore(storeScope, model.ShowHeaderRssUrl_OverrideForStore, blogSettings, x => x.ShowHeaderRssUrl);
            await UpdateOverrideForStore(storeScope, model.ShowBlogOnHomePage_OverrideForStore, blogSettings, x => x.ShowBlogOnHomePage);
            await UpdateOverrideForStore(storeScope, model.HomePageBlogCount_OverrideForStore, blogSettings, x => x.HomePageBlogCount);
            await UpdateOverrideForStore(storeScope, model.MaxTextSizeHomePage_OverrideForStore, blogSettings, x => x.MaxTextSizeHomePage);

            //now clear cache
            await ClearCache();

            //activity log
            await _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Blog");
        }

        public async Task<IActionResult> Vendor()
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
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
        public async Task<IActionResult> Vendor(VendorSettingsModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
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

            await UpdateOverrideForStore(storeScope, model.VendorsBlockItemsToDisplay_OverrideForStore, vendorSettings, x => x.VendorsBlockItemsToDisplay);
            await UpdateOverrideForStore(storeScope, model.ShowVendorOnProductDetailsPage_OverrideForStore, vendorSettings, x => x.ShowVendorOnProductDetailsPage);
            await UpdateOverrideForStore(storeScope, model.AllowCustomersToContactVendors_OverrideForStore, vendorSettings, x => x.AllowCustomersToContactVendors);
            await UpdateOverrideForStore(storeScope, model.AllowCustomersToApplyForVendorAccount_OverrideForStore, vendorSettings, x => x.AllowCustomersToApplyForVendorAccount);
            await UpdateOverrideForStore(storeScope, model.AllowSearchByVendor_OverrideForStore, vendorSettings, x => x.AllowSearchByVendor);
            await UpdateOverrideForStore(storeScope, model.AllowVendorsToEditInfo_OverrideForStore, vendorSettings, x => x.AllowVendorsToEditInfo);
            await UpdateOverrideForStore(storeScope, model.NotifyStoreOwnerAboutVendorInformationChange_OverrideForStore, vendorSettings, x => x.NotifyStoreOwnerAboutVendorInformationChange);
            await UpdateOverrideForStore(storeScope, model.TermsOfServiceEnabled_OverrideForStore, vendorSettings, x => x.TermsOfServiceEnabled);

            //review vendor
            await UpdateOverrideForStore(storeScope, model.VendorReviewsMustBeApproved_OverrideForStore, vendorSettings, x => x.VendorReviewsMustBeApproved);
            await UpdateOverrideForStore(storeScope, model.AllowAnonymousUsersToReviewVendor_OverrideForStore, vendorSettings, x => x.AllowAnonymousUsersToReviewVendor);
            await UpdateOverrideForStore(storeScope, model.VendorReviewPossibleOnlyAfterPurchasing_OverrideForStore, vendorSettings, x => x.VendorReviewPossibleOnlyAfterPurchasing);
            await UpdateOverrideForStore(storeScope, model.NotifyVendorAboutNewVendorReviews_OverrideForStore, vendorSettings, x => x.NotifyVendorAboutNewVendorReviews);

            await _settingService.SaveSetting(vendorSettings);

            //now clear cache
            await ClearCache();

            //activity log
            await _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Vendor");
        }

        public async Task<IActionResult> Forum()
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
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
            model.ForumEditorValues = forumSettings.ForumEditor.ToSelectList(HttpContext);

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Forum(ForumSettingsModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var forumSettings = _settingService.LoadSetting<ForumSettings>(storeScope);
            forumSettings = model.ToEntity(forumSettings);

            await UpdateOverrideForStore(storeScope, model.ForumsEnabled_OverrideForStore, forumSettings, x => x.ForumsEnabled);
            await UpdateOverrideForStore(storeScope, model.RelativeDateTimeFormattingEnabled_OverrideForStore, forumSettings, x => x.RelativeDateTimeFormattingEnabled);
            await UpdateOverrideForStore(storeScope, model.ShowCustomersPostCount_OverrideForStore, forumSettings, x => x.ShowCustomersPostCount);
            await UpdateOverrideForStore(storeScope, model.AllowGuestsToCreatePosts_OverrideForStore, forumSettings, x => x.AllowGuestsToCreatePosts);
            await UpdateOverrideForStore(storeScope, model.AllowGuestsToCreateTopics_OverrideForStore, forumSettings, x => x.AllowGuestsToCreateTopics);
            await UpdateOverrideForStore(storeScope, model.AllowCustomersToEditPosts_OverrideForStore, forumSettings, x => x.AllowCustomersToEditPosts);
            await UpdateOverrideForStore(storeScope, model.AllowCustomersToDeletePosts_OverrideForStore, forumSettings, x => x.AllowCustomersToDeletePosts);
            await UpdateOverrideForStore(storeScope, model.AllowCustomersToManageSubscriptions_OverrideForStore, forumSettings, x => x.AllowCustomersToManageSubscriptions);
            await UpdateOverrideForStore(storeScope, model.TopicsPageSize_OverrideForStore, forumSettings, x => x.TopicsPageSize);
            await UpdateOverrideForStore(storeScope, model.PostsPageSize_OverrideForStore, forumSettings, x => x.PostsPageSize);
            await UpdateOverrideForStore(storeScope, model.ForumEditor_OverrideForStore, forumSettings, x => x.ForumEditor);
            await UpdateOverrideForStore(storeScope, model.SignaturesEnabled_OverrideForStore, forumSettings, x => x.SignaturesEnabled);
            await UpdateOverrideForStore(storeScope, model.AllowPrivateMessages_OverrideForStore, forumSettings, x => x.AllowPrivateMessages);
            await UpdateOverrideForStore(storeScope, model.ShowAlertForPM_OverrideForStore, forumSettings, x => x.ShowAlertForPM);
            await UpdateOverrideForStore(storeScope, model.NotifyAboutPrivateMessages_OverrideForStore, forumSettings, x => x.NotifyAboutPrivateMessages);
            await UpdateOverrideForStore(storeScope, model.ActiveDiscussionsFeedEnabled_OverrideForStore, forumSettings, x => x.ActiveDiscussionsFeedEnabled);
            await UpdateOverrideForStore(storeScope, model.ActiveDiscussionsFeedCount_OverrideForStore, forumSettings, x => x.ActiveDiscussionsFeedCount);
            await UpdateOverrideForStore(storeScope, model.ForumFeedsEnabled_OverrideForStore, forumSettings, x => x.ForumFeedsEnabled);
            await UpdateOverrideForStore(storeScope, model.ForumFeedCount_OverrideForStore, forumSettings, x => x.ForumFeedCount);
            await UpdateOverrideForStore(storeScope, model.SearchResultsPageSize_OverrideForStore, forumSettings, x => x.SearchResultsPageSize);
            await UpdateOverrideForStore(storeScope, model.ActiveDiscussionsPageSize_OverrideForStore, forumSettings, x => x.ActiveDiscussionsPageSize);
            await UpdateOverrideForStore(storeScope, model.AllowPostVoting_OverrideForStore, forumSettings, x => x.AllowPostVoting);
            await UpdateOverrideForStore(storeScope, model.MaxVotesPerDay_OverrideForStore, forumSettings, x => x.MaxVotesPerDay);

            //now clear cache
            await ClearCache();

            //activity log
            await _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Forum");
        }

        public async Task<IActionResult> News()
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
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
        public async Task<IActionResult> News(NewsSettingsModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var newsSettings = _settingService.LoadSetting<NewsSettings>(storeScope);
            newsSettings = model.ToEntity(newsSettings);

            await UpdateOverrideForStore(storeScope, model.Enabled_OverrideForStore, newsSettings, x => x.Enabled);
            await UpdateOverrideForStore(storeScope, model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, newsSettings, x => x.AllowNotRegisteredUsersToLeaveComments);
            await UpdateOverrideForStore(storeScope, model.NotifyAboutNewNewsComments_OverrideForStore, newsSettings, x => x.NotifyAboutNewNewsComments);
            await UpdateOverrideForStore(storeScope, model.ShowNewsOnMainPage_OverrideForStore, newsSettings, x => x.ShowNewsOnMainPage);
            await UpdateOverrideForStore(storeScope, model.MainPageNewsCount_OverrideForStore, newsSettings, x => x.MainPageNewsCount);
            await UpdateOverrideForStore(storeScope, model.NewsArchivePageSize_OverrideForStore, newsSettings, x => x.NewsArchivePageSize);
            await UpdateOverrideForStore(storeScope, model.ShowHeaderRssUrl_OverrideForStore, newsSettings, x => x.ShowHeaderRssUrl);

            //now clear cache
            await ClearCache();

            //activity log
            await _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("News");
        }
        public async Task<IActionResult> Shipping()
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
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
                                     ? await _addressService.GetAddressByIdSettings(shippingSettings.ShippingOriginAddressId)
                                     : null;
            if (originAddress != null)
            {
                model.ShippingOriginAddress = await originAddress.ToModel(_countryService, _stateProvinceService);
            }
            else
                model.ShippingOriginAddress = new AddressModel();

            model.ShippingOriginAddress.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
            foreach (var c in await _countryService.GetAllCountries(showHidden: true))
                model.ShippingOriginAddress.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (originAddress != null && c.Id == originAddress.CountryId) });

            var states = originAddress != null && !String.IsNullOrEmpty(originAddress.CountryId) ? await _stateProvinceService.GetStateProvincesByCountryId(originAddress.CountryId, showHidden: true) : new List<StateProvince>();
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
        public async Task<IActionResult> Shipping(ShippingSettingsModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var shippingSettings = _settingService.LoadSetting<ShippingSettings>(storeScope);
            shippingSettings = model.ToEntity(shippingSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await UpdateOverrideForStore(storeScope, model.ShipToSameAddress_OverrideForStore, shippingSettings, x => x.ShipToSameAddress);
            await UpdateOverrideForStore(storeScope, model.AllowPickUpInStore_OverrideForStore, shippingSettings, x => x.AllowPickUpInStore);
            await UpdateOverrideForStore(storeScope, model.UseWarehouseLocation_OverrideForStore, shippingSettings, x => x.UseWarehouseLocation);
            await UpdateOverrideForStore(storeScope, model.NotifyCustomerAboutShippingFromMultipleLocations_OverrideForStore, shippingSettings, x => x.NotifyCustomerAboutShippingFromMultipleLocations);
            await UpdateOverrideForStore(storeScope, model.FreeShippingOverXEnabled_OverrideForStore, shippingSettings, x => x.FreeShippingOverXEnabled);
            await UpdateOverrideForStore(storeScope, model.FreeShippingOverXValue_OverrideForStore, shippingSettings, x => x.FreeShippingOverXValue);
            await UpdateOverrideForStore(storeScope, model.FreeShippingOverXIncludingTax_OverrideForStore, shippingSettings, x => x.FreeShippingOverXIncludingTax);
            await UpdateOverrideForStore(storeScope, model.EstimateShippingEnabled_OverrideForStore, shippingSettings, x => x.EstimateShippingEnabled);
            await UpdateOverrideForStore(storeScope, model.DisplayShipmentEventsToCustomers_OverrideForStore, shippingSettings, x => x.DisplayShipmentEventsToCustomers);
            await UpdateOverrideForStore(storeScope, model.DisplayShipmentEventsToStoreOwner_OverrideForStore, shippingSettings, x => x.DisplayShipmentEventsToStoreOwner);
            await UpdateOverrideForStore(storeScope, model.BypassShippingMethodSelectionIfOnlyOne_OverrideForStore, shippingSettings, x => x.BypassShippingMethodSelectionIfOnlyOne);

            if (model.ShippingOriginAddress_OverrideForStore || storeScope == "")
            {
                //update address
                var addressId = _settingService.SettingExists(shippingSettings, x => x.ShippingOriginAddressId, storeScope) ?
                    shippingSettings.ShippingOriginAddressId : "";
                var originAddress = await _addressService.GetAddressByIdSettings(addressId) ??
                    new Address {
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                //update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one)
                originAddress = model.ShippingOriginAddress.ToEntity(originAddress);
                if ((await _addressService.GetAddressByIdSettings(addressId)) != null)
                    await _addressService.UpdateAddressSettings(originAddress);
                else
                    await _addressService.InsertAddressSettings(originAddress);

                shippingSettings.ShippingOriginAddressId = originAddress.Id;

                await _settingService.SaveSetting(shippingSettings, x => x.ShippingOriginAddressId, storeScope, false);
            }
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(shippingSettings, x => x.ShippingOriginAddressId, storeScope);

            //now clear cache
            await ClearCache();

            //activity log
            await _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Shipping");
        }
        public async Task<IActionResult> Tax()
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
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

            model.TaxBasedOnValues = taxSettings.TaxBasedOn.ToSelectList(HttpContext);
            model.TaxDisplayTypeValues = taxSettings.TaxDisplayType.ToSelectList(HttpContext);

            //tax categories
            var taxCategories = await _taxCategoryService.GetAllTaxCategories();
            model.TaxCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Configuration.Settings.Tax.TaxCategories.None"), Value = "" });
            foreach (var tc in taxCategories)
                model.TaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id.ToString(), Selected = tc.Id == taxSettings.ShippingTaxClassId });
            model.PaymentMethodAdditionalFeeTaxCategories.Add(new SelectListItem { Text = "---", Value = "" });
            foreach (var tc in taxCategories)
                model.PaymentMethodAdditionalFeeTaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id.ToString(), Selected = tc.Id == taxSettings.PaymentMethodAdditionalFeeTaxClassId });

            //EU VAT countries
            model.EuVatShopCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
            foreach (var c in await _countryService.GetAllCountries(showHidden: true))
                model.EuVatShopCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = c.Id == taxSettings.EuVatShopCountryId });

            //default tax address
            var defaultAddress = !String.IsNullOrEmpty(taxSettings.DefaultTaxAddressId)
                                     ? await _addressService.GetAddressByIdSettings(taxSettings.DefaultTaxAddressId)
                                     : null;
            if (defaultAddress != null)
                model.DefaultTaxAddress = await defaultAddress.ToModel();
            else
                model.DefaultTaxAddress = new AddressModel();

            model.DefaultTaxAddress.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
            foreach (var c in await _countryService.GetAllCountries(showHidden: true))
                model.DefaultTaxAddress.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (defaultAddress != null && c.Id == defaultAddress.CountryId) });

            var states = defaultAddress != null && !String.IsNullOrEmpty(defaultAddress.CountryId) ? await _stateProvinceService.GetStateProvincesByCountryId(defaultAddress.CountryId, showHidden: true) : new List<StateProvince>();
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
        public async Task<IActionResult> Tax(TaxSettingsModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var taxSettings = _settingService.LoadSetting<TaxSettings>(storeScope);
            taxSettings = model.ToEntity(taxSettings);

            await UpdateOverrideForStore(storeScope, model.PricesIncludeTax_OverrideForStore, taxSettings, x => x.PricesIncludeTax);
            await UpdateOverrideForStore(storeScope, model.AllowCustomersToSelectTaxDisplayType_OverrideForStore, taxSettings, x => x.AllowCustomersToSelectTaxDisplayType);
            await UpdateOverrideForStore(storeScope, model.TaxDisplayType_OverrideForStore, taxSettings, x => x.TaxDisplayType);
            await UpdateOverrideForStore(storeScope, model.DisplayTaxSuffix_OverrideForStore, taxSettings, x => x.DisplayTaxSuffix);
            await UpdateOverrideForStore(storeScope, model.DisplayTaxRates_OverrideForStore, taxSettings, x => x.DisplayTaxRates);
            await UpdateOverrideForStore(storeScope, model.HideZeroTax_OverrideForStore, taxSettings, x => x.HideZeroTax);
            await UpdateOverrideForStore(storeScope, model.HideTaxInOrderSummary_OverrideForStore, taxSettings, x => x.HideTaxInOrderSummary);
            await UpdateOverrideForStore(storeScope, model.ForceTaxExclusionFromOrderSubtotal_OverrideForStore, taxSettings, x => x.ForceTaxExclusionFromOrderSubtotal);
            await UpdateOverrideForStore(storeScope, model.TaxBasedOn_OverrideForStore, taxSettings, x => x.TaxBasedOn);

            if (model.DefaultTaxAddress_OverrideForStore || storeScope == "")
            {
                //update address
                var addressId = _settingService.SettingExists(taxSettings, x => x.DefaultTaxAddressId, storeScope) ?
                    taxSettings.DefaultTaxAddressId : "";
                var originAddress = await _addressService.GetAddressByIdSettings(addressId) ??
                    new Address {
                        CreatedOnUtc = DateTime.UtcNow,
                        Id = "",
                    };
                //update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one)
                model.DefaultTaxAddress.Id = addressId;
                originAddress = model.DefaultTaxAddress.ToEntity(originAddress);
                if (!String.IsNullOrEmpty(model.DefaultTaxAddress.Id))
                    await _addressService.UpdateAddressSettings(originAddress);
                else
                    await _addressService.InsertAddressSettings(originAddress);
                taxSettings.DefaultTaxAddressId = originAddress.Id;

                await _settingService.SaveSetting(taxSettings, x => x.DefaultTaxAddressId, storeScope, false);
            }
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(taxSettings, x => x.DefaultTaxAddressId, storeScope);

            await UpdateOverrideForStore(storeScope, model.ShippingIsTaxable_OverrideForStore, taxSettings, x => x.ShippingIsTaxable);
            await UpdateOverrideForStore(storeScope, model.DefaultTaxCategoryId_OverrideForStore, taxSettings, x => x.DefaultTaxCategoryId);
            await UpdateOverrideForStore(storeScope, model.ShippingPriceIncludesTax_OverrideForStore, taxSettings, x => x.ShippingPriceIncludesTax);
            await UpdateOverrideForStore(storeScope, model.ShippingTaxClassId_OverrideForStore, taxSettings, x => x.ShippingTaxClassId);
            await UpdateOverrideForStore(storeScope, model.PaymentMethodAdditionalFeeIsTaxable_OverrideForStore, taxSettings, x => x.PaymentMethodAdditionalFeeIsTaxable);
            await UpdateOverrideForStore(storeScope, model.PaymentMethodAdditionalFeeIncludesTax_OverrideForStore, taxSettings, x => x.PaymentMethodAdditionalFeeIncludesTax);
            await UpdateOverrideForStore(storeScope, model.PaymentMethodAdditionalFeeTaxClassId_OverrideForStore, taxSettings, x => x.PaymentMethodAdditionalFeeTaxClassId);
            await UpdateOverrideForStore(storeScope, model.EuVatEnabled_OverrideForStore, taxSettings, x => x.EuVatEnabled);
            await UpdateOverrideForStore(storeScope, model.EuVatShopCountryId_OverrideForStore, taxSettings, x => x.EuVatShopCountryId);
            await UpdateOverrideForStore(storeScope, model.EuVatAllowVatExemption_OverrideForStore, taxSettings, x => x.EuVatAllowVatExemption);
            await UpdateOverrideForStore(storeScope, model.EuVatUseWebService_OverrideForStore, taxSettings, x => x.EuVatUseWebService);
            await UpdateOverrideForStore(storeScope, model.EuVatAssumeValid_OverrideForStore, taxSettings, x => x.EuVatAssumeValid);
            await UpdateOverrideForStore(storeScope, model.EuVatEmailAdminWhenNewVatSubmitted_OverrideForStore, taxSettings, x => x.EuVatEmailAdminWhenNewVatSubmitted);

            //now clear cache
            await ClearCache();

            //activity log
            await _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Tax");
        }
        public async Task<IActionResult> Catalog()
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
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
                model.AskQuestionOnProduct_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AskQuestionOnProduct, storeScope);
                model.AllowAnonymousUsersToEmailAFriend_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowAnonymousUsersToEmailAFriend, storeScope);
                model.RecentlyViewedProductsNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.RecentlyViewedProductsNumber, storeScope);
                model.RecentlyViewedProductsEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.RecentlyViewedProductsEnabled, storeScope);
                model.RecommendedProductsEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.RecommendedProductsEnabled, storeScope);
                model.SuggestedProductsEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.SuggestedProductsEnabled, storeScope);
                model.SuggestedProductsNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.SuggestedProductsNumber, storeScope);
                model.PersonalizedProductsEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.PersonalizedProductsEnabled, storeScope);
                model.PersonalizedProductsNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.PersonalizedProductsNumber, storeScope);
                model.NewProductsNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NewProductsNumber, storeScope);
                model.NewProductsEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NewProductsEnabled, storeScope);
                model.NewProductsNumberOnHomePage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NewProductsNumberOnHomePage, storeScope);
                model.NewProductsOnHomePage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NewProductsOnHomePage, storeScope);
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
                model.CustomerProductPrice_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.CustomerProductPrice, storeScope);
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
        public async Task<IActionResult> Catalog(CatalogSettingsModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var catalogSettings = _settingService.LoadSetting<CatalogSettings>(storeScope);
            catalogSettings = model.ToEntity(catalogSettings);

            await UpdateOverrideForStore(storeScope, model.AllowViewUnpublishedProductPage_OverrideForStore, catalogSettings, x => x.AllowViewUnpublishedProductPage);
            await UpdateOverrideForStore(storeScope, model.DisplayDiscontinuedMessageForUnpublishedProducts_OverrideForStore, catalogSettings, x => x.DisplayDiscontinuedMessageForUnpublishedProducts);
            await UpdateOverrideForStore(storeScope, model.ShowSkuOnProductDetailsPage_OverrideForStore, catalogSettings, x => x.ShowSkuOnProductDetailsPage);
            await UpdateOverrideForStore(storeScope, model.ShowSkuOnCatalogPages_OverrideForStore, catalogSettings, x => x.ShowSkuOnCatalogPages);
            await UpdateOverrideForStore(storeScope, model.ShowManufacturerPartNumber_OverrideForStore, catalogSettings, x => x.ShowManufacturerPartNumber);
            await UpdateOverrideForStore(storeScope, model.ShowGtin_OverrideForStore, catalogSettings, x => x.ShowGtin);
            await UpdateOverrideForStore(storeScope, model.ShowFreeShippingNotification_OverrideForStore, catalogSettings, x => x.ShowFreeShippingNotification);
            await UpdateOverrideForStore(storeScope, model.AllowProductSorting_OverrideForStore, catalogSettings, x => x.AllowProductSorting);
            await UpdateOverrideForStore(storeScope, model.AllowProductViewModeChanging_OverrideForStore, catalogSettings, x => x.AllowProductViewModeChanging);
            await UpdateOverrideForStore(storeScope, model.ShowProductsFromSubcategories_OverrideForStore, catalogSettings, x => x.ShowProductsFromSubcategories);
            await UpdateOverrideForStore(storeScope, model.ShowCategoryProductNumber_OverrideForStore, catalogSettings, x => x.ShowCategoryProductNumber);
            await UpdateOverrideForStore(storeScope, model.ShowCategoryProductNumberIncludingSubcategories_OverrideForStore, catalogSettings, x => x.ShowCategoryProductNumberIncludingSubcategories);
            await UpdateOverrideForStore(storeScope, model.CategoryBreadcrumbEnabled_OverrideForStore, catalogSettings, x => x.CategoryBreadcrumbEnabled);
            await UpdateOverrideForStore(storeScope, model.ShowShareButton_OverrideForStore, catalogSettings, x => x.ShowShareButton);
            await UpdateOverrideForStore(storeScope, model.PageShareCode_OverrideForStore, catalogSettings, x => x.PageShareCode);
            await UpdateOverrideForStore(storeScope, model.ProductReviewsMustBeApproved_OverrideForStore, catalogSettings, x => x.ProductReviewsMustBeApproved);
            await UpdateOverrideForStore(storeScope, model.AllowAnonymousUsersToReviewProduct_OverrideForStore, catalogSettings, x => x.AllowAnonymousUsersToReviewProduct);
            await UpdateOverrideForStore(storeScope, model.ProductReviewPossibleOnlyAfterPurchasing_OverrideForStore, catalogSettings, x => x.ProductReviewPossibleOnlyAfterPurchasing);
            await UpdateOverrideForStore(storeScope, model.NotifyStoreOwnerAboutNewProductReviews_OverrideForStore, catalogSettings, x => x.NotifyStoreOwnerAboutNewProductReviews);
            await UpdateOverrideForStore(storeScope, model.EmailAFriendEnabled_OverrideForStore, catalogSettings, x => x.EmailAFriendEnabled);
            await UpdateOverrideForStore(storeScope, model.AskQuestionEnabled_OverrideForStore, catalogSettings, x => x.AskQuestionEnabled);
            await UpdateOverrideForStore(storeScope, model.AskQuestionOnProduct_OverrideForStore, catalogSettings, x => x.AskQuestionOnProduct);
            await UpdateOverrideForStore(storeScope, model.AllowAnonymousUsersToEmailAFriend_OverrideForStore, catalogSettings, x => x.AllowAnonymousUsersToEmailAFriend);
            await UpdateOverrideForStore(storeScope, model.RecentlyViewedProductsNumber_OverrideForStore, catalogSettings, x => x.RecentlyViewedProductsNumber);
            await UpdateOverrideForStore(storeScope, model.RecentlyViewedProductsEnabled_OverrideForStore, catalogSettings, x => x.RecentlyViewedProductsEnabled);
            await UpdateOverrideForStore(storeScope, model.RecommendedProductsEnabled_OverrideForStore, catalogSettings, x => x.RecommendedProductsEnabled);
            await UpdateOverrideForStore(storeScope, model.SuggestedProductsEnabled_OverrideForStore, catalogSettings, x => x.SuggestedProductsEnabled);
            await UpdateOverrideForStore(storeScope, model.SuggestedProductsNumber_OverrideForStore, catalogSettings, x => x.SuggestedProductsNumber);
            await UpdateOverrideForStore(storeScope, model.PersonalizedProductsEnabled_OverrideForStore, catalogSettings, x => x.PersonalizedProductsEnabled);
            await UpdateOverrideForStore(storeScope, model.PersonalizedProductsNumber_OverrideForStore, catalogSettings, x => x.PersonalizedProductsNumber);
            await UpdateOverrideForStore(storeScope, model.NewProductsNumber_OverrideForStore, catalogSettings, x => x.NewProductsNumber);
            await UpdateOverrideForStore(storeScope, model.NewProductsEnabled_OverrideForStore, catalogSettings, x => x.NewProductsEnabled);
            await UpdateOverrideForStore(storeScope, model.NewProductsNumberOnHomePage_OverrideForStore, catalogSettings, x => x.NewProductsNumberOnHomePage);
            await UpdateOverrideForStore(storeScope, model.NewProductsOnHomePage_OverrideForStore, catalogSettings, x => x.NewProductsOnHomePage);
            await UpdateOverrideForStore(storeScope, model.CompareProductsEnabled_OverrideForStore, catalogSettings, x => x.CompareProductsEnabled);
            await UpdateOverrideForStore(storeScope, model.ShowBestsellersOnHomepage_OverrideForStore, catalogSettings, x => x.ShowBestsellersOnHomepage);
            await UpdateOverrideForStore(storeScope, model.NumberOfBestsellersOnHomepage_OverrideForStore, catalogSettings, x => x.NumberOfBestsellersOnHomepage);
            await UpdateOverrideForStore(storeScope, model.SearchPageProductsPerPage_OverrideForStore, catalogSettings, x => x.SearchPageProductsPerPage);
            await UpdateOverrideForStore(storeScope, model.SearchPageAllowCustomersToSelectPageSize_OverrideForStore, catalogSettings, x => x.SearchPageAllowCustomersToSelectPageSize);
            await UpdateOverrideForStore(storeScope, model.SearchPagePageSizeOptions_OverrideForStore, catalogSettings, x => x.SearchPagePageSizeOptions);
            await UpdateOverrideForStore(storeScope, model.ProductSearchAutoCompleteEnabled_OverrideForStore, catalogSettings, x => x.ProductSearchAutoCompleteEnabled);
            await UpdateOverrideForStore(storeScope, model.ProductSearchAutoCompleteNumberOfProducts_OverrideForStore, catalogSettings, x => x.ProductSearchAutoCompleteNumberOfProducts);
            await UpdateOverrideForStore(storeScope, model.ShowProductImagesInSearchAutoComplete_OverrideForStore, catalogSettings, x => x.ShowProductImagesInSearchAutoComplete);
            await UpdateOverrideForStore(storeScope, model.ProductSearchTermMinimumLength_OverrideForStore, catalogSettings, x => x.ProductSearchTermMinimumLength);
            await UpdateOverrideForStore(storeScope, model.ProductsAlsoPurchasedEnabled_OverrideForStore, catalogSettings, x => x.ProductsAlsoPurchasedEnabled);
            await UpdateOverrideForStore(storeScope, model.ProductsAlsoPurchasedNumber_OverrideForStore, catalogSettings, x => x.ProductsAlsoPurchasedNumber);
            await UpdateOverrideForStore(storeScope, model.NumberOfProductTags_OverrideForStore, catalogSettings, x => x.NumberOfProductTags);
            await UpdateOverrideForStore(storeScope, model.ProductsByTagPageSize_OverrideForStore, catalogSettings, x => x.ProductsByTagPageSize);
            await UpdateOverrideForStore(storeScope, model.ProductsByTagAllowCustomersToSelectPageSize_OverrideForStore, catalogSettings, x => x.ProductsByTagAllowCustomersToSelectPageSize);
            await UpdateOverrideForStore(storeScope, model.ProductsByTagPageSizeOptions_OverrideForStore, catalogSettings, x => x.ProductsByTagPageSizeOptions);
            await UpdateOverrideForStore(storeScope, model.IncludeShortDescriptionInCompareProducts_OverrideForStore, catalogSettings, x => x.IncludeShortDescriptionInCompareProducts);
            await UpdateOverrideForStore(storeScope, model.IncludeFullDescriptionInCompareProducts_OverrideForStore, catalogSettings, x => x.IncludeFullDescriptionInCompareProducts);
            await UpdateOverrideForStore(storeScope, model.IgnoreDiscounts_OverrideForStore, catalogSettings, x => x.IgnoreDiscounts);
            await UpdateOverrideForStore(storeScope, model.IgnoreFeaturedProducts_OverrideForStore, catalogSettings, x => x.IgnoreFeaturedProducts);
            await UpdateOverrideForStore(storeScope, model.IgnoreAcl_OverrideForStore, catalogSettings, x => x.IgnoreAcl);
            await UpdateOverrideForStore(storeScope, model.IgnoreStoreLimitations_OverrideForStore, catalogSettings, x => x.IgnoreStoreLimitations);
            await UpdateOverrideForStore(storeScope, model.IgnoreFilterableAvailableStartEndDateTime_OverrideForStore, catalogSettings, x => x.IgnoreFilterableAvailableStartEndDateTime);
            await UpdateOverrideForStore(storeScope, model.IgnoreFilterableSpecAttributeOption_OverrideForStore, catalogSettings, x => x.IgnoreFilterableSpecAttributeOption);
            await UpdateOverrideForStore(storeScope, model.CustomerProductPrice_OverrideForStore, catalogSettings, x => x.CustomerProductPrice);
            await UpdateOverrideForStore(storeScope, model.ManufacturersBlockItemsToDisplay_OverrideForStore, catalogSettings, x => x.ManufacturersBlockItemsToDisplay);
            await UpdateOverrideForStore(storeScope, model.DisplayTaxShippingInfoFooter_OverrideForStore, catalogSettings, x => x.DisplayTaxShippingInfoFooter);
            await UpdateOverrideForStore(storeScope, model.DisplayTaxShippingInfoProductDetailsPage_OverrideForStore, catalogSettings, x => x.DisplayTaxShippingInfoProductDetailsPage);
            await UpdateOverrideForStore(storeScope, model.DisplayTaxShippingInfoProductBoxes_OverrideForStore, catalogSettings, x => x.DisplayTaxShippingInfoProductBoxes);
            await UpdateOverrideForStore(storeScope, model.DisplayTaxShippingInfoShoppingCart_OverrideForStore, catalogSettings, x => x.DisplayTaxShippingInfoShoppingCart);
            await UpdateOverrideForStore(storeScope, model.DisplayTaxShippingInfoWishlist_OverrideForStore, catalogSettings, x => x.DisplayTaxShippingInfoWishlist);
            await UpdateOverrideForStore(storeScope, model.DisplayTaxShippingInfoOrderDetailsPage_OverrideForStore, catalogSettings, x => x.DisplayTaxShippingInfoOrderDetailsPage);
            await UpdateOverrideForStore(storeScope, model.ShowProductReviewsPerStore_OverrideForStore, catalogSettings, x => x.ShowProductReviewsPerStore);

            //now clear cache
            await ClearCache();

            //activity log
            await _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));

            //selected tab
            await SaveSelectedTabIndex();

            return RedirectToAction("Catalog");
        }

        #region Sort options

        [HttpPost]
        public async Task<IActionResult> SortOptionsList(DataSourceRequest command)
        {
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var catalogSettings = _settingService.LoadSetting<CatalogSettings>(storeScope);
            var model = new List<SortOptionModel>();
            foreach (int option in Enum.GetValues(typeof(ProductSortingEnum)))
            {
                model.Add(new SortOptionModel() {
                    Id = option,
                    Name = ((ProductSortingEnum)option).GetLocalizedEnum(_localizationService, _workContext),
                    IsActive = !catalogSettings.ProductSortingEnumDisabled.Contains(option),
                    DisplayOrder = catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(option, out int value) ? value : option
                });
            }
            var gridModel = new DataSourceResult {
                Data = model.OrderBy(option => option.DisplayOrder),
                Total = model.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> SortOptionUpdate(SortOptionModel model)
        {
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var catalogSettings = _settingService.LoadSetting<CatalogSettings>(storeScope);

            catalogSettings.ProductSortingEnumDisplayOrder[model.Id] = model.DisplayOrder;
            if (model.IsActive && catalogSettings.ProductSortingEnumDisabled.Contains(model.Id))
                catalogSettings.ProductSortingEnumDisabled.Remove(model.Id);
            if (!model.IsActive && !catalogSettings.ProductSortingEnumDisabled.Contains(model.Id))
                catalogSettings.ProductSortingEnumDisabled.Add(model.Id);

            await _settingService.SaveSetting(catalogSettings, x => x.ProductSortingEnumDisabled, storeScope, false);
            await _settingService.SaveSetting(catalogSettings, x => x.ProductSortingEnumDisplayOrder, storeScope, false);

            await ClearCache();

            return new NullJsonResult();
        }

        #endregion

        public async Task<IActionResult> RewardPoints()
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
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
                model.ReduceRewardPointsAfterCancelOrder_OverrideForStore = _settingService.SettingExists(rewardPointsSettings, x => x.ReduceRewardPointsAfterCancelOrder, storeScope);
                model.DisplayHowMuchWillBeEarned_OverrideForStore = _settingService.SettingExists(rewardPointsSettings, x => x.DisplayHowMuchWillBeEarned, storeScope);
                model.PointsForRegistration_OverrideForStore = _settingService.SettingExists(rewardPointsSettings, x => x.PointsForRegistration, storeScope);
            }
            var currencySettings = _settingService.LoadSetting<CurrencySettings>(storeScope);
            model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyById(currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;

            //order statuses
            model.PointsForPurchases_Awarded_OrderStatuses = OrderStatus.Pending.ToSelectList(HttpContext, false, new int[] { 10, 40 }).ToList();

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> RewardPoints(RewardPointsSettingsModel model)
        {
            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
                var rewardPointsSettings = _settingService.LoadSetting<RewardPointsSettings>(storeScope);
                rewardPointsSettings = model.ToEntity(rewardPointsSettings);

                await UpdateOverrideForStore(storeScope, model.Enabled_OverrideForStore, rewardPointsSettings, x => x.Enabled);
                await UpdateOverrideForStore(storeScope, model.ExchangeRate_OverrideForStore, rewardPointsSettings, x => x.ExchangeRate);
                await UpdateOverrideForStore(storeScope, model.MinimumRewardPointsToUse_OverrideForStore, rewardPointsSettings, x => x.MinimumRewardPointsToUse);
                await UpdateOverrideForStore(storeScope, model.PointsForRegistration_OverrideForStore, rewardPointsSettings, x => x.PointsForRegistration);

                if (model.PointsForPurchases_OverrideForStore || storeScope == "")
                {
                    await _settingService.SaveSetting(rewardPointsSettings, x => x.PointsForPurchases_Amount, storeScope, false);
                    await _settingService.SaveSetting(rewardPointsSettings, x => x.PointsForPurchases_Points, storeScope, false);
                }
                else if (!String.IsNullOrEmpty(storeScope))
                {
                    await _settingService.DeleteSetting(rewardPointsSettings, x => x.PointsForPurchases_Amount, storeScope);
                    await _settingService.DeleteSetting(rewardPointsSettings, x => x.PointsForPurchases_Points, storeScope);
                }

                await UpdateOverrideForStore(storeScope, model.PointsForPurchases_Awarded_OverrideForStore, rewardPointsSettings, x => x.PointsForPurchases_Awarded);
                await UpdateOverrideForStore(storeScope, model.ReduceRewardPointsAfterCancelOrder_OverrideForStore, rewardPointsSettings, x => x.ReduceRewardPointsAfterCancelOrder);
                await UpdateOverrideForStore(storeScope, model.DisplayHowMuchWillBeEarned_OverrideForStore, rewardPointsSettings, x => x.DisplayHowMuchWillBeEarned);

                await _settingService.SaveSetting(rewardPointsSettings, x => x.PointsAccumulatedForAllStores, "", false);

                //now clear cache
                await ClearCache();

                //activity log
                await _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

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
        public async Task<IActionResult> Order()
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var orderSettings = _settingService.LoadSetting<OrderSettings>(storeScope);
            var model = orderSettings.ToModel();
            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.IsReOrderAllowed_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.IsReOrderAllowed, storeScope);
                model.MinOrderSubtotalAmount_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.MinOrderSubtotalAmount, storeScope);
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
                model.ReturnRequests_AllowToSpecifyPickupAddress_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.ReturnRequests_AllowToSpecifyPickupAddress, storeScope);
                model.ReturnRequests_AllowToSpecifyPickupDate_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.ReturnRequests_AllowToSpecifyPickupDate, storeScope);
                model.NumberOfDaysReturnRequestAvailable_OverrideForStore = _settingService.SettingExists(orderSettings, x => x.NumberOfDaysReturnRequestAvailable, storeScope);
            }

            var currencySettings = _settingService.LoadSetting<CurrencySettings>(storeScope);
            model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyById(currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;

            //gift card activation
            model.GiftCards_Activated_OrderStatuses = OrderStatus.Pending.ToSelectList(HttpContext, false, new int[] { 10, 40 }).ToList();
            model.GiftCards_Activated_OrderStatuses.Insert(0, new SelectListItem { Text = "---", Value = "0" });

            //order ident
            model.OrderIdent = await _mediator.Send(new MaxOrderNumberCommand());

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Order(OrderSettingsModel model)
        {
            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
                var orderSettings = _settingService.LoadSetting<OrderSettings>(storeScope);
                orderSettings = model.ToEntity(orderSettings);

                await UpdateOverrideForStore(storeScope, model.IsReOrderAllowed_OverrideForStore, orderSettings, x => x.IsReOrderAllowed);
                await UpdateOverrideForStore(storeScope, model.MinOrderSubtotalAmount_OverrideForStore, orderSettings, x => x.MinOrderSubtotalAmount);
                await UpdateOverrideForStore(storeScope, model.MinOrderSubtotalAmountIncludingTax_OverrideForStore, orderSettings, x => x.MinOrderSubtotalAmountIncludingTax);
                await UpdateOverrideForStore(storeScope, model.AnonymousCheckoutAllowed_OverrideForStore, orderSettings, x => x.AnonymousCheckoutAllowed);
                await UpdateOverrideForStore(storeScope, model.TermsOfServiceOnShoppingCartPage_OverrideForStore, orderSettings, x => x.TermsOfServiceOnShoppingCartPage);
                await UpdateOverrideForStore(storeScope, model.TermsOfServiceOnOrderConfirmPage_OverrideForStore, orderSettings, x => x.TermsOfServiceOnOrderConfirmPage);
                await UpdateOverrideForStore(storeScope, model.OnePageCheckoutEnabled_OverrideForStore, orderSettings, x => x.OnePageCheckoutEnabled);
                await UpdateOverrideForStore(storeScope, model.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab_OverrideForStore, orderSettings, x => x.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab);
                await UpdateOverrideForStore(storeScope, model.DisableBillingAddressCheckoutStep_OverrideForStore, orderSettings, x => x.DisableBillingAddressCheckoutStep);
                await UpdateOverrideForStore(storeScope, model.DisableOrderCompletedPage_OverrideForStore, orderSettings, x => x.DisableOrderCompletedPage);
                await UpdateOverrideForStore(storeScope, model.AttachPdfInvoiceToOrderPlacedEmail_OverrideForStore, orderSettings, x => x.AttachPdfInvoiceToOrderPlacedEmail);
                await UpdateOverrideForStore(storeScope, model.AttachPdfInvoiceToOrderPaidEmail_OverrideForStore, orderSettings, x => x.AttachPdfInvoiceToOrderPaidEmail);
                await UpdateOverrideForStore(storeScope, model.AttachPdfInvoiceToOrderCompletedEmail_OverrideForStore, orderSettings, x => x.AttachPdfInvoiceToOrderCompletedEmail);
                await UpdateOverrideForStore(storeScope, model.ReturnRequestsEnabled_OverrideForStore, orderSettings, x => x.ReturnRequestsEnabled);
                await UpdateOverrideForStore(storeScope, model.ReturnRequests_AllowToSpecifyPickupAddress_OverrideForStore, orderSettings, x => x.ReturnRequests_AllowToSpecifyPickupAddress);
                await UpdateOverrideForStore(storeScope, model.ReturnRequests_AllowToSpecifyPickupDate_OverrideForStore, orderSettings, x => x.ReturnRequests_AllowToSpecifyPickupDate);
                await UpdateOverrideForStore(storeScope, model.NumberOfDaysReturnRequestAvailable_OverrideForStore, orderSettings, x => x.NumberOfDaysReturnRequestAvailable);
                await UpdateOverrideForStore(storeScope, model.UserCanCancelUnpaidOrder_OverrideForStore, orderSettings, x => x.UserCanCancelUnpaidOrder);
                await UpdateOverrideForStore(storeScope, model.AllowCustomerToAddOrderNote_OverrideForStore, orderSettings, x => x.AllowCustomerToAddOrderNote);

                await _settingService.SaveSetting(orderSettings, x => x.DeactivateGiftCardsAfterDeletingOrder, "", false);
                await _settingService.SaveSetting(orderSettings, x => x.CompleteOrderWhenDelivered, "", false);
                await _settingService.SaveSetting(orderSettings, x => x.GiftCards_Activated_OrderStatusId, "", false);
                await _settingService.SaveSetting(orderSettings, x => x.DeactivateGiftCardsAfterCancelOrder, "", false);

                //now clear cache
                await ClearCache();

                //order ident
                if (model.OrderIdent.HasValue && model.OrderIdent.Value > 0)
                {
                    model.OrderIdent = await _mediator.Send(new MaxOrderNumberCommand() { OrderNumber = model.OrderIdent });
                }

                //activity log
                await _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

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
            await SaveSelectedTabIndex();

            return RedirectToAction("Order");
        }

        public async Task<IActionResult> ShoppingCart()
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
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
                model.AllowOnHoldCart_OverrideForStore = _settingService.SettingExists(shoppingCartSettings, x => x.AllowOnHoldCart, storeScope);
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ShoppingCart(ShoppingCartSettingsModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var shoppingCartSettings = _settingService.LoadSetting<ShoppingCartSettings>(storeScope);
            shoppingCartSettings = model.ToEntity(shoppingCartSettings);

            await UpdateOverrideForStore(storeScope, model.DisplayCartAfterAddingProduct_OverrideForStore, shoppingCartSettings, x => x.DisplayCartAfterAddingProduct);
            await UpdateOverrideForStore(storeScope, model.DisplayWishlistAfterAddingProduct_OverrideForStore, shoppingCartSettings, x => x.DisplayWishlistAfterAddingProduct);
            await UpdateOverrideForStore(storeScope, model.MaximumShoppingCartItems_OverrideForStore, shoppingCartSettings, x => x.MaximumShoppingCartItems);
            await UpdateOverrideForStore(storeScope, model.MaximumWishlistItems_OverrideForStore, shoppingCartSettings, x => x.MaximumWishlistItems);
            await UpdateOverrideForStore(storeScope, model.AllowOutOfStockItemsToBeAddedToWishlist_OverrideForStore, shoppingCartSettings, x => x.AllowOutOfStockItemsToBeAddedToWishlist);
            await UpdateOverrideForStore(storeScope, model.MoveItemsFromWishlistToCart_OverrideForStore, shoppingCartSettings, x => x.MoveItemsFromWishlistToCart);
            await UpdateOverrideForStore(storeScope, model.ShowProductImagesOnShoppingCart_OverrideForStore, shoppingCartSettings, x => x.ShowProductImagesOnShoppingCart);
            await UpdateOverrideForStore(storeScope, model.ShowProductImagesOnWishList_OverrideForStore, shoppingCartSettings, x => x.ShowProductImagesOnWishList);
            await UpdateOverrideForStore(storeScope, model.ShowDiscountBox_OverrideForStore, shoppingCartSettings, x => x.ShowDiscountBox);
            await UpdateOverrideForStore(storeScope, model.ShowGiftCardBox_OverrideForStore, shoppingCartSettings, x => x.ShowGiftCardBox);
            await UpdateOverrideForStore(storeScope, model.CrossSellsNumber_OverrideForStore, shoppingCartSettings, x => x.CrossSellsNumber);
            await UpdateOverrideForStore(storeScope, model.EmailWishlistEnabled_OverrideForStore, shoppingCartSettings, x => x.EmailWishlistEnabled);
            await UpdateOverrideForStore(storeScope, model.AllowAnonymousUsersToEmailWishlist_OverrideForStore, shoppingCartSettings, x => x.AllowAnonymousUsersToEmailWishlist);
            await UpdateOverrideForStore(storeScope, model.MiniShoppingCartEnabled_OverrideForStore, shoppingCartSettings, x => x.MiniShoppingCartEnabled);
            await UpdateOverrideForStore(storeScope, model.ShowProductImagesInMiniShoppingCart_OverrideForStore, shoppingCartSettings, x => x.ShowProductImagesInMiniShoppingCart);
            await UpdateOverrideForStore(storeScope, model.MiniShoppingCartProductNumber_OverrideForStore, shoppingCartSettings, x => x.MiniShoppingCartProductNumber);
            await UpdateOverrideForStore(storeScope, model.AllowCartItemEditing_OverrideForStore, shoppingCartSettings, x => x.AllowCartItemEditing);
            await UpdateOverrideForStore(storeScope, model.CartsSharedBetweenStores_OverrideForStore, shoppingCartSettings, x => x.CartsSharedBetweenStores);
            await UpdateOverrideForStore(storeScope, model.AllowOnHoldCart_OverrideForStore, shoppingCartSettings, x => x.AllowOnHoldCart);

            //now clear cache
            await ClearCache();

            //activity log
            await _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("ShoppingCart");
        }


        #region Return request reasons

        public async Task<IActionResult> ReturnRequestReasonList()
        {
            //select second tab
            const int customerFormFieldIndex = 1;
            await SaveSelectedTabIndex(customerFormFieldIndex);
            return RedirectToAction("Order", "Setting");
        }
        [HttpPost]
        public async Task<IActionResult> ReturnRequestReasonList(DataSourceRequest command)
        {
            var reasons = await _returnRequestService.GetAllReturnRequestReasons();
            var gridModel = new DataSourceResult {
                Data = reasons.Select(x => x.ToModel()),
                Total = reasons.Count
            };
            return Json(gridModel);
        }
        //create
        public async Task<IActionResult> ReturnRequestReasonCreate()
        {
            var model = new ReturnRequestReasonModel();
            //locales
            await AddLocales(_languageService, model.Locales);
            return View(model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> ReturnRequestReasonCreate(ReturnRequestReasonModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var rrr = model.ToEntity();
                await _returnRequestService.InsertReturnRequestReason(rrr);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.Order.ReturnRequestReasons.Added"));
                return continueEditing ? RedirectToAction("ReturnRequestReasonEdit", new { id = rrr.Id }) : RedirectToAction("ReturnRequestReasonList");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }
        //edit
        public async Task<IActionResult> ReturnRequestReasonEdit(string id)
        {
            var rrr = await _returnRequestService.GetReturnRequestReasonById(id);
            if (rrr == null)
                //No reason found with the specified id
                return RedirectToAction("ReturnRequestReasonList");

            var model = rrr.ToModel();
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = rrr.GetLocalized(x => x.Name, languageId, false, false);
            });
            return View(model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> ReturnRequestReasonEdit(ReturnRequestReasonModel model, bool continueEditing)
        {
            var rrr = await _returnRequestService.GetReturnRequestReasonById(model.Id);
            if (rrr == null)
                //No reason found with the specified id
                return RedirectToAction("ReturnRequestReasonList");

            if (ModelState.IsValid)
            {
                rrr = model.ToEntity(rrr);
                await _returnRequestService.UpdateReturnRequestReason(rrr);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.Order.ReturnRequestReasons.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();

                    return RedirectToAction("ReturnRequestReasonEdit", new { id = rrr.Id });
                }
                return RedirectToAction("ReturnRequestReasonList");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }
        //delete
        [HttpPost]
        public async Task<IActionResult> ReturnRequestReasonDelete(string id)
        {
            var rrr = await _returnRequestService.GetReturnRequestReasonById(id);
            await _returnRequestService.DeleteReturnRequestReason(rrr);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.Order.ReturnRequestReasons.Deleted"));
            return RedirectToAction("ReturnRequestReasonList");
        }

        #endregion

        #region Return request actions

        public async Task<IActionResult> ReturnRequestActionList()
        {
            //select second tab
            const int customerFormFieldIndex = 1;
            await SaveSelectedTabIndex(customerFormFieldIndex);
            return RedirectToAction("Order", "Setting");
        }
        [HttpPost]
        public async Task<IActionResult> ReturnRequestActionList(DataSourceRequest command)
        {
            var actions = await _returnRequestService.GetAllReturnRequestActions();
            var gridModel = new DataSourceResult {
                Data = actions.Select(x => x.ToModel()),
                Total = actions.Count
            };
            return Json(gridModel);
        }
        //create
        public async Task<IActionResult> ReturnRequestActionCreate()
        {
            var model = new ReturnRequestActionModel();
            //locales
            await AddLocales(_languageService, model.Locales);
            return View(model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> ReturnRequestActionCreate(ReturnRequestActionModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var rra = model.ToEntity();
                await _returnRequestService.InsertReturnRequestAction(rra);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.Order.ReturnRequestActions.Added"));
                return continueEditing ? RedirectToAction("ReturnRequestActionEdit", new { id = rra.Id }) : RedirectToAction("ReturnRequestActionList");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }
        //edit
        public async Task<IActionResult> ReturnRequestActionEdit(string id)
        {
            var rra = await _returnRequestService.GetReturnRequestActionById(id);
            if (rra == null)
                //No action found with the specified id
                return RedirectToAction("ReturnRequestActionList");

            var model = rra.ToModel();
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = rra.GetLocalized(x => x.Name, languageId, false, false);
            });
            return View(model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> ReturnRequestActionEdit(ReturnRequestActionModel model, bool continueEditing)
        {
            var rra = await _returnRequestService.GetReturnRequestActionById(model.Id);
            if (rra == null)
                //No action found with the specified id
                return RedirectToAction("ReturnRequestActionList");

            if (ModelState.IsValid)
            {
                rra = model.ToEntity(rra);
                await _returnRequestService.UpdateReturnRequestAction(rra);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.Order.ReturnRequestActions.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();

                    return RedirectToAction("ReturnRequestActionEdit", new { id = rra.Id });
                }
                return RedirectToAction("ReturnRequestActionList");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }
        //delete
        [HttpPost]
        public async Task<IActionResult> ReturnRequestActionDelete(string id)
        {
            var rra = await _returnRequestService.GetReturnRequestActionById(id);
            await _returnRequestService.DeleteReturnRequestAction(rra);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.Order.ReturnRequestActions.Deleted"));
            return RedirectToAction("ReturnRequestActionList");
        }
        #endregion
        public async Task<IActionResult> Media()
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
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
            var mediaStoreSetting = _settingService.LoadSetting<MediaSettings>("");
            model.PicturesStoredIntoDatabase = mediaStoreSetting.StoreInDb;
            return View(model);
        }
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> Media(MediaSettingsModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var mediaSettings = _settingService.LoadSetting<MediaSettings>(storeScope);
            mediaSettings = model.ToEntity(mediaSettings);

            await UpdateOverrideForStore(storeScope, model.AvatarPictureSize_OverrideForStore, mediaSettings, x => x.AvatarPictureSize);
            await UpdateOverrideForStore(storeScope, model.ProductThumbPictureSize_OverrideForStore, mediaSettings, x => x.ProductThumbPictureSize);
            await UpdateOverrideForStore(storeScope, model.ProductDetailsPictureSize_OverrideForStore, mediaSettings, x => x.ProductDetailsPictureSize);
            await UpdateOverrideForStore(storeScope, model.ProductThumbPictureSizeOnProductDetailsPage_OverrideForStore, mediaSettings, x => x.ProductThumbPictureSizeOnProductDetailsPage);
            await UpdateOverrideForStore(storeScope, model.AssociatedProductPictureSize_OverrideForStore, mediaSettings, x => x.AssociatedProductPictureSize);
            await UpdateOverrideForStore(storeScope, model.CategoryThumbPictureSize_OverrideForStore, mediaSettings, x => x.CategoryThumbPictureSize);
            await UpdateOverrideForStore(storeScope, model.ManufacturerThumbPictureSize_OverrideForStore, mediaSettings, x => x.ManufacturerThumbPictureSize);
            await UpdateOverrideForStore(storeScope, model.VendorThumbPictureSize_OverrideForStore, mediaSettings, x => x.VendorThumbPictureSize);
            await UpdateOverrideForStore(storeScope, model.CartThumbPictureSize_OverrideForStore, mediaSettings, x => x.CartThumbPictureSize);
            await UpdateOverrideForStore(storeScope, model.MiniCartThumbPictureSize_OverrideForStore, mediaSettings, x => x.MiniCartThumbPictureSize);
            await UpdateOverrideForStore(storeScope, model.MaximumImageSize_OverrideForStore, mediaSettings, x => x.MaximumImageSize);
            await UpdateOverrideForStore(storeScope, model.MultipleThumbDirectories_OverrideForStore, mediaSettings, x => x.MultipleThumbDirectories);
            await UpdateOverrideForStore(storeScope, model.DefaultImageQuality_OverrideForStore, mediaSettings, x => x.DefaultImageQuality);

            //now clear cache
            await ClearCache();

            //clear old Thumbs
            await _pictureService.ClearThumbs();

            //activity log
            await _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Media");
        }
        [HttpPost, ActionName("Media")]
        [FormValueRequired("change-picture-storage")]
        public async Task<IActionResult> ChangePictureStorage([FromServices] MediaSettings mediaSettings)
        {
            var storeIdDb = !mediaSettings.StoreInDb;

            //save the new setting value
            await _settingService.SetSetting("MediaSettings.StoreInDb", storeIdDb, "");

            int pageIndex = 0;
            const int pageSize = 100;
            try
            {
                while (true)
                {
                    var pictures = _pictureService.GetPictures(pageIndex, pageSize);
                    pageIndex++;
                    if (!pictures.Any())
                        break;

                    foreach (var picture in pictures)
                    {
                        var pictureBinary = await _pictureService.LoadPictureBinary(picture, !storeIdDb);
                        if (storeIdDb)
                            _pictureService.DeletePictureOnFileSystem(picture);
                        else
                            //now on file system
                            _pictureService.SavePictureInFile(picture.Id, pictureBinary, picture.MimeType);
                        picture.PictureBinary = storeIdDb ? pictureBinary : new byte[0];
                        picture.IsNew = true;

                        await _pictureService.UpdatePicture(picture);
                    }
                }
            }
            finally
            {
            }

            //activity log
            await _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Media");
        }

        public async Task<IActionResult> CustomerUser()
        {
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var customerSettings = _settingService.LoadSetting<CustomerSettings>(storeScope);
            var addressSettings = _settingService.LoadSetting<AddressSettings>(storeScope);
            var dateTimeSettings = _settingService.LoadSetting<DateTimeSettings>(storeScope);
            var externalAuthenticationSettings = _settingService.LoadSetting<ExternalAuthenticationSettings>(storeScope);

            //merge settings
            var model = new CustomerUserSettingsModel {
                CustomerSettings = customerSettings.ToModel(),
                AddressSettings = addressSettings.ToModel()
            };

            model.DateTimeSettings.AllowCustomersToSetTimeZone = dateTimeSettings.AllowCustomersToSetTimeZone;
            model.DateTimeSettings.DefaultStoreTimeZoneId = _dateTimeHelper.DefaultStoreTimeZone.Id;
            foreach (TimeZoneInfo timeZone in _dateTimeHelper.GetSystemTimeZones())
            {
                model.DateTimeSettings.AvailableTimeZones.Add(new SelectListItem {
                    Text = timeZone.DisplayName,
                    Value = timeZone.Id,
                    Selected = timeZone.Id.Equals(_dateTimeHelper.DefaultStoreTimeZone.Id, StringComparison.OrdinalIgnoreCase)
                });
            }

            model.ExternalAuthenticationSettings.AutoRegisterEnabled = externalAuthenticationSettings.AutoRegisterEnabled;

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CustomerUser(CustomerUserSettingsModel model)
        {
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var customerSettings = _settingService.LoadSetting<CustomerSettings>(storeScope);
            var addressSettings = _settingService.LoadSetting<AddressSettings>(storeScope);
            var dateTimeSettings = _settingService.LoadSetting<DateTimeSettings>(storeScope);
            var externalAuthenticationSettings = _settingService.LoadSetting<ExternalAuthenticationSettings>(storeScope);

            customerSettings = model.CustomerSettings.ToEntity(customerSettings);
            await _settingService.SaveSetting(customerSettings);

            addressSettings = model.AddressSettings.ToEntity(addressSettings);
            await _settingService.SaveSetting(addressSettings);

            dateTimeSettings.DefaultStoreTimeZoneId = model.DateTimeSettings.DefaultStoreTimeZoneId;
            dateTimeSettings.AllowCustomersToSetTimeZone = model.DateTimeSettings.AllowCustomersToSetTimeZone;
            await _settingService.SaveSetting(dateTimeSettings);

            externalAuthenticationSettings.AutoRegisterEnabled = model.ExternalAuthenticationSettings.AutoRegisterEnabled;
            await _settingService.SaveSetting(externalAuthenticationSettings);

            //activity log
            await _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));

            //selected tab
            await SaveSelectedTabIndex();

            return RedirectToAction("CustomerUser");
        }
        public async Task<IActionResult> GeneralCommon()
        {
            var model = new GeneralCommonSettingsModel();
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            model.ActiveStoreScopeConfiguration = storeScope;
            //store information
            var storeInformationSettings = _settingService.LoadSetting<StoreInformationSettings>(storeScope);
            var commonSettings = _settingService.LoadSetting<CommonSettings>(storeScope);
            var googleAnalyticsSettings = _settingService.LoadSetting<GoogleAnalyticsSettings>(storeScope);
            var adminareasettings = _settingService.LoadSetting<AdminAreaSettings>(storeScope);

            model.StoreInformationSettings.StoreClosed = storeInformationSettings.StoreClosed;

            //themes
            model.StoreInformationSettings.DefaultStoreTheme = storeInformationSettings.DefaultStoreTheme;
            model.StoreInformationSettings.AvailableStoreThemes = _themeProvider
                .GetThemeConfigurations()
                .Select(x => new GeneralCommonSettingsModel.StoreInformationSettingsModel.ThemeConfigurationModel {
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
            model.StoreInformationSettings.DisplayPrivacyPreference = storeInformationSettings.DisplayPrivacyPreference;
            //social pages
            model.StoreInformationSettings.FacebookLink = storeInformationSettings.FacebookLink;
            model.StoreInformationSettings.TwitterLink = storeInformationSettings.TwitterLink;
            model.StoreInformationSettings.YoutubeLink = storeInformationSettings.YoutubeLink;
            model.StoreInformationSettings.InstagramLink = storeInformationSettings.InstagramLink;
            model.StoreInformationSettings.LinkedInLink = storeInformationSettings.LinkedInLink;
            model.StoreInformationSettings.PinterestLink = storeInformationSettings.PinterestLink;

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
                model.StoreInformationSettings.DisplayPrivacyPreference_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.DisplayPrivacyPreference, storeScope);
                model.StoreInformationSettings.FacebookLink_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.FacebookLink, storeScope);
                model.StoreInformationSettings.TwitterLink_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.TwitterLink, storeScope);
                model.StoreInformationSettings.YoutubeLink_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.YoutubeLink, storeScope);
                model.StoreInformationSettings.InstagramLink_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.InstagramLink, storeScope);
                model.StoreInformationSettings.LinkedInLink_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.LinkedInLink, storeScope);
                model.StoreInformationSettings.PinterestLink_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.PinterestLink, storeScope);
                model.StoreInformationSettings.StoreInDatabaseContactUsForm_OverrideForStore = _settingService.SettingExists(commonSettings, x => x.StoreInDatabaseContactUsForm, storeScope);
                model.StoreInformationSettings.SubjectFieldOnContactUsForm_OverrideForStore = _settingService.SettingExists(commonSettings, x => x.SubjectFieldOnContactUsForm, storeScope);
                model.StoreInformationSettings.UseSystemEmailForContactUsForm_OverrideForStore = _settingService.SettingExists(commonSettings, x => x.UseSystemEmailForContactUsForm, storeScope);
            }

            //seo settings
            var seoSettings = _settingService.LoadSetting<SeoSettings>(storeScope);
            model.SeoSettings.PageTitleSeparator = seoSettings.PageTitleSeparator;
            model.SeoSettings.PageTitleSeoAdjustment = (int)seoSettings.PageTitleSeoAdjustment;
            model.SeoSettings.PageTitleSeoAdjustmentValues = seoSettings.PageTitleSeoAdjustment.ToSelectList(HttpContext);
            model.SeoSettings.DefaultTitle = seoSettings.DefaultTitle;
            model.SeoSettings.DefaultMetaKeywords = seoSettings.DefaultMetaKeywords;
            model.SeoSettings.DefaultMetaDescription = seoSettings.DefaultMetaDescription;
            model.SeoSettings.GenerateProductMetaDescription = seoSettings.GenerateProductMetaDescription;
            model.SeoSettings.ConvertNonWesternChars = seoSettings.ConvertNonWesternChars;
            model.SeoSettings.SeoCharConversion = seoSettings.SeoCharConversion;
            model.SeoSettings.CanonicalUrlsEnabled = seoSettings.CanonicalUrlsEnabled;
            model.SeoSettings.EnableJsBundling = seoSettings.EnableJsBundling;
            model.SeoSettings.EnableCssBundling = seoSettings.EnableCssBundling;
            model.SeoSettings.TwitterMetaTags = seoSettings.TwitterMetaTags;
            model.SeoSettings.OpenGraphMetaTags = seoSettings.OpenGraphMetaTags;
            model.SeoSettings.StorePictureId = seoSettings.StorePictureId;
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
                model.SeoSettings.SeoCharConversion_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.SeoCharConversion, storeScope);
                model.SeoSettings.CanonicalUrlsEnabled_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.CanonicalUrlsEnabled, storeScope);
                model.SeoSettings.EnableJsBundling_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.EnableJsBundling, storeScope);
                model.SeoSettings.EnableCssBundling_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.EnableCssBundling, storeScope);
                model.SeoSettings.TwitterMetaTags_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.TwitterMetaTags, storeScope);
                model.SeoSettings.OpenGraphMetaTags_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.OpenGraphMetaTags, storeScope);
                model.SeoSettings.StorePictureId_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.StorePictureId, storeScope);
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
            model.SecuritySettings.CaptchaShowOnPasswordRecoveryPage = captchaSettings.ShowOnPasswordRecoveryPage;
            model.SecuritySettings.CaptchaShowOnContactUsPage = captchaSettings.ShowOnContactUsPage;
            model.SecuritySettings.CaptchaShowOnEmailWishlistToFriendPage = captchaSettings.ShowOnEmailWishlistToFriendPage;
            model.SecuritySettings.CaptchaShowOnEmailProductToFriendPage = captchaSettings.ShowOnEmailProductToFriendPage;
            model.SecuritySettings.CaptchaShowOnAskQuestionPage = captchaSettings.ShowOnAskQuestionPage;
            model.SecuritySettings.CaptchaShowOnBlogCommentPage = captchaSettings.ShowOnBlogCommentPage;
            model.SecuritySettings.CaptchaShowOnArticleCommentPage = captchaSettings.ShowOnArticleCommentPage;
            model.SecuritySettings.CaptchaShowOnNewsCommentPage = captchaSettings.ShowOnNewsCommentPage;
            model.SecuritySettings.CaptchaShowOnProductReviewPage = captchaSettings.ShowOnProductReviewPage;
            model.SecuritySettings.CaptchaShowOnApplyVendorPage = captchaSettings.ShowOnApplyVendorPage;
            model.SecuritySettings.ReCaptchaVersion = captchaSettings.ReCaptchaVersion;
            model.SecuritySettings.AvailableReCaptchaVersions = ReCaptchaVersion.Version2.ToSelectList(HttpContext, false).ToList();
            model.SecuritySettings.ReCaptchaPublicKey = captchaSettings.ReCaptchaPublicKey;
            model.SecuritySettings.ReCaptchaPrivateKey = captchaSettings.ReCaptchaPrivateKey;

            //PDF settings
            var pdfSettings = _settingService.LoadSetting<PdfSettings>(storeScope);
            model.PdfSettings.LogoPictureId = pdfSettings.LogoPictureId;
            model.PdfSettings.DisablePdfInvoicesForPendingOrders = pdfSettings.DisablePdfInvoicesForPendingOrders;
            model.PdfSettings.InvoiceHeaderText = pdfSettings.InvoiceHeaderText;
            model.PdfSettings.InvoiceFooterText = pdfSettings.InvoiceFooterText;
            //override settings
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.PdfSettings.LogoPictureId_OverrideForStore = _settingService.SettingExists(pdfSettings, x => x.LogoPictureId, storeScope);
                model.PdfSettings.DisablePdfInvoicesForPendingOrders_OverrideForStore = _settingService.SettingExists(pdfSettings, x => x.DisablePdfInvoicesForPendingOrders, storeScope);
                model.PdfSettings.InvoiceHeaderText_OverrideForStore = _settingService.SettingExists(pdfSettings, x => x.InvoiceHeaderText, storeScope);
                model.PdfSettings.InvoiceFooterText_OverrideForStore = _settingService.SettingExists(pdfSettings, x => x.InvoiceFooterText, storeScope);
            }

            //localization
            var localizationSettings = _settingService.LoadSetting<LocalizationSettings>(storeScope);
            model.LocalizationSettings.UseImagesForLanguageSelection = localizationSettings.UseImagesForLanguageSelection;
            model.LocalizationSettings.AutomaticallyDetectLanguage = localizationSettings.AutomaticallyDetectLanguage;
            model.LocalizationSettings.LoadAllLocaleRecordsOnStartup = localizationSettings.LoadAllLocaleRecordsOnStartup;
            model.LocalizationSettings.LoadAllLocalizedPropertiesOnStartup = localizationSettings.LoadAllLocalizedPropertiesOnStartup;

            //full-text support
            model.FullTextSettings.Supported = true;
            model.FullTextSettings.Enabled = commonSettings.UseFullTextSearch;

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
            model.KnowledgebaseSettings.AllowNotRegisteredUsersToLeaveComments = knowledgebaseSettings.AllowNotRegisteredUsersToLeaveComments;
            model.KnowledgebaseSettings.NotifyAboutNewArticleComments = knowledgebaseSettings.NotifyAboutNewArticleComments;

            return View(model);
        }
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> GeneralCommon(GeneralCommonSettingsModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);

            //store information settings
            var storeInformationSettings = _settingService.LoadSetting<StoreInformationSettings>(storeScope);
            var commonSettings = _settingService.LoadSetting<CommonSettings>(storeScope);

            storeInformationSettings.StoreClosed = model.StoreInformationSettings.StoreClosed;
            storeInformationSettings.DefaultStoreTheme = model.StoreInformationSettings.DefaultStoreTheme;
            storeInformationSettings.AllowCustomerToSelectTheme = model.StoreInformationSettings.AllowCustomerToSelectTheme;
            storeInformationSettings.LogoPictureId = model.StoreInformationSettings.LogoPictureId;
            //EU Cookie law
            storeInformationSettings.DisplayEuCookieLawWarning = model.StoreInformationSettings.DisplayEuCookieLawWarning;
            storeInformationSettings.DisplayPrivacyPreference = model.StoreInformationSettings.DisplayPrivacyPreference;
            //social pages
            storeInformationSettings.FacebookLink = model.StoreInformationSettings.FacebookLink;
            storeInformationSettings.TwitterLink = model.StoreInformationSettings.TwitterLink;
            storeInformationSettings.YoutubeLink = model.StoreInformationSettings.YoutubeLink;
            storeInformationSettings.InstagramLink = model.StoreInformationSettings.InstagramLink;
            storeInformationSettings.LinkedInLink = model.StoreInformationSettings.LinkedInLink;
            storeInformationSettings.PinterestLink = model.StoreInformationSettings.PinterestLink;

            //contact us
            commonSettings.StoreInDatabaseContactUsForm = model.StoreInformationSettings.StoreInDatabaseContactUsForm;
            commonSettings.SubjectFieldOnContactUsForm = model.StoreInformationSettings.SubjectFieldOnContactUsForm;
            commonSettings.UseSystemEmailForContactUsForm = model.StoreInformationSettings.UseSystemEmailForContactUsForm;

            await UpdateOverrideForStore(storeScope, model.StoreInformationSettings.StoreClosed_OverrideForStore, storeInformationSettings, x => x.StoreClosed);
            await UpdateOverrideForStore(storeScope, model.StoreInformationSettings.DefaultStoreTheme_OverrideForStore, storeInformationSettings, x => x.DefaultStoreTheme);
            await UpdateOverrideForStore(storeScope, model.StoreInformationSettings.AllowCustomerToSelectTheme_OverrideForStore, storeInformationSettings, x => x.AllowCustomerToSelectTheme);
            await UpdateOverrideForStore(storeScope, model.StoreInformationSettings.LogoPictureId_OverrideForStore, storeInformationSettings, x => x.LogoPictureId);
            await UpdateOverrideForStore(storeScope, model.StoreInformationSettings.DisplayEuCookieLawWarning_OverrideForStore, storeInformationSettings, x => x.DisplayEuCookieLawWarning);
            await UpdateOverrideForStore(storeScope, model.StoreInformationSettings.DisplayPrivacyPreference_OverrideForStore, storeInformationSettings, x => x.DisplayPrivacyPreference);
            await UpdateOverrideForStore(storeScope, model.StoreInformationSettings.FacebookLink_OverrideForStore, storeInformationSettings, x => x.FacebookLink);
            await UpdateOverrideForStore(storeScope, model.StoreInformationSettings.TwitterLink_OverrideForStore, storeInformationSettings, x => x.TwitterLink);
            await UpdateOverrideForStore(storeScope, model.StoreInformationSettings.YoutubeLink_OverrideForStore, storeInformationSettings, x => x.YoutubeLink);
            await UpdateOverrideForStore(storeScope, model.StoreInformationSettings.InstagramLink_OverrideForStore, storeInformationSettings, x => x.InstagramLink);
            await UpdateOverrideForStore(storeScope, model.StoreInformationSettings.LinkedInLink_OverrideForStore, storeInformationSettings, x => x.LinkedInLink);
            await UpdateOverrideForStore(storeScope, model.StoreInformationSettings.PinterestLink_OverrideForStore, storeInformationSettings, x => x.PinterestLink);

            await UpdateOverrideForStore(storeScope, model.StoreInformationSettings.StoreInDatabaseContactUsForm_OverrideForStore, commonSettings, x => x.StoreInDatabaseContactUsForm);
            await UpdateOverrideForStore(storeScope, model.StoreInformationSettings.SubjectFieldOnContactUsForm_OverrideForStore, commonSettings, x => x.SubjectFieldOnContactUsForm);
            await UpdateOverrideForStore(storeScope, model.StoreInformationSettings.UseSystemEmailForContactUsForm_OverrideForStore, commonSettings, x => x.UseSystemEmailForContactUsForm);

            //seo settings
            var seoSettings = _settingService.LoadSetting<SeoSettings>(storeScope);
            seoSettings.PageTitleSeparator = model.SeoSettings.PageTitleSeparator;
            seoSettings.PageTitleSeoAdjustment = (PageTitleSeoAdjustment)model.SeoSettings.PageTitleSeoAdjustment;
            seoSettings.DefaultTitle = model.SeoSettings.DefaultTitle;
            seoSettings.DefaultMetaKeywords = model.SeoSettings.DefaultMetaKeywords;
            seoSettings.DefaultMetaDescription = model.SeoSettings.DefaultMetaDescription;
            seoSettings.GenerateProductMetaDescription = model.SeoSettings.GenerateProductMetaDescription;
            seoSettings.ConvertNonWesternChars = model.SeoSettings.ConvertNonWesternChars;
            seoSettings.SeoCharConversion = model.SeoSettings.SeoCharConversion;
            seoSettings.CanonicalUrlsEnabled = model.SeoSettings.CanonicalUrlsEnabled;
            seoSettings.EnableJsBundling = model.SeoSettings.EnableJsBundling;
            seoSettings.EnableCssBundling = model.SeoSettings.EnableCssBundling;
            seoSettings.TwitterMetaTags = model.SeoSettings.TwitterMetaTags;
            seoSettings.OpenGraphMetaTags = model.SeoSettings.OpenGraphMetaTags;
            seoSettings.StorePictureId = model.SeoSettings.StorePictureId;


            await UpdateOverrideForStore(storeScope, model.SeoSettings.PageTitleSeparator_OverrideForStore, seoSettings, x => x.PageTitleSeparator);
            await UpdateOverrideForStore(storeScope, model.SeoSettings.PageTitleSeoAdjustment_OverrideForStore, seoSettings, x => x.PageTitleSeoAdjustment);
            await UpdateOverrideForStore(storeScope, model.SeoSettings.DefaultTitle_OverrideForStore, seoSettings, x => x.DefaultTitle);
            await UpdateOverrideForStore(storeScope, model.SeoSettings.DefaultMetaKeywords_OverrideForStore, seoSettings, x => x.DefaultMetaKeywords);
            await UpdateOverrideForStore(storeScope, model.SeoSettings.DefaultMetaDescription_OverrideForStore, seoSettings, x => x.DefaultMetaDescription);
            await UpdateOverrideForStore(storeScope, model.SeoSettings.GenerateProductMetaDescription_OverrideForStore, seoSettings, x => x.GenerateProductMetaDescription);
            await UpdateOverrideForStore(storeScope, model.SeoSettings.ConvertNonWesternChars_OverrideForStore, seoSettings, x => x.ConvertNonWesternChars);
            await UpdateOverrideForStore(storeScope, model.SeoSettings.SeoCharConversion_OverrideForStore, seoSettings, x => x.SeoCharConversion);
            await UpdateOverrideForStore(storeScope, model.SeoSettings.CanonicalUrlsEnabled_OverrideForStore, seoSettings, x => x.CanonicalUrlsEnabled);
            await UpdateOverrideForStore(storeScope, model.SeoSettings.EnableJsBundling_OverrideForStore, seoSettings, x => x.EnableJsBundling);
            await UpdateOverrideForStore(storeScope, model.SeoSettings.EnableCssBundling_OverrideForStore, seoSettings, x => x.EnableCssBundling);
            await UpdateOverrideForStore(storeScope, model.SeoSettings.TwitterMetaTags_OverrideForStore, seoSettings, x => x.TwitterMetaTags);
            await UpdateOverrideForStore(storeScope, model.SeoSettings.OpenGraphMetaTags_OverrideForStore, seoSettings, x => x.OpenGraphMetaTags);
            await UpdateOverrideForStore(storeScope, model.SeoSettings.StorePictureId_OverrideForStore, seoSettings, x => x.StorePictureId);

            //security settings
            var securitySettings = _settingService.LoadSetting<SecuritySettings>(storeScope);
            var captchaSettings = _settingService.LoadSetting<CaptchaSettings>(storeScope);
            if (securitySettings.AdminAreaAllowedIpAddresses == null)
                securitySettings.AdminAreaAllowedIpAddresses = new List<string>();
            securitySettings.AdminAreaAllowedIpAddresses.Clear();
            if (!String.IsNullOrEmpty(model.SecuritySettings.AdminAreaAllowedIpAddresses))
                foreach (string s in model.SecuritySettings.AdminAreaAllowedIpAddresses.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    if (!String.IsNullOrWhiteSpace(s))
                        securitySettings.AdminAreaAllowedIpAddresses.Add(s.Trim());
            securitySettings.EnableXsrfProtectionForAdminArea = model.SecuritySettings.EnableXsrfProtectionForAdminArea;
            securitySettings.EnableXsrfProtectionForPublicStore = model.SecuritySettings.EnableXsrfProtectionForPublicStore;
            securitySettings.HoneypotEnabled = model.SecuritySettings.HoneypotEnabled;
            await _settingService.SaveSetting(securitySettings);
            captchaSettings.Enabled = model.SecuritySettings.CaptchaEnabled;
            captchaSettings.ShowOnLoginPage = model.SecuritySettings.CaptchaShowOnLoginPage;
            captchaSettings.ShowOnRegistrationPage = model.SecuritySettings.CaptchaShowOnRegistrationPage;
            captchaSettings.ShowOnPasswordRecoveryPage = model.SecuritySettings.CaptchaShowOnPasswordRecoveryPage;
            captchaSettings.ShowOnContactUsPage = model.SecuritySettings.CaptchaShowOnContactUsPage;
            captchaSettings.ShowOnEmailWishlistToFriendPage = model.SecuritySettings.CaptchaShowOnEmailWishlistToFriendPage;
            captchaSettings.ShowOnAskQuestionPage = model.SecuritySettings.CaptchaShowOnAskQuestionPage;
            captchaSettings.ShowOnEmailProductToFriendPage = model.SecuritySettings.CaptchaShowOnEmailProductToFriendPage;
            captchaSettings.ShowOnBlogCommentPage = model.SecuritySettings.CaptchaShowOnBlogCommentPage;
            captchaSettings.ShowOnArticleCommentPage = model.SecuritySettings.CaptchaShowOnArticleCommentPage;
            captchaSettings.ShowOnNewsCommentPage = model.SecuritySettings.CaptchaShowOnNewsCommentPage;
            captchaSettings.ShowOnProductReviewPage = model.SecuritySettings.CaptchaShowOnProductReviewPage;
            captchaSettings.ShowOnApplyVendorPage = model.SecuritySettings.CaptchaShowOnApplyVendorPage;
            captchaSettings.ReCaptchaVersion = model.SecuritySettings.ReCaptchaVersion;
            captchaSettings.ReCaptchaPublicKey = model.SecuritySettings.ReCaptchaPublicKey;
            captchaSettings.ReCaptchaPrivateKey = model.SecuritySettings.ReCaptchaPrivateKey;
            await _settingService.SaveSetting(captchaSettings);
            if (captchaSettings.Enabled &&
                (String.IsNullOrWhiteSpace(captchaSettings.ReCaptchaPublicKey) || String.IsNullOrWhiteSpace(captchaSettings.ReCaptchaPrivateKey)))
            {
                //captcha is enabled but the keys are not entered
                ErrorNotification("Captcha is enabled but the appropriate keys are not entered");
            }

            //PDF settings
            var pdfSettings = _settingService.LoadSetting<PdfSettings>(storeScope);
            pdfSettings.LogoPictureId = model.PdfSettings.LogoPictureId;
            pdfSettings.DisablePdfInvoicesForPendingOrders = model.PdfSettings.DisablePdfInvoicesForPendingOrders;
            pdfSettings.InvoiceHeaderText = model.PdfSettings.InvoiceHeaderText;
            pdfSettings.InvoiceFooterText = model.PdfSettings.InvoiceFooterText;

            await UpdateOverrideForStore(storeScope, model.PdfSettings.LogoPictureId_OverrideForStore, pdfSettings, x => x.LogoPictureId);
            await UpdateOverrideForStore(storeScope, model.PdfSettings.DisablePdfInvoicesForPendingOrders_OverrideForStore, pdfSettings, x => x.DisablePdfInvoicesForPendingOrders);
            await UpdateOverrideForStore(storeScope, model.PdfSettings.InvoiceHeaderText_OverrideForStore, pdfSettings, x => x.InvoiceHeaderText);
            await UpdateOverrideForStore(storeScope, model.PdfSettings.InvoiceFooterText_OverrideForStore, pdfSettings, x => x.InvoiceFooterText);

            //localization settings
            var localizationSettings = _settingService.LoadSetting<LocalizationSettings>(storeScope);
            localizationSettings.UseImagesForLanguageSelection = model.LocalizationSettings.UseImagesForLanguageSelection;
            localizationSettings.AutomaticallyDetectLanguage = model.LocalizationSettings.AutomaticallyDetectLanguage;
            localizationSettings.LoadAllLocaleRecordsOnStartup = model.LocalizationSettings.LoadAllLocaleRecordsOnStartup;
            localizationSettings.LoadAllLocalizedPropertiesOnStartup = model.LocalizationSettings.LoadAllLocalizedPropertiesOnStartup;
            await _settingService.SaveSetting(localizationSettings);

            //admin settings
            var adminareasettings = _settingService.LoadSetting<AdminAreaSettings>(storeScope);

            await _settingService.SaveSetting(adminareasettings);

            //googleanalytics settings
            var googleAnalyticsSettings = _settingService.LoadSetting<GoogleAnalyticsSettings>(storeScope);
            googleAnalyticsSettings.gaprivateKey = model.GoogleAnalyticsSettings.gaprivateKey;
            googleAnalyticsSettings.gaserviceAccountEmail = model.GoogleAnalyticsSettings.gaserviceAccountEmail;
            googleAnalyticsSettings.gaviewID = model.GoogleAnalyticsSettings.gaviewID;

            await UpdateOverrideForStore(storeScope, model.GoogleAnalyticsSettings.gaprivateKey_OverrideForStore, googleAnalyticsSettings, x => x.gaprivateKey);
            await UpdateOverrideForStore(storeScope, model.GoogleAnalyticsSettings.gaserviceAccountEmail_OverrideForStore, googleAnalyticsSettings, x => x.gaserviceAccountEmail);
            await UpdateOverrideForStore(storeScope, model.GoogleAnalyticsSettings.gaviewID_OverrideForStore, googleAnalyticsSettings, x => x.gaviewID);

            //Menu item settings
            var displayMenuItemSettings = _settingService.LoadSetting<MenuItemSettings>(storeScope);
            displayMenuItemSettings.DisplayHomePageMenu = model.DisplayMenuSettings.DisplayHomePageMenu;
            displayMenuItemSettings.DisplayNewProductsMenu = model.DisplayMenuSettings.DisplayNewProductsMenu;
            displayMenuItemSettings.DisplaySearchMenu = model.DisplayMenuSettings.DisplaySearchMenu;
            displayMenuItemSettings.DisplayCustomerMenu = model.DisplayMenuSettings.DisplayCustomerMenu;
            displayMenuItemSettings.DisplayBlogMenu = model.DisplayMenuSettings.DisplayBlogMenu;
            displayMenuItemSettings.DisplayForumsMenu = model.DisplayMenuSettings.DisplayForumsMenu;
            displayMenuItemSettings.DisplayContactUsMenu = model.DisplayMenuSettings.DisplayContactUsMenu;

            await UpdateOverrideForStore(storeScope, model.DisplayMenuSettings.DisplayHomePageMenu_OverrideForStore, displayMenuItemSettings, x => x.DisplayHomePageMenu);
            await UpdateOverrideForStore(storeScope, model.DisplayMenuSettings.DisplayNewProductsMenu_OverrideForStore, displayMenuItemSettings, x => x.DisplayNewProductsMenu);
            await UpdateOverrideForStore(storeScope, model.DisplayMenuSettings.DisplaySearchMenu_OverrideForStore, displayMenuItemSettings, x => x.DisplaySearchMenu);
            await UpdateOverrideForStore(storeScope, model.DisplayMenuSettings.DisplayCustomerMenu_OverrideForStore, displayMenuItemSettings, x => x.DisplayCustomerMenu);
            await UpdateOverrideForStore(storeScope, model.DisplayMenuSettings.DisplayBlogMenu_OverrideForStore, displayMenuItemSettings, x => x.DisplayBlogMenu);
            await UpdateOverrideForStore(storeScope, model.DisplayMenuSettings.DisplayForumsMenu_OverrideForStore, displayMenuItemSettings, x => x.DisplayForumsMenu);
            await UpdateOverrideForStore(storeScope, model.DisplayMenuSettings.DisplayContactUsMenu_OverrideForStore, displayMenuItemSettings, x => x.DisplayContactUsMenu);

            //Knowledgebase
            var knowledgebaseSettings = _settingService.LoadSetting<KnowledgebaseSettings>(storeScope);
            knowledgebaseSettings.Enabled = model.KnowledgebaseSettings.Enabled;
            knowledgebaseSettings.AllowNotRegisteredUsersToLeaveComments = model.KnowledgebaseSettings.AllowNotRegisteredUsersToLeaveComments;
            knowledgebaseSettings.NotifyAboutNewArticleComments = model.KnowledgebaseSettings.NotifyAboutNewArticleComments;

            if (model.KnowledgebaseSettings.Enabled_OverrideForStore || storeScope == "")
                await _settingService.SaveSetting(knowledgebaseSettings, x => x.Enabled, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(knowledgebaseSettings, x => x.Enabled, storeScope);

            if (model.KnowledgebaseSettings.AllowNotRegisteredUsersToLeaveComments_OverrideForStore || storeScope == "")
                await _settingService.SaveSetting(knowledgebaseSettings, x => x.AllowNotRegisteredUsersToLeaveComments, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(knowledgebaseSettings, x => x.AllowNotRegisteredUsersToLeaveComments, storeScope);

            if (model.KnowledgebaseSettings.NotifyAboutNewArticleComments_OverrideForStore || storeScope == "")
                await _settingService.SaveSetting(knowledgebaseSettings, x => x.NotifyAboutNewArticleComments, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(knowledgebaseSettings, x => x.NotifyAboutNewArticleComments, storeScope);

            //now clear cache
            await ClearCache();

            //activity log
            await _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));

            //selected tab
            await SaveSelectedTabIndex();

            return RedirectToAction("GeneralCommon");
        }
        [HttpPost, ActionName("GeneralCommon")]
        [FormValueRequired("changeencryptionkey")]
        public async Task<IActionResult> ChangeEncryptionKey(GeneralCommonSettingsModel model)
        {
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var securitySettings = _settingService.LoadSetting<SecuritySettings>(storeScope);

            try
            {
                if (model.SecuritySettings.EncryptionKey == null)
                    model.SecuritySettings.EncryptionKey = "";

                model.SecuritySettings.EncryptionKey = model.SecuritySettings.EncryptionKey.Trim();

                var newEncryptionPrivateKey = model.SecuritySettings.EncryptionKey;
                if (String.IsNullOrEmpty(newEncryptionPrivateKey) || newEncryptionPrivateKey.Length != 24)
                    throw new GrandException(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TooShort"));

                string oldEncryptionPrivateKey = securitySettings.EncryptionKey;
                if (oldEncryptionPrivateKey == newEncryptionPrivateKey)
                    throw new GrandException(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TheSame"));

                //update encrypted order info
                var orders = await _orderService.SearchOrders();
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
                    await _orderService.UpdateOrder(order);
                }

                //update user information
                //optimization - load only users with PasswordFormat.Encrypted
                var customers = await _customerService.GetAllCustomersByPasswordFormat(PasswordFormat.Encrypted);
                foreach (var customer in customers)
                {
                    string decryptedPassword = _encryptionService.DecryptText(customer.Password, oldEncryptionPrivateKey);
                    string encryptedPassword = _encryptionService.EncryptText(decryptedPassword, newEncryptionPrivateKey);

                    customer.Password = encryptedPassword;
                    await _customerService.UpdateCustomerPassword(customer);
                }

                securitySettings.EncryptionKey = newEncryptionPrivateKey;
                await _settingService.SaveSetting(securitySettings);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.Changed"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
            }

            //selected tab
            await SaveSelectedTabIndex();

            return RedirectToAction("GeneralCommon");
        }
        [HttpPost, ActionName("GeneralCommon")]
        [FormValueRequired("togglefulltext")]
        public async Task<IActionResult> ToggleFullText()
        {
            //https://docs.mongodb.com/manual/reference/text-search-languages/#text-search-languages
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var commonSettings = _settingService.LoadSetting<CommonSettings>(storeScope);
            try
            {
                if (commonSettings.UseFullTextSearch)
                {
                    await _mediator.Send(new UseFullTextSearchCommand() { UseFullTextSearch = false });
                    commonSettings.UseFullTextSearch = false;
                    await _settingService.SaveSetting(commonSettings);
                    SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.FullTextSettings.Disabled"));
                }
                else
                {
                    await _mediator.Send(new UseFullTextSearchCommand() { UseFullTextSearch = true });
                    commonSettings.UseFullTextSearch = true;
                    await _settingService.SaveSetting(commonSettings);
                    SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.FullTextSettings.Enabled"));
                }
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
            }

            //selected tab
            await SaveSelectedTabIndex();

            return RedirectToAction("GeneralCommon");
        }
        //all settings
        public IActionResult AllSettings() => View();

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AllSettings(DataSourceRequest command, SettingFilterModel model)
        {
            var settings = new List<SettingModel>();
            foreach (var x in _settingService.GetAllSettings())
            {
                string storeName;
                if (String.IsNullOrEmpty(x.StoreId))
                {
                    storeName = _localizationService.GetResource("Admin.Configuration.Settings.AllSettings.Fields.StoreName.AllStores");
                }
                else
                {
                    var store = await _storeService.GetStoreById(x.StoreId);
                    storeName = store != null ? store.Shortcut : "Unknown";
                }
                var settingModel = new SettingModel {
                    Id = x.Id,
                    Name = x.Name,
                    Value = x.Value,
                    Store = storeName,
                    StoreId = string.IsNullOrEmpty(x.StoreId) ? " " : x.StoreId
                };
                settings.Add(settingModel);
            }

            if (model != null)
            {
                if (!string.IsNullOrEmpty(model.SettingFilterName))
                    settings = settings.Where(x => x.Name.ToLowerInvariant().Contains(model.SettingFilterName.ToLowerInvariant())).ToList();
                if (!string.IsNullOrEmpty(model.SettingFilterValue))
                    settings = settings.Where(x => x.Value.ToLowerInvariant().Contains(model.SettingFilterValue.ToLowerInvariant())).ToList();
            }
            var gridModel = new DataSourceResult {
                Data = settings.PagedForCommand(command).ToList(),
                Total = settings.Count()
            };

            return Json(gridModel);
        }

        public async Task<IActionResult> PushNotifications()
        {
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<PushNotificationsSettings>(storeScope);

            var model = new ConfigurationModel {
                AllowGuestNotifications = settings.AllowGuestNotifications,
                AuthDomain = settings.AuthDomain,
                DatabaseUrl = settings.DatabaseUrl,
                ProjectId = settings.ProjectId,
                PushApiKey = settings.PublicApiKey,
                SenderId = settings.SenderId,
                StorageBucket = settings.StorageBucket,
                PrivateApiKey = settings.PrivateApiKey,
                Enabled = settings.Enabled
            };

            return View(model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PushNotifications(ConfigurationModel model)
        {
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
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
            await _settingService.SaveSetting(settings);

            //edit js file needed by firebase
            var jsFilePath = CommonHelper.WebMapPath("firebase-messaging-sw.js");
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

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return await PushNotifications();
        }

        public async Task<IActionResult> AdminSearch()
        {
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<AdminSearchSettings>(storeScope);

            var model = new AdminSearchSettingsModel {
                SearchInBlogs = settings.SearchInBlogs,
                SearchInCategories = settings.SearchInCategories,
                SearchInCustomers = settings.SearchInCustomers,
                SearchInManufacturers = settings.SearchInManufacturers,
                SearchInNews = settings.SearchInNews,
                SearchInOrders = settings.SearchInOrders,
                SearchInProducts = settings.SearchInProducts,
                SearchInTopics = settings.SearchInTopics,
                MinSearchTermLength = settings.MinSearchTermLength,
                MaxSearchResultsCount = settings.MaxSearchResultsCount,
                ProductsDisplayOrder = settings.ProductsDisplayOrder,
                CategoriesDisplayOrder = settings.CategoriesDisplayOrder,
                ManufacturersDisplayOrder = settings.ManufacturersDisplayOrder,
                TopicsDisplayOrder = settings.TopicsDisplayOrder,
                NewsDisplayOrder = settings.NewsDisplayOrder,
                BlogsDisplayOrder = settings.BlogsDisplayOrder,
                CustomersDisplayOrder = settings.CustomersDisplayOrder,
                OrdersDisplayOrder = settings.OrdersDisplayOrder,
                SearchInMenu = settings.SearchInMenu,
                MenuDisplayOrder = settings.MenuDisplayOrder
            };

            return View(model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AdminSearch(DataSourceRequest command, AdminSearchSettingsModel model)
        {
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<AdminSearchSettings>(storeScope);
            settings.SearchInBlogs = model.SearchInBlogs;
            settings.SearchInCategories = model.SearchInCategories;
            settings.SearchInCustomers = model.SearchInCustomers;
            settings.SearchInManufacturers = model.SearchInManufacturers;
            settings.SearchInNews = model.SearchInNews;
            settings.SearchInOrders = model.SearchInOrders;
            settings.SearchInProducts = model.SearchInProducts;
            settings.SearchInTopics = model.SearchInTopics;
            settings.MinSearchTermLength = model.MinSearchTermLength;
            settings.MaxSearchResultsCount = model.MaxSearchResultsCount;
            settings.ProductsDisplayOrder = model.ProductsDisplayOrder;
            settings.CategoriesDisplayOrder = model.CategoriesDisplayOrder;
            settings.ManufacturersDisplayOrder = model.ManufacturersDisplayOrder;
            settings.TopicsDisplayOrder = model.TopicsDisplayOrder;
            settings.NewsDisplayOrder = model.NewsDisplayOrder;
            settings.BlogsDisplayOrder = model.BlogsDisplayOrder;
            settings.CustomersDisplayOrder = model.CustomersDisplayOrder;
            settings.OrdersDisplayOrder = model.OrdersDisplayOrder;
            settings.SearchInMenu = model.SearchInMenu;
            settings.MenuDisplayOrder = model.MenuDisplayOrder;

            await _settingService.SaveSetting(settings);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return await AdminSearch();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SettingUpdate(SettingModel model)
        {
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

            var setting = await _settingService.GetSettingById(model.Id);
            if (setting == null)
                return Content("No setting could be loaded with the specified ID");

            var storeId = model.StoreId;

            if (!setting.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase) ||
                setting.StoreId != storeId)
            {
                //setting name or store has been changed
                await _settingService.DeleteSetting(setting);
            }

            await _settingService.SetSetting(model.Name, model.Value, storeId);

            //activity log
            await _customerActivityService.InsertActivity("EditSettings", "", _localizationService.GetResource("ActivityLog.EditSettings"));

            //now clear cache
            await ClearCache();

            return new NullJsonResult();
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SettingAdd(SettingModel model)
        {
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
            await _settingService.SetSetting(model.Name, model.Value, storeId);

            //activity log
            await _customerActivityService.InsertActivity("AddNewSetting", "", _localizationService.GetResource("ActivityLog.AddNewSetting"), model.Name);

            //now clear cache
            await ClearCache();

            return new NullJsonResult();
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SettingDelete(string id)
        {
            var setting = await _settingService.GetSettingById(id);
            if (setting == null)
                throw new ArgumentException("No setting found with the specified id");
            await _settingService.DeleteSetting(setting);

            //activity log
            await _customerActivityService.InsertActivity("DeleteSetting", "", _localizationService.GetResource("ActivityLog.DeleteSetting"), setting.Name);

            //now clear cache
            await ClearCache();

            return new NullJsonResult();
        }

        #endregion
    }
}
