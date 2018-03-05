using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Vendors;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Framework.Security;
using Grand.Web.Models.Catalog;
using Grand.Web.Services;
using Grand.Framework.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Grand.Framework.Security.Captcha;
using Grand.Services.Events;
using Grand.Services.Orders;
using Grand.Web.Models.Vendors;
using Grand.Framework.Controllers;
using Grand.Core.Domain.Orders;
using Grand.Core.Infrastructure;
using System.Collections.Generic;

namespace Grand.Web.Controllers
{
    public partial class CatalogController : BasePublicController
    {
        #region Fields

        private readonly ICatalogWebService _catalogWebService;
        private readonly ICategoryService _categoryService;
        private readonly IProductWebService _productWebService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IProductTagService _productTagService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerActionEventService _customerActionEventService;
        private readonly IVendorWebService _vendorWebService;
        private readonly CaptchaSettings _captchaSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IOrderService _orderService;
        #endregion

        #region Constructors

        public CatalogController(ICatalogWebService catalogWebService,
            ICategoryService categoryService,
            IProductWebService productWebService,
            IManufacturerService manufacturerService,
            IProductService productService, 
            IVendorService vendorService,
            IWorkContext workContext, 
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IWebHelper webHelper, 
            IProductTagService productTagService,
            IGenericAttributeService genericAttributeService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService, 
            ICustomerActivityService customerActivityService,
            ICustomerActionEventService customerActionEventService,
            IVendorWebService vendorWebService,
            CaptchaSettings captchaSettings,
            MediaSettings mediaSettings,
            CatalogSettings catalogSettings,
            VendorSettings vendorSettings,
            ICacheManager cacheManager,
            IEventPublisher eventPublisher,
            IOrderService orderService)
        {
            this._catalogWebService = catalogWebService;
            this._categoryService = categoryService;
            this._productWebService = productWebService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._vendorService = vendorService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._productTagService = productTagService;
            this._genericAttributeService = genericAttributeService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._permissionService = permissionService;
            this._customerActivityService = customerActivityService;
            this._customerActionEventService = customerActionEventService;
            this._vendorWebService = vendorWebService;
            this._captchaSettings = captchaSettings;
            this._mediaSettings = mediaSettings;
            this._catalogSettings = catalogSettings;
            this._vendorSettings = vendorSettings;
            this._eventPublisher = eventPublisher;
            this._orderService = orderService;
        }

        #endregion

        #region Categories
        
        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult Category(string categoryId, CatalogPagingFilteringModel command)
        {
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null)
                return InvokeHttp404();

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a category before publishing
            if (!category.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(category))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(category))
                return InvokeHttp404();

            var customer = _workContext.CurrentCustomer;

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(customer, 
                SystemCustomerAttributeNames.LastContinueShoppingPage, 
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                DisplayEditLink(Url.Action("Edit", "Category", new { id = category.Id, area = "Admin" }));

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewCategory", category.Id, _localizationService.GetResource("ActivityLog.PublicStore.ViewCategory"), category.Name);
            _customerActionEventService.Viewed(customer, this.HttpContext.Request.Path.ToString(), this.Request.Headers[HeaderNames.Referer].ToString() != null ? Request.Headers["Referer"].ToString() : "");
            //model
            var model = _catalogWebService.PrepareCategory(category, command);

            //template
            var templateViewPath = _catalogWebService.PrepareCategoryTemplateViewPath(category.CategoryTemplateId);

            return View(templateViewPath, model);
        }

        #endregion

