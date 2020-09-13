using Grand.Core;
using Grand.Core.Configuration;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Media;
using Grand.Domain.Stores;
using Grand.Domain.Tax;
using Grand.Framework.Localization;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Captcha;
using Grand.Framework.Themes;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Stores;
using Grand.Web.Commands.Models.Common;
using Grand.Web.Features.Models.Common;
using Grand.Web.Models.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class CommonController : BasePublicController
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IMediator _mediator;
        private readonly CaptchaSettings _captchaSettings;

        #endregion

        #region Constructors

        public CommonController(
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IMediator mediator,
            CaptchaSettings captchaSettings)
        {
            _localizationService = localizationService;
            _workContext = workContext;
            _storeContext = storeContext;
            _mediator = mediator;
            _captchaSettings = captchaSettings;
        }

        #endregion

        #region Methods

        //page not found
        public virtual IActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            Response.ContentType = "text/html";
            return View();
        }

        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> SetLanguage(
            [FromServices] ILanguageService languageService,
            [FromServices] GrandConfig config,
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
            if (config.SeoFriendlyUrlsForLanguagesEnabled)
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
        public virtual async Task<IActionResult> SetCurrency(
            [FromServices] ICurrencyService currencyService,
            string customerCurrency, string returnUrl = "")
        {
            var currency = await currencyService.GetCurrencyById(customerCurrency);
            if (currency != null)
                await _workContext.SetWorkingCurrency(currency);

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
            [FromServices] CommonSettings commonSettings,
            string store, string returnUrl = "")
        {
            var currentstoreid = _storeContext.CurrentStore.Id;
            if (currentstoreid != store)
            {
                if (commonSettings.AllowToSelectStore)
                {
                    var selectedstore = await storeService.GetStoreById(store);
                    if (selectedstore != null)
                        await _storeContext.SetStoreCookie(store);
                }
            }
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
            var taxDisplayType = (TaxDisplayType)Enum.ToObject(typeof(TaxDisplayType), customerTaxType);
            await _workContext.SetTaxDisplayType(taxDisplayType);

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
            var model = await _mediator.Send(new ContactUsCommand() {
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore
            });
            return View(model);
        }

        [HttpPost, ActionName("ContactUs")]
        [AutoValidateAntiforgeryToken]
        [ValidateCaptcha]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual async Task<IActionResult> ContactUsSend(ContactUsModel model, IFormCollection form, bool captchaValid)
        {
            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(new ContactUsSendCommand() {
                    CaptchaValid = captchaValid,
                    Form = form,
                    Model = model,
                    Store = _storeContext.CurrentStore
                });

                if (result.errors.Any())
                {
                    foreach (var item in result.errors)
                    {
                        ModelState.AddModelError("", item);
                    }
                }
                else
                {
                    model = result.model;
                    return View(model);
                }
            }
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage;

            return View(model);
        }


        //sitemap page
        public virtual async Task<IActionResult> Sitemap([FromServices] CommonSettings commonSettings)
        {
            if (!commonSettings.SitemapEnabled)
                return RedirectToRoute("HomePage");

            var model = await _mediator.Send(new GetSitemap() {
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore
            });
            return View(model);
        }

        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual async Task<IActionResult> SitemapXml(int? id, string seocode,
            [FromServices] ILanguageService languageService,
            [FromServices] CommonSettings commonSettings)
        {
            if (!commonSettings.SitemapEnabled)
                return RedirectToRoute("HomePage");

            var lang = _workContext.WorkingLanguage;
            if (!string.IsNullOrEmpty(seocode))
            {
                var seolang = (await languageService.GetAllLanguages()).FirstOrDefault(x => x.UniqueSeoCode.ToLowerInvariant() == seocode.ToLowerInvariant());
                if (seolang != null)
                    lang = seolang;
            }
            var siteMap = await _mediator.Send(new GetSitemapXml() {
                Id = id,
                Customer = _workContext.CurrentCustomer,
                Language = lang,
                Store = _storeContext.CurrentStore,
                UrlHelper = Url,
            });

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
        public virtual async Task<IActionResult> EuCookieLawAccept(bool accept,
            [FromServices] StoreInformationSettings storeInformationSettings,
            [FromServices] IGenericAttributeService genericAttributeService,
            [FromServices] ICookiePreference cookiePreference)
        {
            if (!storeInformationSettings.DisplayEuCookieLawWarning)
                //disabled
                return Json(new { stored = false });

            //save consentcookies
            await genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.ConsentCookies, "", _storeContext.CurrentStore.Id);
            var dictionary = new Dictionary<string, bool>();
            var consentCookies = cookiePreference.GetConsentCookies();
            foreach (var item in consentCookies.Where(x=>x.AllowToDisable))
            {
                dictionary.Add(item.SystemName, accept);
            }
            if(dictionary.Any())
                await genericAttributeService.SaveAttribute<Dictionary<string, bool>>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.ConsentCookies, dictionary, _storeContext.CurrentStore.Id);

            //save setting - EuCookieLawAccepted
            await genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.EuCookieLawAccepted, true, _storeContext.CurrentStore.Id);

            return Json(new { stored = true });
        }

        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> PrivacyPreference([FromServices] StoreInformationSettings
            storeInformationSettings)
        {
            if (!storeInformationSettings.DisplayPrivacyPreference)
                //disabled
                return Json(new { html = "" });

            var model = await _mediator.Send(new GetPrivacyPreference() {
                Customer = _workContext.CurrentCustomer,
                Store = _storeContext.CurrentStore
            });

            return Json(new
            {
                html = await RenderPartialViewToString(model)
            });
        }

        [HttpPost]
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> PrivacyPreference(IFormCollection form,
            [FromServices] StoreInformationSettings storeInformationSettings,
            [FromServices] IGenericAttributeService genericAttributeService,
            [FromServices] ICookiePreference _cookiePreference)
        {

            if (!storeInformationSettings.DisplayPrivacyPreference)
                return Json(new { success = false });

            var consent = "ConsentCookies";
            await genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.ConsentCookies, "", _storeContext.CurrentStore.Id);
            var selectedConsentCookies = new List<string>();
            foreach (var item in form)
            {
                if (item.Key.StartsWith(consent))
                    selectedConsentCookies.Add(item.Value);
            }
            var dictionary = new Dictionary<string, bool>();
            var consentCookies = _cookiePreference.GetConsentCookies();
            foreach (var item in consentCookies)
            {
                if (item.AllowToDisable)
                    dictionary.Add(item.SystemName, selectedConsentCookies.Contains(item.SystemName));
            }

            await genericAttributeService.SaveAttribute<Dictionary<string, bool>>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.ConsentCookies, dictionary, _storeContext.CurrentStore.Id);

            return Json(new { success = true });
        }

        //robots.txt file
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> RobotsTextFile()
        {
            var sb = await _mediator.Send(new GetRobotsTextFile());
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
        public virtual async Task<IActionResult> ContactAttributeChange(IFormCollection form)
        {
            var result = await _mediator.Send(new ContactAttributeChangeCommand() {
                Form = form,
                Customer = _workContext.CurrentCustomer,
                Store = _storeContext.CurrentStore
            });
            return Json(new
            {
                enabledattributeids = result.enabledAttributeIds.ToArray(),
                disabledattributeids = result.disabledAttributeIds.ToArray()
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> UploadFileContactAttribute(string attributeId, [FromServices] IDownloadService downloadService,
            [FromServices] IContactAttributeService contactAttributeService)
        {
            var attribute = await contactAttributeService.GetContactAttributeById(attributeId);
            if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
            {
                return Json(new
                {
                    success = false,
                    downloadGuid = Guid.Empty,
                });
            }
            var form = await HttpContext.Request.ReadFormAsync();
            var httpPostedFile = form.Files.FirstOrDefault();
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
            if (String.IsNullOrEmpty(fileName) && form.ContainsKey(qqFileNameParameter))
                fileName = form[qqFileNameParameter].ToString();
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


        [HttpPost, ActionName("PopupInteractiveForm")]
        public virtual async Task<IActionResult> PopupInteractiveForm(IFormCollection formCollection)
        {
            var result = await _mediator.Send(new PopupInteractiveCommand() { Form = formCollection });
            return Json(new
            {
                success = result.Any(),
                errors = result
            });
        }

        #endregion
    }
}
