using Microsoft.AspNetCore.Mvc;
using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Vendors;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Seo;
using Grand.Services.Vendors;
using Grand.Framework.Security;
using Grand.Framework.Security.Captcha;
using Grand.Web.Models.Vendors;
using System;
using Grand.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Grand.Services.Media;
using Grand.Core.Domain.Common;
using Grand.Framework.Controllers;
using Grand.Core.Domain.Media;
using Grand.Web.Services;
using Grand.Services.Directory;
using Grand.Web.Extensions;

namespace Grand.Web.Controllers
{
    public partial class VendorController : BasePublicController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IVendorService _vendorService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly IAddressWebService _addressWebService;
        private readonly ICountryService _countryService;

        private readonly LocalizationSettings _localizationSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CommonSettings _commonSettings;
        private readonly MediaSettings _mediaSettings;
        #endregion

        #region Constructors

        public VendorController(IWorkContext workContext,
            ILocalizationService localizationService,
            ICustomerService customerService,
            IWorkflowMessageService workflowMessageService,
            IVendorService vendorService,
            IUrlRecordService urlRecordService,
            IPictureService pictureService,
            IAddressWebService addressWebService,
            ICountryService countryService,
            LocalizationSettings localizationSettings,
            VendorSettings vendorSettings,
            CaptchaSettings captchaSettings,
            CommonSettings commonSettings,
            MediaSettings mediaSettings)
        {
            this._workContext = workContext;
            this._localizationService = localizationService;
            this._customerService = customerService;
            this._workflowMessageService = workflowMessageService;
            this._vendorService = vendorService;
            this._urlRecordService = urlRecordService;
            this._pictureService = pictureService;
            this._addressWebService = addressWebService;
            this._countryService = countryService;
            this._localizationSettings = localizationSettings;
            this._vendorSettings = vendorSettings;
            this._captchaSettings = captchaSettings;
            this._commonSettings = commonSettings;
            this._mediaSettings = mediaSettings;
        }

        #endregion

        #region Utilities

        protected virtual void UpdatePictureSeoNames(Vendor vendor)
        {
            var picture = _pictureService.GetPictureById(vendor.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(vendor.Name));
        }

        #endregion

        #region Methods

        public virtual IActionResult ApplyVendor()
        {
            if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
                return RedirectToRoute("HomePage");

            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = new ApplyVendorModel();
            if (!String.IsNullOrEmpty(_workContext.CurrentCustomer.VendorId))
            {
                //already applied for vendor account
                model.DisableFormInput = true;
                model.Result = _localizationService.GetResource("Vendors.ApplyAccount.AlreadyApplied");
                return View(model);
            }

            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnApplyVendorPage;
            model.Email = _workContext.CurrentCustomer.Email;
            model.TermsOfServiceEnabled = _vendorSettings.TermsOfServiceEnabled;
            model.TermsOfServicePopup = _commonSettings.PopupForTermsOfServiceLinks;

            _addressWebService.PrepareVendorAddressModel(model: model.Address,
                address: null,
                excludeProperties: false,
                prePopulateWithCustomerFields: true,
                customer: _workContext.CurrentCustomer,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id),
                vendorSettings: _vendorSettings);

