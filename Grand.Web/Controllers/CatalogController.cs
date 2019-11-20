using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Vendors;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
using Grand.Framework.Security.Captcha;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Interfaces;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Vendors;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class CatalogController : BasePublicController
    {
        #region Fields

        private readonly ICatalogViewModelService _catalogViewModelService;
        private readonly IProductViewModelService _productViewModelService;        
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
        private readonly IVendorViewModelService _vendorViewModelService;
        private readonly VendorSettings _vendorSettings;
        
        #endregion

        #region Constructors

        public CatalogController(ICatalogViewModelService catalogViewModelService,
            IProductViewModelService productViewModelService,
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
            IVendorViewModelService vendorViewModelService,
            VendorSettings vendorSettings)
        {
            _catalogViewModelService = catalogViewModelService;
            _productViewModelService = productViewModelService;
            _vendorService = vendorService;
            _workContext = workContext;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _genericAttributeService = genericAttributeService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _permissionService = permissionService;
            _customerActivityService = customerActivityService;
            _customerActionEventService = customerActionEventService;
            _vendorViewModelService = vendorViewModelService;
            _vendorSettings = vendorSettings;
        }

        #endregion

        #region Utilities

        protected async Task SaveLastContinueShoppingPage(Customer customer)
        {
            await _genericAttributeService.SaveAttribute(customer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);
        }

        #endregion

        #region Categories

        public virtual async Task<IActionResult> Category(string categoryId, CatalogPagingFilteringModel command)
        {
            var category = await _catalogViewModelService.GetCategoryById(categoryId);
            if (category == null)
                return InvokeHttp404();

            var customer = _workContext.CurrentCustomer;

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a category before publishing
            if (!category.Published && !await _permissionService.Authorize(StandardPermissionProvider.ManageCategories, customer))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(category, customer))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(category))
                return InvokeHttp404();

            //'Continue shopping' URL
            await SaveLastContinueShoppingPage(customer);

            //display "edit" (manage) link
            if (await _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel, customer) && await _permissionService.Authorize(StandardPermissionProvider.ManageCategories, customer))
                DisplayEditLink(Url.Action("Edit", "Category", new { id = category.Id, area = "Admin" }));

            //activity log
            await _customerActivityService.InsertActivity("PublicStore.ViewCategory", category.Id, _localizationService.GetResource("ActivityLog.PublicStore.ViewCategory"), category.Name);
            await _customerActionEventService.Viewed(customer, this.HttpContext.Request.Path.ToString(), this.Request.Headers[HeaderNames.Referer].ToString() != null ? Request.Headers["Referer"].ToString() : "");
            //model
            var model = await _catalogViewModelService.PrepareCategory(category, command);

            //template
            var templateViewPath = await _catalogViewModelService.PrepareCategoryTemplateViewPath(category.CategoryTemplateId);

            return View(templateViewPath, model);
        }

        #endregion

        #region Manufacturers

        public virtual async Task<IActionResult> Manufacturer(string manufacturerId, CatalogPagingFilteringModel command)
        {
            var manufacturer = await _catalogViewModelService.GetManufacturerById(manufacturerId);
            if (manufacturer == null)
                return InvokeHttp404();

            var customer = _workContext.CurrentCustomer;

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a manufacturer before publishing
            if (!manufacturer.Published && !await _permissionService.Authorize(StandardPermissionProvider.ManageManufacturers, customer))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(manufacturer, customer))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(manufacturer))
                return InvokeHttp404();

            //'Continue shopping' URL
            await SaveLastContinueShoppingPage(customer);

            //display "edit" (manage) link
            if (await _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel, customer) && await _permissionService.Authorize(StandardPermissionProvider.ManageManufacturers, customer))
                DisplayEditLink(Url.Action("Edit", "Manufacturer", new { id = manufacturer.Id, area = "Admin" }));

            //activity log
            await _customerActivityService.InsertActivity("PublicStore.ViewManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.PublicStore.ViewManufacturer"), manufacturer.Name);
            await _customerActionEventService.Viewed(customer, this.HttpContext.Request.Path.ToString(), this.Request.Headers[HeaderNames.Referer].ToString() != null ? Request.Headers[HeaderNames.Referer].ToString() : "");

            //model
            var model = await _catalogViewModelService.PrepareManufacturer(manufacturer, command);

            //template
            var templateViewPath = await _catalogViewModelService.PrepareManufacturerTemplateViewPath(manufacturer.ManufacturerTemplateId);

            return View(templateViewPath, model);
        }

        public virtual async Task<IActionResult> ManufacturerAll()
        {
            var model = await _catalogViewModelService.PrepareManufacturerAll();
            return View(model);
        }

        #endregion

        #region Vendors

        public virtual async Task<IActionResult> Vendor(string vendorId, CatalogPagingFilteringModel command)
        {
            var vendor = await _vendorService.GetVendorById(vendorId);
            if (vendor == null || vendor.Deleted || !vendor.Active)
                return InvokeHttp404();

            //Vendor is active?
            if (!vendor.Active)
                return InvokeHttp404();

            var customer = _workContext.CurrentCustomer;

            //'Continue shopping' URL
            await SaveLastContinueShoppingPage(customer);

            //display "edit" (manage) link
            if (await _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel, customer) && await _permissionService.Authorize(StandardPermissionProvider.ManageManufacturers, customer))
                DisplayEditLink(Url.Action("Edit", "Vendor", new { id = vendor.Id, area = "Admin" }));

            var model = await _catalogViewModelService.PrepareVendor(vendor, command);
            //review
            model.VendorReviewOverview = _vendorViewModelService.PrepareVendorReviewOverviewModel(vendor);

            return View(model);
        }

        public virtual async Task<IActionResult> VendorAll()
        {
            //we don't allow viewing of vendors if "vendors" block is hidden
            if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
                return RedirectToRoute("HomePage");

            var model = await _catalogViewModelService.PrepareVendorAll();
            return View(model);
        }

        #endregion


        #region Vendor reviews

        public virtual async Task<IActionResult> VendorReviews(string vendorId)
        {
            var vendor = await _vendorService.GetVendorById(vendorId);
            if (vendor == null || !vendor.Active || !vendor.AllowCustomerReviews)
                return RedirectToRoute("HomePage");

            var model = new VendorReviewsModel();
            await _vendorViewModelService.PrepareVendorReviewsModel(model, vendor);
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
        public virtual async Task<IActionResult> VendorReviewsAdd(string vendorId, VendorReviewsModel model, bool captchaValid, 
            [FromServices] IOrderService orderService, [FromServices] IMediator eventPublisher, [FromServices] CaptchaSettings captchaSettings)
        {
            var vendor = await _vendorService.GetVendorById(vendorId);
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
                    !(await orderService.SearchOrders(customerId: _workContext.CurrentCustomer.Id, vendorId: vendorId, os: OrderStatus.Complete)).Any())
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("VendorReviews.VendorReviewPossibleOnlyAfterPurchasing"));

            if (ModelState.IsValid)
            {
                var vendorReview = await _vendorViewModelService.InsertVendorReview(vendor, model);
                //activity log
                await _customerActivityService.InsertActivity("PublicStore.AddVendorReview", vendor.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddVendorReview"), vendor.Name);

                //raise event
                if (vendorReview.IsApproved)
                    await eventPublisher.Publish(new VendorReviewApprovedEvent(vendorReview));

                await _vendorViewModelService.PrepareVendorReviewsModel(model, vendor);
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
            await _vendorViewModelService.PrepareVendorReviewsModel(model, vendor);
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> SetVendorReviewHelpfulness(string VendorReviewId, string vendorId, bool washelpful, [FromServices] ICustomerService customerService)
        {
            var vendor = await _vendorService.GetVendorById(vendorId);
            var vendorReview = await _vendorService.GetVendorReviewById(VendorReviewId);
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
                if (!customer.HasContributions)
                {
                    await customerService.UpdateContributions(customer);
                }
            }

            //new totals
            vendorReview.HelpfulYesTotal = vendorReview.VendorReviewHelpfulnessEntries.Count(x => x.WasHelpful);
            vendorReview.HelpfulNoTotal = vendorReview.VendorReviewHelpfulnessEntries.Count(x => !x.WasHelpful);
            await _vendorService.UpdateVendorReview(vendorReview);

            return Json(new
            {
                Result = _localizationService.GetResource("VendorReviews.Helpfulness.SuccessfullyVoted"),
                TotalYes = vendorReview.HelpfulYesTotal,
                TotalNo = vendorReview.HelpfulNoTotal
            });
        }

        #endregion

        #region Product tags

        public virtual async Task<IActionResult> ProductsByTag(string productTagId, CatalogPagingFilteringModel command, [FromServices] IProductTagService productTagService)
        {
            var productTag = await productTagService.GetProductTagById(productTagId);
            if (productTag == null)
                return InvokeHttp404();

            var model = await _catalogViewModelService.PrepareProductsByTag(productTag, command);
            return View(model);
        }
        public virtual async Task<IActionResult> ProductsByTagName(string seName, CatalogPagingFilteringModel command, [FromServices] IProductTagService productTagService)
        {
            var productTag = await productTagService.GetProductTagBySeName(seName);
            if (productTag == null)
                return InvokeHttp404();

            var model = await _catalogViewModelService.PrepareProductsByTag(productTag, command);
            return View("ProductsByTag", model);
        }

        public virtual async Task<IActionResult> ProductTagsAll()
        {
            var model = await _catalogViewModelService.PrepareProductTagsAll();
            return View(model);
        }

        #endregion

        #region Searching

        public virtual async Task<IActionResult> Search(SearchModel model, CatalogPagingFilteringModel command)
        {
            //'Continue shopping' URL
            await SaveLastContinueShoppingPage(_workContext.CurrentCustomer);
            
            //Prepare model
            var searchmodel = await _catalogViewModelService.PrepareSearch(model, command);

            return View(searchmodel);
        }

        public virtual async Task<IActionResult> SearchTermAutoComplete(string term, string categoryId, [FromServices] CatalogSettings catalogSettings)
        {
            if (String.IsNullOrWhiteSpace(term) || term.Length < catalogSettings.ProductSearchTermMinimumLength)
                return Content("");

            var result = await _catalogViewModelService.PrepareSearchAutoComplete(term, categoryId);
            
            return Json(result);
        }

        #endregion
    }
}
