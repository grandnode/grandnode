using Grand.Core;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Media;
using Grand.Domain.Seo;
using Grand.Domain.Vendors;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Captcha;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Seo;
using Grand.Services.Vendors;
using Grand.Web.Commands.Models.Vendors;
using Grand.Web.Extensions;
using Grand.Web.Features.Models.Common;
using Grand.Web.Models.Common;
using Grand.Web.Models.Vendors;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class VendorController : BasePublicController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IVendorService _vendorService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ICountryService _countryService;
        private readonly IMediator _mediator;
        private readonly LocalizationSettings _localizationSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CommonSettings _commonSettings;
        private readonly MediaSettings _mediaSettings;
        #endregion

        #region Constructors

        public VendorController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            ICustomerService customerService,
            IWorkflowMessageService workflowMessageService,
            IVendorService vendorService,
            IUrlRecordService urlRecordService,
            IPictureService pictureService,
            ICountryService countryService,
            IMediator mediator,
            LocalizationSettings localizationSettings,
            VendorSettings vendorSettings,
            CaptchaSettings captchaSettings,
            CommonSettings commonSettings,
            MediaSettings mediaSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _customerService = customerService;
            _workflowMessageService = workflowMessageService;
            _vendorService = vendorService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _countryService = countryService;
            _mediator = mediator;
            _localizationSettings = localizationSettings;
            _vendorSettings = vendorSettings;
            _captchaSettings = captchaSettings;
            _commonSettings = commonSettings;
            _mediaSettings = mediaSettings;
        }

        #endregion

        #region Utilities

        protected virtual async Task UpdatePictureSeoNames(Vendor vendor)
        {
            var picture = await _pictureService.GetPictureById(vendor.PictureId);
            if (picture != null)
                await _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(vendor.Name));
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> ApplyVendor()
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
            var countries = await _countryService.GetAllCountries(_workContext.WorkingLanguage.Id);
            model.Address = await _mediator.Send(new GetVendorAddress() {
                Language = _workContext.WorkingLanguage,
                Address = null,
                ExcludeProperties = false,
                PrePopulateWithCustomerFields = true,
                Customer = _workContext.CurrentCustomer,
                LoadCountries = () => countries,
            });

            return View(model);
        }

        [HttpPost, ActionName("ApplyVendor")]
        [AutoValidateAntiforgeryToken]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> ApplyVendorSubmit(ApplyVendorModel model, bool captchaValid, IFormFile uploadedFile)
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

            string pictureId = string.Empty;
            string contentType = string.Empty;
            byte[] vendorPictureBinary = null;

            if (uploadedFile != null && !string.IsNullOrEmpty(uploadedFile.FileName))
            {
                try
                {
                    contentType = uploadedFile.ContentType;
                    if (string.IsNullOrEmpty(contentType))
                        ModelState.AddModelError("", "Empty content type");
                    else
                        if (!contentType.StartsWith("image"))
                            ModelState.AddModelError("", "Only image content type is allowed");

                    vendorPictureBinary = uploadedFile.GetPictureBits();
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", _localizationService.GetResource("Vendors.ApplyAccount.Picture.ErrorMessage"));
                }
            }

            if (ModelState.IsValid)
            {
                if (vendorPictureBinary != null && !string.IsNullOrEmpty(contentType))
                {
                    var picture = await _pictureService.InsertPicture(vendorPictureBinary, contentType, null);
                    if (picture != null)
                        pictureId = picture.Id;
                }

                var description = Core.Html.HtmlHelper.FormatText(model.Description, false, false, true, false, false, false);
                var address = new Address();
                //disabled by default
                var vendor = new Vendor {
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
                await _vendorService.InsertVendor(vendor);

                //search engine name (the same as vendor name)                
                var seName = await vendor.ValidateSeName(vendor.Name, vendor.Name, true, HttpContext.RequestServices.GetRequiredService<SeoSettings>(),
                    HttpContext.RequestServices.GetRequiredService<IUrlRecordService>(), HttpContext.RequestServices.GetRequiredService<ILanguageService>());
                await _urlRecordService.SaveSlug(vendor, seName, "");

                //associate to the current customer
                //but a store owner will have to manually add this customer role to "Vendors" role
                //if he wants to grant access to admin area
                _workContext.CurrentCustomer.VendorId = vendor.Id;
                await _customerService.UpdateCustomerVendor(_workContext.CurrentCustomer);

                //notify store owner here (email)
                await _workflowMessageService.SendNewVendorAccountApplyStoreOwnerNotification(_workContext.CurrentCustomer,
                    vendor, _storeContext.CurrentStore, _localizationSettings.DefaultAdminLanguageId);

                model.DisableFormInput = true;
                model.Result = _localizationService.GetResource("Vendors.ApplyAccount.Submitted");
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnApplyVendorPage;
            var countries = await _countryService.GetAllCountries(_workContext.WorkingLanguage.Id);
            model.Address = await _mediator.Send(new GetVendorAddress() {
                Language = _workContext.WorkingLanguage,
                Address = null,
                Model = model.Address,
                ExcludeProperties = false,
                PrePopulateWithCustomerFields = true,
                Customer = _workContext.CurrentCustomer,
                LoadCountries = () => countries,
            });
            return View(model);
        }

        public virtual async Task<IActionResult> Info()
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
            model.GenericAttributes = vendor.GenericAttributes;
            model.PictureUrl = await _pictureService.GetPictureUrl(vendor.PictureId, _mediaSettings.AvatarPictureSize, false);
            var countries = await _countryService.GetAllCountries(_workContext.WorkingLanguage.Id);
            model.Address = await _mediator.Send(new GetVendorAddress() {
                Language = _workContext.WorkingLanguage,
                Address = vendor.Address,
                ExcludeProperties = false,
                LoadCountries = () => countries,
            });

            return View(model);
        }

        [HttpPost, ActionName("Info")]
        [AutoValidateAntiforgeryToken]
        [FormValueRequired("save-info-button")]
        public virtual async Task<IActionResult> Info(VendorInfoModel model, IFormFile uploadedFile)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (_workContext.CurrentVendor == null || !_vendorSettings.AllowVendorsToEditInfo)
                return RedirectToRoute("CustomerInfo");

            string pictureId = string.Empty;
            string contentType = string.Empty;
            byte[] vendorPictureBinary = null;

            if (uploadedFile != null && !string.IsNullOrEmpty(uploadedFile.FileName))
            {
                try
                {
                    contentType = uploadedFile.ContentType;
                    if (string.IsNullOrEmpty(contentType))
                        ModelState.AddModelError("", "Empty content type");
                    else
                        if (!contentType.StartsWith("image"))
                        ModelState.AddModelError("", "Only image content type is allowed");

                    vendorPictureBinary = uploadedFile.GetPictureBits();
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", _localizationService.GetResource("Account.VendorInfo.Picture.ErrorMessage"));
                }
            }

            var vendor = _workContext.CurrentVendor;
            var prevPicture = await _pictureService.GetPictureById(vendor.PictureId);

            if (ModelState.IsValid && ModelState.ErrorCount == 0)
            {
                var description = Core.Html.HtmlHelper.FormatText(model.Description, false, false, true, false, false, false);

                vendor.Name = model.Name;
                vendor.Email = model.Email;
                vendor.Description = description;

                if (vendorPictureBinary != null && !string.IsNullOrEmpty(contentType))
                {
                    var picture = await _pictureService.InsertPicture(vendorPictureBinary, contentType, null);
                    if (picture != null)
                        vendor.PictureId = picture.Id;
                }
                if (prevPicture != null)
                    await _pictureService.DeletePicture(prevPicture);

                //update picture seo file name
                await UpdatePictureSeoNames(vendor);
                model.Address.ToEntity(vendor.Address, true);

                await _vendorService.UpdateVendor(vendor);

                //notifications
                if (_vendorSettings.NotifyStoreOwnerAboutVendorInformationChange)
                    await _workflowMessageService.SendVendorInformationChangeNotification(vendor, _storeContext.CurrentStore, _localizationSettings.DefaultAdminLanguageId);

                return RedirectToAction("Info");
            }
            var countries = await _countryService.GetAllCountries(_workContext.WorkingLanguage.Id);
            model.Address = await _mediator.Send(new GetVendorAddress() {
                Language = _workContext.WorkingLanguage,
                Model = model.Address,
                Address = vendor.Address,
                ExcludeProperties = false,
                LoadCountries = () => countries,
            });

            return View(model);
        }

        [HttpPost, ActionName("Info")]
        [AutoValidateAntiforgeryToken]
        [FormValueRequired("remove-picture")]
        public virtual async Task<IActionResult> RemovePicture()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (_workContext.CurrentVendor == null || !_vendorSettings.AllowVendorsToEditInfo)
                return RedirectToRoute("CustomerInfo");

            var vendor = _workContext.CurrentVendor;
            var picture = await _pictureService.GetPictureById(vendor.PictureId);

            if (picture != null)
                await _pictureService.DeletePicture(picture);

            vendor.PictureId = "";
            await _vendorService.UpdateVendor(vendor);

            //notifications
            if (_vendorSettings.NotifyStoreOwnerAboutVendorInformationChange)
                await _workflowMessageService.SendVendorInformationChangeNotification(vendor, _storeContext.CurrentStore, _localizationSettings.DefaultAdminLanguageId);

            return RedirectToAction("Info");
        }

        //contact vendor page
        public virtual async Task<IActionResult> ContactVendor(string vendorId)
        {
            if (!_vendorSettings.AllowCustomersToContactVendors)
                return RedirectToRoute("HomePage");

            var vendor = await _vendorService.GetVendorById(vendorId);
            if (vendor == null || !vendor.Active || vendor.Deleted)
                return RedirectToRoute("HomePage");

            var model = new ContactVendorModel {
                Email = _workContext.CurrentCustomer.Email,
                FullName = _workContext.CurrentCustomer.GetFullName(),
                SubjectEnabled = _commonSettings.SubjectFieldOnContactUsForm,
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage,
                VendorId = vendor.Id,
                VendorName = vendor.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)
            };

            return View(model);
        }

        [HttpPost, ActionName("ContactVendor")]
        [AutoValidateAntiforgeryToken]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> ContactVendor(ContactVendorModel model, bool captchaValid)
        {
            if (!_vendorSettings.AllowCustomersToContactVendors)
                return RedirectToRoute("HomePage");

            var vendor = await _vendorService.GetVendorById(model.VendorId);
            if (vendor == null || !vendor.Active || vendor.Deleted)
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            model.VendorName = vendor.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);

            if (ModelState.IsValid)
            {
                model = await _mediator.Send(new ContactVendorSendCommand() { Model = model, Vendor = vendor, Store = _storeContext.CurrentStore });;
                return View(model);
            }

            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage;
            return View(model);
        }
        #endregion
    }
}
