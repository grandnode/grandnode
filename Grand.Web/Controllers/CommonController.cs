using Grand.Core;
using Grand.Core.Domain;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Vendors;
using Grand.Framework.Localization;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
using Grand.Framework.Security.Captcha;
using Grand.Framework.Themes;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Interfaces;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class CommonController : BasePublicController
    {
        #region Fields
        private readonly ICommonViewModelService _commonViewModelService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerActionEventService _customerActionEventService;
        private readonly IPopupService _popupService;
        private readonly IContactAttributeService _contactAttributeService;
        private readonly CommonSettings _commonSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Constructors

        public CommonController(
            ICommonViewModelService commonViewModelService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerActivityService customerActivityService,
            ICustomerActionEventService customerActionEventService,
            IPopupService popupService,
            IContactAttributeService contactAttributeService,
            CommonSettings commonSettings,
            CaptchaSettings captchaSettings,
            VendorSettings vendorSettings
            )
        {
            _commonViewModelService = commonViewModelService;
            _localizationService = localizationService;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerActivityService = customerActivityService;
            _customerActionEventService = customerActionEventService;
            _popupService = popupService;
            _contactAttributeService = contactAttributeService;
            _commonSettings = commonSettings;
            _captchaSettings = captchaSettings;
            _vendorSettings = vendorSettings;
        }

        #endregion

        #region Methods

        //page not found
        public virtual IActionResult PageNotFound([FromServices] ILogger logger)
        {
            if (_commonSettings.Log404Errors)
            {
                var statusCodeReExecuteFeature = HttpContext?.Features?.Get<IStatusCodeReExecuteFeature>();
                logger.Error(string.Format("Error 404. The requested page ({0}) was not found", statusCodeReExecuteFeature?.OriginalPath),
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
        public virtual async Task<IActionResult> SetLanguage(
            [FromServices] ILanguageService languageService,
            [FromServices] LocalizationSettings localizationSettings,
            string langid, string returnUrl = "")
        {

            var language = await languageService.GetLanguageById(langid);
            if (!language?.Published ?? false)
                language = _workContext.WorkingLanguage;

            //home page
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");

            //language part in URL
            if (localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                //remove current language code if it's already localized URL
                if (await returnUrl.IsLocalizedUrlAsync(languageService, this.Request.PathBase, true))
                    returnUrl = returnUrl.RemoveLanguageSeoCodeFromUrl(this.Request.PathBase, true);

                //and add code of passed language
                returnUrl = returnUrl.AddLanguageSeoCodeToUrl(this.Request.PathBase, true, language);
            }

            await _workContext.SetWorkingLanguage(language);

            return Redirect(returnUrl);
        }

        //helper method to redirect users.
        public virtual IActionResult InternalRedirect(string url, bool permanentRedirect)
        {
            //ensure it's invoked from our GenericPathRoute class
            if (HttpContext.Items["grand.RedirectFromGenericPathRoute"] == null ||
                !Convert.ToBoolean(HttpContext.Items["grand.RedirectFromGenericPathRoute"]))
            {
                url = Url.RouteUrl("HomePage");
                permanentRedirect = false;
            }

            //home page
            if (string.IsNullOrEmpty(url))
            {
                url = Url.RouteUrl("HomePage");
                permanentRedirect = false;
            }

            //prevent open redirection attack
            if (!Url.IsLocalUrl(url))
            {
                url = Url.RouteUrl("HomePage");
                permanentRedirect = false;
            }

            url = Uri.EscapeUriString(WebUtility.UrlDecode(url));

            if (permanentRedirect)
                return RedirectPermanent(url);
            return Redirect(url);
        }

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> SetCurrency(string customerCurrency, string returnUrl = "")
        {
            await _commonViewModelService.SetCurrency(customerCurrency);

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
        public virtual async Task<IActionResult> SetStore(
            [FromServices] IStoreService storeService,
            string store, string returnUrl = "")
        {
            var currentstoreid = _storeContext.CurrentStore.Id;
            if (currentstoreid != store)
                await _commonViewModelService.SetStore(store);

            var prevStore = await storeService.GetStoreById(currentstoreid);
            var currStore = await storeService.GetStoreById(store);

            if (prevStore != null && currStore != null)
            {
                if (prevStore.Url != currStore.Url)
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
        public virtual async Task<IActionResult> SetTaxType(int customerTaxType, string returnUrl = "")
        {
            await _commonViewModelService.SetTaxType(customerTaxType);

            //home page
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");

            return Redirect(returnUrl);
        }

        //contact us page
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual async Task<IActionResult> ContactUs()
        {
            var model = await _commonViewModelService.PrepareContactUs();
            return View(model);
        }

        [HttpPost, ActionName("ContactUs")]
        [PublicAntiForgery]
        [ValidateCaptcha]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual async Task<IActionResult> ContactUsSend(ContactUsModel model, IFormCollection form, bool captchaValid,
            [FromServices] IContactAttributeFormatter contactAttributeFormatter)
        {
            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            //parse contact attributes
            var attributeXml = await _commonViewModelService.ParseContactAttributes(form);
            var contactAttributeWarnings = await _commonViewModelService.GetContactAttributesWarnings(attributeXml);
            if (contactAttributeWarnings.Any())
            {
                foreach (var item in contactAttributeWarnings)
                {
                    ModelState.AddModelError("", item);
                }
            }

            if (ModelState.IsValid)
            {
                model.ContactAttributeXml = attributeXml;
                model.ContactAttributeInfo = await contactAttributeFormatter.FormatAttributes(attributeXml, _workContext.CurrentCustomer);
                model = await _commonViewModelService.SendContactUs(model);
                //activity log
                await _customerActivityService.InsertActivity("PublicStore.ContactUs", "", _localizationService.GetResource("ActivityLog.PublicStore.ContactUs"));
                return View(model);
            }

            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage;
            model.ContactAttributes = await _commonViewModelService.PrepareContactAttributeModel(attributeXml);

            return View(model);
        }
        //contact vendor page
        public virtual async Task<IActionResult> ContactVendor(string vendorId, [FromServices] IVendorService vendorService)
        {
            if (!_vendorSettings.AllowCustomersToContactVendors)
                return RedirectToRoute("HomePage");

            var vendor = await vendorService.GetVendorById(vendorId);
            if (vendor == null || !vendor.Active || vendor.Deleted)
                return RedirectToRoute("HomePage");

            var model = await _commonViewModelService.PrepareContactVendor(vendor);

            return View(model);
        }
        [HttpPost, ActionName("ContactVendor")]
        [PublicAntiForgery]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> ContactVendorSend(ContactVendorModel model, bool captchaValid, [FromServices] IVendorService vendorService)
        {
            if (!_vendorSettings.AllowCustomersToContactVendors)
                return RedirectToRoute("HomePage");

            var vendor = await vendorService.GetVendorById(model.VendorId);
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
                model = await _commonViewModelService.SendContactVendor(model, vendor);
                return View(model);
            }

            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage;
            return View(model);
        }

        //sitemap page
        public virtual async Task<IActionResult> Sitemap()
        {
            if (!_commonSettings.SitemapEnabled)
                return RedirectToRoute("HomePage");

            var model = await _commonViewModelService.PrepareSitemap();
            return View(model);
        }

        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual async Task<IActionResult> SitemapXml(int? id)
        {
            if (!_commonSettings.SitemapEnabled)
                return RedirectToRoute("HomePage");
            var siteMap = await _commonViewModelService.SitemapXml(id, this.Url);

            return Content(siteMap, "text/xml");
        }

        public virtual async Task<IActionResult> SetStoreTheme(
            [FromServices] IThemeContext themeContext, string themeName, string returnUrl = "")
        {
            await themeContext.SetWorkingTheme(themeName);

            //home page
            if (string.IsNullOrEmpty(returnUrl))
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
        public virtual async Task<IActionResult> EuCookieLawAccept([FromServices] StoreInformationSettings storeInformationSettings,
            [FromServices] IGenericAttributeService genericAttributeService)
        {
            if (!storeInformationSettings.DisplayEuCookieLawWarning)
                //disabled
                return Json(new { stored = false });

            //save setting
            await genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.EuCookieLawAccepted, true, _storeContext.CurrentStore.Id);
            return Json(new { stored = true });
        }

        //robots.txt file
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> RobotsTextFile()
        {
            var sb = await _commonViewModelService.PrepareRobotsTextFile();
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
        public virtual IActionResult StoreClosed() => View();

        [HttpPost]
        public virtual async Task<IActionResult> ContactAttributeChange(IFormCollection form,
            [FromServices] IContactAttributeParser contactAttributeParser)
        {
            var attributeXml = await _commonViewModelService.ParseContactAttributes(form);

            var enabledAttributeIds = new List<string>();
            var disabledAttributeIds = new List<string>();
            var attributes = await _contactAttributeService.GetAllContactAttributes(_storeContext.CurrentStore.Id);
            foreach (var attribute in attributes)
            {
                var conditionMet = await contactAttributeParser.IsConditionMet(attribute, attributeXml);
                if (conditionMet.HasValue)
                {
                    if (conditionMet.Value)
                        enabledAttributeIds.Add(attribute.Id);
                    else
                        disabledAttributeIds.Add(attribute.Id);
                }
            }

            return Json(new
            {
                enabledattributeids = enabledAttributeIds.ToArray(),
                disabledattributeids = disabledAttributeIds.ToArray()
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> UploadFileContactAttribute(string attributeId, [FromServices] IDownloadService downloadService)
        {
            var attribute = await _contactAttributeService.GetContactAttributeById(attributeId);
            if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
            {
                return Json(new
                {
                    success = false,
                    downloadGuid = Guid.Empty,
                });
            }

            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No file uploaded",
                    downloadGuid = Guid.Empty,
                });
            }

            var fileBinary = httpPostedFile.GetDownloadBits();

            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (String.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = Path.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            var fileExtension = Path.GetExtension(fileName);
            if (!String.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            if (attribute.ValidationFileMaximumSize.HasValue)
            {
                //compare in bytes
                var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                {
                    //when returning JSON the mime-type must be set to text/plain
                    //otherwise some browsers will pop-up a "Save As" dialog.
                    return Json(new
                    {
                        success = false,
                        message = string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value),
                        downloadGuid = Guid.Empty,
                    });
                }
            }

            var download = new Download {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = Path.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };

            await downloadService.InsertDownload(download);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.FileUploaded"),
                downloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                downloadGuid = download.DownloadGuid,
            });
        }

        //Get banner for customer
        [HttpGet]
        public virtual async Task<IActionResult> GetActivePopup()
        {
            var result = await _popupService.GetActivePopupByCustomerId(_workContext.CurrentCustomer.Id);
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
        public virtual async Task<IActionResult> RemovePopup(string Id)
        {
            await _popupService.MovepopupToArchive(Id, _workContext.CurrentCustomer.Id);
            return Json("");
        }


        [HttpGet]
        public virtual async Task<IActionResult> CustomerActionEventUrl(string curl, string purl)
        {
            await _customerActionEventService.Url(_workContext.CurrentCustomer, curl, purl);
            return Json
                (
                    new { empty = "" }
                );
        }

        [HttpPost, ActionName("PopupInteractiveForm")]
        public virtual async Task<IActionResult> PopupInteractiveForm(IFormCollection formCollection,
           [FromServices] IInteractiveFormService interactiveFormService, [FromServices] IQueuedEmailService queuedEmailService,
           [FromServices] IEmailAccountService emailAccountService)
        {

            var formid = formCollection["Id"];
            var form = await interactiveFormService.GetFormById(formid);
            if (form == null)
                return Content("");
            string enquiry = "";

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
                        ModelState.AddModelError("", string.Format(_localizationService.GetResource("PopupInteractiveForm.Fields.Regex"), item.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id)));
                    }
                }
                if (item.IsRequired)
                {
                    var valuesStr = formCollection[item.SystemName];
                    if (string.IsNullOrEmpty(valuesStr))
                        ModelState.AddModelError("", string.Format(_localizationService.GetResource("PopupInteractiveForm.Fields.IsRequired"), item.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id)));
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
                            ModelState.AddModelError("", string.Format(_localizationService.GetResource("PopupInteractiveForm.Fields.TextboxMinimumLength"), item.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), item.ValidationMinLength.Value));
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
                            ModelState.AddModelError("", string.Format(_localizationService.GetResource("PopupInteractiveForm.Fields.TextboxMaximumLength"), item.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), item.ValidationMaxLength.Value));
                        }
                    }
                }

            }

            if (ModelState.Keys.Count() == 0)
            {
                var emailAccount = await emailAccountService.GetEmailAccountById(form.EmailAccountId);
                if (emailAccount == null)
                    emailAccount = (await emailAccountService.GetAllEmailAccounts()).FirstOrDefault();
                if (emailAccount == null)
                    throw new Exception("No email account could be loaded");

                string from;
                string fromName;
                string subject = string.Format(_localizationService.GetResource("PopupInteractiveForm.EmailForm"), form.Name);
                from = emailAccount.Email;
                fromName = emailAccount.DisplayName;

                await queuedEmailService.InsertQueuedEmail(new QueuedEmail {
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
                await _customerActivityService.InsertActivity("PublicStore.InteractiveForm", form.Id, string.Format(_localizationService.GetResource("ActivityLog.PublicStore.InteractiveForm"), form.Name));
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
