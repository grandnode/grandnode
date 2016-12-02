using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Vendors;
using Grand.Core.Domain.Vendors;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Discounts;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Vendors;
using Grand.Services.Discounts;
using Grand.Web.Framework.Controllers;
using Grand.Web.Framework.Kendoui;
using Grand.Web.Framework.Mvc;
using System;
using MongoDB.Bson;

namespace Grand.Admin.Controllers
{
    public partial class VendorController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IVendorService _vendorService;
        private readonly IPermissionService _permissionService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILanguageService _languageService;
        private readonly IPictureService _pictureService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDiscountService _discountService;
        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Constructors

        public VendorController(ICustomerService customerService, 
            ILocalizationService localizationService,
            IVendorService vendorService, 
            IPermissionService permissionService,
            IUrlRecordService urlRecordService,
            ILanguageService languageService,
            IPictureService pictureService,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            VendorSettings vendorSettings)
        {
            this._customerService = customerService;
            this._localizationService = localizationService;
            this._vendorService = vendorService;
            this._permissionService = permissionService;
            this._urlRecordService = urlRecordService;
            this._languageService = languageService;
            this._pictureService = pictureService;
            this._dateTimeHelper = dateTimeHelper;
            this._vendorSettings = vendorSettings;
            this._discountService = discountService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual void UpdatePictureSeoNames(Vendor vendor)
        {
            var picture = _pictureService.GetPictureById(vendor.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(vendor.Name));
        }


        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(Vendor vendor, VendorModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();

            foreach (var local in model.Locales)
            {
                var seName = vendor.ValidateSeName(local.SeName, local.Name, false);
                _urlRecordService.SaveSlug(vendor, seName, local.LanguageId);

                localized.Add(new LocalizedProperty()
                {
                    LanguageId = local.LanguageId,
                    LocaleKey = "Description",
                    LocaleValue = local.Description
                });

                localized.Add(new LocalizedProperty()
                {
                    LanguageId = local.LanguageId,
                    LocaleKey = "MetaDescription",
                    LocaleValue = local.MetaDescription
                });

                localized.Add(new LocalizedProperty()
                {
                    LanguageId = local.LanguageId,
                    LocaleKey = "MetaKeywords",
                    LocaleValue = local.MetaKeywords
                });

                localized.Add(new LocalizedProperty()
                {
                    LanguageId = local.LanguageId,
                    LocaleKey = "MetaTitle",
                    LocaleValue = local.MetaTitle
                });

                localized.Add(new LocalizedProperty()
                {
                    LanguageId = local.LanguageId,
                    LocaleKey = "Name",
                    LocaleValue = local.Name
                });

            }
            return localized;

        }
        [NonAction]
        protected virtual void PrepareDiscountModel(VendorModel model, Vendor vendor, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableDiscounts = _discountService
                .GetAllDiscounts(DiscountType.AssignedToVendors, showHidden: true)
                .Select(d => d.ToModel())
                .ToList();

            if (!excludeProperties && vendor != null)
            {
                model.SelectedDiscountIds = vendor.AppliedDiscounts.ToArray();
            }
        }
        #endregion

        #region Methods

        //list
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            var model = new VendorListModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command, VendorListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

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

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();


            var model = new VendorModel();
            //locales
            AddLocales(_languageService, model.Locales);
            //discounts
            PrepareDiscountModel(model, null, true);
            //default values
            model.PageSize = 6;
            model.Active = true;
            model.AllowCustomersToSelectPageSize = true;
            model.PageSizeOptions = _vendorSettings.DefaultVendorPageSizeOptions;

            //default value
            model.Active = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public ActionResult Create(VendorModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var vendor = model.ToEntity();
                _vendorService.InsertVendor(vendor);

                //discounts
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToVendors, showHidden: true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                        vendor.AppliedDiscounts.Add(discount.Id);
                }

                //search engine name
                model.SeName = vendor.ValidateSeName(model.SeName, vendor.Name, true);
                vendor.Locales = UpdateLocales(vendor, model);
                vendor.SeName = model.SeName;
                _vendorService.UpdateVendor(vendor);

                //update picture seo file name
                UpdatePictureSeoNames(vendor);

                _urlRecordService.SaveSlug(vendor, model.SeName, "");


                SuccessNotification(_localizationService.GetResource("Admin.Vendors.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = vendor.Id }) : RedirectToAction("List");
            }
            //discounts
            PrepareDiscountModel(model, null, true);
            //If we got this far, something failed, redisplay form
            return View(model);
        }


        //edit
        public ActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

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
            PrepareDiscountModel(model, vendor, false);
            //associated customer emails
            model.AssociatedCustomers = _customerService
                .GetAllCustomers(vendorId: vendor.Id)
                .Select(c => new VendorModel.AssociatedCustomerInfo()
                {
                    Id = c.Id,
                    Email = c.Email
                })
                .ToList();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(VendorModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(model.Id);
            if (vendor == null || vendor.Deleted)
                //No vendor found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                string prevPictureId = vendor.PictureId;
                vendor = model.ToEntity(vendor);
                vendor.Locales = UpdateLocales(vendor, model);
                model.SeName = vendor.ValidateSeName(model.SeName, vendor.Name, true);

                //discounts
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToVendors, showHidden: true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    {
                        //new discount
                        if (vendor.AppliedDiscounts.Count(d => d == discount.Id) == 0)
                            vendor.AppliedDiscounts.Add(discount.Id);
                    }
                    else
                    {
                        //remove discount
                        if (vendor.AppliedDiscounts.Count(d => d == discount.Id) > 0)
                            vendor.AppliedDiscounts.Remove(discount.Id);
                    }
                }

                vendor.SeName = model.SeName;

                _vendorService.UpdateVendor(vendor);
                //search engine name                
                _urlRecordService.SaveSlug(vendor, model.SeName, "");

                //delete an old picture (if deleted or updated)
                if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != vendor.PictureId)
                {
                    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }
                //update picture seo file name
                UpdatePictureSeoNames(vendor);


