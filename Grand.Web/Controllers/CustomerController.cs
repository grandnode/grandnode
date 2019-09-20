using Grand.Core;
using Grand.Core.Domain;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Tax;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
using Grand.Framework.Security.Captcha;
using Grand.Services.Authentication;
using Grand.Services.Authentication.External;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.ExportImport;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Tax;
using Grand.Web.Extensions;
using Grand.Web.Interfaces;
using Grand.Web.Models.Customer;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class CustomerController : BasePublicController
    {
        #region Fields

        private readonly ICustomerViewModelService _customerViewModelService;
        private readonly IGrandAuthenticationService _authenticationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ITaxService _taxService;
        private readonly ICountryService _countryService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IAddressViewModelService _addressViewModelService;
        private readonly IMediator _mediator;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly TaxSettings _taxSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;

        #endregion

        #region Ctor

        public CustomerController(
            ICustomerViewModelService customerViewModelService,
            IGrandAuthenticationService authenticationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            ICustomerAttributeParser customerAttributeParser,
            IGenericAttributeService genericAttributeService,
            ICustomerRegistrationService customerRegistrationService,
            ITaxService taxService,
            ICountryService countryService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ICustomerActivityService customerActivityService,
            IAddressViewModelService addressViewModelService,
            IMediator mediator,
            IWorkflowMessageService workflowMessageService,
            CaptchaSettings captchaSettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            LocalizationSettings localizationSettings,
            TaxSettings taxSettings
            )
        {
            _customerViewModelService = customerViewModelService;
            _authenticationService = authenticationService;
            _dateTimeSettings = dateTimeSettings;
            _taxSettings = taxSettings;
            _localizationService = localizationService;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
            _customerAttributeParser = customerAttributeParser;
            _genericAttributeService = genericAttributeService;
            _customerRegistrationService = customerRegistrationService;
            _taxService = taxService;
            _customerSettings = customerSettings;
            _countryService = countryService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _customerActivityService = customerActivityService;
            _addressViewModelService = addressViewModelService;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _captchaSettings = captchaSettings;
            _mediator = mediator;
        }

        #endregion

        #region Utilities


        /// <summary>
        /// Prepare custom customer attribute models
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="overrideAttributesXml">When specified we do not use attributes of a customer</param>
        /// <returns>A list of customer attribute models</returns>


        #endregion

        #region Login / logout

        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Login(bool? checkoutAsGuest)
        {
            var model = _customerViewModelService.PrepareLogin(checkoutAsGuest);
            return View(model);
        }

        [HttpPost]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        [ValidateCaptcha]
        [PublicAntiForgery]
        public virtual async Task<IActionResult> Login(LoginModel model, string returnUrl, bool captchaValid,
                       [FromServices] IShoppingCartService shoppingCartService)
        {
            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }
                var loginResult = await _customerRegistrationService.ValidateCustomer(_customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password);
                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                            var customer = _customerSettings.UsernamesEnabled ? await _customerService.GetCustomerByUsername(model.Username) : await _customerService.GetCustomerByEmail(model.Email);

                            //migrate shopping cart
                            await shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer, true);

                            //sign in new customer
                            await _authenticationService.SignIn(customer, model.RememberMe);

                            //raise event       
                            await _mediator.Publish(new CustomerLoggedinEvent(customer));

                            //activity log
                            await _customerActivityService.InsertActivity("PublicStore.Login", "", _localizationService.GetResource("ActivityLog.PublicStore.Login"), customer);


                            if (String.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                                return RedirectToRoute("HomePage");

                            return Redirect(returnUrl);
                        }
                    case CustomerLoginResults.CustomerNotExist:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist"));
                        break;
                    case CustomerLoginResults.Deleted:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.Deleted"));
                        break;
                    case CustomerLoginResults.NotActive:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotActive"));
                        break;
                    case CustomerLoginResults.NotRegistered:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotRegistered"));
                        break;
                    case CustomerLoginResults.LockedOut:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.LockedOut"));
                        break;
                    case CustomerLoginResults.WrongPassword:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials"));
                        break;
                    default:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials"));
                        break;
                }
            }

            //If we got this far, something failed, redisplay form
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage;
            return View(model);
        }

        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> Logout([FromServices] StoreInformationSettings storeInformationSettings)
        {
            if (_workContext.OriginalCustomerIfImpersonated != null)
            {
                //logout impersonated customer
                await _genericAttributeService.SaveAttribute<int?>(_workContext.OriginalCustomerIfImpersonated,
                    SystemCustomerAttributeNames.ImpersonatedCustomerId, null);

                //redirect back to customer details page (admin area)
                return this.RedirectToAction("Edit", "Customer", new { id = _workContext.CurrentCustomer.Id, area = "Admin" });

            }

            //activity log
            await _customerActivityService.InsertActivity("PublicStore.Logout", "", _localizationService.GetResource("ActivityLog.PublicStore.Logout"));
            //standard logout 
            await _authenticationService.SignOut();

            //EU Cookie
            if (storeInformationSettings.DisplayEuCookieLawWarning)
            {
                //the cookie law message should not pop up immediately after logout.
                //otherwise, the user will have to click it again...
                //and thus next visitor will not click it... so violation for that cookie law..
                //the only good solution in this case is to store a temporary variable
                //indicating that the EU cookie popup window should not be displayed on the next page open (after logout redirection to homepage)
                //but it'll be displayed for further page loads
                TempData["Grand.IgnoreEuCookieLawWarning"] = true;
            }
            return RedirectToRoute("HomePage");
        }

        #endregion

        #region Password recovery

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult PasswordRecovery()
        {
            var model = _customerViewModelService.PreparePasswordRecovery();
            return View(model);
        }

        [HttpPost, ActionName("PasswordRecovery")]
        [PublicAntiForgery]
        [FormValueRequired("send-email")]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> PasswordRecoverySend(PasswordRecoveryModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = await _customerService.GetCustomerByEmail(model.Email);
                if (customer != null && customer.Active && !customer.Deleted)
                {
                    await _customerViewModelService.PasswordRecoverySend(model, customer);
                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.EmailHasBeenSent");
                }
                else
                {
                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.EmailNotFound");
                }

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> PasswordRecoveryConfirm(string token, string email)
        {
            var customer = await _customerService.GetCustomerByEmail(email);
            if (customer == null)
                return RedirectToRoute("HomePage");

            var model = await _customerViewModelService.PreparePasswordRecoveryConfirmModel(customer, token);

            return View(model);
        }

        [HttpPost, ActionName("PasswordRecoveryConfirm")]
        [PublicAntiForgery]
        [FormValueRequired("set-password")]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> PasswordRecoveryConfirmPOST(string token, string email, PasswordRecoveryConfirmModel model)
        {
            var customer = await _customerService.GetCustomerByEmail(email);
            if (customer == null)
                return RedirectToRoute("HomePage");

            //validate token
            if (!customer.IsPasswordRecoveryTokenValid(token))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.WrongToken");
            }

            //validate token expiration date
            if (customer.IsPasswordRecoveryLinkExpired(_customerSettings))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.LinkExpired");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var response = await _customerRegistrationService.ChangePassword(new ChangePasswordRequest(email,
                    false, _customerSettings.DefaultPasswordFormat, model.NewPassword));
                if (response.Success)
                {
                    await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken, "");

                    model.DisablePasswordChanging = true;
                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.PasswordHasBeenChanged");
                }
                else
                {
                    model.Result = response.Errors.FirstOrDefault();
                }

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Register

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> Register()
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            var model = new RegisterModel();
            model = await _customerViewModelService.PrepareRegisterModel(model, false);
            //enable newsletter by default
            model.Newsletter = _customerSettings.NewsletterTickedByDefault;

            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        [ValidateHoneypot]
        [PublicAntiForgery]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> Register(RegisterModel model, string returnUrl, bool captchaValid, IFormCollection form,
            [FromServices] IAddressService addressService, [FromServices] ICustomerActionEventService customerActionEventService, [FromServices] IWebHelper webHelper)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            if (_workContext.CurrentCustomer.IsRegistered())
            {
                //Already registered customer. 
                await _authenticationService.SignOut();

                //Save a new record
                _workContext.CurrentCustomer = await _customerService.InsertGuestCustomer(_storeContext.CurrentStore);
            }
            var customer = _workContext.CurrentCustomer;

            //custom customer attributes
            var customerAttributesXml = await _customerViewModelService.ParseCustomAttributes(form);
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarnings(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (ModelState.IsValid && ModelState.ErrorCount == 0)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }

                bool isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                var registrationRequest = new CustomerRegistrationRequest(customer, model.Email,
                    _customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password,
                    _customerSettings.DefaultPasswordFormat, _storeContext.CurrentStore.Id, isApproved);
                var registrationResult = await _customerRegistrationService.RegisterCustomer(registrationRequest);
                if (registrationResult.Success)
                {
                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    {
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
                    }
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);

                        var vat = await _taxService.GetVatNumberStatus(model.VatNumber);

                        await _genericAttributeService.SaveAttribute(customer,
                            SystemCustomerAttributeNames.VatNumberStatusId,
                            (int)vat.status);

                        //send VAT number admin notification
                        if (!String.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                            await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer, model.VatNumber, vat.address, _localizationSettings.DefaultAdminLanguageId);

                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
                    await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
                    await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        DateTime? dateOfBirth = model.ParseDateOfBirth();
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, dateOfBirth);
                    }
                    if (_customerSettings.CompanyEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
                    if (_customerSettings.StreetAddressEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
                    if (_customerSettings.StreetAddress2Enabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
                    if (_customerSettings.ZipPostalCodeEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
                    if (_customerSettings.CityEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
                    if (_customerSettings.CountryEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId, model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        var categories = new List<string>();
                        foreach (string formKey in form.Keys)
                        {
                            if (formKey.Contains("customernewsletterCategory_"))
                            {
                                try
                                {
                                    var category = formKey.Split('_')[1];
                                    categories.Add(category);
                                }
                                catch { }
                            }
                        }

                        //save newsletter value
                        var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(model.Email, _storeContext.CurrentStore.Id);
                        if (newsletter != null)
                        {
                            newsletter.Categories.Clear();
                            categories.ForEach(x => newsletter.Categories.Add(x));
                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                await _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                            }
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                var newsLetterSubscription = new NewsLetterSubscription {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = model.Email,
                                    CustomerId = customer.Id,
                                    Active = true,
                                    StoreId = _storeContext.CurrentStore.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                };
                                categories.ForEach(x => newsLetterSubscription.Categories.Add(x));
                                await _newsLetterSubscriptionService.InsertNewsLetterSubscription(newsLetterSubscription);
                            }
                        }
                    }

                    //save customer attributes
                    await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, customerAttributesXml);

                    //login customer now
                    if (isApproved)
                        await _authenticationService.SignIn(customer, true);

                    //insert default address (if possible)
                    var defaultAddress = new Address {
                        FirstName = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.FirstName),
                        LastName = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.LastName),
                        Email = customer.Email,
                        Company = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Company),
                        VatNumber = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.VatNumber),
                        CountryId = !String.IsNullOrEmpty(await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.CountryId)) ?
                            await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.CountryId) : "",
                        StateProvinceId = !String.IsNullOrEmpty(await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StateProvinceId)) ?
                            await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StateProvinceId) : "",
                        City = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.City),
                        Address1 = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StreetAddress),
                        Address2 = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StreetAddress2),
                        ZipPostalCode = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.ZipPostalCode),
                        PhoneNumber = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Phone),
                        FaxNumber = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Fax),
                        CreatedOnUtc = customer.CreatedOnUtc,
                    };

                    if (await addressService.IsAddressValid(defaultAddress))
                    {
                        //set default address
                        defaultAddress.CustomerId = customer.Id;
                        customer.Addresses.Add(defaultAddress);
                        await _customerService.InsertAddress(defaultAddress);
                        customer.BillingAddress = defaultAddress;
                        await _customerService.UpdateBillingAddress(defaultAddress);
                        customer.ShippingAddress = defaultAddress;
                        await _customerService.UpdateShippingAddress(defaultAddress);
                    }

                    //notifications
                    if (_customerSettings.NotifyNewCustomerRegistration)
                        await _workflowMessageService.SendCustomerRegisteredNotificationMessage(customer, _localizationSettings.DefaultAdminLanguageId);

                    //New customer has a free shipping for the first order
                    if (_customerSettings.RegistrationFreeShipping)
                        await _customerService.UpdateFreeShipping(customer.Id, true);

                    await customerActionEventService.Registration(customer);

                    //raise event       
                    await _mediator.Publish(new CustomerRegisteredEvent(customer));

                    switch (_customerSettings.UserRegistrationType)
                    {
                        case UserRegistrationType.EmailValidation:
                            {
                                //email validation message
                                await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AccountActivationToken, Guid.NewGuid().ToString());
                                await _workflowMessageService.SendCustomerEmailValidationMessage(customer, _workContext.WorkingLanguage.Id);

                                //result
                                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation });
                            }
                        case UserRegistrationType.AdminApproval:
                            {
                                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval });
                            }
                        case UserRegistrationType.Standard:
                            {
                                //send customer welcome message
                                await _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

                                var redirectUrl = Url.RouteUrl("RegisterResult", new { resultId = (int)UserRegistrationType.Standard }, HttpContext.Request.Scheme);
                                if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                                {
                                    redirectUrl = webHelper.ModifyQueryString(redirectUrl, "returnurl", returnUrl);
                                }
                                return Redirect(redirectUrl);
                            }
                        default:
                            {
                                return RedirectToRoute("HomePage");
                            }
                    }
                }

                //errors
                foreach (var error in registrationResult.Errors)
                    ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, redisplay form
            model = await _customerViewModelService.PrepareRegisterModel(model, true, customerAttributesXml);
            return View(model);
        }
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult RegisterResult(int resultId)
        {
            var resultText = "";
            switch ((UserRegistrationType)resultId)
            {
                case UserRegistrationType.Disabled:
                    resultText = _localizationService.GetResource("Account.Register.Result.Disabled");
                    break;
                case UserRegistrationType.Standard:
                    resultText = _localizationService.GetResource("Account.Register.Result.Standard");
                    break;
                case UserRegistrationType.AdminApproval:
                    resultText = _localizationService.GetResource("Account.Register.Result.AdminApproval");
                    break;
                case UserRegistrationType.EmailValidation:
                    resultText = _localizationService.GetResource("Account.Register.Result.EmailValidation");
                    break;
                default:
                    break;
            }
            var model = new RegisterResultModel {
                Result = resultText
            };
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> CheckUsernameAvailability(string username)
        {
            var usernameAvailable = false;
            var statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.NotAvailable");

            if (_customerSettings.UsernamesEnabled && !String.IsNullOrWhiteSpace(username))
            {
                if (_workContext.CurrentCustomer != null &&
                    _workContext.CurrentCustomer.Username != null &&
                    _workContext.CurrentCustomer.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                {
                    statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.CurrentUsername");
                }
                else
                {
                    var customer = await _customerService.GetCustomerByUsername(username);
                    if (customer == null)
                    {
                        statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.Available");
                        usernameAvailable = true;
                    }
                }
            }

            return Json(new { Available = usernameAvailable, Text = statusText });
        }

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> AccountActivation(string token, string email)
        {
            var customer = await _customerService.GetCustomerByEmail(email);
            if (customer == null)
                return RedirectToRoute("HomePage");

            var cToken = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.AccountActivationToken);
            if (String.IsNullOrEmpty(cToken))
                return RedirectToRoute("HomePage");

            if (!cToken.Equals(token, StringComparison.OrdinalIgnoreCase))
                return RedirectToRoute("HomePage");

            //activate user account
            customer.Active = true;
            customer.StoreId = _storeContext.CurrentStore.Id;
            await _customerService.UpdateActive(customer);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AccountActivationToken, "");

            //send welcome message
            await _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

            var model = new AccountActivationModel();
            model.Result = _localizationService.GetResource("Account.AccountActivation.Activated");
            return View(model);
        }

        #endregion

        #region My account / Info

        public virtual async Task<IActionResult> Info()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var customer = _workContext.CurrentCustomer;

            var model = new CustomerInfoModel();
            model = await _customerViewModelService.PrepareInfoModel(model, customer, false);

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual async Task<IActionResult> Info(CustomerInfoModel model, IFormCollection form, [FromServices] ForumSettings forumSettings)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var customer = _workContext.CurrentCustomer;

            //custom customer attributes
            var customerAttributesXml = await _customerViewModelService.ParseCustomAttributes(form);
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarnings(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            try
            {
                if (ModelState.IsValid && ModelState.ErrorCount == 0)
                {
                    //username 
                    if (_customerSettings.UsernamesEnabled && this._customerSettings.AllowUsersToChangeUsernames)
                    {
                        if (!customer.Username.Equals(model.Username.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            //change username
                            await _customerRegistrationService.SetUsername(customer, model.Username.Trim());
                            //re-authenticate
                            if (_workContext.OriginalCustomerIfImpersonated == null)
                                await _authenticationService.SignIn(customer, true);
                        }
                    }
                    //email
                    if (!customer.Email.Equals(model.Email.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        //change email
                        await _customerRegistrationService.SetEmail(customer, model.Email.Trim());
                        //re-authenticate (if usernames are disabled)
                        //do not authenticate users in impersonation mode
                        if (_workContext.OriginalCustomerIfImpersonated == null)
                        {
                            //re-authenticate (if usernames are disabled)
                            if (!_customerSettings.UsernamesEnabled)
                                await _authenticationService.SignIn(customer, true);
                        }
                    }

                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    {
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
                    }
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        var prevVatNumber = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.VatNumber);

                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);

                        if (prevVatNumber != model.VatNumber)
                        {
                            var vat = (await _taxService.GetVatNumberStatus(model.VatNumber));
                            await _genericAttributeService.SaveAttribute(customer,
                                    SystemCustomerAttributeNames.VatNumberStatusId,
                                    (int)vat.status);

                            //send VAT number admin notification
                            if (!String.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer, model.VatNumber, vat.address, _localizationSettings.DefaultAdminLanguageId);
                        }
                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
                    await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
                    await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        DateTime? dateOfBirth = model.ParseDateOfBirth();
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, dateOfBirth);
                    }
                    if (_customerSettings.CompanyEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
                    if (_customerSettings.StreetAddressEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
                    if (_customerSettings.StreetAddress2Enabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
                    if (_customerSettings.ZipPostalCodeEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
                    if (_customerSettings.CityEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
                    if (_customerSettings.CountryEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId, model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        var categories = new List<string>();
                        foreach (string formKey in form.Keys)
                        {
                            if (formKey.Contains("customernewsletterCategory_"))
                            {
                                try
                                {
                                    var category = formKey.Split('_')[1];
                                    categories.Add(category);
                                }
                                catch { }
                            }
                        }
                        //save newsletter value
                        var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, _storeContext.CurrentStore.Id);

                        if (newsletter != null)
                        {
                            newsletter.Categories.Clear();
                            categories.ForEach(x => newsletter.Categories.Add(x));

                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                await _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                            }
                            else
                            {
                                newsletter.Active = false;
                                await _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                            }
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                var newsLetterSubscription = new NewsLetterSubscription {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customer.Email,
                                    CustomerId = customer.Id,
                                    Active = true,
                                    StoreId = _storeContext.CurrentStore.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                };
                                categories.ForEach(x => newsLetterSubscription.Categories.Add(x));
                                await _newsLetterSubscriptionService.InsertNewsLetterSubscription(newsLetterSubscription);
                            }
                        }
                    }
                    if (forumSettings.ForumsEnabled && forumSettings.SignaturesEnabled)
                        await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Signature, model.Signature);

                    //save customer attributes
                    await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.CustomCustomerAttributes, customerAttributesXml);

                    return RedirectToRoute("CustomerInfo");
                }
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
            }

            //If we got this far, something failed, redisplay form
            model = await _customerViewModelService.PrepareInfoModel(model, customer, true, customerAttributesXml);
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual async Task<IActionResult> RemoveExternalAssociation(string id, [FromServices] IExternalAuthenticationService openAuthenticationService)
        {

            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            //ensure it's our record
            var ear = (await openAuthenticationService.GetExternalIdentifiersFor(_workContext.CurrentCustomer))
                .FirstOrDefault(x => x.Id == id);

            if (ear == null)
            {
                return Json(new
                {
                    redirect = Url.Action("Info"),
                });
            }
            await openAuthenticationService.DeleteExternalAuthenticationRecord(ear);

            return Json(new
            {
                redirect = Url.Action("Info"),
            });
        }


        public virtual async Task<IActionResult> Export([FromServices] IExportManager exportManager)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (!_customerSettings.AllowUsersToExportData)
                return Challenge();

            var customer = _workContext.CurrentCustomer;
            byte[] bytes = await exportManager.ExportCustomerToXlsx(customer, _storeContext.CurrentStore.Id);
            return File(bytes, "text/xls", "PersonalInfo.xlsx");

        }
        #endregion

        #region My account / Addresses

        public virtual async Task<IActionResult> Addresses()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = await _customerViewModelService.PrepareAddressList(_workContext.CurrentCustomer);
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual async Task<IActionResult> AddressDelete(string addressId)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var customer = _workContext.CurrentCustomer;

            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address != null)
            {
                customer.RemoveAddress(address);
                address.CustomerId = customer.Id;
                await _customerService.DeleteAddress(address);
            }

            return Json(new
            {
                redirect = Url.RouteUrl("CustomerAddresses"),
            });

        }

        public virtual async Task<IActionResult> AddressAdd()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();
            var countries = await _countryService.GetAllCountries(_workContext.WorkingLanguage.Id);
            var model = new CustomerAddressEditModel();
            await _addressViewModelService.PrepareModel(model: model.Address,
                address: null,
                excludeProperties: false,
                loadCountries: () => countries);

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual async Task<IActionResult> AddressAdd(CustomerAddressEditModel model, IFormCollection form)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var customer = _workContext.CurrentCustomer;

            //custom address attributes
            var customAttributes = await _addressViewModelService.ParseCustomAddressAttributes(form);
            var customAttributeWarnings = await _addressViewModelService.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid && ModelState.ErrorCount == 0)
            {
                var address = model.Address.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                customer.Addresses.Add(address);
                address.CustomerId = customer.Id;

                await _customerService.InsertAddress(address);

                return RedirectToRoute("CustomerAddresses");
            }
            var countries = await _countryService.GetAllCountries(_workContext.WorkingLanguage.Id);
            //If we got this far, something failed, redisplay form
            await _addressViewModelService.PrepareModel(model: model.Address,
                address: null,
                excludeProperties: true,
                loadCountries: () => countries);

            return View(model);
        }

        public virtual async Task<IActionResult> AddressEdit(string addressId)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var customer = _workContext.CurrentCustomer;
            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address == null)
                //address is not found
                return RedirectToRoute("CustomerAddresses");

            var countries = await _countryService.GetAllCountries(_workContext.WorkingLanguage.Id);
            var model = new CustomerAddressEditModel();
            await _addressViewModelService.PrepareModel(model: model.Address,
                address: address,
                excludeProperties: false,
                loadCountries: () => countries);

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual async Task<IActionResult> AddressEdit(CustomerAddressEditModel model, string addressId, IFormCollection form)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var customer = _workContext.CurrentCustomer;
            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address == null)
                //address is not found
                return RedirectToRoute("CustomerAddresses");

            //custom address attributes
            var customAttributes = await _addressViewModelService.ParseCustomAddressAttributes(form);
            var customAttributeWarnings = await _addressViewModelService.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid && ModelState.ErrorCount == 0)
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;
                address.CustomerId = customer.Id;
                await _customerService.UpdateAddress(address);

                if (customer.BillingAddress?.Id == address.Id)
                    await _customerService.UpdateBillingAddress(address);
                if (customer.ShippingAddress?.Id == address.Id)
                    await _customerService.UpdateShippingAddress(address);

                return RedirectToRoute("CustomerAddresses");
            }
            var countries = await _countryService.GetAllCountries(_workContext.WorkingLanguage.Id);
            //If we got this far, something failed, redisplay form
            await _addressViewModelService.PrepareModel(model: model.Address,
                address: address,
                excludeProperties: true,
                loadCountries: () => countries);
            return View(model);
        }

        #endregion

        #region My account / Downloadable products

        public virtual async Task<IActionResult> DownloadableProducts()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (_customerSettings.HideDownloadableProductsTab)
                return RedirectToRoute("CustomerInfo");

            var model = await _customerViewModelService.PrepareDownloadableProducts(_workContext.CurrentCustomer.Id);
            return View(model);
        }

        public virtual async Task<IActionResult> UserAgreement(Guid orderItemId)
        {
            var model = await _customerViewModelService.PrepareUserAgreement(orderItemId);
            if (model == null)
                return RedirectToRoute("HomePage");

            return View(model);
        }

        #endregion

        #region My account / Change password

        public virtual IActionResult ChangePassword([FromServices] CustomerSettings customerSettings)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = new ChangePasswordModel();

            //display the cause of the change password 
            if (_workContext.CurrentCustomer.PasswordIsExpired(customerSettings))
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Account.ChangePassword.PasswordIsExpired"));

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var customer = _workContext.CurrentCustomer;

            if (ModelState.IsValid)
            {
                var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
                var changePasswordResult = await _customerRegistrationService.ChangePassword(changePasswordRequest);
                if (changePasswordResult.Success)
                {
                    model.Result = _localizationService.GetResource("Account.ChangePassword.Success");
                    return View(model);
                }

                //errors
                foreach (var error in changePasswordResult.Errors)
                    ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion


        #region My account / Delete account

        public virtual IActionResult DeleteAccount()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (!_customerSettings.AllowUsersToDeleteAccount)
                return RedirectToRoute("CustomerInfo");

            var model = new DeleteAccountModel();

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual async Task<IActionResult> DeleteAccount(DeleteAccountModel model)
        {
            var customer = _workContext.CurrentCustomer;
            if (!customer.IsRegistered())
                return Challenge();

            if (!_customerSettings.AllowUsersToDeleteAccount)
                return RedirectToRoute("CustomerInfo");

            if (ModelState.IsValid)
            {
                var loginResult = await _customerRegistrationService.ValidateCustomer(_customerSettings.UsernamesEnabled ? customer.Username : customer.Email, model.Password);

                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                            //activity log
                            await _customerActivityService.InsertActivity("PublicStore.DeleteAccount", "", _localizationService.GetResource("ActivityLog.DeleteAccount"));

                            //delete account 
                            await _customerViewModelService.DeleteAccount(customer);

                            //standard logout 
                            await _authenticationService.SignOut();

                            return RedirectToRoute("HomePage");
                        }
                    case CustomerLoginResults.CustomerNotExist:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist"));
                        break;
                    case CustomerLoginResults.Deleted:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.Deleted"));
                        break;
                    case CustomerLoginResults.NotActive:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotActive"));
                        break;
                    case CustomerLoginResults.NotRegistered:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotRegistered"));
                        break;
                    case CustomerLoginResults.LockedOut:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.LockedOut"));
                        break;
                    case CustomerLoginResults.WrongPassword:
                    default:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials"));
                        break;
                }
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region My account / Avatar

        public virtual async Task<IActionResult> Avatar()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return RedirectToRoute("CustomerInfo");

            var model = await _customerViewModelService.PrepareAvatar(_workContext.CurrentCustomer);

            return View(model);
        }

        [HttpPost, ActionName("Avatar")]
        [PublicAntiForgery]
        [FormValueRequired("upload-avatar")]
        public virtual async Task<IActionResult> UploadAvatar(CustomerAvatarModel model, IFormFile uploadedFile, [FromServices] IPictureService pictureService, [FromServices] MediaSettings mediaSettings)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return RedirectToRoute("CustomerInfo");

            var customer = _workContext.CurrentCustomer;

            if (ModelState.IsValid)
            {
                try
                {
                    var customerAvatar = await pictureService.GetPictureById(await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.AvatarPictureId));
                    if ((uploadedFile != null) && (!String.IsNullOrEmpty(uploadedFile.FileName)))
                    {
                        int avatarMaxSize = _customerSettings.AvatarMaximumSizeBytes;
                        if (uploadedFile.Length > avatarMaxSize)
                            throw new GrandException(string.Format(_localizationService.GetResource("Account.Avatar.MaximumUploadedFileSize"), avatarMaxSize));

                        byte[] customerPictureBinary = uploadedFile.GetPictureBits();
                        if (customerAvatar != null)
                            customerAvatar = await pictureService.UpdatePicture(customerAvatar.Id, customerPictureBinary, uploadedFile.ContentType, null);
                        else
                            customerAvatar = await pictureService.InsertPicture(customerPictureBinary, uploadedFile.ContentType, null);
                    }

                    string customerAvatarId = "";
                    if (customerAvatar != null)
                        customerAvatarId = customerAvatar.Id;

                    await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AvatarPictureId, customerAvatarId);

                    model.AvatarUrl = await pictureService.GetPictureUrl(
                        await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.AvatarPictureId),
                        mediaSettings.AvatarPictureSize,
                        false);
                    return View(model);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError("", exc.Message);
                }
            }

            //If we got this far, something failed, redisplay form
            model.AvatarUrl = await pictureService.GetPictureUrl(
                await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.AvatarPictureId),
                mediaSettings.AvatarPictureSize,
                false);
            return View(model);
        }

        [HttpPost, ActionName("Avatar")]
        [PublicAntiForgery]
        [FormValueRequired("remove-avatar")]
        public virtual async Task<IActionResult> RemoveAvatar(CustomerAvatarModel model, [FromServices] IPictureService pictureService)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return RedirectToRoute("CustomerInfo");

            var customer = _workContext.CurrentCustomer;
            var customerAvatar = await pictureService.GetPictureById(await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.AvatarPictureId));
            if (customerAvatar != null)
                await pictureService.DeletePicture(customerAvatar);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AvatarPictureId, 0);

            return RedirectToRoute("CustomerAvatar");
        }

        #endregion

        #region My account / Auctions

        public virtual async Task<IActionResult> Auctions()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (_customerSettings.HideAuctionsTab)
                return RedirectToRoute("CustomerInfo");

            var model = await _customerViewModelService.PrepareAuctions(_workContext.CurrentCustomer);

            return View(model);
        }

        #endregion

        #region My account / Notes

        public virtual async Task<IActionResult> Notes()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (_customerSettings.HideNotesTab)
                return RedirectToRoute("CustomerInfo");

            var model = await _customerViewModelService.PrepareNotes(_workContext.CurrentCustomer);

            return View(model);
        }

        #endregion

        #region My account / Documents

        public virtual async Task<IActionResult> Documents()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (_customerSettings.HideDocumentsTab)
                return RedirectToRoute("CustomerInfo");

            var model = await _customerViewModelService.PrepareDocuments(_workContext.CurrentCustomer);

            return View(model);
        }

        #endregion

        #region My account / Reviews

        public virtual async Task<IActionResult> Reviews()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (_customerSettings.HideReviewsTab)
                return RedirectToRoute("CustomerInfo");

            var model = await _customerViewModelService.PrepareReviews(_workContext.CurrentCustomer);

            return View(model);
        }

        #endregion

        #region My account / Reviews

        public virtual async Task<IActionResult> Courses()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (_customerSettings.HideCoursesTab)
                return RedirectToRoute("CustomerInfo");

            var model = await _customerViewModelService.PrepareCourses(_workContext.CurrentCustomer, _storeContext.CurrentStore);

            return View(model);
        }

        #endregion
    }
}
