using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Infrastructure;
using Grand.Framework.Extensions;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Models.Catalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class ProductReviewController : BaseAdminController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IStoreService _storeService;

        #endregion Fields

        #region Constructors

        public ProductReviewController(IProductService productService, 
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService, 
            IPermissionService permissionService,
            IEventPublisher eventPublisher,
            IStoreService storeService)
        {
            this._productService = productService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._eventPublisher = eventPublisher;
            this._storeService = storeService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual void PrepareProductReviewModel(ProductReviewModel model,
            ProductReview productReview, bool excludeProperties, bool formatReviewText)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (productReview == null)
                throw new ArgumentNullException("productReview");
            var product = _productService.GetProductById(productReview.ProductId);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(productReview.CustomerId);
            var store = _storeService.GetStoreById(productReview.StoreId);
            model.Id = productReview.Id;
            model.StoreName = store!=null ? store.Name : "";
            model.ProductId = productReview.ProductId;
            model.ProductName = product.Name;
            model.CustomerId = productReview.CustomerId;
            model.CustomerInfo = customer != null ? customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest") : "";
            model.Rating = productReview.Rating;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(productReview.CreatedOnUtc, DateTimeKind.Utc);
            model.Signature = productReview.Signature;
            if (!excludeProperties)
            {
                model.Title = productReview.Title;
                if (formatReviewText)
                {
                    model.ReviewText = Core.Html.HtmlHelper.FormatText(productReview.ReviewText, false, true, false, false, false, false);
                    model.ReplyText = Core.Html.HtmlHelper.FormatText(productReview.ReplyText, false, true, false, false, false, false);
                }
                else
                {
                    model.ReviewText = productReview.ReviewText;
                    model.ReplyText = productReview.ReplyText;
                }
                model.IsApproved = productReview.IsApproved;
            }
        }

        #endregion

        #region Methods

        //list
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductReviews))
                return AccessDeniedView();

            var model = new ProductReviewListModel();
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            var stores = _storeService.GetAllStores().Select(st => new SelectListItem() { Text = st.Name, Value = st.Id.ToString() });
                        foreach (var selectListItem in stores)
                model.AvailableStores.Add(selectListItem);
            return View(model);
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command, ProductReviewListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductReviews))
                return AccessDeniedView();

            DateTime? createdOnFromValue = (model.CreatedOnFrom == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? createdToFromValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var productReviews = _productService.GetAllProductReviews("", null, 
                createdOnFromValue, createdToFromValue, model.SearchText, model.SearchStoreId, model.SearchProductId);
            var gridModel = new DataSourceResult
            {
                Data = productReviews.PagedForCommand(command).Select(x =>
                {
                    var m = new ProductReviewModel();
                    PrepareProductReviewModel(m, x, false, true);
                    return m;
                }),
                Total = productReviews.Count,
            };

            return Json(gridModel);
        }

        //edit
        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductReviews))
                return AccessDeniedView();

            var productReview = _productService.GetProductReviewById(id); 
            var product = _productService.GetProductById(productReview.ProductId);

            if (productReview == null)
                //No product review found with the specified id
                return RedirectToAction("List");

            var model = new ProductReviewModel();
            PrepareProductReviewModel(model, productReview, false, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(ProductReviewModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductReviews))
                return AccessDeniedView();

            var productReview = _productService.GetProductReviewById(model.Id);
            var product = _productService.GetProductById(productReview.ProductId);

            if (productReview == null)
                //No product review found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                productReview.Title = model.Title;
                productReview.ReviewText = model.ReviewText;
                productReview.IsApproved = model.IsApproved;
                productReview.ReplyText= model.ReplyText;
                productReview.Signature= model.Signature;

                _productService.UpdateProductReview(productReview);

                //update product totals
                _productService.UpdateProductReviewTotals(product);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.ProductReviews.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = productReview.Id, ProductId = productReview.ProductId }) : RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            PrepareProductReviewModel(model, productReview, true, false);
            return View(model);
        }
        
        //delete
        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductReviews))
                return AccessDeniedView();

            var productReview = _productService.GetProductReviewById(id);
            if (productReview == null)
                //No product review found with the specified id
                return RedirectToAction("List");

            _productService.DeleteProductReview(productReview);

            var product = _productService.GetProductById(productReview.ProductId);
            //update product totals
            _productService.UpdateProductReviewTotals(product);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.ProductReviews.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult ApproveSelected(ICollection<string> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductReviews))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                foreach (var id in selectedIds)
                {
                    string idReview = id.Split(':').First().ToString();
                    string idProduct = id.Split(':').Last().ToString();
                    var product = _productService.GetProductById(idProduct);
                    var productReview = _productService.GetProductReviewById(idReview);
                    if (productReview != null)
                    {
                        var previousIsApproved = productReview.IsApproved;
                        productReview.IsApproved = true;
                        _productService.UpdateProductReview(productReview);
                        _productService.UpdateProductReviewTotals(product);

                        //raise event (only if it wasn't approved before)
                        if (!previousIsApproved)
                            _eventPublisher.Publish(new ProductReviewApprovedEvent(productReview));
                    }
                }
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult DisapproveSelected(ICollection<string> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductReviews))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                foreach (var id in selectedIds)
                {
                    string idReview = id.Split(':').First().ToString();
                    string idProduct = id.Split(':').Last().ToString();

                    var product = _productService.GetProductById(idProduct);
                    var productReview = _productService.GetProductReviewById(idReview);
                    if (productReview != null)
                    {
                        productReview.IsApproved = false;
                        _productService.UpdateProductReview(productReview);
                        //update product totals
                        _productService.UpdateProductReviewTotals(product);
                    }
                }
            }

            return Json(new { Result = true });
        }


        public IActionResult ProductSearchAutoComplete(string term)
        {
            const int searchTermMinimumLength = 3;
            if (String.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content("");

            //products
            const int productNumber = 15;
            var products = _productService.SearchProducts(
                keywords: term,
                pageSize: productNumber,
                showHidden: true);

            var result = (from p in products
                          select new
                          {
                              label = p.Name,
                              productid = p.Id
                          })
                .ToList();
            return Json(result);
        }
        #endregion
    }
}
