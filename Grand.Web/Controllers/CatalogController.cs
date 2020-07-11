using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Vendors;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Captcha;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Notifications.Vendors;
using Grand.Services.Queries.Models.Orders;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Commands.Models.Vendors;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Features.Models.Vendors;
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

        private readonly IVendorService _vendorService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICategoryService _categoryService;
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
        private readonly IMediator _mediator;

        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Constructors
        public CatalogController(
            IVendorService vendorService,
            IManufacturerService manufacturerService,
            ICategoryService categoryService,
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
            IMediator mediator,
            VendorSettings vendorSettings)
        {
            _vendorService = vendorService;
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
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
            _mediator = mediator;
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

        private VendorReviewOverviewModel PrepareVendorReviewOverviewModel(Vendor vendor)
        {
            var model = new VendorReviewOverviewModel() {
                RatingSum = vendor.ApprovedRatingSum,
                TotalReviews = vendor.ApprovedTotalReviews,
                VendorId = vendor.Id,
                AllowCustomerReviews = vendor.AllowCustomerReviews
            };
            return model;
        }

        #endregion

        #region Categories

        public virtual async Task<IActionResult> Category(string categoryId, CatalogPagingFilteringModel command)
        {
            var category = await _categoryService.GetCategoryById(categoryId);
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
            await _customerActionEventService.Viewed(customer, HttpContext.Request.Path.ToString(), Request.Headers[HeaderNames.Referer].ToString() != null ? Request.Headers["Referer"].ToString() : "");

            //model
            var model = await _mediator.Send(new GetCategory() {
                Category = category,
                Command = command,
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore
            });

            //template
            var templateViewPath = await _mediator.Send(new GetCategoryTemplateViewPath() { TemplateId = category.CategoryTemplateId });

            return View(templateViewPath, model);
        }

        #endregion

        #region Manufacturers

        public virtual async Task<IActionResult> Manufacturer(string manufacturerId, CatalogPagingFilteringModel command)
        {
            var manufacturer = await _manufacturerService.GetManufacturerById(manufacturerId);
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
            await _customerActionEventService.Viewed(customer, HttpContext.Request.Path.ToString(), Request.Headers[HeaderNames.Referer].ToString() != null ? Request.Headers[HeaderNames.Referer].ToString() : "");

            //model
            var model = await _mediator.Send(new GetManufacturer() {
                Command = command,
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Manufacturer = manufacturer,
                Store = _storeContext.CurrentStore
            });

            //template
            var templateViewPath = await _mediator.Send(new GetManufacturerTemplateViewPath() { TemplateId = manufacturer.ManufacturerTemplateId });

            return View(templateViewPath, model);
        }

        public virtual async Task<IActionResult> ManufacturerAll()
        {
            var model = await _mediator.Send(new GetManufacturerAll() {
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore
            });
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

            var model = await _mediator.Send(new GetVendor() {
                Command = command,
                Vendor = vendor,
                Language = _workContext.WorkingLanguage,
                Customer = _workContext.CurrentCustomer,
                Store = _storeContext.CurrentStore,
            });
            //review
            model.VendorReviewOverview = PrepareVendorReviewOverviewModel(vendor);

            return View(model);
        }

        public virtual async Task<IActionResult> VendorAll()
        {
            //we don't allow viewing of vendors if "vendors" block is hidden
            if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
                return RedirectToRoute("HomePage");

            var model = await _mediator.Send(new GetVendorAll() { Language = _workContext.WorkingLanguage });
            return View(model);
        }

        #endregion

        #region Vendor reviews

        public virtual async Task<IActionResult> VendorReviews(string vendorId)
        {
            var vendor = await _vendorService.GetVendorById(vendorId);
            if (vendor == null || !vendor.Active || !vendor.AllowCustomerReviews)
                return RedirectToRoute("HomePage");

            var model = await _mediator.Send(new GetVendorReviews() { Vendor = vendor });

            //only registered users can leave reviews
            if (_workContext.CurrentCustomer.IsGuest() && !_vendorSettings.AllowAnonymousUsersToReviewVendor)
                ModelState.AddModelError("", _localizationService.GetResource("VendorReviews.OnlyRegisteredUsersCanWriteReviews"));
            //default value
            model.AddVendorReview.Rating = _vendorSettings.DefaultVendorRatingValue;
            return View(model);
        }

        [HttpPost, ActionName("VendorReviews")]
        [FormValueRequired("add-review")]
        [AutoValidateAntiforgeryToken]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> VendorReviewsAdd(string vendorId, VendorReviewsModel model, bool captchaValid,
            [FromServices] CaptchaSettings captchaSettings)
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
                    !(await _mediator.Send(new GetOrderQuery() {
                        CustomerId = _workContext.CurrentCustomer.Id,
                        VendorId = vendorId,
                        Os = OrderStatus.Complete,
                        PageSize = 1
                    })).Any())
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("VendorReviews.VendorReviewPossibleOnlyAfterPurchasing"));

            if (ModelState.IsValid)
            {
                var vendorReview = await _mediator.Send(new InsertVendorReviewCommand() { Vendor = vendor, Store = _storeContext.CurrentStore, Model = model });
                //activity log
                await _customerActivityService.InsertActivity("PublicStore.AddVendorReview", vendor.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddVendorReview"), vendor.Name);

                //raise event
                if (vendorReview.IsApproved)
                    await _mediator.Publish(new VendorReviewApprovedEvent(vendorReview));

                model = await _mediator.Send(new GetVendorReviews() { Vendor = vendor });
                model.AddVendorReview.Title = null;
                model.AddVendorReview.ReviewText = null;

                model.AddVendorReview.SuccessfullyAdded = true;
                if (!vendorReview.IsApproved)
                    model.AddVendorReview.Result = _localizationService.GetResource("VendorReviews.SeeAfterApproving");
                else
                    model.AddVendorReview.Result = _localizationService.GetResource("VendorReviews.SuccessfullyAdded");

                return View(model);
            }
            model = await _mediator.Send(new GetVendorReviews() { Vendor = vendor });

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

            vendorReview = await _mediator.Send(new SetVendorReviewHelpfulnessCommand() {
                Customer = _workContext.CurrentCustomer,
                Vendor = vendor,
                Review = vendorReview,
                Washelpful = washelpful
            });

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

            var model = await _mediator.Send(new GetProductsByTag() {
                Command = command,
                Language = _workContext.WorkingLanguage,
                ProductTag = productTag,
                Customer = _workContext.CurrentCustomer,
                Store = _storeContext.CurrentStore
            });
            return View(model);
        }
        public virtual async Task<IActionResult> ProductsByTagName(string seName, CatalogPagingFilteringModel command, [FromServices] IProductTagService productTagService)
        {
            var productTag = await productTagService.GetProductTagBySeName(seName);
            if (productTag == null)
                return InvokeHttp404();

            var model = await _mediator.Send(new GetProductsByTag() {
                Command = command,
                Language = _workContext.WorkingLanguage,
                ProductTag = productTag,
                Customer = _workContext.CurrentCustomer,
                Store = _storeContext.CurrentStore
            });
            return View("ProductsByTag", model);
        }

        public virtual async Task<IActionResult> ProductTagsAll()
        {
            var model = await _mediator.Send(new GetProductTagsAll() {
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore
            });
            return View(model);
        }

        #endregion

        #region Searching

        public virtual async Task<IActionResult> Search(SearchModel model, CatalogPagingFilteringModel command, string searchCategoryId)
        {
            //'Continue shopping' URL
            await SaveLastContinueShoppingPage(_workContext.CurrentCustomer);
            if (model != null && !string.IsNullOrEmpty(searchCategoryId))
            {
                model.cid = searchCategoryId;
                model.adv = true;
                model.isc = true;
            }
            //Prepare model
            var isSearchTermSpecified = HttpContext?.Request?.Query.ContainsKey("q");
            var searchmodel = await _mediator.Send(new GetSearch() {
                Command = command,
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                IsSearchTermSpecified = isSearchTermSpecified.HasValue ? isSearchTermSpecified.Value : false,
                Language = _workContext.WorkingLanguage,
                Model = model,
                Store = _storeContext.CurrentStore
            });
            return View(searchmodel);
        }

        public virtual async Task<IActionResult> SearchTermAutoComplete(string term, string categoryId, [FromServices] CatalogSettings catalogSettings)
        {
            if (String.IsNullOrWhiteSpace(term) || term.Length < catalogSettings.ProductSearchTermMinimumLength)
                return Content("");

            var result = await _mediator.Send(new GetSearchAutoComplete() {
                CategoryId = categoryId,
                Term = term,
                Customer = _workContext.CurrentCustomer,
                Store = _storeContext.CurrentStore,
                Language = _workContext.WorkingLanguage,
                Currency = _workContext.WorkingCurrency
            });
            return Json(result);
        }

        #endregion
    }
}
