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

        private readonly LocalizationSettings _localizationSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly CaptchaSettings _captchaSettings;

        #endregion

        #region Constructors

        public VendorController(IWorkContext workContext,
            ILocalizationService localizationService,
            ICustomerService customerService,
            IWorkflowMessageService workflowMessageService,
            IVendorService vendorService,
            IUrlRecordService urlRecordService,
            LocalizationSettings localizationSettings,
            VendorSettings vendorSettings,
            CaptchaSettings captchaSettings)
        {
            this._workContext = workContext;
            this._localizationService = localizationService;
            this._customerService = customerService;
            this._workflowMessageService = workflowMessageService;
            this._vendorService = vendorService;
            this._urlRecordService = urlRecordService;

            this._localizationSettings = localizationSettings;
            this._vendorSettings = vendorSettings;
            this._captchaSettings = captchaSettings;
        }

        #endregion

        #region Methods

        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult ApplyVendor()
        {
            if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
                return RedirectToRoute("HomePage");

            if (!_workContext.CurrentCustomer.IsRegistered())
                return new UnauthorizedResult();

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
            return View(model);
        }

        [HttpPost, ActionName("ApplyVendor")]
        [PublicAntiForgery]
        [ValidateCaptcha]
        public virtual IActionResult ApplyVendorSubmit(ApplyVendorModel model, bool captchaValid)
        {
            if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
                return RedirectToRoute("HomePage");

            if (!_workContext.CurrentCustomer.IsRegistered())
                return new UnauthorizedResult();

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnApplyVendorPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (ModelState.IsValid)
            {
                //disabled by default
                var vendor = new Vendor
                {
                    Name = model.Name,
                    Email = model.Email,
                    //some default settings
                    PageSize = 6,
                    AllowCustomersToSelectPageSize = true,
                    PageSizeOptions = _vendorSettings.DefaultVendorPageSizeOptions
                };
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
            return View(model);
        }

        #endregion
    }
}
