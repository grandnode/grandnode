using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Security;
using Grand.Core.Domain.Tax;
using Grand.Core.Domain.Vendors;
using Grand.Core.Infrastructure;
using Grand.Framework.Security.Captcha;
using Grand.Services.Authentication.External;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Models.Common;
using Grand.Web.Models.Customer;
using Grand.Web.Models.Newsletter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Services
{
    public partial class CustomerViewModelService: ICustomerViewModelService
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
            this._externalAuthenticationService = externalAuthenticationService;
            this._customerAttributeParser = customerAttributeParser;
            this._customerAttributeService = customerAttributeService;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._genericAttributeService = genericAttributeService;
            this._workflowMessageService = workflowMessageService;
            this._returnRequestService = returnRequestService;
            this._storeMappingService = storeMappingService;
            this._addressViewModelService = addressViewModelService;
            this._orderService = orderService;
            this._downloadService = downloadService;
            this._pictureService = pictureService;
            this._productService = productService;
            this._auctionService = auctionService;
            this._newsletterCategoryService = newsletterCategoryService;
            this._customerSettings = customerSettings;
            this._dateTimeSettings = dateTimeSettings;
            this._taxSettings = taxSettings;
            this._forumSettings = forumSettings;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
            this._securitySettings = securitySettings;
            this._captchaSettings = captchaSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._orderSettings = orderSettings;
            this._mediaSettings = mediaSettings;
            this._vendorSettings = vendorSettings;
        }

        public virtual void DeleteAccount(Customer customer)
        {
            //send notification to customer
            _workflowMessageService.SendCustomerDeleteStoreOwnerNotification(customer, EngineContext.Current.Resolve<LocalizationSettings>().DefaultAdminLanguageId);

            //delete emails
            EngineContext.Current.Resolve<IQueuedEmailService>().DeleteCustomerEmail(customer.Email);

            //delete newsletter subscription
            var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, _storeContext.CurrentStore.Id);
            if (newsletter != null)
                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);

            //delete account
            EngineContext.Current.Resolve<ICustomerService>().DeleteCustomer(customer);
        }

        public virtual IList<CustomerAttributeModel> PrepareCustomAttributes(Customer customer,
            string overrideAttributesXml = "")
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var result = new List<CustomerAttributeModel>();

            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in customerAttributes)
            {
                var attributeModel = new CustomerAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = attribute.CustomerAttributeValues;
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new CustomerAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name),
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(valueModel);
                    }
                }

                //set already selected attributes
                var selectedAttributesXml = !String.IsNullOrEmpty(overrideAttributesXml) ?
                    overrideAttributesXml :
                    customer.GetAttribute<string>(SystemCustomerAttributeNames.CustomCustomerAttributes);
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
                                var selectedValues = _customerAttributeParser.ParseCustomerAttributeValues(selectedAttributesXml);
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

        public virtual CustomerInfoModel PrepareInfoModel(CustomerInfoModel model, Customer customer,
            bool excludeProperties, string overrideCustomCustomerAttributesXml = "")
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (customer == null)
                throw new ArgumentNullException("customer");

            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            if(model.AllowCustomersToSetTimeZone)
                foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                    model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == _dateTimeHelper.CurrentTimeZone.Id) });

            if (!excludeProperties)
            {
                model.VatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);
                model.FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                model.LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                model.Gender = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
                var dateOfBirth = customer.GetAttribute<DateTime?>(SystemCustomerAttributeNames.DateOfBirth);
                if (dateOfBirth.HasValue)
                {
                    model.DateOfBirthDay = dateOfBirth.Value.Day;
                    model.DateOfBirthMonth = dateOfBirth.Value.Month;
                    model.DateOfBirthYear = dateOfBirth.Value.Year;
                }
                model.Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company);
                model.StreetAddress = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress);
                model.StreetAddress2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2);
                model.ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode);
                model.City = customer.GetAttribute<string>(SystemCustomerAttributeNames.City);
                model.CountryId = customer.GetAttribute<string>(SystemCustomerAttributeNames.CountryId);
                model.StateProvinceId = customer.GetAttribute<string>(SystemCustomerAttributeNames.StateProvinceId);
                model.Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                model.Fax = customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax);

                //newsletter
                var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, _storeContext.CurrentStore.Id);
                if (newsletter == null)
                    newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByCustomerId(customer.Id);

                model.Newsletter = newsletter != null && newsletter.Active;

                var categories = _newsletterCategoryService.GetAllNewsletterCategory().ToList();
                categories.ForEach(x => model.NewsletterCategories.Add(new NewsletterSimpleCategory() {
                    Id = x.Id,
                    Description = x.GetLocalized(y=>y.Description),
                    Name = x.GetLocalized(y=>y.Name),
                    Selected = newsletter == null ? false: newsletter.Categories.Contains(x.Id),
                }));

                model.Signature = customer.GetAttribute<string>(SystemCustomerAttributeNames.Signature);

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
                foreach (var c in _countryService.GetAllCountries(_workContext.WorkingLanguage.Id))
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.GetLocalized(x => x.Name),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_customerSettings.StateProvinceEnabled)
                {
                    //states
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId, _workContext.WorkingLanguage.Id).ToList();
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem { Text = s.GetLocalized(x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                        }
                    }
                    else
                    {
                        bool anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = _localizationService.GetResource(anyCountrySelected ? "Address.OtherNonUS" : "Address.SelectState"),
                            Value = ""
                        });
                    }

                }
            }
            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            model.VatNumberStatusNote = ((VatNumberStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId))
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
            foreach (var ear in _externalAuthenticationService.GetExternalIdentifiersFor(customer))
            {
                var authMethod = _externalAuthenticationService.LoadExternalAuthenticationMethodBySystemName(ear.ProviderSystemName);
                if (authMethod == null || !authMethod.IsMethodActive(_externalAuthenticationSettings))
                    continue;

                model.AssociatedExternalAuthRecords.Add(new CustomerInfoModel.AssociatedExternalAuthModel
                {
                    Id = ear.Id,
                    Email = ear.Email,
                    ExternalIdentifier = ear.ExternalDisplayIdentifier,
                    AuthMethodName = authMethod.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id)
                });
            }

            //custom customer attributes
            var customAttributes = PrepareCustomAttributes(customer, overrideCustomCustomerAttributesXml);
            foreach (var attribute in customAttributes)
                model.CustomerAttributes.Add(attribute);

            return model;
        }

        public virtual RegisterModel PrepareRegisterModel(RegisterModel model, bool excludeProperties,
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

                foreach (var c in _countryService.GetAllCountries(_workContext.WorkingLanguage.Id))
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.GetLocalized(x => x.Name),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_customerSettings.StateProvinceEnabled)
                {
                    //states
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId, _workContext.WorkingLanguage.Id).ToList();
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem { Text = s.GetLocalized(x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                        }
                    }
                    else
                    {
                        bool anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = _localizationService.GetResource(anyCountrySelected ? "Address.OtherNonUS" : "Address.SelectState"),
                            Value = ""
                        });
                    }

                }
            }

            //custom customer attributes
            var customAttributes = PrepareCustomAttributes(_workContext.CurrentCustomer, overrideCustomCustomerAttributesXml);
            foreach (var item in customAttributes)
            {
                model.CustomerAttributes.Add(item);
            }

            //newsletter categories
            var newsletterCategories = _newsletterCategoryService.GetNewsletterCategoriesByStore(_storeContext.CurrentStore.Id);
            foreach (var item in newsletterCategories)
            {
                model.NewsletterCategories.Add(new NewsletterSimpleCategory()
                {
                    Id = item.Id,
                    Name = item.GetLocalized(x => x.Name),
                    Description = item.GetLocalized(x => x.Description),
                    Selected = item.Selected
                });
            }
            return model;
        }

        public virtual string ParseCustomAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            string attributesXml = "";
            var attributes = _customerAttributeService.GetAllCustomerAttributes();
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

        public virtual PasswordRecoveryConfirmModel PreparePasswordRecoveryConfirmModel(Customer customer, string token)
        {
            var model = new PasswordRecoveryConfirmModel();

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
            }
            return model;
        }

        public virtual void PasswordRecoverySend(PasswordRecoveryModel model, Customer customer)
        {
            //save token and current date
            var passwordRecoveryToken = Guid.NewGuid();
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken, passwordRecoveryToken.ToString());
            DateTime? generatedDateTime = DateTime.UtcNow;
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryTokenDateGenerated, generatedDateTime);

            //send email
            _workflowMessageService.SendCustomerPasswordRecoveryMessage(customer, _workContext.WorkingLanguage.Id);

        }
        public virtual CustomerNavigationModel PrepareNavigation(int selectedTabId = 0)
        {
            var model = new CustomerNavigationModel();
            model.HideAvatar = !_customerSettings.AllowCustomersToUploadAvatars;
            model.HideRewardPoints = !_rewardPointsSettings.Enabled;
            model.HideDeleteAccount = !_customerSettings.AllowUsersToDeleteAccount;
            model.HideForumSubscriptions = !_forumSettings.ForumsEnabled || !_forumSettings.AllowCustomersToManageSubscriptions;
            model.HideReturnRequests = !_orderSettings.ReturnRequestsEnabled ||
                _returnRequestService.SearchReturnRequests(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id, "", null, 0, 1).Count == 0;
            model.HideDownloadableProducts = _customerSettings.HideDownloadableProductsTab;
            model.HideBackInStockSubscriptions = _customerSettings.HideBackInStockSubscriptionsTab;
            model.HideAuctions = _customerSettings.HideAuctionsTab;
            model.HideNotes = _customerSettings.HideNotesTab;
            if (_vendorSettings.AllowVendorsToEditInfo && _workContext.CurrentVendor != null)
            {
                model.ShowVendorInfo = true;
            }
            model.SelectedTab = (CustomerNavigationEnum)selectedTabId;

            return model;
        }

        public virtual CustomerAddressListModel PrepareAddressList(Customer customer)
        {
            var model = new CustomerAddressListModel();
            var addresses = customer.Addresses
                .Where(a => a.CountryId == "" ||
                _storeMappingService.Authorize(_countryService.GetCountryById(a.CountryId))
                )
                .ToList();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                _addressViewModelService.PrepareModel(model: addressModel,
                    address: address,
                    excludeProperties: false,
                    loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id));
                model.Addresses.Add(addressModel);
            }

            return model;
        }
        public virtual CustomerDownloadableProductsModel PrepareDownloadableProducts(string customerId)
        {
            var model = new CustomerDownloadableProductsModel();
            var items = _orderService.GetAllOrderItems(null, customerId, null, null,
                null, null, null, true);
            var productService = EngineContext.Current.Resolve<IProductService>();
            foreach (var item in items)
            {
                var order = _orderService.GetOrderByOrderItemId(item.Id);
                var product = productService.GetProductByIdIncludeArch(item.ProductId);
                var itemModel = new CustomerDownloadableProductsModel.DownloadableProductsModel
                {
                    OrderItemGuid = item.OrderItemGuid,
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(item.CreatedOnUtc, DateTimeKind.Utc),
                    ProductName = product.GetLocalized(x => x.Name),
                    ProductSeName = product.GetSeName(),
                    ProductAttributes = item.AttributeDescription,
                    ProductId = item.ProductId
                };
                model.Items.Add(itemModel);

                if (_downloadService.IsDownloadAllowed(item))
                    itemModel.DownloadId = product.DownloadId;

                if (_downloadService.IsLicenseDownloadAllowed(item))
                    itemModel.LicenseId = !String.IsNullOrEmpty(item.LicenseDownloadId) ? item.LicenseDownloadId : "";
            }
            return model;
        }

        public virtual UserAgreementModel PrepareUserAgreement(Guid orderItemId)
        {
            var orderItem = _orderService.GetOrderItemByGuid(orderItemId);
            if (orderItem == null)
                return null;

            var product = EngineContext.Current.Resolve<IProductService>().GetProductById(orderItem.ProductId);
            if (product == null || !product.HasUserAgreement)
                return null;

            var model = new UserAgreementModel();
            model.UserAgreementText = product.UserAgreementText;
            model.OrderItemGuid = orderItemId;
            return model;

        }
        public virtual CustomerAvatarModel PrepareAvatar(Customer customer)
        {
            var model = new CustomerAvatarModel();
            model.AvatarUrl = _pictureService.GetPictureUrl(
                customer.GetAttribute<string>(SystemCustomerAttributeNames.AvatarPictureId),
                _mediaSettings.AvatarPictureSize,
                false);

            return model;
        }

        public virtual CustomerAuctionsModel PrepareAuctions(Customer customer)
        {
            var model = new CustomerAuctionsModel();
            var priceFormatter = EngineContext.Current.Resolve<IPriceFormatter>();

            var customerBids = _auctionService.GetBidsByCustomerId(customer.Id).GroupBy(x => x.ProductId);
            foreach (var item in customerBids)
            {
                var product = _productService.GetProductById(item.Key);
                if (product != null)
                {
                    var bid = new ProductBidTuple();
                    bid.Ended = product.AuctionEnded;
                    bid.OrderId = item.Where(x => x.Win && x.CustomerId == customer.Id).FirstOrDefault()?.OrderId;
                    var amount = product.HighestBid;
                    bid.CurrentBidAmount = priceFormatter.FormatPrice(amount);
                    bid.CurrentBidAmountValue = amount;
                    bid.HighestBidder = product.HighestBidder == customer.Id;
                    bid.EndBidDate = product.AvailableEndDateTimeUtc.HasValue ? product.AvailableEndDateTimeUtc.Value : DateTime.MaxValue;
                    bid.ProductName = product.GetLocalized(x => x.Name);
                    bid.ProductSeName = product.GetSeName();
                    bid.BidAmountValue = item.Max(x => x.Amount);
                    bid.BidAmount = priceFormatter.FormatPrice(bid.BidAmountValue);
                    model.ProductBidList.Add(bid);
                }
            }

            model.CustomerId = customer.Id;

            return model;
        }

        public virtual CustomerNotesModel PrepareNotes(Customer customer)
        {
            var model = new CustomerNotesModel();
            model.CustomerId = customer.Id;
            var customerservice = EngineContext.Current.Resolve<ICustomerService>();
            var notes = customerservice.GetCustomerNotes(_workContext.CurrentCustomer.Id, true);
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
    }
}