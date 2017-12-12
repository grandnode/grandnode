using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Grand.Core;
using Grand.Core.Domain;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Vendors;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Vendors;
using Grand.Framework.Localization;
using Grand.Framework.Security;
using Grand.Framework.Security.Captcha;
using Grand.Framework.Themes;
using Grand.Web.Models.Common;
using System.Text.RegularExpressions;
using Grand.Web.Services;
using Grand.Core.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Grand.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Grand.Services.Stores;

namespace Grand.Web.Controllers
{
    public partial class CommonController : BasePublicController
    {
        #region Fields
        private readonly ICommonWebService _commonWebService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerActionEventService _customerActionEventService;
        private readonly IPopupService _popupService;
        private readonly IInteractiveFormService _interactiveFormService;
        private readonly ILogger _logger;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly CommonSettings _commonSettings;
        private readonly ForumSettings _forumSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Constructors

        public CommonController(
            ICommonWebService commonWebService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IStoreService storeService,
            ICustomerActivityService customerActivityService,
            ICustomerActionEventService customerActionEventService,
            IPopupService popupService,
            IInteractiveFormService interactiveFormService,
            ILogger logger,
            StoreInformationSettings storeInformationSettings,
            CommonSettings commonSettings,
            ForumSettings forumSettings,
            LocalizationSettings localizationSettings,
            CaptchaSettings captchaSettings,
            VendorSettings vendorSettings
            )
        {
            this._commonWebService = commonWebService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._customerActivityService = customerActivityService;
            this._customerActionEventService = customerActionEventService;
            this._popupService = popupService;
            this._interactiveFormService = interactiveFormService;
            this._logger = logger;
            this._storeInformationSettings = storeInformationSettings;
            this._commonSettings = commonSettings;
            this._forumSettings = forumSettings;
            this._localizationSettings = localizationSettings;
            this._captchaSettings = captchaSettings;
            this._vendorSettings = vendorSettings;
        }

        #endregion

        #region Methods

        //page not found
        public virtual IActionResult PageNotFound()
        {
            if (_commonSettings.Log404Errors)
            {
                var statusCodeReExecuteFeature = HttpContext?.Features?.Get<IStatusCodeReExecuteFeature>();
                //TODO add locale resource
                _logger.Error(string.Format("Error 404. The requested page ({0}) was not found", statusCodeReExecuteFeature?.OriginalPath),
                    customer: _workContext.CurrentCustomer);
            }

            this.Response.StatusCode = 404;
            this.Response.ContentType = "text/html";

            return View();
        }

        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult SetLanguage(string langid, string returnUrl = "")
        {

            _commonWebService.SetLanguage(langid);

            //home page
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");

            //language part in URL
            if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                //remove current language code if it's already localized URL
                if (returnUrl.IsLocalizedUrl(this.Request.PathBase, true, out Language urlLanguage))
                    returnUrl = returnUrl.RemoveLanguageSeoCodeFromUrl(this.Request.PathBase, true);

                //and add code of passed language
                returnUrl = returnUrl.AddLanguageSeoCodeToUrl(this.Request.PathBase, true, _workContext.WorkingLanguage);
            }
            return Redirect(returnUrl);
        }
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult SetCurrency(string customerCurrency, string returnUrl = "")
        {
            _commonWebService.SetCurrency(customerCurrency);

            //home page
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");

            return Redirect(returnUrl);
        }

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult SetStore(string store, string returnUrl = "")
        {
            var currentstoreid = _storeContext.CurrentStore.Id;
            if (currentstoreid != store)
                _commonWebService.SetStore(store);

            var prevStore = _storeService.GetStoreById(currentstoreid);
            var currStore = _storeService.GetStoreById(store);

            if(prevStore!=null && currStore !=null)
            {
                if(prevStore.Url!=currStore.Url)
                {
                    return Redirect(currStore.SslEnabled ? currStore.SecureUrl : currStore.Url);
                }
            }

            //home page
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");

            return Redirect(returnUrl);
        }

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult SetTaxType(int customerTaxType, string returnUrl = "")
        {
            _commonWebService.SetTaxType(customerTaxType);

            //home page
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");

            return Redirect(returnUrl);
        }

        //contact us page
        [HttpsRequirement(SslRequirement.Yes)]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual IActionResult ContactUs()
        {
            var model = _commonWebService.PrepareContactUs();
            return View(model);
        }

