using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Grand.Core;
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
        private readonly IProductWebService _productWebService;        
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerActionEventService _customerActionEventService;
        private readonly IVendorWebService _vendorWebService;
        private readonly VendorSettings _vendorSettings;
        
        #endregion

        #region Constructors

        public CatalogController(ICatalogWebService catalogWebService,
            IProductWebService productWebService,
            IVendorService vendorService,
            IWorkContext workContext, 
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IWebHelper webHelper, 
            IGenericAttributeService genericAttributeService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService, 
            ICustomerActivityService customerActivityService,
            ICustomerActionEventService customerActionEventService,
            IVendorWebService vendorWebService,
            VendorSettings vendorSettings)
        {
            this._catalogWebService = catalogWebService;
            this._productWebService = productWebService;
            this._vendorService = vendorService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._genericAttributeService = genericAttributeService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._permissionService = permissionService;
            this._customerActivityService = customerActivityService;
            this._customerActionEventService = customerActionEventService;
            this._vendorWebService = vendorWebService;
            this._vendorSettings = vendorSettings;
        }

        #endregion

        #region Categories
        
        public virtual IActionResult Category(string categoryId, CatalogPagingFilteringModel command)
        {
            var category = _catalogWebService.GetCategoryById(categoryId);
            if (category == null)
                return InvokeHttp404();

            var customer = _workContext.CurrentCustomer;

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a category before publishing
            if (!category.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageCategories, customer))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(category, customer))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(category))
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(customer, 
                SystemCustomerAttributeNames.LastContinueShoppingPage, 
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel, customer) && _permissionService.Authorize(StandardPermissionProvider.ManageCategories, customer))
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

        public virtual IActionResult Manufacturer(string manufacturerId, CatalogPagingFilteringModel command)
        {
            var manufacturer = _catalogWebService.GetManufacturerById(manufacturerId);
            if (manufacturer == null)
                return InvokeHttp404();

            var customer = _workContext.CurrentCustomer;

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a manufacturer before publishing
            if (!manufacturer.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers, customer))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(manufacturer, customer))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(manufacturer))
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(customer, 
                SystemCustomerAttributeNames.LastContinueShoppingPage, 
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);
            
            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel, customer) && _permissionService.Authorize(StandardPermissionProvider.ManageManufacturers, customer))
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

        public virtual IActionResult ManufacturerAll()
        {
            var model = _catalogWebService.PrepareManufacturerAll();
            return View(model);
        }

        #endregion

        #region Vendors

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
        public virtual IActionResult VendorReviewsAdd(string vendorId, VendorReviewsModel model, bool captchaValid, 
            [FromServices] IOrderService orderService, [FromServices] IEventPublisher eventPublisher, [FromServices] CaptchaSettings captchaSettings)
        {
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null || !vendor.Active || !vendor.AllowCustomerReviews)
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (captchaSettings.Enabled && captchaSettings.ShowOnVendorReviewPage && !captchaValid)
            {
                ModelState.AddModelError("", captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_vendorSettings.AllowAnonymousUsersToReviewVendor)
            {
                ModelState.AddModelError("", _localizationService.GetResource("VendorReviews.OnlyRegisteredUsersCanWriteReviews"));
            }

            //allow reviews only by customer that bought something from this vendor
            if (_vendorSettings.VendorReviewPossibleOnlyAfterPurchasing &&
                    !orderService.SearchOrders(customerId: _workContext.CurrentCustomer.Id, vendorId: vendorId, os: OrderStatus.Complete).Any())
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("VendorReviews.VendorReviewPossibleOnlyAfterPurchasing"));

            if (ModelState.IsValid)
            {
                var vendorReview = _vendorWebService.InsertVendorReview(vendor, model);
                //activity log
                _customerActivityService.InsertActivity("PublicStore.AddVendorReview", vendor.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddVendorReview"), vendor.Name);

                //raise event
                if (vendorReview.IsApproved)
                    eventPublisher.Publish(new VendorReviewApprovedEvent(vendorReview));

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

        public virtual IActionResult ProductsByTag(string productTagId, CatalogPagingFilteringModel command, [FromServices] IProductTagService productTagService)
        {
            var productTag = productTagService.GetProductTagById(productTagId);
            if (productTag == null)
                return InvokeHttp404();

            var model = _catalogWebService.PrepareProductsByTag(productTag, command);
            return View(model);
        }

        public virtual IActionResult ProductTagsAll()
        {
            var model = _catalogWebService.PrepareProductTagsAll();
            return View(model);
        }

        #endregion

        #region Searching

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

        public virtual IActionResult SearchTermAutoComplete(string term, string categoryId, 
            [FromServices] IProductService productService, [FromServices] CatalogSettings catalogSettings,
            [FromServices] MediaSettings mediaSettings)
        {
            if (String.IsNullOrWhiteSpace(term) || term.Length < catalogSettings.ProductSearchTermMinimumLength)
                return Content("");

            //products
            var productNumber = catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;
            var categoryIds = new List<string>();
            if (!string.IsNullOrEmpty(categoryId))
            {
                categoryIds.Add(categoryId);
                if (catalogSettings.ShowProductsFromSubcategoriesInSearchBox)
                {
                    //include subcategories
                    categoryIds.AddRange(_catalogWebService.GetChildCategoryIds(categoryId));
                }
            }


            var products = productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                keywords: term,
                categoryIds: categoryIds,
                searchSku: catalogSettings.SearchBySku,
                searchDescriptions: catalogSettings.SearchByDescription,
                languageId: _workContext.WorkingLanguage.Id,
                visibleIndividuallyOnly: true,
                pageSize: productNumber);

            var models =  _productWebService.PrepareProductOverviewModels(products, false, catalogSettings.ShowProductImagesInSearchAutoComplete, mediaSettings.AutoCompleteSearchThumbPictureSize).ToList();
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
