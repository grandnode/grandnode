using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Forums;
using Grand.Domain.Messages;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using Grand.Domain.Tax;
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
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class CustomerViewModelService : ICustomerViewModelService
    {
        private readonly ICustomerService _customerService;
        private readonly ICustomerProductService _customerProductService;
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
        private readonly IServiceProvider _serviceProvider;

        public CustomerViewModelService(
            ICustomerService customerService,
            ICustomerProductService customerProductService,
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
            IDownloadService downloadService,
            IServiceProvider serviceProvider)
        {
            _customerService = customerService;
            _customerProductService = customerProductService;
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
            _serviceProvider = serviceProvider;
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

        protected virtual async Task SaveCustomerTags(Customer customer, string[] customerTags)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            //product tags
            var existingCustomerTags = customer.CustomerTags.ToList();
            var customerTagsToRemove = new List<CustomerTag>();
            foreach (var existingCustomerTag in existingCustomerTags)
            {
                bool found = false;
                var existingCustomerTagName = await _customerTagService.GetCustomerTagById(existingCustomerTag);
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
                    await _customerTagService.DeleteTagFromCustomer(existingCustomerTagName.Id, customer.Id);
                }
            }

            foreach (string customerTagName in customerTags)
            {
                CustomerTag customerTag;
                var customerTag2 = await _customerTagService.GetCustomerTagByName(customerTagName);
                if (customerTag2 == null)
                {
                    customerTag = new CustomerTag {
                        Name = customerTagName,
                    };
                    await _customerTagService.InsertCustomerTag(customerTag);
                }
                else
                {
                    customerTag = customerTag2;
                }
                if (!customer.CustomerTags.Contains(customerTag.Id))
                {
                    await _customerTagService.InsertTagToCustomer(customerTag.Id, customer.Id);
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

        protected virtual async Task<IList<CustomerModel.AssociatedExternalAuthModel>> GetAssociatedExternalAuthRecords(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var result = new List<CustomerModel.AssociatedExternalAuthModel>();
            foreach (var record in await _openAuthenticationService.GetExternalIdentifiersFor(customer))
            {
                var method = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(record.ProviderSystemName);
                if (method == null)
                    continue;

                result.Add(new CustomerModel.AssociatedExternalAuthModel {
                    Id = record.Id,
                    Email = record.Email,
                    ExternalIdentifier = record.ExternalIdentifier,
                    AuthMethodName = method.PluginDescriptor.FriendlyName
                });
            }

            return result;
        }

        protected virtual async Task<CustomerModel> PrepareCustomerModelForList(Customer customer)
        {
            return new CustomerModel {
                Id = customer.Id,
                Email = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest"),
                Username = customer.Username,
                FullName = customer.GetFullName(),
                Company = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Company),
                Phone = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Phone),
                ZipPostalCode = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.ZipPostalCode),
                CustomerRoleNames = GetCustomerRolesNames(customer.CustomerRoles.ToList()),
                Active = customer.Active,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc),
                LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc),
            };
        }

        protected virtual async Task PrepareVendorsModel(CustomerModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableVendors.Add(new SelectListItem {
                Text = _localizationService.GetResource("Admin.Customers.Customers.Fields.Vendor.None"),
                Value = ""
            });
            var vendors = await _vendorService.GetAllVendors(showHidden: true);
            foreach (var vendor in vendors)
            {
                model.AvailableVendors.Add(new SelectListItem {
                    Text = vendor.Name,
                    Value = vendor.Id.ToString()
                });
            }
        }

        protected virtual async Task PrepareStoresModel(CustomerModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableStores.Add(new SelectListItem {
                Text = _localizationService.GetResource("Admin.Customers.Customers.Fields.StaffStore.None"),
                Value = ""
            });
            var stores = await _storeService.GetAllStores();
            foreach (var store in stores)
            {
                model.AvailableStores.Add(new SelectListItem {
                    Text = store.Shortcut,
                    Value = store.Id.ToString()
                });
            }
        }

        protected virtual async Task PrepareCustomerAttributeModel(CustomerModel model, Customer customer)
        {
            var customerAttributes = await _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in customerAttributes)
            {
                var attributeModel = new CustomerModel.CustomerAttributeModel {
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
                        var attributeValueModel = new CustomerModel.CustomerAttributeValueModel {
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
                    var selectedCustomerAttributes = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.CustomCustomerAttributes);
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
                                    var selectedValues = await _customerAttributeParser.ParseCustomerAttributeValues(selectedCustomerAttributes);
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

        public virtual async Task<CustomerListModel> PrepareCustomerListModel()
        {
            var registered = await _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);
            var model = new CustomerListModel {
                UsernamesEnabled = _customerSettings.UsernamesEnabled,
                CompanyEnabled = _customerSettings.CompanyEnabled,
                PhoneEnabled = _customerSettings.PhoneEnabled,
                ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled,
                AvailableCustomerRoles = (await _customerService.GetAllCustomerRoles(showHidden: true)).Select(cr => new SelectListItem() { Text = cr.Name, Value = cr.Id.ToString(), Selected = (cr.Id == registered.Id) }).ToList(),
                AvailableCustomerTags = (await _customerTagService.GetAllCustomerTags()).Select(ct => new SelectListItem() { Text = ct.Name, Value = ct.Id.ToString() }).ToList(),
                SearchCustomerRoleIds = new List<string> { (await _customerService.GetAllCustomerRoles(showHidden: true)).FirstOrDefault(x => x.Id == registered.Id).Id },
            };
            return model;
        }

        public virtual async Task<(IEnumerable<CustomerModel> customerModelList, int totalCount)> PrepareCustomerList(CustomerListModel model,
            string[] searchCustomerRoleIds, string[] searchCustomerTagIds, int pageIndex, int pageSize)
        {
            var customers = await _customerService.GetAllCustomers(
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

            var customermodellist = new List<CustomerModel>();
            foreach (var item in customers)
            {
                customermodellist.Add(await PrepareCustomerModelForList(item));
            }
            return (customermodellist, customers.TotalCount);
        }

        public virtual async Task PrepareCustomerModel(CustomerModel model, Customer customer, bool excludeProperties)
        {
            var allStores = await _storeService.GetAllStores();
            if (customer != null)
            {
                model.Id = customer.Id;
                model.ShowMessageContactForm = _commonSettings.StoreInDatabaseContactUsForm;
                if (!excludeProperties)
                {
                    model.Email = customer.Email;
                    model.Username = customer.Username;
                    model.VendorId = customer.VendorId;
                    model.StaffStoreId = customer.StaffStoreId;
                    model.AdminComment = customer.AdminComment;
                    model.IsTaxExempt = customer.IsTaxExempt;
                    model.FreeShipping = customer.FreeShipping;
                    model.Active = customer.Active;
                    model.Owner = customer.IsOwner() ? "" : (await _customerService.GetCustomerById(customer.OwnerId))?.Email;
                    var result = new StringBuilder();
                    foreach (var item in customer.CustomerTags)
                    {
                        var ct = await _customerTagService.GetCustomerTagById(item);
                        result.Append(ct.Name);
                        result.Append(", ");
                    }
                    model.CustomerTags = result.ToString();
                    var affiliate = await _affiliateService.GetAffiliateById(customer.AffiliateId);
                    if (affiliate != null)
                    {
                        model.AffiliateId = affiliate.Id;
                        model.AffiliateName = affiliate.GetFullName();
                    }

                    model.TimeZoneId = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.TimeZoneId);
                    model.VatNumber = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.VatNumber);
                    model.VatNumberStatusNote = ((VatNumberStatus)await customer.GetAttribute<int>(_genericAttributeService, SystemCustomerAttributeNames.VatNumberStatusId))
                        .GetLocalizedEnum(_localizationService, _workContext);
                    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc);
                    model.LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);
                    if (customer.LastPurchaseDateUtc.HasValue)
                        model.LastPurchaseDate = _dateTimeHelper.ConvertToUserTime(customer.LastPurchaseDateUtc.Value, DateTimeKind.Utc);
                    model.LastIpAddress = customer.LastIpAddress;
                    model.UrlReferrer = customer.UrlReferrer;
                    model.LastVisitedPage = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.LastVisitedPage);
                    model.LastUrlReferrer = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.LastUrlReferrer);

                    model.SelectedCustomerRoleIds = customer.CustomerRoles.Select(cr => cr.Id).ToArray();
                    //newsletter subscriptions
                    if (!String.IsNullOrEmpty(customer.Email))
                    {
                        var newsletterSubscriptionStoreIds = new List<string>();
                        foreach (var store in allStores)
                        {
                            var newsletterSubscription = await _newsLetterSubscriptionService
                                .GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                            if (newsletterSubscription != null && newsletterSubscription.Active)
                                newsletterSubscriptionStoreIds.Add(store.Id);
                        }
                        model.SelectedNewsletterSubscriptionStoreIds = newsletterSubscriptionStoreIds.ToArray();
                    }


                    //form fields
                    model.FirstName = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.FirstName);
                    model.LastName = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.LastName);
                    model.Gender = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Gender);
                    model.DateOfBirth = await customer.GetAttribute<DateTime?>(_genericAttributeService, SystemCustomerAttributeNames.DateOfBirth);
                    model.Company = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Company);
                    model.StreetAddress = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StreetAddress);
                    model.StreetAddress2 = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StreetAddress2);
                    model.ZipPostalCode = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.ZipPostalCode);
                    model.City = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.City);
                    model.CountryId = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.CountryId);
                    model.StateProvinceId = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StateProvinceId);
                    model.Phone = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Phone);
                    model.Fax = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Fax);
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
            await PrepareVendorsModel(model);

            //stores
            await PrepareStoresModel(model);

            //customer attributes
            await PrepareCustomerAttributeModel(model, customer);

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
                foreach (var c in await _countryService.GetAllCountries(showHidden: true))
                {
                    model.AvailableCountries.Add(new SelectListItem {
                        Text = c.Name,
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_customerSettings.StateProvinceEnabled)
                {
                    //states
                    var states = await _stateProvinceService.GetStateProvincesByCountryId(model.CountryId);
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

                        model.AvailableStates.Add(new SelectListItem {
                            Text = _localizationService.GetResource(anyCountrySelected ? "Admin.Address.OtherNonUS" : "Admin.Address.SelectState"),
                            Value = ""
                        });
                    }
                }
            }

            //newsletter subscriptions
            model.AvailableNewsletterSubscriptionStores = allStores
                .Select(s => new StoreModel() { Id = s.Id, Name = s.Shortcut })
                .ToList();


            //customer roles
            model.AvailableCustomerRoles = (await _customerService.GetAllCustomerRoles(showHidden: true))
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
                    model.RewardPointsAvailableStores.Add(new SelectListItem {
                        Text = store.Shortcut,
                        Value = store.Id.ToString(),
                        Selected = (store.Id == _storeContext.CurrentStore.Id)
                    });
                }

                //external authentication records
                model.AssociatedExternalAuthRecords = await GetAssociatedExternalAuthRecords(customer);

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
        public virtual async Task<Customer> InsertCustomerModel(CustomerModel model)
        {
            var ownerId = string.Empty;
            if (!string.IsNullOrEmpty(model.Owner))
            {
                var customerOwner = await _customerService.GetCustomerByEmail(model.Owner);
                if (customerOwner != null)
                {
                    ownerId = customerOwner.Id;
                }
            }
            var customer = new Customer {
                CustomerGuid = Guid.NewGuid(),
                Email = model.Email,
                Username = model.Username,
                VendorId = model.VendorId,
                StaffStoreId = model.StaffStoreId,
                AdminComment = model.AdminComment,
                IsTaxExempt = model.IsTaxExempt,
                FreeShipping = model.FreeShipping,
                Active = model.Active,
                StoreId = _storeContext.CurrentStore.Id,
                OwnerId = ownerId,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };
            await _customerService.InsertCustomer(customer);

            //form fields
            if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
            if (_customerSettings.GenderEnabled)
                await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
            if (_customerSettings.DateOfBirthEnabled)
                await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, model.DateOfBirth);
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

            //custom customer attributes
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, model.CustomAttributes);

            //newsletter subscriptions
            if (!String.IsNullOrEmpty(customer.Email))
            {
                var allStores = await _storeService.GetAllStores();
                foreach (var store in allStores)
                {
                    var newsletterSubscription = await _newsLetterSubscriptionService
                        .GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                    if (model.SelectedNewsletterSubscriptionStoreIds != null &&
                        model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id))
                    {
                        //subscribed
                        if (newsletterSubscription == null)
                        {
                            await _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription {
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
                            await _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletterSubscription);
                        }
                    }
                }
            }

            var allCustomerRoles = await _customerService.GetAllCustomerRoles(showHidden: true);
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
                await _customerService.InsertCustomerRoleInCustomer(customerRole);
            }


            //ensure that a customer with a vendor associated is not in "Administrators" role
            //otherwise, he won't be have access to the other functionality in admin area
            if (customer.IsAdmin() && !String.IsNullOrEmpty(customer.VendorId))
            {
                customer.VendorId = "";
                await _customerService.UpdateCustomerVendor(customer);
            }

            //ensure that a customer in the Vendors role has a vendor account associated.
            //otherwise, he will have access to ALL products
            if (customer.IsVendor() && string.IsNullOrEmpty(customer.VendorId))
            {
                var vendorRole = customer
                    .CustomerRoles
                    .FirstOrDefault(x => x.SystemName == SystemCustomerRoleNames.Vendors);
                customer.CustomerRoles.Remove(vendorRole);
                vendorRole.CustomerId = customer.Id;
                await _customerService.DeleteCustomerRoleInCustomer(vendorRole);
            }
            //ensure that a customer in the Staff role has a staff account associated.
            //otherwise, he will have access to ALL products
            if (customer.IsStaff() && string.IsNullOrEmpty(customer.StaffStoreId))
            {
                var staffRole = customer
                    .CustomerRoles
                    .FirstOrDefault(x => x.SystemName == SystemCustomerRoleNames.Staff);
                customer.CustomerRoles.Remove(staffRole);
                staffRole.CustomerId = customer.Id;
                await _customerService.DeleteCustomerRoleInCustomer(staffRole);
            }
            //tags
            await SaveCustomerTags(customer, ParseCustomerTags(model.CustomerTags));

            //activity log
            await _customerActivityService.InsertActivity("AddNewCustomer", customer.Id, _localizationService.GetResource("ActivityLog.AddNewCustomer"), customer.Id);

            return customer;
        }

        public virtual async Task<Customer> UpdateCustomerModel(Customer customer, CustomerModel model)
        {
            customer.AdminComment = model.AdminComment;
            customer.IsTaxExempt = model.IsTaxExempt;
            customer.FreeShipping = model.FreeShipping;
            customer.Active = model.Active;
            //email
            if (!String.IsNullOrWhiteSpace(model.Email))
            {
                await _customerRegistrationService.SetEmail(customer, model.Email);
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
                    await _customerRegistrationService.SetUsername(customer, model.Username);
                }
                else
                {
                    customer.Username = model.Username;
                }
            }

            if (!string.IsNullOrEmpty(model.Owner))
            {
                var customerOwner = await _customerService.GetCustomerByEmail(model.Owner);
                if (customerOwner != null)
                {
                    customer.OwnerId = customerOwner.Id;
                }
            }
            else
                customer.OwnerId = string.Empty;

            //VAT number
            if (_taxSettings.EuVatEnabled)
            {
                var prevVatNumber = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.VatNumber);

                await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);
                //set VAT number status
                if (!String.IsNullOrEmpty(model.VatNumber))
                {
                    if (!model.VatNumber.Equals(prevVatNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        var checkVatService = _serviceProvider.GetRequiredService<IVatService>();
                        await _genericAttributeService.SaveAttribute(customer,
                            SystemCustomerAttributeNames.VatNumberStatusId,
                            (int)(await checkVatService.GetVatNumberStatus(model.VatNumber)).status);
                    }
                }
                else
                {
                    await _genericAttributeService.SaveAttribute(customer,
                        SystemCustomerAttributeNames.VatNumberStatusId,
                        (int)VatNumberStatus.Empty);
                }
            }

            //vendor
            customer.VendorId = model.VendorId;

            //staff store
            customer.StaffStoreId = model.StaffStoreId;

            //form fields
            if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
            if (_customerSettings.GenderEnabled)
                await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
            if (_customerSettings.DateOfBirthEnabled)
                await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, model.DateOfBirth);
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

            //custom customer attributes
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, model.CustomAttributes);

            //newsletter subscriptions
            if (!String.IsNullOrEmpty(customer.Email))
            {
                var allStores = await _storeService.GetAllStores();
                foreach (var store in allStores)
                {
                    var newsletterSubscription = await _newsLetterSubscriptionService
                        .GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                    if (model.SelectedNewsletterSubscriptionStoreIds != null &&
                        model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id))
                    {
                        //subscribed
                        if (newsletterSubscription == null)
                        {
                            await _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription {
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
                            await _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletterSubscription);
                        }
                    }
                }
            }
            var allCustomerRoles = await _customerService.GetAllCustomerRoles(showHidden: true);
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
                        customer.CustomerRoles.Remove(customer.CustomerRoles.First(x => x.Id == customerRole.Id));
                }
            }
            await _customerService.UpdateCustomerinAdminPanel(customer);


            //ensure that a customer with a vendor associated is not in "Administrators" role
            //otherwise, he won't have access to the other functionality in admin area
            if (customer.IsAdmin() && !String.IsNullOrEmpty(customer.VendorId))
            {
                customer.VendorId = "";
                await _customerService.UpdateCustomerinAdminPanel(customer);
            }

            //ensure that a customer with a staff associated is not in "Administrators" role
            //otherwise, he won't have access to the other functionality in admin area
            if (customer.IsAdmin() && !String.IsNullOrEmpty(customer.StaffStoreId))
            {
                customer.StaffStoreId = "";
                await _customerService.UpdateCustomerinAdminPanel(customer);
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
                await _customerService.DeleteCustomerRoleInCustomer(vendorRole);
            }

            //tags
            await SaveCustomerTags(customer, ParseCustomerTags(model.CustomerTags));

            //activity log
            await _customerActivityService.InsertActivity("EditCustomer", customer.Id, _localizationService.GetResource("ActivityLog.EditCustomer"), customer.Id);
            return customer;
        }

        public virtual async Task DeleteCustomer(Customer customer)
        {
            await _customerService.DeleteCustomer(customer);

            //remove newsletter subscription (if exists)
            foreach (var store in await _storeService.GetAllStores())
            {
                var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                if (subscription != null)
                    await _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);
            }

            //activity log
            await _customerActivityService.InsertActivity("DeleteCustomer", customer.Id, _localizationService.GetResource("ActivityLog.DeleteCustomer"), customer.Id);
        }

        public virtual async Task DeleteSelected(IList<string> selectedIds)
        {
            var customers = new List<Customer>();
            customers.AddRange(await _customerService.GetCustomersByIds(selectedIds.ToArray()));
            for (var i = 0; i < customers.Count; i++)
            {
                var customer = customers[i];
                if (customer.Id != _workContext.CurrentCustomer.Id)
                {
                    await _customerService.DeleteCustomer(customer);
                }
                //activity log
                await _customerActivityService.InsertActivity("DeleteCustomer", customer.Id, _localizationService.GetResource("ActivityLog.DeleteCustomer"), customer.Id);
            }
        }

        public async Task SendEmail(Customer customer, CustomerModel.SendEmailModel model)
        {
            var emailAccount = await _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = (await _emailAccountService.GetAllEmailAccounts()).FirstOrDefault();
            if (emailAccount == null)
                throw new GrandException("Email account can't be loaded");

            var email = new QueuedEmail {
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
            await _queuedEmailService.InsertQueuedEmail(email);
            await _customerActivityService.InsertActivity("CustomerAdmin.SendEmail", "", _localizationService.GetResource("ActivityLog.SendEmailfromAdminPanel"), customer, model.Subject);
        }
        public virtual async Task SendPM(Customer customer, CustomerModel.SendPmModel model)
        {
            var privateMessage = new PrivateMessage {
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

            await _forumService.InsertPrivateMessage(privateMessage);
            await _customerActivityService.InsertActivity("CustomerAdmin.SendPM", "", _localizationService.GetResource("ActivityLog.SendPMfromAdminPanel"), customer, model.Subject);
        }
        public virtual async Task<IEnumerable<CustomerModel.RewardPointsHistoryModel>> PrepareRewardPointsHistoryModel(string customerId)
        {
            var model = new List<CustomerModel.RewardPointsHistoryModel>();
            foreach (var rph in await _rewardPointsService.GetRewardPointsHistory(customerId, showHidden: true))
            {
                var store = await _storeService.GetStoreById(rph.StoreId);
                model.Add(new CustomerModel.RewardPointsHistoryModel {
                    StoreName = store != null ? store.Shortcut : "Unknown",
                    Points = rph.Points,
                    PointsBalance = rph.PointsBalance,
                    Message = rph.Message,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(rph.CreatedOnUtc, DateTimeKind.Utc)
                });
            }
            return model;
        }

        public virtual async Task<RewardPointsHistory> InsertRewardPointsHistory(string customerId, string storeId, int addRewardPointsValue, string addRewardPointsMessage)
        {
            return await _rewardPointsService.AddRewardPointsHistory(customerId, addRewardPointsValue, storeId, addRewardPointsMessage);
        }

        public virtual async Task<IEnumerable<AddressModel>> PrepareAddressModel(Customer customer)
        {
            var addresses = customer.Addresses.OrderByDescending(a => a.CreatedOnUtc).ThenByDescending(a => a.Id).ToList();
            var addressesListModel = new List<AddressModel>();
            foreach (var x in addresses)
            {
                var model = await x.ToModel(_countryService, _stateProvinceService);
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
                var customAttributesFormatted = await _addressAttributeFormatter.FormatAttributes(x.CustomAttributes);
                if (!String.IsNullOrEmpty(customAttributesFormatted))
                {
                    //already encoded
                    addressHtmlSb.AppendFormat("<br />{0}", customAttributesFormatted);
                }
                addressHtmlSb.Append("</div>");
                model.AddressHtml = addressHtmlSb.ToString();
                addressesListModel.Add(model);
            }
            return addressesListModel;
        }
        public virtual async Task DeleteAddress(Customer customer, Address address)
        {
            customer.RemoveAddress(address);
            await _customerService.UpdateCustomerinAdminPanel(customer);
        }

        public virtual async Task<Address> InsertAddressModel(Customer customer, CustomerAddressModel model, string customAttributes)
        {
            var address = model.Address.ToEntity();
            address.CustomAttributes = customAttributes;
            address.CreatedOnUtc = DateTime.UtcNow;
            customer.Addresses.Add(address);
            await _customerService.UpdateCustomerinAdminPanel(customer);
            return address;
        }

        public virtual async Task PrepareAddressModel(CustomerAddressModel model, Address address, Customer customer, bool excludeProperties)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            model.CustomerId = customer.Id;
            if (address != null)
            {
                if (!excludeProperties)
                {
                    model.Address = await address.ToModel(_countryService, _stateProvinceService);
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
            foreach (var c in await _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.Address.CountryId) });
            //states
            var states = !String.IsNullOrEmpty(model.Address.CountryId) ? await _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId, showHidden: true) : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.Address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "" });
            //customer attribute services
            await model.Address.PrepareCustomAddressAttributes(address, _addressAttributeService, _addressAttributeParser);
        }

        public virtual async Task<Address> UpdateAddressModel(Customer customer, Address address, CustomerAddressModel model, string customAttributes)
        {
            address = model.Address.ToEntity(address);
            address.CustomAttributes = customAttributes;
            await _customerService.UpdateCustomerinAdminPanel(customer);
            return address;
        }

        public virtual async Task<(IEnumerable<CustomerModel.OrderModel> orderModels, int totalCount)> PrepareOrderModel(string customerId, int pageIndex, int pageSize)
        {
            var orders = await _orderService.SearchOrders(customerId: customerId, pageIndex: pageIndex - 1, pageSize: pageSize);
            var ordersModelList = new List<CustomerModel.OrderModel>();
            foreach (var order in orders)
            {
                var store = await _storeService.GetStoreById(order.StoreId);
                var orderModel = new CustomerModel.OrderModel {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    OrderCode = order.Code,
                    OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                    PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
                    ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
                    OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false),
                    StoreName = store != null ? store.Shortcut : "Unknown",
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
                };
                ordersModelList.Add(orderModel);
            }
            return (ordersModelList, orders.TotalCount);

        }

        public virtual CustomerReportsModel PrepareCustomerReportsModel()
        {
            var model = new CustomerReportsModel {
                //customers by number of orders
                BestCustomersByNumberOfOrders = new BestCustomersReportModel()
            };
            model.BestCustomersByNumberOfOrders.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(_localizationService, _workContext, false).ToList();
            model.BestCustomersByNumberOfOrders.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            model.BestCustomersByNumberOfOrders.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(_localizationService, _workContext, false).ToList();
            model.BestCustomersByNumberOfOrders.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            model.BestCustomersByNumberOfOrders.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(_localizationService, _workContext, false).ToList();
            model.BestCustomersByNumberOfOrders.AvailableShippingStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            //customers by order total
            model.BestCustomersByOrderTotal = new BestCustomersReportModel {
                AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(_localizationService, _workContext, false).ToList()
            };
            model.BestCustomersByOrderTotal.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            model.BestCustomersByOrderTotal.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(_localizationService, _workContext, false).ToList();
            model.BestCustomersByOrderTotal.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            model.BestCustomersByOrderTotal.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(_localizationService, _workContext, false).ToList();
            model.BestCustomersByOrderTotal.AvailableShippingStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            return model;
        }

        public virtual async Task<IList<RegisteredCustomerReportLineModel>> GetReportRegisteredCustomersModel(string storeId)
        {
            var report = new List<RegisteredCustomerReportLineModel>
            {
                new RegisteredCustomerReportLineModel
                {
                    Period = _localizationService.GetResource("Admin.Reports.Customers.RegisteredCustomers.Fields.Period.7days"),
                    Customers = await _customerReportService.GetRegisteredCustomersReport(storeId, 7)
                },

                new RegisteredCustomerReportLineModel
                {
                    Period = _localizationService.GetResource("Admin.Reports.Customers.RegisteredCustomers.Fields.Period.14days"),
                    Customers = await _customerReportService.GetRegisteredCustomersReport(storeId, 14)
                },
                new RegisteredCustomerReportLineModel
                {
                    Period = _localizationService.GetResource("Admin.Reports.Customers.RegisteredCustomers.Fields.Period.month"),
                    Customers = await _customerReportService.GetRegisteredCustomersReport(storeId, 30)
                },
                new RegisteredCustomerReportLineModel
                {
                    Period = _localizationService.GetResource("Admin.Reports.Customers.RegisteredCustomers.Fields.Period.year"),
                    Customers = await _customerReportService.GetRegisteredCustomersReport(storeId, 365)
                }
            };

            return report;
        }

        public virtual async Task<(IEnumerable<BestCustomerReportLineModel> bestCustomerReportLineModels, int totalCount)> PrepareBestCustomerReportLineModel(BestCustomersReportModel model, int orderBy, int pageIndex, int pageSize)
        {
            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;

            var items = _customerReportService.GetBestCustomersReport(model.StoreId, startDateValue, endDateValue,
                orderStatus, paymentStatus, shippingStatus, 2, pageIndex - 1, pageSize);

            var report = new List<BestCustomerReportLineModel>();
            foreach (var x in items)
            {
                var m = new BestCustomerReportLineModel {
                    CustomerId = x.CustomerId,
                    OrderTotal = _priceFormatter.FormatPrice(x.OrderTotal, true, false),
                    OrderCount = x.OrderCount,
                };
                var customer = await _customerService.GetCustomerById(x.CustomerId);
                if (customer != null)
                {
                    m.CustomerName = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                }
                report.Add(m);
            }
            return (report, items.TotalCount);
        }
        public virtual async Task<IList<ShoppingCartItemModel>> PrepareShoppingCartItemModel(string customerId, int cartTypeId)
        {
            var customer = await _customerService.GetCustomerById(customerId);
            var cart = customer.ShoppingCartItems.Where(x => x.ShoppingCartTypeId == cartTypeId).ToList();

            var items = new List<ShoppingCartItemModel>();
            foreach (var sci in cart)
            {
                var store = await _storeService.GetStoreById(sci.StoreId);
                var product = await _productService.GetProductById(sci.ProductId);
                var sciModel = new ShoppingCartItemModel {
                    Id = sci.Id,
                    Store = store != null ? store.Shortcut : "Unknown",
                    ProductId = sci.ProductId,
                    Quantity = sci.Quantity,
                    ProductName = product.Name,
                    AttributeInfo = await _productAttributeFormatter.FormatAttributes(product, sci.AttributesXml),
                    UnitPrice = _priceFormatter.FormatPrice((await _taxService.GetProductPrice(product, (await _priceCalculationService.GetUnitPrice(sci)).unitprice)).productprice),
                    Total = _priceFormatter.FormatPrice((await _taxService.GetProductPrice(product, (await _priceCalculationService.GetSubTotal(sci)).subTotal)).productprice),
                    UpdatedOn = _dateTimeHelper.ConvertToUserTime(sci.UpdatedOnUtc, DateTimeKind.Utc)
                };
                items.Add(sciModel);
            }
            return items;
        }
        public virtual async Task DeleteCart(Customer customer, string id)
        {
            var cart = customer.ShoppingCartItems.FirstOrDefault(a => a.Id == id);
            if (cart != null)
            {
                await _serviceProvider.GetRequiredService<IShoppingCartService>()
                    .DeleteShoppingCartItem(customer, cart, ensureOnlyActiveCheckoutAttributes: true);
                await _customerService.UpdateCustomerinAdminPanel(customer);
            }
        }
        public virtual async Task<(IEnumerable<CustomerModel.ProductPriceModel> productPriceModels, int totalCount)> PrepareProductPriceModel(string customerId, int pageIndex, int pageSize)
        {
            var productPrices = await _customerProductService.GetProductsPriceByCustomer(customerId, pageIndex - 1, pageSize);
            var items = new List<CustomerModel.ProductPriceModel>();
            foreach (var x in productPrices)
            {
                var m = new CustomerModel.ProductPriceModel {
                    Id = x.Id,
                    Price = x.Price,
                    ProductId = x.ProductId,
                    ProductName = (await _productService.GetProductById(x.ProductId))?.Name
                };
                items.Add(m);
            }
            return (items, productPrices.TotalCount);
        }
        public virtual async Task<(IEnumerable<CustomerModel.ProductModel> productModels, int totalCount)> PreparePersonalizedProducts(string customerId, int pageIndex, int pageSize)
        {
            var products = await _customerProductService.GetProductsByCustomer(customerId, pageIndex - 1, pageSize);
            var items = new List<CustomerModel.ProductModel>();
            foreach (var x in products)
            {
                var m = new CustomerModel.ProductModel {
                    Id = x.Id,
                    DisplayOrder = x.DisplayOrder,
                    ProductId = x.ProductId,
                    ProductName = (await _productService.GetProductById(x.ProductId))?.Name
                };
                items.Add(m);
            }
            return (items, products.TotalCount);
        }
        public virtual async Task<CustomerModel.AddProductModel> PrepareCustomerModelAddProductModel()
        {
            var model = new CustomerModel.AddProductModel();
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = await _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in await _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in await _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(_localizationService, _workContext, false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });

            return model;
        }

        public virtual async Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(CustomerModel.AddProductModel model, int pageIndex, int pageSize)
        {
            var products = await _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel(_dateTimeHelper)).ToList(), products.TotalCount);
        }

        public virtual async Task InsertCustomerAddProductModel(string customerId, bool personalized, CustomerModel.AddProductModel model)
        {
            foreach (string id in model.SelectedProductIds)
            {
                var product = await _productService.GetProductById(id);
                if (product != null)
                {
                    if (!personalized)
                    {
                        if (!(await _customerProductService.GetPriceByCustomerProduct(customerId, id)).HasValue)
                        {
                            await _customerProductService.InsertCustomerProductPrice(new CustomerProductPrice() { CustomerId = customerId, ProductId = id, Price = product.Price });
                        }
                    }
                    else
                    {
                        if (await _customerProductService.GetCustomerProduct(customerId, id) == null)
                        {
                            await _customerProductService.InsertCustomerProduct(new CustomerProduct() { CustomerId = customerId, ProductId = id, DisplayOrder = 0 });
                        }

                    }
                }
            }
        }
        public virtual async Task UpdateProductPrice(CustomerModel.ProductPriceModel model)
        {
            var productPrice = await _customerProductService.GetCustomerProductPriceById(model.Id);
            if (productPrice != null)
            {
                productPrice.Price = model.Price;
                await _customerProductService.UpdateCustomerProductPrice(productPrice);
            }
        }
        public virtual async Task DeleteProductPrice(string id)
        {
            var productPrice = await _customerProductService.GetCustomerProductPriceById(id);
            if (productPrice == null)
                throw new ArgumentException("No productPrice found with the specified id");

            await _customerProductService.DeleteCustomerProductPrice(productPrice);
        }
        public virtual async Task UpdatePersonalizedProduct(CustomerModel.ProductModel model)
        {
            var customerproduct = await _customerProductService.GetCustomerProduct(model.Id);
            if (customerproduct != null)
            {
                customerproduct.DisplayOrder = model.DisplayOrder;
                await _customerProductService.UpdateCustomerProduct(customerproduct);
            }
        }
        public virtual async Task DeletePersonalizedProduct(string id)
        {
            var customerproduct = await _customerProductService.GetCustomerProduct(id);
            if (customerproduct == null)
                throw new ArgumentException("No customerproduct found with the specified id");

            await _customerProductService.DeleteCustomerProduct(customerproduct);
        }
        public virtual async Task<(IEnumerable<CustomerModel.ActivityLogModel> activityLogModels, int totalCount)> PrepareActivityLogModel(string customerId, int pageIndex, int pageSize)
        {
            var activityLog = await _customerActivityService.GetAllActivities(null, null, null, customerId, "", null, pageIndex - 1, pageSize);
            var items = new List<CustomerModel.ActivityLogModel>();
            foreach (var x in activityLog)
            {
                var m = new CustomerModel.ActivityLogModel {
                    Id = x.Id,
                    ActivityLogTypeName = (await _customerActivityService.GetActivityTypeById(x.ActivityLogTypeId))?.Name,
                    Comment = x.Comment,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                    IpAddress = x.IpAddress
                };
                items.Add(m);
            }
            return (items, activityLog.TotalCount);
        }
        public virtual async Task<(IEnumerable<ContactFormModel> contactFormModels, int totalCount)> PrepareContactFormModel(string customerId, string vendorId, int pageIndex, int pageSize)
        {
            var contactform = await _contactUsService.GetAllContactUs(storeId: "", vendorId: vendorId, customerId: customerId, pageIndex: pageIndex - 1, pageSize: pageSize);
            var items = new List<ContactFormModel>();
            foreach (var x in contactform)
            {
                var store = await _storeService.GetStoreById(x.StoreId);
                var m = x.ToModel();
                m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                m.Enquiry = "";
                m.Email = m.FullName + " - " + m.Email;
                m.Store = store != null ? store.Shortcut : "-empty-";
                items.Add(m);
            }
            return (items, contactform.TotalCount);
        }
        public virtual async Task<(IEnumerable<CustomerModel.BackInStockSubscriptionModel> backInStockSubscriptionModels, int totalCount)> PrepareBackInStockSubscriptionModel(string customerId, int pageIndex, int pageSize)
        {
            var subscriptions = await _backInStockSubscriptionService.GetAllSubscriptionsByCustomerId(customerId, "", pageIndex - 1, pageSize);
            var items = new List<CustomerModel.BackInStockSubscriptionModel>();
            foreach (var x in subscriptions)
            {
                var store = await _storeService.GetStoreById(x.StoreId);
                var product = await _productService.GetProductById(x.ProductId);
                var m = new CustomerModel.BackInStockSubscriptionModel {
                    Id = x.Id,
                    StoreName = store != null ? store.Shortcut : "Unknown",
                    ProductId = x.ProductId,
                    ProductName = product != null ? product.Name : "Unknown",
                    AttributeDescription = string.IsNullOrEmpty(x.AttributeXml) ? "" : await _productAttributeFormatter.FormatAttributes(product, x.AttributeXml),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                };
                items.Add(m);
            }
            return (items, subscriptions.TotalCount);
        }
        public virtual async Task<IList<CustomerModel.CustomerNote>> PrepareCustomerNoteList(string customerId)
        {
            var customerNoteModels = new List<CustomerModel.CustomerNote>();
            foreach (var customerNote in (await _customerService.GetCustomerNotes(customerId))
                .OrderByDescending(on => on.CreatedOnUtc))
            {
                var download = await _downloadService.GetDownloadById(customerNote.DownloadId);
                customerNoteModels.Add(new CustomerModel.CustomerNote {
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
        public virtual async Task<CustomerNote> InsertCustomerNote(string customerId, string downloadId, bool displayToCustomer, string title, string message)
        {
            var customerNote = new CustomerNote {
                DisplayToCustomer = displayToCustomer,
                Title = title,
                Note = message,
                DownloadId = downloadId,
                CustomerId = customerId,
                CreatedOnUtc = DateTime.UtcNow,
            };
            await _customerService.InsertCustomerNote(customerNote);

            //new customer note notification
            if (displayToCustomer)
            {
                //email
                await _workflowMessageService.SendNewCustomerNoteAddedCustomerNotification(customerNote,
                    await _customerService.GetCustomerById(customerId), _storeContext.CurrentStore, _workContext.WorkingLanguage.Id);

            }
            return customerNote;
        }
        public virtual async Task DeleteCustomerNote(string id, string customerId)
        {
            var customerNote = await _customerService.GetCustomerNote(id);
            if (customerNote == null)
                throw new ArgumentException("No customer note found with the specified id");

            await _customerService.DeleteCustomerNote(customerNote);

        }
    }
}