        [HttpPost, ActionName("ContactUs")]
        [PublicAntiForgery]
        [ValidateCaptcha]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual IActionResult ContactUsSend(ContactUsModel model, bool captchaValid)
        {
            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (ModelState.IsValid)
            {
                model = _commonWebService.SendContactUs(model);
                //activity log
                _customerActivityService.InsertActivity("PublicStore.ContactUs", "", _localizationService.GetResource("ActivityLog.PublicStore.ContactUs"));
                return View(model);
            }
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage;
            return View(model);
        }
        //contact vendor page
        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult ContactVendor(string vendorId)
        {
            if (!_vendorSettings.AllowCustomersToContactVendors)
                return RedirectToRoute("HomePage");

            var vendor = EngineContext.Current.Resolve<IVendorService>().GetVendorById(vendorId);
            if (vendor == null || !vendor.Active || vendor.Deleted)
                return RedirectToRoute("HomePage");

            var model = _commonWebService.PrepareContactVendor(vendor);

            return View(model);
        }
        [HttpPost, ActionName("ContactVendor")]
        [PublicAntiForgery]
        [ValidateCaptcha]
        public virtual IActionResult ContactVendorSend(ContactVendorModel model, bool captchaValid)
        {
            if (!_vendorSettings.AllowCustomersToContactVendors)
                return RedirectToRoute("HomePage");

            var vendor = EngineContext.Current.Resolve<IVendorService>().GetVendorById(model.VendorId);
            if (vendor == null || !vendor.Active || vendor.Deleted)
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            model.VendorName = vendor.GetLocalized(x => x.Name);

            if (ModelState.IsValid)
            {
                model = _commonWebService.SendContactVendor(model, vendor);
                return View(model);
            }

            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage;
            return View(model);
        }

        //sitemap page
        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult Sitemap()
        {
            if (!_commonSettings.SitemapEnabled)
                return RedirectToRoute("HomePage");

            var model = _commonWebService.PrepareSitemap();
            return View(model);
        }

        //SEO sitemap page
        [HttpsRequirement(SslRequirement.No)]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual IActionResult SitemapXml(int? id)
        {
            if (!_commonSettings.SitemapEnabled)
                return RedirectToRoute("HomePage");
            var siteMap = _commonWebService.SitemapXml(id, this.Url);

            return Content(siteMap, "text/xml");
        }