                SuccessNotification(_localizationService.GetResource("Admin.Vendors.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit",  new {id = vendor.Id});
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //discounts
            PrepareDiscountModel(model, vendor, true);

            //associated customer emails
            model.AssociatedCustomers = _customerService
                .GetAllCustomers(vendorId: vendor.Id)
                .Select(c => new VendorModel.AssociatedCustomerInfo()
                {
                    Id = c.Id,
                    Email = c.Email
                })
                .ToList();

            return View(model);
        }

        //delete
        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(id);
            if (vendor == null)
                //No vendor found with the specified id
                return RedirectToAction("List");

            //clear associated customer references
            var associatedCustomers = _customerService.GetAllCustomers(vendorId: vendor.Id);
            foreach (var customer in associatedCustomers)
            {
                customer.VendorId = "";
                _customerService.UpdateCustomer(customer);
            }

            _vendorService.DeleteVendor(vendor);
            SuccessNotification(_localizationService.GetResource("Admin.Vendors.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Vendor notes

        [HttpPost]
        public ActionResult VendorNotesSelect(string vendorId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                throw new ArgumentException("No vendor found with the specified id");

            var vendorNoteModels = new List<VendorModel.VendorNote>();
            foreach (var vendorNote in vendor.VendorNotes
                .OrderByDescending(vn => vn.CreatedOnUtc))
            {
                vendorNoteModels.Add(new VendorModel.VendorNote
                {
                    Id = vendorNote.Id,
                    VendorId = vendor.Id,
                    Note = vendorNote.FormatVendorNoteText(),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(vendorNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }

            var gridModel = new DataSourceResult
            {
                Data = vendorNoteModels,
                Total = vendorNoteModels.Count
            };

            return Json(gridModel);
        }

        [ValidateInput(false)]
        public ActionResult VendorNoteAdd(string vendorId, string message)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var vendorNote = new VendorNote
            {
                Note = message,
                VendorId = vendorId,
                CreatedOnUtc = DateTime.UtcNow,
            };
            vendor.VendorNotes.Add(vendorNote);
            _vendorService.UpdateVendor(vendor);

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult VendorNoteDelete(string id, string vendorId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                throw new ArgumentException("No vendor found with the specified id");

            var vendorNote = vendor.VendorNotes.FirstOrDefault(vn => vn.Id == id);
            if (vendorNote == null)
                throw new ArgumentException("No vendor note found with the specified id");
            vendorNote.VendorId = vendor.Id;

            _vendorService.DeleteVendorNote(vendorNote);

            return new NullJsonResult();
        }

        #endregion


    }
}
