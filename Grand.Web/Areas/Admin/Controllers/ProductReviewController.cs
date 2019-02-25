using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.ProductReviews)]
    public partial class ProductReviewController : BaseAdminController
    {
        #region Fields
        private readonly IProductReviewViewModelService _productReviewViewModelService;
        private readonly ILocalizationService _localizationService;

        #endregion Fields

        #region Constructors

        public ProductReviewController(
            IProductReviewViewModelService productReviewViewModelService,
            ILocalizationService localizationService)
        {
            this._productReviewViewModelService = productReviewViewModelService;
            this._localizationService = localizationService;
        }

        #endregion

        #region Methods

        //list
        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List()
        {
            var model = _productReviewViewModelService.PrepareProductReviewListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command, ProductReviewListModel model)
        {
            var productReviews = _productReviewViewModelService.PrepareProductReviewsModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = productReviews.productReviewModels.ToList(),
                Total = productReviews.totalCount,
            };

            return Json(gridModel);
        }

        //edit
        public IActionResult Edit(string id, [FromServices] IProductService productService)
        {
            var productReview = productService.GetProductReviewById(id);

            if (productReview == null)
                //No product review found with the specified id
                return RedirectToAction("List");

            var model = new ProductReviewModel();
            _productReviewViewModelService.PrepareProductReviewModel(model, productReview, false, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(ProductReviewModel model, bool continueEditing, [FromServices] IProductService productService)
        {
            var productReview = productService.GetProductReviewById(model.Id);
            if (productReview == null)
                //No product review found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                productReview = _productReviewViewModelService.UpdateProductReview(productReview, model);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.ProductReviews.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = productReview.Id, ProductId = productReview.ProductId }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            _productReviewViewModelService.PrepareProductReviewModel(model, productReview, true, false);
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id, [FromServices] IProductService productService)
        {
            var productReview = productService.GetProductReviewById(id);
            if (productReview == null)
                //No product review found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                _productReviewViewModelService.DeleteProductReview(productReview);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.ProductReviews.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = productReview.Id });
        }

        [HttpPost]
        public IActionResult ApproveSelected(ICollection<string> selectedIds)
        {
            if (selectedIds != null)
            {
                _productReviewViewModelService.ApproveSelected(selectedIds.ToList());
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult DisapproveSelected(ICollection<string> selectedIds)
        {
            if (selectedIds != null)
            {
                _productReviewViewModelService.DisapproveSelected(selectedIds.ToList());
            }

            return Json(new { Result = true });
        }


        public IActionResult ProductSearchAutoComplete(string term, [FromServices] IProductService productService)
        {
            const int searchTermMinimumLength = 3;
            if (String.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content("");

            //products
            const int productNumber = 15;
            var products = productService.SearchProducts(
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
