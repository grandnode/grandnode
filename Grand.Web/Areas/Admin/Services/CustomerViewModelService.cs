using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Tax;
using Grand.Core.Infrastructure;
using Grand.Framework.Extensions;
using Grand.Framework.Mvc.Models;
using Grand.Services.Affiliates;
using Grand.Services.Authentication.External;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Messages;
using Grand.Web.Areas.Admin.Models.ShoppingCart;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class CustomerViewModelService : ICustomerViewModelService
    {
        private readonly ICustomerService _customerService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerReportService _customerReportService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IRewardPointsService _rewardPointsService;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly TaxSettings _taxSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly CustomerSettings _customerSettings;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly IVendorService _vendorService;
        private readonly IStoreContext _storeContext;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderService _orderService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IForumService _forumService;
        private readonly IExternalAuthenticationService _openAuthenticationService;
        private readonly AddressSettings _addressSettings;
        private readonly CommonSettings _commonSettings;
        private readonly IStoreService _storeService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAffiliateService _affiliateService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly IContactUsService _contactUsService;
        private readonly ICustomerTagService _customerTagService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IDownloadService _downloadService;

        public CustomerViewModelService(ICustomerService customerService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IGenericAttributeService genericAttributeService,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerReportService customerReportService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IRewardPointsService rewardPointsService,
            DateTimeSettings dateTimeSettings,
            TaxSettings taxSettings,
            RewardPointsSettings rewardPointsSettings,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            CustomerSettings customerSettings,
            ITaxService taxService,
            IWorkContext workContext,
            IVendorService vendorService,
            IStoreContext storeContext,
            IPriceFormatter priceFormatter,
            IOrderService orderService,
            ICustomerActivityService customerActivityService,
            IPriceCalculationService priceCalculationService,
            IProductAttributeFormatter productAttributeFormatter,
            IQueuedEmailService queuedEmailService,
            EmailAccountSettings emailAccountSettings,
            IEmailAccountService emailAccountService,
            IForumService forumService,
            IExternalAuthenticationService openAuthenticationService,
            AddressSettings addressSettings,
            CommonSettings commonSettings,
            IStoreService storeService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAffiliateService affiliateService,
            IWorkflowMessageService workflowMessageService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            IContactUsService contactUsService,
            ICustomerTagService customerTagService,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IDownloadService downloadService)
        {
            _customerService = customerService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _genericAttributeService = genericAttributeService;
            _customerRegistrationService = customerRegistrationService;
            _customerReportService = customerReportService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _rewardPointsService = rewardPointsService;
            _dateTimeSettings = dateTimeSettings;
            _taxSettings = taxSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _customerSettings = customerSettings;
            _commonSettings = commonSettings;
            _taxService = taxService;
            _workContext = workContext;
            _vendorService = vendorService;
            _storeContext = storeContext;
            _priceFormatter = priceFormatter;
            _orderService = orderService;
            _customerActivityService = customerActivityService;
            _priceCalculationService = priceCalculationService;
            _productAttributeFormatter = productAttributeFormatter;
            _queuedEmailService = queuedEmailService;
            _emailAccountSettings = emailAccountSettings;
            _emailAccountService = emailAccountService;
            _forumService = forumService;
            _openAuthenticationService = openAuthenticationService;
            _addressSettings = addressSettings;
            _storeService = storeService;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _addressAttributeParser = addressAttributeParser;
            _addressAttributeService = addressAttributeService;
            _addressAttributeFormatter = addressAttributeFormatter;
            _affiliateService = affiliateService;
            _workflowMessageService = workflowMessageService;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _contactUsService = contactUsService;
            _customerTagService = customerTagService;
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _downloadService = downloadService;
        }


        #region Utilities

        protected virtual string[] ParseCustomerTags(string customerTags)
        {
            var result = new List<string>();
            if (!String.IsNullOrWhiteSpace(customerTags))
            {
                string[] values = customerTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string val1 in values)
                    if (!String.IsNullOrEmpty(val1.Trim()))
                        result.Add(val1.Trim());
            }
            return result.ToArray();
        }

        protected virtual void SaveCustomerTags(Customer customer, string[] customerTags)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            //product tags
            var existingCustomerTags = customer.CustomerTags.ToList();
            var customerTagsToRemove = new List<CustomerTag>();
            foreach (var existingCustomerTag in existingCustomerTags)
            {
                bool found = false;
                var existingCustomerTagName = _customerTagService.GetCustomerTagById(existingCustomerTag);
                foreach (string newCustomerTag in customerTags)
                {
                    if (existingCustomerTagName.Name.Equals(newCustomerTag, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    customerTagsToRemove.Add(existingCustomerTagName);
                    _customerTagService.DeleteTagFromCustomer(existingCustomerTagName.Id, customer.Id);
                }
            }

            foreach (string customerTagName in customerTags)
            {
                CustomerTag customerTag;
                var customerTag2 = _customerTagService.GetCustomerTagByName(customerTagName);
                if (customerTag2 == null)
                {
                    customerTag = new CustomerTag
                    {
                        Name = customerTagName,
                    };
                    _customerTagService.InsertCustomerTag(customerTag);
                }
                else
                {
                    customerTag = customerTag2;
                }
                if (!customer.CustomerTags.Contains(customerTag.Id))
                {
                    _customerTagService.InsertTagToCustomer(customerTag.Id, customer.Id);
                }
            }
        }



        protected virtual string GetCustomerRolesNames(IList<CustomerRole> customerRoles, string separator = ",")
        {
            var sb = new StringBuilder();
            for (int i = 0; i < customerRoles.Count; i++)
            {
                sb.Append(customerRoles[i].Name);
                if (i != customerRoles.Count - 1)
                {
                    sb.Append(separator);
                    sb.Append(" ");
                }
            }
            return sb.ToString();
        }

        protected virtual IList<CustomerModel.AssociatedExternalAuthModel> GetAssociatedExternalAuthRecords(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var result = new List<CustomerModel.AssociatedExternalAuthModel>();
            foreach (var record in _openAuthenticationService.GetExternalIdentifiersFor(customer))
            {
                var method = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(record.ProviderSystemName);
                if (method == null)
                    continue;

                result.Add(new CustomerModel.AssociatedExternalAuthModel
                {
                    Id = record.Id,
                    Email = record.Email,
                    ExternalIdentifier = record.ExternalIdentifier,
                    AuthMethodName = method.PluginDescriptor.FriendlyName
                });
            }

            return result;
        }

        protected virtual CustomerModel PrepareCustomerModelForList(Customer customer)
        {
            return new CustomerModel
            {
                Id = customer.Id,
                Email = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest"),
                Username = customer.Username,
                FullName = customer.GetFullName(),
                Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company),
                Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode),
                CustomerRoleNames = GetCustomerRolesNames(customer.CustomerRoles.ToList()),
                Active = customer.Active,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc),
                LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc),
            };
        }

        protected virtual void PrepareVendorsModel(CustomerModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableVendors.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Customers.Customers.Fields.Vendor.None"),
                Value = ""
            });
            var vendors = _vendorService.GetAllVendors(showHidden: true);
            foreach (var vendor in vendors)
            {
                model.AvailableVendors.Add(new SelectListItem
                {
                    Text = vendor.Name,
                    Value = vendor.Id.ToString()
                });
            }
        }

        protected virtual void PrepareCustomerAttributeModel(CustomerModel model, Customer customer)
        {
            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in customerAttributes)
            {
                var attributeModel = new CustomerModel.CustomerAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.Name,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = attribute.CustomerAttributeValues;
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new CustomerModel.CustomerAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }


                //set already selected attributes
                if (customer != null)
                {
                    var selectedCustomerAttributes = customer.GetAttribute<string>(SystemCustomerAttributeNames.CustomCustomerAttributes);
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                            {
                                if (!String.IsNullOrEmpty(selectedCustomerAttributes))
                                {
                                    //clear default selection
                                    foreach (var item in attributeModel.Values)
                                        item.IsPreSelected = false;

                                    //select new values
                                    var selectedValues = _customerAttributeParser.ParseCustomerAttributeValues(selectedCustomerAttributes);
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
                                if (!String.IsNullOrEmpty(selectedCustomerAttributes))
                                {
                                    var enteredText = _customerAttributeParser.ParseValues(selectedCustomerAttributes, attribute.Id);
                                    if (enteredText.Count > 0)
                                        attributeModel.DefaultValue = enteredText[0];
                                }
                            }
                            break;
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                        case AttributeControlType.FileUpload:
                        default:
                            //not supported attribute control types
                            break;
                    }
                }

                model.CustomerAttributes.Add(attributeModel);
            }
        }

        #endregion

        public virtual CustomerListModel PrepareCustomerListModel()
        {
            var model = new CustomerListModel
            {
                UsernamesEnabled = _customerSettings.UsernamesEnabled,
                CompanyEnabled = _customerSettings.CompanyEnabled,
                PhoneEnabled = _customerSettings.PhoneEnabled,
                ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled,
                AvailableCustomerRoles = _customerService.GetAllCustomerRoles(true).Select(cr => new SelectListItem() { Text = cr.Name, Value = cr.Id.ToString(), Selected = (cr.Id == _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Id) }).ToList(),
                AvailableCustomerTags = _customerTagService.GetAllCustomerTags().Select(ct => new SelectListItem() { Text = ct.Name, Value = ct.Id.ToString() }).ToList(),
                SearchCustomerRoleIds = new List<string> { _customerService.GetAllCustomerRoles(true).FirstOrDefault(x => x.Id == _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Id).Id },
            };
            return model;
        }

        public virtual (IEnumerable<CustomerModel> customerModelList, int totalCount) PrepareCustomerList(CustomerListModel model,
            string[] searchCustomerRoleIds, string[] searchCustomerTagIds, int pageIndex, int pageSize)
        {
            var customers = _customerService.GetAllCustomers(
                customerRoleIds: searchCustomerRoleIds,
                customerTagIds: searchCustomerTagIds,
                email: model.SearchEmail,
                username: model.SearchUsername,
                firstName: model.SearchFirstName,
                lastName: model.SearchLastName,
                company: model.SearchCompany,
                phone: model.SearchPhone,
                zipPostalCode: model.SearchZipPostalCode,
                loadOnlyWithShoppingCart: false,
                pageIndex: pageIndex - 1,
                pageSize: pageSize);

            return (customers.Select(PrepareCustomerModelForList),
                    customers.TotalCount);
        }

        public virtual void PrepareCustomerModel(CustomerModel model, Customer customer, bool excludeProperties)
        {
            var allStores = _storeService.GetAllStores();
            if (customer != null)
            {
                model.Id = customer.Id;
                model.ShowMessageContactForm = _commonSettings.StoreInDatabaseContactUsForm;
                if (!excludeProperties)
                {
                    model.Email = customer.Email;
                    model.Username = customer.Username;
                    model.VendorId = customer.VendorId;
                    model.AdminComment = customer.AdminComment;
                    model.IsTaxExempt = customer.IsTaxExempt;
                    model.FreeShipping = customer.FreeShipping;
                    model.Active = customer.Active;
                    var result = new StringBuilder();
                    foreach (var item in customer.CustomerTags)
                    {
                        var ct = _customerTagService.GetCustomerTagById(item);
                        result.Append(ct.Name);
                        result.Append(", ");
                    }
                    model.CustomerTags = result.ToString();
                    var affiliate = _affiliateService.GetAffiliateById(customer.AffiliateId);
                    if (affiliate != null)
                    {
                        model.AffiliateId = affiliate.Id;
                        model.AffiliateName = affiliate.GetFullName();
                    }

                    model.TimeZoneId = customer.GetAttribute<string>(SystemCustomerAttributeNames.TimeZoneId);
                    model.VatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);
                    model.VatNumberStatusNote = ((VatNumberStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId))
                        .GetLocalizedEnum(_localizationService, _workContext);
                    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc);
                    model.LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);
                    if (customer.LastPurchaseDateUtc.HasValue)
                        model.LastPurchaseDate = _dateTimeHelper.ConvertToUserTime(customer.LastPurchaseDateUtc.Value, DateTimeKind.Utc);
                    model.LastIpAddress = customer.LastIpAddress;
                    model.UrlReferrer = customer.UrlReferrer;
                    model.LastVisitedPage = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastVisitedPage);
                    model.LastUrlReferrer = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastUrlReferrer);

                    model.SelectedCustomerRoleIds = customer.CustomerRoles.Select(cr => cr.Id).ToArray();
                    //newsletter subscriptions
                    if (!String.IsNullOrEmpty(customer.Email))
                    {
                        var newsletterSubscriptionStoreIds = new List<string>();
                        foreach (var store in allStores)
                        {
                            var newsletterSubscription = _newsLetterSubscriptionService
                                .GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                            if (newsletterSubscription != null)
                                newsletterSubscriptionStoreIds.Add(store.Id);
                            model.SelectedNewsletterSubscriptionStoreIds = newsletterSubscriptionStoreIds.ToArray();
                        }
                    }


                    //form fields
                    model.FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                    model.LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                    model.Gender = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
                    model.DateOfBirth = customer.GetAttribute<DateTime?>(SystemCustomerAttributeNames.DateOfBirth);
                    model.Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company);
                    model.StreetAddress = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress);
                    model.StreetAddress2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2);
                    model.ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode);
                    model.City = customer.GetAttribute<string>(SystemCustomerAttributeNames.City);
                    model.CountryId = customer.GetAttribute<string>(SystemCustomerAttributeNames.CountryId);
                    model.StateProvinceId = customer.GetAttribute<string>(SystemCustomerAttributeNames.StateProvinceId);
                    model.Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                    model.Fax = customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax);
                }
            }

            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.AllowUsersToChangeUsernames = _customerSettings.AllowUsersToChangeUsernames;
            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (tzi.Id == model.TimeZoneId) });
            if (customer != null)
            {
                model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            }
            else
            {
                model.DisplayVatNumber = false;
            }

            //vendors
            PrepareVendorsModel(model);
            //customer attributes
            PrepareCustomerAttributeModel(model, customer);

            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.FaxEnabled = _customerSettings.FaxEnabled;

            //countries and states
            if (_customerSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
                foreach (var c in _countryService.GetAllCountries(showHidden: true))
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_customerSettings.StateProvinceEnabled)
                {
                    //states
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId).ToList();
                    if (states.Count > 0)
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectState"), Value = "" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                        }
                    }
                    else
                    {
                        bool anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = _localizationService.GetResource(anyCountrySelected ? "Admin.Address.OtherNonUS" : "Admin.Address.SelectState"),
                            Value = ""
                        });
                    }
                }
            }

            //newsletter subscriptions
            model.AvailableNewsletterSubscriptionStores = allStores
                .Select(s => new StoreModel() { Id = s.Id, Name = s.Name })
                .ToList();


            //customer roles
            model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();

            if (model.SelectedCustomerRoleIds == null && customer == null && model.AvailableCustomerRoles.Count > 0)
            {
                model.SelectedCustomerRoleIds = new[] {model.AvailableCustomerRoles
                     .FirstOrDefault(c=>c.SystemName==SystemCustomerRoleNames.Registered).Id };
            }

            if (customer != null)
            {
                //reward points history
                model.DisplayRewardPointsHistory = _rewardPointsSettings.Enabled;
                model.AddRewardPointsValue = 0;
                model.AddRewardPointsMessage = "Some comment here...";

                //stores
                foreach (var store in allStores)
                {
                    model.RewardPointsAvailableStores.Add(new SelectListItem
                    {
                        Text = store.Name,
                        Value = store.Id.ToString(),
                        Selected = (store.Id == _storeContext.CurrentStore.Id)
                    });
                }

                //external authentication records
                model.AssociatedExternalAuthRecords = GetAssociatedExternalAuthRecords(customer);

            }
            else
            {
                model.DisplayRewardPointsHistory = false;
            }

            //sending of the welcome message:
            //1. "admin approval" registration method
            //2. already created customer
            //3. registered
            model.AllowSendingOfWelcomeMessage = _customerSettings.UserRegistrationType == UserRegistrationType.AdminApproval &&
                customer != null &&
                customer.IsRegistered();
            //sending of the activation message
            //1. "email validation" registration method
            //2. already created customer
            //3. registered
            //4. not active
            model.AllowReSendingOfActivationMessage = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation &&
                customer != null &&
                customer.IsRegistered() &&
                !customer.Active;
        }

        public virtual string ValidateCustomerRoles(IList<CustomerRole> customerRoles)
        {
            if (customerRoles == null)
                throw new ArgumentNullException("customerRoles");

            //ensure a customer is not added to both 'Guests' and 'Registered' customer roles
            //ensure that a customer is in at least one required role ('Guests' and 'Registered')
            bool isInGuestsRole = customerRoles.FirstOrDefault(cr => cr.SystemName == SystemCustomerRoleNames.Guests) != null;
            bool isInRegisteredRole = customerRoles.FirstOrDefault(cr => cr.SystemName == SystemCustomerRoleNames.Registered) != null;
            if (isInGuestsRole && isInRegisteredRole)
                return "The customer cannot be in both 'Guests' and 'Registered' customer roles";
            if (!isInGuestsRole && !isInRegisteredRole)
                return "Add the customer to 'Guests' or 'Registered' customer role";

            //no errors
            return "";
        }
        public virtual Customer InsertCustomerModel(CustomerModel model)
        {
            var customer = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Email = model.Email,
                Username = model.Username,
                VendorId = model.VendorId,
                AdminComment = model.AdminComment,
                IsTaxExempt = model.IsTaxExempt,
                FreeShipping = model.FreeShipping,
                Active = model.Active,
                StoreId = _storeContext.CurrentStore.Id,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };
            _customerService.InsertCustomer(customer);

            //form fields
            if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
            if (_customerSettings.GenderEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
            if (_customerSettings.DateOfBirthEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, model.DateOfBirth);
            if (_customerSettings.CompanyEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
            if (_customerSettings.StreetAddressEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
            if (_customerSettings.StreetAddress2Enabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
            if (_customerSettings.ZipPostalCodeEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
            if (_customerSettings.CityEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
            if (_customerSettings.CountryEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
            if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId, model.StateProvinceId);
            if (_customerSettings.PhoneEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
            if (_customerSettings.FaxEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);

            //custom customer attributes
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, model.CustomAttributes);

            //newsletter subscriptions
            if (!String.IsNullOrEmpty(customer.Email))
            {
                var allStores = _storeService.GetAllStores();
                foreach (var store in allStores)
                {
                    var newsletterSubscription = _newsLetterSubscriptionService
                        .GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                    if (model.SelectedNewsletterSubscriptionStoreIds != null &&
                        model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id))
                    {
                        //subscribed
                        if (newsletterSubscription == null)
                        {
                            _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                            {
                                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                CustomerId = customer.Id,
                                Email = customer.Email,
                                Active = true,
                                StoreId = store.Id,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                        }
                    }
                    else
                    {
                        //not subscribed
                        if (newsletterSubscription != null)
                        {
                            _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletterSubscription);
                        }
                    }
                }
            }

            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            var newCustomerRoles = new List<CustomerRole>();
            foreach (var customerRole in allCustomerRoles)
                if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    newCustomerRoles.Add(customerRole);

            //customer roles
            foreach (var customerRole in newCustomerRoles)
            {
                //ensure that the current customer cannot add to "Administrators" system role if he's not an admin himself
                if (customerRole.SystemName == SystemCustomerRoleNames.Administrators &&
                    !_workContext.CurrentCustomer.IsAdmin())
                    continue;

                customer.CustomerRoles.Add(customerRole);
                customerRole.CustomerId = customer.Id;
                _customerService.InsertCustomerRoleInCustomer(customerRole);
            }


            //ensure that a customer with a vendor associated is not in "Administrators" role
            //otherwise, he won't be have access to the other functionality in admin area
            if (customer.IsAdmin() && !String.IsNullOrEmpty(customer.VendorId))
            {
                customer.VendorId = "";
                _customerService.UpdateCustomerVendor(customer);
            }

            //ensure that a customer in the Vendors role has a vendor account associated.
            //otherwise, he will have access to ALL products
            if (customer.IsVendor() && !String.IsNullOrEmpty(customer.VendorId))
            {
                var vendorRole = customer
                    .CustomerRoles
                    .FirstOrDefault(x => x.SystemName == SystemCustomerRoleNames.Vendors);
                customer.CustomerRoles.Remove(vendorRole);
                vendorRole.CustomerId = customer.Id;
                _customerService.DeleteCustomerRoleInCustomer(vendorRole);
            }

            //tags
            SaveCustomerTags(customer, ParseCustomerTags(model.CustomerTags));

            //activity log
            _customerActivityService.InsertActivity("AddNewCustomer", customer.Id, _localizationService.GetResource("ActivityLog.AddNewCustomer"), customer.Id);

            return customer;
        }

        public virtual Customer UpdateCustomerModel(Customer customer, CustomerModel model)
        {
            customer.AdminComment = model.AdminComment;
            customer.IsTaxExempt = model.IsTaxExempt;
            customer.FreeShipping = model.FreeShipping;
            customer.Active = model.Active;
            //email
            if (!String.IsNullOrWhiteSpace(model.Email))
            {
                _customerRegistrationService.SetEmail(customer, model.Email);
            }
            else
            {
                customer.Email = model.Email;
            }

            //username
            if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
            {
                if (!String.IsNullOrWhiteSpace(model.Username))
                {
                    _customerRegistrationService.SetUsername(customer, model.Username);
                }
                else
                {
                    customer.Username = model.Username;
                }
            }

            //VAT number
            if (_taxSettings.EuVatEnabled)
            {
                var prevVatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);

                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);
                //set VAT number status
                if (!String.IsNullOrEmpty(model.VatNumber))
                {
                    if (!model.VatNumber.Equals(prevVatNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        _genericAttributeService.SaveAttribute(customer,
                            SystemCustomerAttributeNames.VatNumberStatusId,
                            (int)_taxService.GetVatNumberStatus(model.VatNumber));
                    }
                }
                else
                {
                    _genericAttributeService.SaveAttribute(customer,
                        SystemCustomerAttributeNames.VatNumberStatusId,
                        (int)VatNumberStatus.Empty);
                }
            }

            //vendor
            customer.VendorId = model.VendorId;

            //form fields
            if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
            if (_customerSettings.GenderEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
            if (_customerSettings.DateOfBirthEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, model.DateOfBirth);
            if (_customerSettings.CompanyEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
            if (_customerSettings.StreetAddressEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
            if (_customerSettings.StreetAddress2Enabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
            if (_customerSettings.ZipPostalCodeEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
            if (_customerSettings.CityEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
            if (_customerSettings.CountryEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
            if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId, model.StateProvinceId);
            if (_customerSettings.PhoneEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
            if (_customerSettings.FaxEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);

            //custom customer attributes
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, model.CustomAttributes);

            //newsletter subscriptions
            if (!String.IsNullOrEmpty(customer.Email))
            {
                var allStores = _storeService.GetAllStores();
                foreach (var store in allStores)
                {
                    var newsletterSubscription = _newsLetterSubscriptionService
                        .GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                    if (model.SelectedNewsletterSubscriptionStoreIds != null &&
                        model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id))
                    {
                        //subscribed
                        if (newsletterSubscription == null)
                        {
                            _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                            {
                                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                CustomerId = customer.Id,
                                Email = customer.Email,
                                Active = true,
                                StoreId = store.Id,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                        }
                    }
                    else
                    {
                        //not subscribed
                        if (newsletterSubscription != null)
                        {
                            _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletterSubscription);
                        }
                    }
                }
            }
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            var newCustomerRoles = new List<CustomerRole>();
            foreach (var customerRole in allCustomerRoles)
                if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    newCustomerRoles.Add(customerRole);
            //customer roles
            foreach (var customerRole in allCustomerRoles)
            {
                //ensure that the current customer cannot add/remove to/from "Administrators" system role
                //if he's not an admin himself
                if (customerRole.SystemName == SystemCustomerRoleNames.Administrators &&
                    !_workContext.CurrentCustomer.IsAdmin())
                    continue;

                if (model.SelectedCustomerRoleIds != null &&
                    model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (customer.CustomerRoles.Count(cr => cr.Id == customerRole.Id) == 0)
                    {
                        customer.CustomerRoles.Add(customerRole);
                    }
                }
                else
                {
                    //remove role
                    if (customer.CustomerRoles.Count(cr => cr.Id == customerRole.Id) > 0)
                        customer.CustomerRoles.Remove(customer.CustomerRoles.First(x=>x.Id == customerRole.Id));
                }
            }
            _customerService.UpdateCustomerinAdminPanel(customer);


            //ensure that a customer with a vendor associated is not in "Administrators" role
            //otherwise, he won't have access to the other functionality in admin area
            if (customer.IsAdmin() && !String.IsNullOrEmpty(customer.VendorId))
            {
                customer.VendorId = "";
                _customerService.UpdateCustomerinAdminPanel(customer);
            }

            //ensure that a customer in the Vendors role has a vendor account associated.
            //otherwise, he will have access to ALL products
            if (customer.IsVendor() && String.IsNullOrEmpty(customer.VendorId))
            {
                var vendorRole = customer
                    .CustomerRoles
                    .FirstOrDefault(x => x.SystemName == SystemCustomerRoleNames.Vendors);
                customer.CustomerRoles.Remove(vendorRole);
                vendorRole.CustomerId = customer.Id;
                _customerService.DeleteCustomerRoleInCustomer(vendorRole);
            }

            //tags
            SaveCustomerTags(customer, ParseCustomerTags(model.CustomerTags));

            //activity log
            _customerActivityService.InsertActivity("EditCustomer", customer.Id, _localizationService.GetResource("ActivityLog.EditCustomer"), customer.Id);
            return customer;
        }

        public virtual void DeleteCustomer(Customer customer)
        {
            _customerService.DeleteCustomer(customer);

            //remove newsletter subscription (if exists)
            foreach (var store in _storeService.GetAllStores())
            {
                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                if (subscription != null)
                    _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);
            }

            //activity log
            _customerActivityService.InsertActivity("DeleteCustomer", customer.Id, _localizationService.GetResource("ActivityLog.DeleteCustomer"), customer.Id);
        }
        public void SendEmail(Customer customer, CustomerModel.SendEmailModel model)
        {
            var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
            if (emailAccount == null)
                throw new GrandException("Email account can't be loaded");

            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                EmailAccountId = emailAccount.Id,
                FromName = emailAccount.DisplayName,
                From = emailAccount.Email,
                ToName = customer.GetFullName(),
                To = customer.Email,
                Subject = model.Subject,
                Body = model.Body,
                CreatedOnUtc = DateTime.UtcNow,
                DontSendBeforeDateUtc = (model.SendImmediately || !model.DontSendBeforeDate.HasValue) ?
                        null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DontSendBeforeDate.Value)
            };
            _queuedEmailService.InsertQueuedEmail(email);
            _customerActivityService.InsertActivity("CustomerAdmin.SendEmail", "", _localizationService.GetResource("ActivityLog.SendEmailfromAdminPanel"), customer, model.Subject);
        }
        public virtual void SendPM(Customer customer, CustomerModel.SendPmModel model)
        {
            var privateMessage = new PrivateMessage
            {
                StoreId = _storeContext.CurrentStore.Id,
                ToCustomerId = customer.Id,
                FromCustomerId = _workContext.CurrentCustomer.Id,
                Subject = model.Subject,
                Text = model.Message,
                IsDeletedByAuthor = false,
                IsDeletedByRecipient = false,
                IsRead = false,
                CreatedOnUtc = DateTime.UtcNow
            };

            _forumService.InsertPrivateMessage(privateMessage);
            _customerActivityService.InsertActivity("CustomerAdmin.SendPM", "", _localizationService.GetResource("ActivityLog.SendPMfromAdminPanel"), customer, model.Subject);
        }
        public virtual IEnumerable<CustomerModel.RewardPointsHistoryModel> PrepareRewardPointsHistoryModel(string customerId)
        {
            var model = new List<CustomerModel.RewardPointsHistoryModel>();
            foreach (var rph in _rewardPointsService.GetRewardPointsHistory(customerId, true))
            {
                var store = _storeService.GetStoreById(rph.StoreId);
                model.Add(new CustomerModel.RewardPointsHistoryModel
                {
                    StoreName = store != null ? store.Name : "Unknown",
                    Points = rph.Points,
                    PointsBalance = rph.PointsBalance,
                    Message = rph.Message,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(rph.CreatedOnUtc, DateTimeKind.Utc)
                });
            }
            return model;
        }

        public virtual RewardPointsHistory InsertRewardPointsHistory(string customerId, string storeId, int addRewardPointsValue, string addRewardPointsMessage)
        {
            return _rewardPointsService.AddRewardPointsHistory(customerId, addRewardPointsValue, storeId, addRewardPointsMessage);
        }

        public virtual IEnumerable<AddressModel> PrepareAddressModel(Customer customer)
        {
            var addresses = customer.Addresses.OrderByDescending(a => a.CreatedOnUtc).ThenByDescending(a => a.Id).ToList();
            return addresses.Select(x =>
            {
                var model = x.ToModel();
                var addressHtmlSb = new StringBuilder("<div>");
                if (_addressSettings.CompanyEnabled && !String.IsNullOrEmpty(model.Company))
                    addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Company));
                if (_addressSettings.StreetAddressEnabled && !String.IsNullOrEmpty(model.Address1))
                    addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Address1));
                if (_addressSettings.StreetAddress2Enabled && !String.IsNullOrEmpty(model.Address2))
                    addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Address2));
                if (_addressSettings.CityEnabled && !String.IsNullOrEmpty(model.City))
                    addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.City));
                if (_addressSettings.StateProvinceEnabled && !String.IsNullOrEmpty(model.StateProvinceName))
                    addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.StateProvinceName));
                if (_addressSettings.ZipPostalCodeEnabled && !String.IsNullOrEmpty(model.ZipPostalCode))
                    addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.ZipPostalCode));
                if (_addressSettings.CountryEnabled && !String.IsNullOrEmpty(model.CountryName))
                    addressHtmlSb.AppendFormat("{0}", WebUtility.HtmlEncode(model.CountryName));
                var customAttributesFormatted = _addressAttributeFormatter.FormatAttributes(x.CustomAttributes);
                if (!String.IsNullOrEmpty(customAttributesFormatted))
                {
                    //already encoded
                    addressHtmlSb.AppendFormat("<br />{0}", customAttributesFormatted);
                }
                addressHtmlSb.Append("</div>");
                model.AddressHtml = addressHtmlSb.ToString();
                return model;
            });
        }
        public void DeleteAddress(Customer customer, Address address)
        {
            customer.RemoveAddress(address);
            _customerService.UpdateCustomerinAdminPanel(customer);
        }

        public virtual Address InsertAddressModel(Customer customer, CustomerAddressModel model, string customAttributes)
        {
            var address = model.Address.ToEntity();
            address.CustomAttributes = customAttributes;
            address.CreatedOnUtc = DateTime.UtcNow;
            customer.Addresses.Add(address);
            _customerService.UpdateCustomerinAdminPanel(customer);
            return address;

        }
        public virtual void PrepareAddressModel(CustomerAddressModel model, Address address, Customer customer, bool excludeProperties)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            model.CustomerId = customer.Id;
            if (address != null)
            {
                if (!excludeProperties)
                {
                    model.Address = address.ToModel();
                }
            }

            if (model.Address == null)
                model.Address = new AddressModel();

            model.Address.FirstNameEnabled = true;
            model.Address.FirstNameRequired = true;
            model.Address.LastNameEnabled = true;
            model.Address.LastNameRequired = true;
            model.Address.EmailEnabled = true;
            model.Address.EmailRequired = true;
            model.Address.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.Address.CompanyRequired = _addressSettings.CompanyRequired;
            model.Address.VatNumberEnabled = _addressSettings.VatNumberEnabled;
            model.Address.VatNumberRequired = _addressSettings.VatNumberRequired;
            model.Address.CountryEnabled = _addressSettings.CountryEnabled;
            model.Address.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.Address.CityEnabled = _addressSettings.CityEnabled;
            model.Address.CityRequired = _addressSettings.CityRequired;
            model.Address.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.Address.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.Address.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.Address.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.Address.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.Address.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.Address.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.Address.PhoneRequired = _addressSettings.PhoneRequired;
            model.Address.FaxEnabled = _addressSettings.FaxEnabled;
            model.Address.FaxRequired = _addressSettings.FaxRequired;
            //countries
            model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.Address.CountryId) });
            //states
            var states = !String.IsNullOrEmpty(model.Address.CountryId) ? _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId, showHidden: true).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.Address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "" });
            //customer attribute services
            model.Address.PrepareCustomAddressAttributes(address, _addressAttributeService, _addressAttributeParser);
        }

        public virtual Address UpdateAddressModel(Customer customer, Address address, CustomerAddressModel model, string customAttributes)
        {
            address = model.Address.ToEntity(address);
            address.CustomAttributes = customAttributes;
            _customerService.UpdateCustomerinAdminPanel(customer);
            return address;
        }

        public virtual (IEnumerable<CustomerModel.OrderModel> orderModels, int totalCount) PrepareOrderModel(string customerId, int pageIndex, int pageSize)
        {
            var orders = _orderService.SearchOrders(customerId: customerId, pageIndex: pageIndex - 1, pageSize: pageIndex);
            return (orders.Select(order =>
            {
                var store = _storeService.GetStoreById(order.StoreId);
                var orderModel = new CustomerModel.OrderModel
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                    PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
                    ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
                    OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false),
                    StoreName = store != null ? store.Name : "Unknown",
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
                };
                return orderModel;
            }), orders.TotalCount);

        }
        public virtual CustomerReportsModel PrepareCustomerReportsModel()
        {
            var model = new CustomerReportsModel();
            //customers by number of orders
            model.BestCustomersByNumberOfOrders = new BestCustomersReportModel();
            model.BestCustomersByNumberOfOrders.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.BestCustomersByNumberOfOrders.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            model.BestCustomersByNumberOfOrders.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.BestCustomersByNumberOfOrders.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            model.BestCustomersByNumberOfOrders.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();
            model.BestCustomersByNumberOfOrders.AvailableShippingStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            //customers by order total
            model.BestCustomersByOrderTotal = new BestCustomersReportModel();
            model.BestCustomersByOrderTotal.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.BestCustomersByOrderTotal.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            model.BestCustomersByOrderTotal.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.BestCustomersByOrderTotal.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            model.BestCustomersByOrderTotal.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();
            model.BestCustomersByOrderTotal.AvailableShippingStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            return model;
        }

        public virtual IList<RegisteredCustomerReportLineModel> GetReportRegisteredCustomersModel()
        {
            var report = new List<RegisteredCustomerReportLineModel>();
            report.Add(new RegisteredCustomerReportLineModel
            {
                Period = _localizationService.GetResource("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.7days"),
                Customers = _customerReportService.GetRegisteredCustomersReport(7)
            });

            report.Add(new RegisteredCustomerReportLineModel
            {
                Period = _localizationService.GetResource("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.14days"),
                Customers = _customerReportService.GetRegisteredCustomersReport(14)
            });
            report.Add(new RegisteredCustomerReportLineModel
            {
                Period = _localizationService.GetResource("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.month"),
                Customers = _customerReportService.GetRegisteredCustomersReport(30)
            });
            report.Add(new RegisteredCustomerReportLineModel
            {
                Period = _localizationService.GetResource("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.year"),
                Customers = _customerReportService.GetRegisteredCustomersReport(365)
            });

            return report;
        }

        public virtual (IEnumerable<BestCustomerReportLineModel> bestCustomerReportLineModels, int totalCount) PrepareBestCustomerReportLineModel(BestCustomersReportModel model, int orderBy, int pageIndex, int pageSize)
        {
            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;

            var items = _customerReportService.GetBestCustomersReport(startDateValue, endDateValue,
                orderStatus, paymentStatus, shippingStatus, 2, pageIndex - 1, pageSize);
            return (items.Select(x =>
            {
                var m = new BestCustomerReportLineModel
                {
                    CustomerId = x.CustomerId,
                    OrderTotal = _priceFormatter.FormatPrice(x.OrderTotal, true, false),
                    OrderCount = x.OrderCount,
                };
                var customer = _customerService.GetCustomerById(x.CustomerId);
                if (customer != null)
                {
                    m.CustomerName = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                }
                return m;
            }), items.TotalCount);
        }
        public virtual IList<ShoppingCartItemModel> PrepareShoppingCartItemModel(string customerId, int cartTypeId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            var cart = customer.ShoppingCartItems.Where(x => x.ShoppingCartTypeId == cartTypeId).ToList();

            return cart.Select(sci =>
            {
                decimal taxRate;
                var store = _storeService.GetStoreById(sci.StoreId);
                var product = _productService.GetProductById(sci.ProductId);
                var sciModel = new ShoppingCartItemModel
                {
                    Id = sci.Id,
                    Store = store != null ? store.Name : "Unknown",
                    ProductId = sci.ProductId,
                    Quantity = sci.Quantity,
                    ProductName = product.Name,
                    AttributeInfo = _productAttributeFormatter.FormatAttributes(product, sci.AttributesXml),
                    UnitPrice = _priceFormatter.FormatPrice(_taxService.GetProductPrice(product, _priceCalculationService.GetUnitPrice(sci), out taxRate)),
                    Total = _priceFormatter.FormatPrice(_taxService.GetProductPrice(product, _priceCalculationService.GetSubTotal(sci), out taxRate)),
                    UpdatedOn = _dateTimeHelper.ConvertToUserTime(sci.UpdatedOnUtc, DateTimeKind.Utc)
                };
                return sciModel;
            }).ToList();
        }
        public virtual void DeleteCart(Customer customer, string id)
        {
            var cart = customer.ShoppingCartItems.FirstOrDefault(a => a.Id == id);
            if (cart != null)
            {
                EngineContext.Current.Resolve<IShoppingCartService>()
                    .DeleteShoppingCartItem(_workContext.CurrentCustomer, cart, ensureOnlyActiveCheckoutAttributes: true);
                _customerService.UpdateCustomerinAdminPanel(customer);
            }
        }
        public virtual (IEnumerable<CustomerModel.ProductPriceModel> productPriceModels, int totalCount) PrepareProductPriceModel(string customerId, int pageIndex, int pageSize)
        {
            var productPrices = _customerService.GetProductsPriceByCustomer(customerId, pageIndex - 1, pageSize);
            return (productPrices.Select(x =>
            {
                var m = new CustomerModel.ProductPriceModel
                {
                    Id = x.Id,
                    Price = x.Price,
                    ProductId = x.ProductId,
                    ProductName = _productService.GetProductById(x.ProductId)?.Name
                };
                return m;
            }), productPrices.TotalCount);
        }
        public virtual (IEnumerable<CustomerModel.ProductModel> productModels, int totalCount) PreparePersonalizedProducts(string customerId, int pageIndex, int pageSize)
        {
            var products = _customerService.GetProductsByCustomer(customerId, pageIndex - 1, pageSize);
            return (products.Select(x =>
            {
                var m = new CustomerModel.ProductModel
                {
                    Id = x.Id,
                    DisplayOrder = x.DisplayOrder,
                    ProductId = x.ProductId,
                    ProductName = _productService.GetProductById(x.ProductId)?.Name
                };
                return m;
            }), products.TotalCount);
        }
        public virtual CustomerModel.AddProductModel PrepareCustomerModelAddProductModel()
        {
            var model = new CustomerModel.AddProductModel();
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });

            return model;
        }

        public virtual (IList<ProductModel> products, int totalCount) PrepareProductModel(CustomerModel.AddProductModel model, int pageIndex, int pageSize)
        {
            var products = _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel()).ToList(), products.TotalCount);
        }

        public virtual void InsertCustomerAddProductModel(string customerId, bool personalized, CustomerModel.AddProductModel model)
        {
            foreach (string id in model.SelectedProductIds)
            {
                var product = _productService.GetProductById(id);
                if (product != null)
                {
                    if (!personalized)
                    {
                        if (!_customerService.GetPriceByCustomerProduct(customerId, id).HasValue)
                        {
                            _customerService.InsertCustomerProductPrice(new CustomerProductPrice() { CustomerId = customerId, ProductId = id, Price = product.Price });
                        }
                    }
                    else
                    {
                        if (_customerService.GetCustomerProduct(customerId, id) == null)
                        {
                            _customerService.InsertCustomerProduct(new CustomerProduct() { CustomerId = customerId, ProductId = id, DisplayOrder = 0 });
                        }

                    }
                }
            }
        }
        public virtual void UpdateProductPrice(CustomerModel.ProductPriceModel model)
        {
            var productPrice = _customerService.GetCustomerProductPriceById(model.Id);
            if (productPrice != null)
            {
                productPrice.Price = model.Price;
                _customerService.UpdateCustomerProductPrice(productPrice);
            }
        }
        public virtual void DeleteProductPrice(string id)
        {
            var productPrice = _customerService.GetCustomerProductPriceById(id);
            if (productPrice == null)
                throw new ArgumentException("No productPrice found with the specified id");

            _customerService.DeleteCustomerProductPrice(productPrice);
        }
        public virtual void UpdatePersonalizedProduct(CustomerModel.ProductModel model)
        {
            var customerproduct = _customerService.GetCustomerProduct(model.Id);
            if (customerproduct != null)
            {
                customerproduct.DisplayOrder = model.DisplayOrder;
                _customerService.UpdateCustomerProduct(customerproduct);
            }
        }
        public virtual void DeletePersonalizedProduct(string id)
        {
            var customerproduct = _customerService.GetCustomerProduct(id);
            if (customerproduct == null)
                throw new ArgumentException("No customerproduct found with the specified id");

            _customerService.DeleteCustomerProduct(customerproduct);
        }
        public virtual (IEnumerable<CustomerModel.ActivityLogModel> activityLogModels, int totalCount) PrepareActivityLogModel(string customerId, int pageIndex, int pageSize)
        {
            var activityLog = _customerActivityService.GetAllActivities(null, null, customerId, "", null, pageIndex - 1, pageSize);
            return (activityLog.Select(x =>
            {
                var m = new CustomerModel.ActivityLogModel
                {
                    Id = x.Id,
                    ActivityLogTypeName = _customerActivityService.GetActivityTypeById(x.ActivityLogTypeId)?.Name,
                    Comment = x.Comment,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                    IpAddress = x.IpAddress
                };
                return m;
            }), activityLog.TotalCount);
        }
        public virtual (IEnumerable<ContactFormModel> contactFormModels, int totalCount) PrepareContactFormModel(string customerId, string vendorId, int pageIndex, int pageSize)
        {
            var contactform = _contactUsService.GetAllContactUs(storeId: "", vendorId: vendorId, customerId: customerId, pageIndex: pageIndex - 1, pageSize: pageSize);
            return (contactform.Select(x =>
            {
                var store = _storeService.GetStoreById(x.StoreId);
                var m = x.ToModel();
                m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                m.Enquiry = "";
                m.Email = m.FullName + " - " + m.Email;
                m.Store = store != null ? store.Name : "-empty-";
                return m;
            }), contactform.TotalCount);
        }
        public virtual (IEnumerable<CustomerModel.BackInStockSubscriptionModel> backInStockSubscriptionModels, int totalCount) PrepareBackInStockSubscriptionModel(string customerId, int pageIndex, int pageSize)
        {
            var subscriptions = _backInStockSubscriptionService.GetAllSubscriptionsByCustomerId(customerId, "", pageIndex - 1, pageSize);
            return (subscriptions.Select(x =>
            {
                var store = _storeService.GetStoreById(x.StoreId);
                var product = EngineContext.Current.Resolve<IProductService>().GetProductById(x.ProductId);
                var m = new CustomerModel.BackInStockSubscriptionModel
                {
                    Id = x.Id,
                    StoreName = store != null ? store.Name : "Unknown",
                    ProductId = x.ProductId,
                    ProductName = product != null ? product.Name : "Unknown",
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                };
                return m;

            }), subscriptions.TotalCount);
        }
        public virtual IList<CustomerModel.CustomerNote> PrepareCustomerNoteList(string customerId)
        {
            var customerNoteModels = new List<CustomerModel.CustomerNote>();
            foreach (var customerNote in _customerService.GetCustomerNotes(customerId)
                .OrderByDescending(on => on.CreatedOnUtc))
            {
                var download = _downloadService.GetDownloadById(customerNote.DownloadId);
                customerNoteModels.Add(new CustomerModel.CustomerNote
                {
                    Id = customerNote.Id,
                    CustomerId = customerId,
                    DownloadId = String.IsNullOrEmpty(customerNote.DownloadId) ? "" : customerNote.DownloadId,
                    DownloadGuid = download != null ? download.DownloadGuid : Guid.Empty,
                    DisplayToCustomer = customerNote.DisplayToCustomer,
                    Title = customerNote.Title,
                    Note = customerNote.FormatCustomerNoteText(),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(customerNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }
            return customerNoteModels;
        }
        public virtual CustomerNote InsertCustomerNote(string customerId, string downloadId, bool displayToCustomer, string title, string message)
        {
            var customerNote = new CustomerNote
            {
                DisplayToCustomer = displayToCustomer,
                Title = title,
                Note = message,
                DownloadId = downloadId,
                CustomerId = customerId,
                CreatedOnUtc = DateTime.UtcNow,
            };
            _customerService.InsertCustomerNote(customerNote);

            //new customer note notification
            if (displayToCustomer)
            {
                //email
                _workflowMessageService.SendNewCustomerNoteAddedCustomerNotification(
                    customerNote, _workContext.WorkingLanguage.Id);

            }
            return customerNote;
        }
        public virtual void DeleteCustomerNote(string id, string customerId)
        {
            var customerNote = _customerService.GetCustomerNote(id);
            if (customerNote == null)
                throw new ArgumentException("No customer note found with the specified id");

            _customerService.DeleteCustomerNote(customerNote);

        }
    }
}
