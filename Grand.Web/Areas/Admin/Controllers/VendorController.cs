using Grand.Core;
using Grand.Core.Domain.Vendors;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Vendors;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Vendors)]
    public partial class VendorController : BaseAdminController
    {
        #region Fields
        private readonly IVendorViewModelService _vendorViewModelService;
        private readonly ILocalizationService _localizationService;
        private readonly IVendorService _vendorService;
        private readonly ILanguageService _languageService;
        private readonly VendorSettings _vendorSettings;
        #endregion

        #region Constructors

        public VendorController(
            IVendorViewModelService vendorViewModelService,
            ILocalizationService localizationService,
            IVendorService vendorService,
            ILanguageService languageService,
            VendorSettings vendorSettings)
        {
            this._vendorViewModelService = vendorViewModelService;
            this._localizationService = localizationService;
            this._vendorService = vendorService;
            this._languageService = languageService;
            this._vendorSettings = vendorSettings;
        }

        #endregion

        #region Methods

        //list
        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List()
        {
            var model = new VendorListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command, VendorListModel model)
        {
            var vendors = _vendorService.GetAllVendors(model.SearchName, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = vendors.Select(x =>
                {
                    var vendorModel = x.ToModel();
                    return vendorModel;
                }),
                Total = vendors.TotalCount,
            };
            return Json(gridModel);
        }

        //create
        public IActionResult Create()
        {
            var model = _vendorViewModelService.PrepareVendorModel();
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Create(VendorModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var vendor = _vendorViewModelService.InsertVendorModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Vendors.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = vendor.Id }) : RedirectToAction("List");
            }
            //prepare address model
            _vendorViewModelService.PrepareVendorAddressModel(model, null);
            //discounts
            _vendorViewModelService.PrepareDiscountModel(model, null, true);
            //stores
            _vendorViewModelService.PrepareStore(model);

            //If we got this far, something failed, redisplay form
            return View(model);
        }


        //edit
        public IActionResult Edit(string id)
        {
            var vendor = _vendorService.GetVendorById(id);
            if (vendor == null || vendor.Deleted)
                //No vendor found with the specified id
                return RedirectToAction("List");

            var model = vendor.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = vendor.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = vendor.GetLocalized(x => x.Description, languageId, false, false);
                locale.MetaKeywords = vendor.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = vendor.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = vendor.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = vendor.GetSeName(languageId, false, false);
            });
            //discounts
            _vendorViewModelService.PrepareDiscountModel(model, vendor, false);
            //associated customer emails
            model.AssociatedCustomers = _vendorViewModelService.AssociatedCustomers(vendor.Id);
            //prepare address model
            _vendorViewModelService.PrepareVendorAddressModel(model, vendor);
            //stores
            _vendorViewModelService.PrepareStore(model);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(VendorModel model, bool continueEditing)
        {
            var vendor = _vendorService.GetVendorById(model.Id);
            if (vendor == null || vendor.Deleted)
                //No vendor found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                vendor = _vendorViewModelService.UpdateVendorModel(vendor, model);

                SuccessNotification(_localizationService.GetResource("Admin.Vendors.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = vendor.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //discounts
            _vendorViewModelService.PrepareDiscountModel(model, vendor, true);
            //prepare address model
            _vendorViewModelService.PrepareVendorAddressModel(model, vendor);
            //associated customer emails
            model.AssociatedCustomers = _vendorViewModelService.AssociatedCustomers(vendor.Id);
            //stores
            _vendorViewModelService.PrepareStore(model);

            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id)
        {
            var vendor = _vendorService.GetVendorById(id);
            if (vendor == null)
                //No vendor found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                _vendorViewModelService.DeleteVendor(vendor);
                SuccessNotification(_localizationService.GetResource("Admin.Vendors.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = vendor.Id });
        }

        #endregion

        #region Vendor notes

        [HttpPost]
        public IActionResult VendorNotesSelect(string vendorId, DataSourceRequest command)
        {
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                throw new ArgumentException("No vendor found with the specified id");

            var vendorNoteModels = _vendorViewModelService.PrepareVendorNote(vendor);
            var gridModel = new DataSourceResult
            {
                Data = vendorNoteModels,
                Total = vendorNoteModels.Count
            };

            return Json(gridModel);
        }

        public IActionResult VendorNoteAdd(string vendorId, string message)
        {
            if (ModelState.IsValid)
            {
                var result = _vendorViewModelService.InsertVendorNote(vendorId, message);
                return Json(new { Result = result });
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult VendorNoteDelete(string id, string vendorId)
        {
            if (ModelState.IsValid)
            {
                _vendorViewModelService.DeleteVendorNote(id, vendorId);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Reviews

        [HttpPost]
        public IActionResult Reviews(DataSourceRequest command, string vendorId, [FromServices] IWorkContext workContext)
        {
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                throw new ArgumentException("No vendor found with the specified id");

            //a vendor should have access only to his own profile
            if (workContext.CurrentVendor != null && vendor.Id != workContext.CurrentVendor.Id)
                return Content("This is not your vendor");

            var vendorReviews = _vendorService.GetAllVendorReviews("", null,
                null, null, "", vendorId, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = vendorReviews.Select(x =>
                {
                    var m = new VendorReviewModel();
                    _vendorViewModelService.PrepareVendorReviewModel(m, x, false, true);
                    return m;
                }),
                Total = vendorReviews.TotalCount,
            };

            return Json(gridModel);
        }

        #endregion

    }
}