        #region Manufacturers

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult Manufacturer(string manufacturerId, CatalogPagingFilteringModel command)
        {
            var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null)
                return InvokeHttp404();

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a manufacturer before publishing
            if (!manufacturer.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(manufacturer))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(manufacturer))
                return InvokeHttp404();

            var customer = _workContext.CurrentCustomer;

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(customer, 
                SystemCustomerAttributeNames.LastContinueShoppingPage, 
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);
            
            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                DisplayEditLink(Url.Action("Edit", "Manufacturer", new { id = manufacturer.Id, area = "Admin" }));
            
            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.PublicStore.ViewManufacturer"), manufacturer.Name);
            _customerActionEventService.Viewed(customer, this.HttpContext.Request.Path.ToString(), this.Request.Headers[HeaderNames.Referer].ToString() != null ? Request.Headers[HeaderNames.Referer].ToString() : "");

            //model
            var model = _catalogWebService.PrepareManufacturer(manufacturer, command);

            //template
            var templateViewPath = _catalogWebService.PrepareManufacturerTemplateViewPath(manufacturer.ManufacturerTemplateId);

            return View(templateViewPath, model);
        }

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult ManufacturerAll()
        {
            var model = _catalogWebService.PrepareManufacturerAll();
            return View(model);
        }

        #endregion

        #region Vendors

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult Vendor(string vendorId, CatalogPagingFilteringModel command)
        {
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null || vendor.Deleted || !vendor.Active)
                return InvokeHttp404();

            //Vendor is active?
            if (!vendor.Active)
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);
            
            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                DisplayEditLink(Url.Action("Edit", "Vendor", new { id = vendor.Id, area = "Admin" }));

            var model = _catalogWebService.PrepareVendor(vendor, command);
            //review
            model.VendorReviewOverview = _vendorWebService.PrepareVendorReviewOverviewModel(vendor);

            return View(model);
        }

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult VendorAll()
        {
            //we don't allow viewing of vendors if "vendors" block is hidden
            if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
                return RedirectToRoute("HomePage");

            var model = _catalogWebService.PrepareVendorAll();
            return View(model);
        }

        #endregion


        #region Vendor reviews

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult VendorReviews(string vendorId)
        {
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null || !vendor.Active || !vendor.AllowCustomerReviews)
                return RedirectToRoute("HomePage");

            var model = new VendorReviewsModel();
            _vendorWebService.PrepareVendorReviewsModel(model, vendor);
            //only registered users can leave reviews
            if (_workContext.CurrentCustomer.IsGuest() && !_vendorSettings.AllowAnonymousUsersToReviewVendor)
                ModelState.AddModelError("", _localizationService.GetResource("VendorReviews.OnlyRegisteredUsersCanWriteReviews"));
            //default value
            model.AddVendorReview.Rating = _vendorSettings.DefaultVendorRatingValue;
            return View(model);
        }

        [HttpPost, ActionName("VendorReviews")]
        [FormValueRequired("add-review")]
        [PublicAntiForgery]
        [ValidateCaptcha]
        public virtual IActionResult VendorReviewsAdd(string vendorId, VendorReviewsModel model, bool captchaValid)
        {
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null || !vendor.Active || !vendor.AllowCustomerReviews)
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnVendorReviewPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_vendorSettings.AllowAnonymousUsersToReviewVendor)
            {
                ModelState.AddModelError("", _localizationService.GetResource("VendorReviews.OnlyRegisteredUsersCanWriteReviews"));
            }

            //allow reviews only by customer that bought something from this vendor
            if (_vendorSettings.VendorReviewPossibleOnlyAfterPurchasing &&
                    !_orderService.SearchOrders(customerId: _workContext.CurrentCustomer.Id, vendorId: vendorId, os: OrderStatus.Complete).Any())
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("VendorReviews.VendorReviewPossibleOnlyAfterPurchasing"));

            if (ModelState.IsValid)
            {
                var vendorReview = _vendorWebService.InsertVendorReview(vendor, model);
                //activity log
                _customerActivityService.InsertActivity("PublicStore.AddVendorReview", vendor.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddVendorReview"), vendor.Name);

                //raise event
                if (vendorReview.IsApproved)
                    _eventPublisher.Publish(new VendorReviewApprovedEvent(vendorReview));

                _vendorWebService.PrepareVendorReviewsModel(model, vendor);
                model.AddVendorReview.Title = null;
                model.AddVendorReview.ReviewText = null;

                model.AddVendorReview.SuccessfullyAdded = true;
                if (!vendorReview.IsApproved)
                    model.AddVendorReview.Result = _localizationService.GetResource("VendorReviews.SeeAfterApproving");
                else
                    model.AddVendorReview.Result = _localizationService.GetResource("VendorReviews.SuccessfullyAdded");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            _vendorWebService.PrepareVendorReviewsModel(model, vendor);
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult SetVendorReviewHelpfulness(string VendorReviewId, string vendorId, bool washelpful)
        {
            var vendor = _vendorService.GetVendorById(vendorId);
            var vendorReview = _vendorService.GetVendorReviewById(VendorReviewId);
            if (vendorReview == null)
                throw new ArgumentException("No vendor review found with the specified id");

            var customer = _workContext.CurrentCustomer;

            if (customer.IsGuest() && !_vendorSettings.AllowAnonymousUsersToReviewVendor)
            {
                return Json(new
                {
                    Result = _localizationService.GetResource("VendorReviews.Helpfulness.OnlyRegistered"),
                    TotalYes = vendorReview.HelpfulYesTotal,
                    TotalNo = vendorReview.HelpfulNoTotal
                });
            }

            //customers aren't allowed to vote for their own reviews
            if (vendorReview.CustomerId == customer.Id)
            {
                return Json(new
                {
                    Result = _localizationService.GetResource("VendorReviews.Helpfulness.YourOwnReview"),
                    TotalYes = vendorReview.HelpfulYesTotal,
                    TotalNo = vendorReview.HelpfulNoTotal
                });
            }

            //delete previous helpfulness
            var prh = vendorReview.VendorReviewHelpfulnessEntries
                .FirstOrDefault(x => x.CustomerId == customer.Id);
            if (prh != null)
            {
                //existing one
                prh.WasHelpful = washelpful;
            }
            else
            {
                //insert new helpfulness
                prh = new VendorReviewHelpfulness
                {
                    VendorReviewId = vendorReview.Id,
                    CustomerId = customer.Id,
                    WasHelpful = washelpful,
                };
                vendorReview.VendorReviewHelpfulnessEntries.Add(prh);
                if (!customer.IsHasVendorReviewH)
                {
                    customer.IsHasVendorReviewH = true;
                    EngineContext.Current.Resolve<ICustomerService>().UpdateHasVendorReviewH(customer.Id);
                }
            }

            //new totals
            vendorReview.HelpfulYesTotal = vendorReview.VendorReviewHelpfulnessEntries.Count(x => x.WasHelpful);
            vendorReview.HelpfulNoTotal = vendorReview.VendorReviewHelpfulnessEntries.Count(x => !x.WasHelpful);
            _vendorService.UpdateVendorReview(vendorReview);

            return Json(new
            {
                Result = _localizationService.GetResource("VendorReviews.Helpfulness.SuccessfullyVoted"),
                TotalYes = vendorReview.HelpfulYesTotal,
                TotalNo = vendorReview.HelpfulNoTotal
            });
        }

        #endregion

        #region Product tags


        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult ProductsByTag(string productTagId, CatalogPagingFilteringModel command)
        {
            var productTag = _productTagService.GetProductTagById(productTagId);
            if (productTag == null)
                return InvokeHttp404();

            var model = _catalogWebService.PrepareProductsByTag(productTag, command);
            return View(model);
        }

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult ProductTagsAll()
        {
            var model = _catalogWebService.PrepareProductTagsAll();
            return View(model);
        }

        #endregion

        #region Searching

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult Search(SearchModel model, CatalogPagingFilteringModel command)
        {
            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            var searchmodel = _catalogWebService.PrepareSearch(model, command);

            return View(searchmodel);
        }

        public virtual IActionResult SearchTermAutoComplete(string term, string categoryId)
        {
            if (String.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return Content("");

            //products
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;
            var categoryIds = new List<string>();
            if (!string.IsNullOrEmpty(categoryId))
            {
                categoryIds.Add(categoryId);
                if (_catalogSettings.ShowProductsFromSubcategoriesInSearchBox)
                {
                    //include subcategories
                    categoryIds.AddRange(_catalogWebService.GetChildCategoryIds(categoryId));
                }
            }


            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                keywords: term,
                categoryIds: categoryIds,
                searchSku: _catalogSettings.SearchBySku,
                searchDescriptions: _catalogSettings.SearchByDescription,
                languageId: _workContext.WorkingLanguage.Id,
                visibleIndividuallyOnly: true,
                pageSize: productNumber);

            var models =  _productWebService.PrepareProductOverviewModels(products, false, _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize).ToList();
            var result = (from p in models
                          select new
                          {
                              label = p.Name,
                              producturl = Url.RouteUrl("Product", new { SeName = p.SeName }),
                              productpictureurl = p.DefaultPictureModel.ImageUrl
                          })
                          .ToList();
            return Json(result);
        }

        #endregion
    }
}