            return View(model);
        }

        [HttpPost, ActionName("ApplyVendor")]
        [PublicAntiForgery]
        [ValidateCaptcha]
        public virtual IActionResult ApplyVendorSubmit(ApplyVendorModel model, bool captchaValid, IFormFile uploadedFile)
        {
            if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
                return RedirectToRoute("HomePage");

            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnApplyVendorPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            string pictureId = "";

            if (uploadedFile != null && !string.IsNullOrEmpty(uploadedFile.FileName))
            {
                try
                {
                    var contentType = uploadedFile.ContentType;
                    var vendorPictureBinary = uploadedFile.GetPictureBits();
                    var picture = _pictureService.InsertPicture(vendorPictureBinary, contentType, null);

                    if (picture != null)
                        pictureId = picture.Id;
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", _localizationService.GetResource("Vendors.ApplyAccount.Picture.ErrorMessage"));
                }
            }

            if (ModelState.IsValid)
            {
                var description = Core.Html.HtmlHelper.FormatText(model.Description, false, false, true, false, false, false);
                var address = new Address();
                //disabled by default
                var vendor = new Vendor
                {
                    Name = model.Name,
                    Email = model.Email,
                    Description = description,
                    PageSize = 6,
                    PictureId = pictureId,
                    AllowCustomersToSelectPageSize = true,
                    PageSizeOptions = _vendorSettings.DefaultVendorPageSizeOptions,
                    AllowCustomerReviews = _vendorSettings.DefaultAllowCustomerReview,
                };
                model.Address.ToEntity(vendor.Address, true);
                _vendorService.InsertVendor(vendor);
                //search engine name (the same as vendor name)
                var seName = vendor.ValidateSeName(vendor.Name, vendor.Name, true);
                _urlRecordService.SaveSlug(vendor, seName, "");

                //associate to the current customer
                //but a store owner will have to manually add this customer role to "Vendors" role
                //if he wants to grant access to admin area
                _workContext.CurrentCustomer.VendorId = vendor.Id;
                _customerService.UpdateCustomerVendor(_workContext.CurrentCustomer);

                //notify store owner here (email)
                _workflowMessageService.SendNewVendorAccountApplyStoreOwnerNotification(_workContext.CurrentCustomer,
                    vendor, _localizationSettings.DefaultAdminLanguageId);

                model.DisableFormInput = true;
                model.Result = _localizationService.GetResource("Vendors.ApplyAccount.Submitted");
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnApplyVendorPage;

            _addressWebService.PrepareVendorAddressModel(model: model.Address,
                address: null,
                excludeProperties: false,
                prePopulateWithCustomerFields: true,
                customer: _workContext.CurrentCustomer,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id),
                vendorSettings: _vendorSettings);

            return View(model);
        }

        public virtual IActionResult Info()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (_workContext.CurrentVendor == null || !_vendorSettings.AllowVendorsToEditInfo)
                return RedirectToRoute("CustomerInfo");

            var model = new VendorInfoModel();
            var vendor = _workContext.CurrentVendor;
            model.Description = vendor.Description;
            model.Email = vendor.Email;
            model.Name = vendor.Name;

            var picture = _pictureService.GetPictureById(vendor.PictureId);
            var pictureSize = _mediaSettings.AvatarPictureSize;
            model.PictureUrl = picture != null ? _pictureService.GetPictureUrl(picture, pictureSize) : string.Empty;

            _addressWebService.PrepareVendorAddressModel(model: model.Address,
                address: vendor.Address,
                excludeProperties: false,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id),
                vendorSettings: _vendorSettings);

            return View(model);
        }

        [HttpPost, ActionName("Info")]
        [PublicAntiForgery]
        [FormValueRequired("save-info-button")]
        public virtual IActionResult Info(VendorInfoModel model, IFormFile uploadedFile)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (_workContext.CurrentVendor == null || !_vendorSettings.AllowVendorsToEditInfo)
                return RedirectToRoute("CustomerInfo");

            Picture picture = null;

            if (uploadedFile != null && !string.IsNullOrEmpty(uploadedFile.FileName))
            {
                try
                {
                    var contentType = uploadedFile.ContentType;
                    var vendorPictureBinary = uploadedFile.GetPictureBits();
                    picture = _pictureService.InsertPicture(vendorPictureBinary, contentType, null);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", _localizationService.GetResource("Account.VendorInfo.Picture.ErrorMessage"));
                }
            }

            var vendor = _workContext.CurrentVendor;
            var prevPicture = _pictureService.GetPictureById(vendor.PictureId);

            if (ModelState.IsValid && ModelState.ErrorCount == 0)
            {
                var description = Core.Html.HtmlHelper.FormatText(model.Description, false, false, true, false, false, false);

                vendor.Name = model.Name;
                vendor.Email = model.Email;
                vendor.Description = description;

                if (picture != null)
                {
                    vendor.PictureId = picture.Id;

                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }

                //update picture seo file name
                UpdatePictureSeoNames(vendor);
                model.Address.ToEntity(vendor.Address, true);

                _vendorService.UpdateVendor(vendor);

                //notifications
                if (_vendorSettings.NotifyStoreOwnerAboutVendorInformationChange)
                    _workflowMessageService.SendVendorInformationChangeNotification(vendor, _localizationSettings.DefaultAdminLanguageId);

                return RedirectToAction("Info");
            }

            _addressWebService.PrepareVendorAddressModel(model: model.Address,
                address: vendor.Address,
                excludeProperties: false,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id),
                vendorSettings: _vendorSettings);

            return View(model);
        }

        [HttpPost, ActionName("Info")]
        [PublicAntiForgery]
        [FormValueRequired("remove-picture")]
        public virtual IActionResult RemovePicture()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (_workContext.CurrentVendor == null || !_vendorSettings.AllowVendorsToEditInfo)
                return RedirectToRoute("CustomerInfo");

            var vendor = _workContext.CurrentVendor;
            var picture = _pictureService.GetPictureById(vendor.PictureId);

            if (picture != null)
                _pictureService.DeletePicture(picture);

            vendor.PictureId = "";
            _vendorService.UpdateVendor(vendor);

            //notifications
            if (_vendorSettings.NotifyStoreOwnerAboutVendorInformationChange)
                _workflowMessageService.SendVendorInformationChangeNotification(vendor, _localizationSettings.DefaultAdminLanguageId);

            return RedirectToAction("Info");
        }

        #endregion
    }
}