        public virtual IActionResult SetStoreTheme(string themeName, string returnUrl = "")
        {
            EngineContext.Current.Resolve<IThemeContext>().WorkingThemeName = themeName;

            //home page
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");

            return Redirect(returnUrl);
        }

       
        [HttpPost]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult EuCookieLawAccept()
        {
            if (!_storeInformationSettings.DisplayEuCookieLawWarning)
                //disabled
                return Json(new { stored = false });

            //save setting
            EngineContext.Current.Resolve<IGenericAttributeService>().SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.EuCookieLawAccepted, true, _storeContext.CurrentStore.Id);
            return Json(new { stored = true });
        }

        //robots.txt file
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult RobotsTextFile()
        {
            var sb = _commonWebService.PrepareRobotsTextFile();
            return Content(sb, "text/plain");
        }

        public virtual IActionResult GenericUrl()
        {
            //seems that no entity was found
            return InvokeHttp404();
        }

        //store is closed
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual IActionResult StoreClosed()
        {
            return View();
        }


        //Get banner for customer
        [HttpGet]
        public virtual IActionResult GetActivePopup()
        {
            var result = _popupService.GetActivePopupByCustomerId(_workContext.CurrentCustomer.Id);
            if (result != null)
            {
                return Json
                    (
                        new { Id = result.Id, Body = result.Body, PopupTypeId = result.PopupTypeId }
                    );
            }
            else
                return Json
                    (
                        new { empty = "" }
                    );
        }

        [HttpPost]
        public virtual IActionResult RemovePopup(string Id)
        {
            _popupService.MovepopupToArchive(Id, _workContext.CurrentCustomer.Id);
            return Json("");
        }


        [HttpGet]
        public virtual IActionResult CustomerActionEventUrl(string curl, string purl)
        {
            _customerActionEventService.Url(_workContext.CurrentCustomer, curl, purl);
            return Json
                (
                    new { empty = "" }
                );
        }

        [HttpPost, ActionName("PopupInteractiveForm")]
        public virtual IActionResult PopupInteractiveForm(IFormCollection formCollection)
        {

            var formid = formCollection["Id"];
            var form = _interactiveFormService.GetFormById(formid);
            if (form == null)
                return Content("");
            string enquiry = "";

            var queuedEmailService = EngineContext.Current.Resolve<IQueuedEmailService>();
            var emailAccountService = EngineContext.Current.Resolve<IEmailAccountService>();
            foreach (var item in form.FormAttributes)
            {
                enquiry += string.Format("{0}: {1} <br />", item.Name, formCollection[item.SystemName]);

                if (!string.IsNullOrEmpty(item.RegexValidation))
                {
                    var valuesStr = formCollection[item.SystemName];
                    Regex regex = new Regex(item.RegexValidation);
                    Match match = regex.Match(valuesStr);
                    if (!match.Success)
                    {
                        ModelState.AddModelError("", string.Format(_localizationService.GetResource("PopupInteractiveForm.Fields.Regex"), item.GetLocalized(a => a.Name)));
                    }
                }
                if (item.IsRequired)
                {
                    var valuesStr = formCollection[item.SystemName];
                    if (string.IsNullOrEmpty(valuesStr))
                        ModelState.AddModelError("", string.Format(_localizationService.GetResource("PopupInteractiveForm.Fields.IsRequired"), item.GetLocalized(a => a.Name)));
                }
                if (item.ValidationMinLength.HasValue)
                {
                    if (item.AttributeControlType == FormControlType.TextBox ||
                        item.AttributeControlType == FormControlType.MultilineTextbox)
                    {
                        var valuesStr = formCollection[item.SystemName].ToString();
                        int enteredTextLength = String.IsNullOrEmpty(valuesStr) ? 0 : valuesStr.Length;
                        if (item.ValidationMinLength.Value > enteredTextLength)
                        {
                            ModelState.AddModelError("", string.Format(_localizationService.GetResource("PopupInteractiveForm.Fields.TextboxMinimumLength"), item.GetLocalized(a => a.Name), item.ValidationMinLength.Value));
                        }
                    }
                }
                if (item.ValidationMaxLength.HasValue)
                {
                    if (item.AttributeControlType == FormControlType.TextBox ||
                        item.AttributeControlType == FormControlType.MultilineTextbox)
                    {
                        var valuesStr = formCollection[item.SystemName].ToString();
                        int enteredTextLength = String.IsNullOrEmpty(valuesStr) ? 0 : valuesStr.Length;
                        if (item.ValidationMaxLength.Value < enteredTextLength)
                        {
                            ModelState.AddModelError("", string.Format(_localizationService.GetResource("PopupInteractiveForm.Fields.TextboxMaximumLength"), item.GetLocalized(a => a.Name), item.ValidationMaxLength.Value));
                        }
                    }
                }

            }

            if (ModelState.Keys.Count() == 0)
            {
                var emailAccount = emailAccountService.GetEmailAccountById(form.EmailAccountId);
                if (emailAccount == null)
                    emailAccount = emailAccountService.GetAllEmailAccounts().FirstOrDefault();
                if (emailAccount == null)
                    throw new Exception("No email account could be loaded");

                string from;
                string fromName;
                string subject = string.Format(_localizationService.GetResource("PopupInteractiveForm.EmailForm"), form.Name);
                from = emailAccount.Email;
                fromName = emailAccount.DisplayName;

                queuedEmailService.InsertQueuedEmail(new QueuedEmail
                {
                    From = from,
                    FromName = fromName,
                    To = emailAccount.Email,
                    ToName = emailAccount.DisplayName,
                    Priority = QueuedEmailPriority.High,
                    Subject = subject,
                    Body = enquiry,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id
                });

                //activity log
                _customerActivityService.InsertActivity("PublicStore.InteractiveForm", form.Id, string.Format(_localizationService.GetResource("ActivityLog.PublicStore.InteractiveForm"), form.Name));
            }

            return Json(new
            {
                success = ModelState.Keys.Count() == 0,
                errors = ModelState.Keys.SelectMany(k => ModelState[k].Errors)
                                .Select(m => m.ErrorMessage).ToArray()
            });

        }

        #endregion
    }
}
