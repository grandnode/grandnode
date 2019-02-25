using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Vendors;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.VendorReviews)]
    public partial class VendorReviewController : BaseAdminController
    {
        #region Fields
        private readonly IVendorViewModelService _vendorViewModelService;
        private readonly IVendorService _vendorService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        #endregion Fields

        #region Constructors

        public VendorReviewController(
            IVendorViewModelService vendorViewModelService,
            IVendorService vendorService,
            ILocalizationService localizationService,
            IWorkContext workContext)
        {
            this._vendorViewModelService = vendorViewModelService;
            this._vendorService = vendorService;
            this._localizationService = localizationService;
            this._workContext = workContext;
        }

        #endregion

        #region Methods

        //list
        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List()
        {
            var model = new VendorReviewListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command, VendorReviewListModel model)
        {
            var vendorId = string.Empty;
            //vendor
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }
            //admin
            else if (_workContext.CurrentCustomer.IsAdmin())
            {
                vendorId = model.SearchVendorId;
            }
            else
                return AccessDeniedView();

            model.SearchVendorId = vendorId;
            var vendorReviews = _vendorViewModelService.PrepareVendorReviewModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = vendorReviews.vendorReviewModels.ToList(),
                Total = vendorReviews.totalCount,
            };

            return Json(gridModel);
        }

        //edit
        public IActionResult Edit(string id)
        {
            var vendorReview = _vendorService.GetVendorReviewById(id);

            if (vendorReview == null)
                //No vendor review found with the specified id
                return RedirectToAction("List");

            var model = new VendorReviewModel();
            _vendorViewModelService.PrepareVendorReviewModel(model, vendorReview, false, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(VendorReviewModel model, bool continueEditing)
        {
            var vendorReview = _vendorService.GetVendorReviewById(model.Id);
            if (vendorReview == null)
                //No vendor review found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                vendorReview = _vendorViewModelService.UpdateVendorReviewModel(vendorReview, model);
                SuccessNotification(_localizationService.GetResource("Admin.VendorReviews.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = vendorReview.Id, VendorId = vendorReview.VendorId }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            _vendorViewModelService.PrepareVendorReviewModel(model, vendorReview, true, false);
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id)
        {
            var vendorReview = _vendorService.GetVendorReviewById(id);
            if (vendorReview == null)
                //No vendor review found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                _vendorViewModelService.DeleteVendorReview(vendorReview);

                SuccessNotification(_localizationService.GetResource("Admin.VendorReviews.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = id });
        }

        [HttpPost]
        public IActionResult ApproveSelected(ICollection<string> selectedIds)
        {
            if (selectedIds != null)
            {
                _vendorViewModelService.ApproveVendorReviews(selectedIds.ToList());
            }
            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult DisapproveSelected(ICollection<string> selectedIds)
        {
            if (selectedIds != null)
            {
                _vendorViewModelService.DisapproveVendorReviews(selectedIds.ToList());
            }

            return Json(new { Result = true });
        }

        public IActionResult VendorSearchAutoComplete(string term)
        {
            const int searchTermMinimumLength = 3;
            if (String.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content("");

            var vendors = _vendorService.SearchVendors(
                keywords: term);

            var result = (from p in vendors
                          select new
                          {
                              label = p.Name,
                              vendorid = p.Id
                          })
                .ToList();
            return Json(result);
        }
        #endregion
    }
}