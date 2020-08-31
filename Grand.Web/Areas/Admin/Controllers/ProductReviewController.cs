using Grand.Core;
using Grand.Domain.Customers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.ProductReviews)]
    public partial class ProductReviewController : BaseAdminController
    {
        #region Fields

        private readonly IProductReviewViewModelService _productReviewViewModelService;
        private readonly IProductReviewService _productReviewService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;

        #endregion Fields

        #region Constructors

        public ProductReviewController(
            IProductReviewViewModelService productReviewViewModelService, 
            IProductReviewService productReviewService,
            ILocalizationService localizationService,
            IWorkContext workContext)
        {
            _productReviewViewModelService = productReviewViewModelService;
            _productReviewService = productReviewService;
            _localizationService = localizationService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        //list
        public IActionResult Index() => RedirectToAction("List");

        public async Task<IActionResult> List()
        {
            var model = await _productReviewViewModelService.PrepareProductReviewListModel(_workContext.CurrentCustomer.StaffStoreId);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command, ProductReviewListModel model)
        {
            //limit for store manager
            if (_workContext.CurrentCustomer.IsStaff())
                model.SearchStoreId = _workContext.CurrentCustomer.StaffStoreId;

            var (productReviewModels, totalCount) = await _productReviewViewModelService.PrepareProductReviewsModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = productReviewModels.ToList(),
                Total = totalCount,
            };

            return Json(gridModel);
        }

        //edit
        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var productReview = await _productReviewService.GetProductReviewById(id);

            if (productReview == null)
                //No product review found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && productReview.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var model = new ProductReviewModel();
            await _productReviewViewModelService.PrepareProductReviewModel(model, productReview, false, false);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(ProductReviewModel model, bool continueEditing)
        {
            var productReview = await _productReviewService.GetProductReviewById(model.Id);
            if (productReview == null)
                //No product review found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && productReview.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                productReview = await _productReviewViewModelService.UpdateProductReview(productReview, model);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.ProductReviews.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = productReview.Id, ProductId = productReview.ProductId }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            await _productReviewViewModelService.PrepareProductReviewModel(model, productReview, true, false);
            return View(model);
        }

        //delete
        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var productReview = await _productReviewService.GetProductReviewById(id);
            if (productReview == null)
                //No product review found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && productReview.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                await _productReviewViewModelService.DeleteProductReview(productReview);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.ProductReviews.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = productReview.Id });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ApproveSelected(ICollection<string> selectedIds)
        {
            if (selectedIds != null)
            {
                await _productReviewViewModelService.ApproveSelected(selectedIds.ToList(), _workContext.CurrentCustomer.StaffStoreId);
            }

            return Json(new { Result = true });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> DisapproveSelected(ICollection<string> selectedIds)
        {
            if (selectedIds != null)
            {
                await _productReviewViewModelService.DisapproveSelected(selectedIds.ToList(), _workContext.CurrentCustomer.StaffStoreId);
            }

            return Json(new { Result = true });
        }

        
        public async Task<IActionResult> ProductSearchAutoComplete(string term, [FromServices] IProductService productService)
        {
            const int searchTermMinimumLength = 3;
            if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content("");

            var storeId = string.Empty;
            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            //products
            const int productNumber = 15;
            var products = (await productService.SearchProducts(
                storeId: storeId,
                keywords: term,
                pageSize: productNumber,
                showHidden: true)).products;

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
