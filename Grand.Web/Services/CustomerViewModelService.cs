using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Security;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Tax;
using Grand.Core.Domain.Vendors;
using Grand.Framework.Security.Captcha;
using Grand.Services.Authentication.External;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Documents;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Interfaces;
using Grand.Web.Models.Common;
using Grand.Web.Models.Customer;
using Grand.Web.Models.Newsletter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class CustomerViewModelService : ICustomerViewModelService
    {

        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAddressViewModelService _addressViewModelService;
        private readonly IOrderService _orderService;
        private readonly IDownloadService _downloadService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IAuctionService _auctionService;
        private readonly INewsletterCategoryService _newsletterCategoryService;
        private readonly IServiceProvider _serviceProvider;

        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ForumSettings _forumSettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly SecuritySettings _securitySettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly OrderSettings _orderSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly VendorSettings _vendorSettings;

        public CustomerViewModelService(
                    IExternalAuthenticationService externalAuthenticationService,
                    ICustomerAttributeParser customerAttributeParser,
                    ICustomerAttributeService customerAttributeService,
                    ILocalizationService localizationService,
                    IDateTimeHelper dateTimeHelper,
                    INewsLetterSubscriptionService newsLetterSubscriptionService,
                    IWorkContext workContext,
                    IStoreContext storeContext,
                    ICountryService countryService,
                    IStateProvinceService stateProvinceService,
                    IGenericAttributeService genericAttributeService,
                    IWorkflowMessageService workflowMessageService,
                    IReturnRequestService returnRequestService,
                    IStoreMappingService storeMappingService,
                    IAddressViewModelService addressViewModelService,
                    IOrderService orderService,
                    IDownloadService downloadService,
                    IPictureService pictureService,
                    IProductService productService,
                    IAuctionService auctionService,
                    INewsletterCategoryService newsletterCategoryService,
                    IServiceProvider serviceProvider,
                    CustomerSettings customerSettings,
                    DateTimeSettings dateTimeSettings,
                    TaxSettings taxSettings,
                    ForumSettings forumSettings,
                    ExternalAuthenticationSettings externalAuthenticationSettings,
                    SecuritySettings securitySettings,
                    CaptchaSettings captchaSettings,
                    RewardPointsSettings rewardPointsSettings,
                    OrderSettings orderSettings,
                    MediaSettings mediaSettings,
                    VendorSettings vendorSettings
            )
        {
            _externalAuthenticationService = externalAuthenticationService;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _workContext = workContext;
            _storeContext = storeContext;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _genericAttributeService = genericAttributeService;
            _workflowMessageService = workflowMessageService;
            _returnRequestService = returnRequestService;
            _storeMappingService = storeMappingService;
            _addressViewModelService = addressViewModelService;
            _orderService = orderService;
            _downloadService = downloadService;
            _pictureService = pictureService;
            _productService = productService;
            _auctionService = auctionService;
            _newsletterCategoryService = newsletterCategoryService;
            _serviceProvider = serviceProvider;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _taxSettings = taxSettings;
            _forumSettings = forumSettings;
            _externalAuthenticationSettings = externalAuthenticationSettings;
            _securitySettings = securitySettings;
            _captchaSettings = captchaSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _orderSettings = orderSettings;
            _mediaSettings = mediaSettings;
            _vendorSettings = vendorSettings;
        }

        public virtual async Task DeleteAccount(Customer customer)
        {
            //send notification to customer
            await _workflowMessageService.SendCustomerDeleteStoreOwnerNotification(customer, _serviceProvider.GetRequiredService<LocalizationSettings>().DefaultAdminLanguageId);

            //delete emails
            await _serviceProvider.GetRequiredService<IQueuedEmailService>().DeleteCustomerEmail(customer.Email);

            //delete newsletter subscription
            var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, _storeContext.CurrentStore.Id);
            if (newsletter != null)
                await _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);

            //delete account
            await _serviceProvider.GetRequiredService<ICustomerService>().DeleteCustomer(customer);
        }

        public virtual async Task<IList<CustomerAttributeModel>> PrepareCustomAttributes(Customer customer,
            string overrideAttributesXml = "")
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var result = new List<CustomerAttributeModel>();

            var customerAttributes = await _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in customerAttributes)
            {
                var attributeModel = new CustomerAttributeModel {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = attribute.CustomerAttributeValues;
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new CustomerAttributeValueModel {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(valueModel);
                    }
                }

                //set already selected attributes
                var selectedAttributesXml = !String.IsNullOrEmpty(overrideAttributesXml) ?
                    overrideAttributesXml :
                    await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.CustomCustomerAttributes);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                        {
                            if (!String.IsNullOrEmpty(selectedAttributesXml))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues = await _customerAttributeParser.ParseCustomerAttributeValues(selectedAttributesXml);
                                foreach (var attributeValue in selectedValues)
                                    if (attributeModel.Id == attributeValue.CustomerAttributeId)
                                        foreach (var item in attributeModel.Values)
                                            if (attributeValue.Id == item.Id)
                                                item.IsPreSelected = true;
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //do nothing
                            //values are already pre-set
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            if (!String.IsNullOrEmpty(selectedAttributesXml))
                            {
                                var enteredText = _customerAttributeParser.ParseValues(selectedAttributesXml, attribute.Id);
                                if (enteredText.Any())
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.FileUpload:
                    default:
                        //not supported attribute control types
                        break;
                }

                result.Add(attributeModel);
            }


            return result;

        }

        public virtual async Task<CustomerInfoModel> PrepareInfoModel(CustomerInfoModel model, Customer customer,
            bool excludeProperties, string overrideCustomCustomerAttributesXml = "")
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (customer == null)
                throw new ArgumentNullException("customer");

            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            if (model.AllowCustomersToSetTimeZone)
                foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                    model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == _dateTimeHelper.CurrentTimeZone.Id) });

            if (!excludeProperties)
            {
                model.VatNumber = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.VatNumber);
                model.FirstName = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.FirstName);
                model.LastName = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.LastName);
                model.Gender = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Gender);
                var dateOfBirth = await customer.GetAttribute<DateTime?>(_genericAttributeService, SystemCustomerAttributeNames.DateOfBirth);
                if (dateOfBirth.HasValue)
                {
                    model.DateOfBirthDay = dateOfBirth.Value.Day;
                    model.DateOfBirthMonth = dateOfBirth.Value.Month;
                    model.DateOfBirthYear = dateOfBirth.Value.Year;
                }
                model.Company = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Company);
                model.StreetAddress = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StreetAddress);
                model.StreetAddress2 = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StreetAddress2);
                model.ZipPostalCode = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.ZipPostalCode);
                model.City = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.City);
                model.CountryId = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.CountryId);
                model.StateProvinceId = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StateProvinceId);
                model.Phone = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Phone);
                model.Fax = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Fax);

                //newsletter
                var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, _storeContext.CurrentStore.Id);
                if (newsletter == null)
                    newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByCustomerId(customer.Id);

                model.Newsletter = newsletter != null && newsletter.Active;

                var categories = (await _newsletterCategoryService.GetAllNewsletterCategory()).ToList();
                categories.ForEach(x => model.NewsletterCategories.Add(new NewsletterSimpleCategory() {
                    Id = x.Id,
                    Description = x.GetLocalized(y => y.Description, _workContext.WorkingLanguage.Id),
                    Name = x.GetLocalized(y => y.Name, _workContext.WorkingLanguage.Id),
                    Selected = newsletter == null ? false : newsletter.Categories.Contains(x.Id),
                }));

                model.Signature = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Signature);

                model.Email = customer.Email;
                model.Username = customer.Username;
            }
            else
            {
                if (_customerSettings.UsernamesEnabled && !_customerSettings.AllowUsersToChangeUsernames)
                    model.Username = customer.Username;
            }

            //countries and states
            if (_customerSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "" });
                foreach (var c in await _countryService.GetAllCountries(_workContext.WorkingLanguage.Id))
                {
                    model.AvailableCountries.Add(new SelectListItem {
                        Text = c.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_customerSettings.StateProvinceEnabled)
                {
                    //states
                    var states = await _stateProvinceService.GetStateProvincesByCountryId(model.CountryId, _workContext.WorkingLanguage.Id);
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem { Text = s.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                        }
                    }
                    else
                    {
                        bool anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                        model.AvailableStates.Add(new SelectListItem {
                            Text = _localizationService.GetResource(anyCountrySelected ? "Address.OtherNonUS" : "Address.SelectState"),
                            Value = ""
                        });
                    }

                }
            }
            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            model.VatNumberStatusNote = ((VatNumberStatus)await customer.GetAttribute<int>(_genericAttributeService, SystemCustomerAttributeNames.VatNumberStatusId))
                .GetLocalizedEnum(_localizationService, _workContext);
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.DateOfBirthRequired = _customerSettings.DateOfBirthRequired;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.CompanyRequired = _customerSettings.CompanyRequired;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _customerSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _customerSettings.ZipPostalCodeRequired;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CityRequired = _customerSettings.CityRequired;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.CountryRequired = _customerSettings.CountryRequired;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.StateProvinceRequired = _customerSettings.StateProvinceRequired;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.PhoneRequired = _customerSettings.PhoneRequired;
            model.FaxEnabled = _customerSettings.FaxEnabled;
            model.FaxRequired = _customerSettings.FaxRequired;
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.AllowUsersToChangeUsernames = _customerSettings.AllowUsersToChangeUsernames;
            model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
            model.SignatureEnabled = _forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled;

            //external authentication
            model.NumberOfExternalAuthenticationProviders = _externalAuthenticationService
                           .LoadActiveExternalAuthenticationMethods(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id).Count;
            foreach (var ear in await _externalAuthenticationService.GetExternalIdentifiersFor(customer))
            {
                var authMethod = _externalAuthenticationService.LoadExternalAuthenticationMethodBySystemName(ear.ProviderSystemName);
                if (authMethod == null || !authMethod.IsMethodActive(_externalAuthenticationSettings))
                    continue;

                model.AssociatedExternalAuthRecords.Add(new CustomerInfoModel.AssociatedExternalAuthModel {
                    Id = ear.Id,
                    Email = ear.Email,
                    ExternalIdentifier = ear.ExternalDisplayIdentifier,
                    AuthMethodName = authMethod.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id)
                });
            }

            //custom customer attributes
            var customAttributes = await PrepareCustomAttributes(customer, overrideCustomCustomerAttributesXml);
            foreach (var attribute in customAttributes)
                model.CustomerAttributes.Add(attribute);

            return model;
        }

        public virtual async Task<RegisterModel> PrepareRegisterModel(RegisterModel model, bool excludeProperties,
            string overrideCustomCustomerAttributesXml = "")
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            if (model.AllowCustomersToSetTimeZone)
                foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                    model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == _dateTimeHelper.CurrentTimeZone.Id) });

            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            //form fields
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.DateOfBirthRequired = _customerSettings.DateOfBirthRequired;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.CompanyRequired = _customerSettings.CompanyRequired;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _customerSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _customerSettings.ZipPostalCodeRequired;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CityRequired = _customerSettings.CityRequired;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.CountryRequired = _customerSettings.CountryRequired;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.StateProvinceRequired = _customerSettings.StateProvinceRequired;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.PhoneRequired = _customerSettings.PhoneRequired;
            model.FaxEnabled = _customerSettings.FaxEnabled;
            model.FaxRequired = _customerSettings.FaxRequired;
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
            model.AcceptPrivacyPolicyEnabled = _customerSettings.AcceptPrivacyPolicyEnabled;
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
            model.HoneypotEnabled = _securitySettings.HoneypotEnabled;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage;

            //countries and states
            if (_customerSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "" });

                foreach (var c in await _countryService.GetAllCountries(_workContext.WorkingLanguage.Id))
                {
                    model.AvailableCountries.Add(new SelectListItem {
                        Text = c.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_customerSettings.StateProvinceEnabled)
                {
                    //states
                    var states = await _stateProvinceService.GetStateProvincesByCountryId(model.CountryId, _workContext.WorkingLanguage.Id);
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem { Text = s.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                        }
                    }
                    else
                    {
                        bool anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                        model.AvailableStates.Add(new SelectListItem {
                            Text = _localizationService.GetResource(anyCountrySelected ? "Address.OtherNonUS" : "Address.SelectState"),
                            Value = ""
                        });
                    }

                }
            }

            //custom customer attributes
            var customAttributes = await PrepareCustomAttributes(_workContext.CurrentCustomer, overrideCustomCustomerAttributesXml);
            foreach (var item in customAttributes)
            {
                model.CustomerAttributes.Add(item);
            }

            //newsletter categories
            var newsletterCategories = await _newsletterCategoryService.GetNewsletterCategoriesByStore(_storeContext.CurrentStore.Id);
            foreach (var item in newsletterCategories)
            {
                model.NewsletterCategories.Add(new NewsletterSimpleCategory() {
                    Id = item.Id,
                    Name = item.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    Description = item.GetLocalized(x => x.Description, _workContext.WorkingLanguage.Id),
                    Selected = item.Selected
                });
            }
            return model;
        }

        public virtual async Task<string> ParseCustomAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            string attributesXml = "";
            var attributes = await _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in attributes)
            {
                string controlId = string.Format("customer_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, ctrlAttributes);
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId].ToString();
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (!String.IsNullOrEmpty(item))
                                        attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                            attribute, item);
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = attribute.CustomerAttributeValues;
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                            attribute, selectedAttributeId);
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId].ToString();
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.Trim();
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported customer attributes
                    default:
                        break;
                }
            }

            return attributesXml;

        }

        public virtual LoginModel PrepareLogin(bool? checkoutAsGuest)
        {
            var model = new LoginModel();
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.CheckoutAsGuest = checkoutAsGuest.GetValueOrDefault();
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage;
            return model;
        }

        public virtual PasswordRecoveryModel PreparePasswordRecovery()
        {
            var model = new PasswordRecoveryModel();
            return model;
        }

        public virtual async Task<PasswordRecoveryConfirmModel> PreparePasswordRecoveryConfirmModel(Customer customer, string token)
        {
            var model = new PasswordRecoveryConfirmModel();

            //validate token
            if (!(customer.IsPasswordRecoveryTokenValid(token)))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.WrongToken");
            }

            //validate token expiration date
            if (customer.IsPasswordRecoveryLinkExpired(_customerSettings))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.LinkExpired");
            }
            return await Task.FromResult(model);
        }

        public virtual async Task PasswordRecoverySend(PasswordRecoveryModel model, Customer customer)
        {
            //save token and current date
            var passwordRecoveryToken = Guid.NewGuid();
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken, passwordRecoveryToken.ToString());
            DateTime? generatedDateTime = DateTime.UtcNow;
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryTokenDateGenerated, generatedDateTime);
            //send email
            await _workflowMessageService.SendCustomerPasswordRecoveryMessage(customer, _workContext.WorkingLanguage.Id);
        }

        public virtual async Task<CustomerNavigationModel> PrepareNavigation(int selectedTabId = 0)
        {
            var model = new CustomerNavigationModel();
            model.HideAvatar = !_customerSettings.AllowCustomersToUploadAvatars;
            model.HideRewardPoints = !_rewardPointsSettings.Enabled;
            model.HideDeleteAccount = !_customerSettings.AllowUsersToDeleteAccount;
            model.HideForumSubscriptions = !_forumSettings.ForumsEnabled || !_forumSettings.AllowCustomersToManageSubscriptions;
            model.HideReturnRequests = !_orderSettings.ReturnRequestsEnabled ||
                !(await _returnRequestService.SearchReturnRequests(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id, "", null, 0, 1)).Any();
            model.HideDownloadableProducts = _customerSettings.HideDownloadableProductsTab;
            model.HideBackInStockSubscriptions = _customerSettings.HideBackInStockSubscriptionsTab;
            model.HideAuctions = _customerSettings.HideAuctionsTab;
            model.HideNotes = _customerSettings.HideNotesTab;
            model.HideDocuments = _customerSettings.HideDocumentsTab;
            model.HideReviews = _customerSettings.HideReviewsTab;
            model.HideCourses = _customerSettings.HideCoursesTab;
            if (_vendorSettings.AllowVendorsToEditInfo && _workContext.CurrentVendor != null)
            {
                model.ShowVendorInfo = true;
            }
            model.SelectedTab = (CustomerNavigationEnum)selectedTabId;

            return model;
        }

        public virtual async Task<CustomerAddressListModel> PrepareAddressList(Customer customer)
        {
            var model = new CustomerAddressListModel();
            var addresses = new List<Address>();
            foreach (var item in customer.Addresses)
            {
                if (string.IsNullOrEmpty(item.CountryId))
                {
                    addresses.Add(item);
                    continue;
                }
                var country = await _countryService.GetCountryById(item.CountryId);
                if (country != null || _storeMappingService.Authorize(country))
                {
                    addresses.Add(item);
                    continue;
                }
            }

            foreach (var address in addresses)
            {
                var countries = await _countryService.GetAllCountries(_workContext.WorkingLanguage.Id);
                var addressModel = new AddressModel();
                await _addressViewModelService.PrepareModel(model: addressModel,
                    address: address,
                    excludeProperties: false,
                    loadCountries: () => countries);
                model.Addresses.Add(addressModel);
            }

            return model;
        }
        public virtual async Task<CustomerDownloadableProductsModel> PrepareDownloadableProducts(string customerId)
        {
            var model = new CustomerDownloadableProductsModel();
            var items = await _orderService.GetAllOrderItems(null, customerId, null, null,
                null, null, null, true);
            var productService = _serviceProvider.GetRequiredService<IProductService>();
            foreach (var item in items)
            {
                var order = await _orderService.GetOrderByOrderItemId(item.Id);
                var product = await productService.GetProductByIdIncludeArch(item.ProductId);
                var itemModel = new CustomerDownloadableProductsModel.DownloadableProductsModel {
                    OrderItemGuid = item.OrderItemGuid,
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(item.CreatedOnUtc, DateTimeKind.Utc),
                    ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id),
                    ProductAttributes = item.AttributeDescription,
                    ProductId = item.ProductId
                };
                model.Items.Add(itemModel);

                if (await _downloadService.IsDownloadAllowed(item))
                    itemModel.DownloadId = product.DownloadId;

                if (await _downloadService.IsLicenseDownloadAllowed(item))
                    itemModel.LicenseId = !String.IsNullOrEmpty(item.LicenseDownloadId) ? item.LicenseDownloadId : "";
            }
            return model;
        }

        public virtual async Task<UserAgreementModel> PrepareUserAgreement(Guid orderItemId)
        {
            var orderItem = await _orderService.GetOrderItemByGuid(orderItemId);
            if (orderItem == null)
                return null;

            var product = await _serviceProvider.GetRequiredService<IProductService>().GetProductById(orderItem.ProductId);
            if (product == null || !product.HasUserAgreement)
                return null;

            var model = new UserAgreementModel();
            model.UserAgreementText = product.UserAgreementText;
            model.OrderItemGuid = orderItemId;
            return model;

        }
        public virtual async Task<CustomerAvatarModel> PrepareAvatar(Customer customer)
        {
            var model = new CustomerAvatarModel();
            model.AvatarUrl = await _pictureService.GetPictureUrl(
                await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.AvatarPictureId),
                _mediaSettings.AvatarPictureSize,
                false);

            return model;
        }

        public virtual async Task<CustomerAuctionsModel> PrepareAuctions(Customer customer)
        {
            var model = new CustomerAuctionsModel();
            var priceFormatter = _serviceProvider.GetRequiredService<IPriceFormatter>();

            var customerBids = (await _auctionService.GetBidsByCustomerId(customer.Id)).GroupBy(x => x.ProductId);
            foreach (var item in customerBids)
            {
                var product = await _productService.GetProductById(item.Key);
                if (product != null)
                {
                    var bid = new ProductBidTuple();
                    bid.Ended = product.AuctionEnded;
                    bid.OrderId = item.Where(x => x.Win && x.CustomerId == customer.Id).FirstOrDefault()?.OrderId;
                    var amount = product.HighestBid;
                    bid.CurrentBidAmount = priceFormatter.FormatPrice(amount);
                    bid.CurrentBidAmountValue = amount;
                    bid.HighestBidder = product.HighestBidder == customer.Id;
                    bid.EndBidDate = product.AvailableEndDateTimeUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(product.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc) : DateTime.MaxValue;
                    bid.ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
                    bid.ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id);
                    bid.BidAmountValue = item.Max(x => x.Amount);
                    bid.BidAmount = priceFormatter.FormatPrice(bid.BidAmountValue);
                    model.ProductBidList.Add(bid);
                }
            }

            model.CustomerId = customer.Id;

            return model;
        }

        public virtual async Task<CustomerNotesModel> PrepareNotes(Customer customer)
        {
            var model = new CustomerNotesModel();
            model.CustomerId = customer.Id;
            var customerservice = _serviceProvider.GetRequiredService<ICustomerService>();
            var notes = await customerservice.GetCustomerNotes(_workContext.CurrentCustomer.Id, true);
            foreach (var item in notes)
            {
                var mm = new Models.Customer.CustomerNote();
                mm.NoteId = item.Id;
                mm.CreatedOn = _dateTimeHelper.ConvertToUserTime(item.CreatedOnUtc, DateTimeKind.Utc);
                mm.Note = item.Note;
                mm.Title = item.Title;
                mm.DownloadId = item.DownloadId;
                model.CustomerNoteList.Add(mm);
            }
            return model;
        }

        public virtual async Task<DocumentsModel> PrepareDocuments(Customer customer)
        {
            var model = new DocumentsModel();
            model.CustomerId = customer.Id;
            var documentService = _serviceProvider.GetRequiredService<IDocumentService>();
            var documentTypeService = _serviceProvider.GetRequiredService<IDocumentTypeService>();
            var documents = await documentService.GetAll(customer.Id);
            foreach (var item in documents.Where(x => x.Published).OrderBy(x => x.DisplayOrder))
            {
                var doc = new Document();
                doc.Id = item.Id;
                doc.Amount = item.TotalAmount;
                doc.OutstandAmount = item.OutstandAmount;
                doc.Link = item.Link;
                doc.Name = item.Name;
                doc.Number = item.Number;
                doc.Quantity = item.Quantity;
                doc.Status = item.DocumentStatus.GetLocalizedEnum(_localizationService, _workContext);
                doc.Description = item.Description;
                doc.DocDate = item.DocDate;
                doc.DueDate = item.DueDate;
                doc.DocumentType = (await documentTypeService.GetById(item.DocumentTypeId))?.Name;
                doc.DownloadId = item.DownloadId;
                model.DocumentList.Add(doc);
            }
            return model;
        }

        public virtual async Task<CustomerProductReviewsModel> PrepareReviews(Customer customer)
        {
            var reviewsModel = new CustomerProductReviewsModel();

            reviewsModel.CustomerId = customer.Id;
            reviewsModel.CustomerInfo = customer != null ? customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest") : "";

            var productReviews = await _productService.GetAllProductReviews(customer.Id);
            foreach (var productReview in productReviews)
            {
                var product = await _productService.GetProductById(productReview.ProductId);

                var reviewModel = new CustomerProductReviewModel();

                reviewModel.Id = productReview.Id;
                reviewModel.ProductId = productReview.ProductId;
                reviewModel.ProductName = product.Name;
                reviewModel.ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id);
                reviewModel.Rating = productReview.Rating;
                reviewModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(productReview.CreatedOnUtc, DateTimeKind.Utc);
                reviewModel.Signature = productReview.Signature;
                reviewModel.ReviewText = productReview.ReviewText;
                reviewModel.ReplyText = productReview.ReplyText;
                reviewModel.IsApproved = productReview.IsApproved;

                reviewsModel.Reviews.Add(reviewModel);
            }

            return reviewsModel;
        }

        public virtual async Task<CoursesModel> PrepareCourses(Customer customer, Store store)
        {
            var courseService = _serviceProvider.GetRequiredService<ICourseViewModelService>();
            var model = await courseService.GetCoursesByCustomer(customer, store.Id);
            return model;
        }

    }
}