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
using Grand.Framework.Controllers;
using Grand.Framework.Extensions;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Affiliates;
using Grand.Services.Authentication.External;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.ExportImport;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.ShoppingCart;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class CustomerController : BaseAdminController
    {
        #region Fields

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
        private readonly IAddressService _addressService;
        private readonly CustomerSettings _customerSettings;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly IVendorService _vendorService;
        private readonly IStoreContext _storeContext;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderService _orderService;
        private readonly IExportManager _exportManager;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IPermissionService _permissionService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ForumSettings _forumSettings;
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

        #endregion

        #region Constructors

        public CustomerController(ICustomerService customerService,
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
            IAddressService addressService,
            CustomerSettings customerSettings,
            ITaxService taxService,
            IWorkContext workContext,
            IVendorService vendorService,
            IStoreContext storeContext,
            IPriceFormatter priceFormatter,
            IOrderService orderService,
            IExportManager exportManager,
            ICustomerActivityService customerActivityService,
            IPriceCalculationService priceCalculationService,
            IProductAttributeFormatter productAttributeFormatter,
            IPermissionService permissionService,
            IQueuedEmailService queuedEmailService,
            EmailAccountSettings emailAccountSettings,
            IEmailAccountService emailAccountService,
            ForumSettings forumSettings,
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
            this._customerService = customerService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._genericAttributeService = genericAttributeService;
            this._customerRegistrationService = customerRegistrationService;
            this._customerReportService = customerReportService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._rewardPointsService = rewardPointsService;
            this._dateTimeSettings = dateTimeSettings;
            this._taxSettings = taxSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._addressService = addressService;
            this._customerSettings = customerSettings;
            this._commonSettings = commonSettings;
            this._taxService = taxService;
            this._workContext = workContext;
            this._vendorService = vendorService;
            this._storeContext = storeContext;
            this._priceFormatter = priceFormatter;
            this._orderService = orderService;
            this._exportManager = exportManager;
            this._customerActivityService = customerActivityService;
            this._priceCalculationService = priceCalculationService;
            this._productAttributeFormatter = productAttributeFormatter;
            this._permissionService = permissionService;
            this._queuedEmailService = queuedEmailService;
            this._emailAccountSettings = emailAccountSettings;
            this._emailAccountService = emailAccountService;
            this._forumSettings = forumSettings;
            this._forumService = forumService;
            this._openAuthenticationService = openAuthenticationService;
            this._addressSettings = addressSettings;
            this._storeService = storeService;
            this._customerAttributeParser = customerAttributeParser;
            this._customerAttributeService = customerAttributeService;
            this._addressAttributeParser = addressAttributeParser;
            this._addressAttributeService = addressAttributeService;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._affiliateService = affiliateService;
            this._workflowMessageService = workflowMessageService;
            this._backInStockSubscriptionService = backInStockSubscriptionService;
            this._contactUsService = contactUsService;
            this._customerTagService = customerTagService;
            this._productService = productService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._downloadService = downloadService;
        }

        #endregion

        #region Utilities

        [NonAction]
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

        [NonAction]
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



        [NonAction]
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

        [NonAction]
        protected virtual IList<RegisteredCustomerReportLineModel> GetReportRegisteredCustomersModel()
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

        [NonAction]
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

        [NonAction]
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

        [NonAction]
        protected virtual string ValidateCustomerRoles(IList<CustomerRole> customerRoles)
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

        [NonAction]
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

        [NonAction]
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

        [NonAction]
        protected virtual string ParseCustomCustomerAttributes(Customer customer, IFormCollection form)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (form == null)
                throw new ArgumentNullException("form");

            string attributesXml = "";
            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in customerAttributes)
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
                            var cblAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
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
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.ToString().Trim();
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

        [NonAction]
        protected virtual void PrepareCustomerModel(CustomerModel model, Customer customer, bool excludeProperties)
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
                .Select(s => new CustomerModel.StoreModel() { Id = s.Id, Name = s.Name })
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

        [NonAction]
        protected virtual void PrepareAddressModel(CustomerAddressModel model, Address address, Customer customer, bool excludeProperties)
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

        #endregion

        #region Customers

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

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
            return View(model);
        }

        [HttpPost]
        public IActionResult CustomerList(DataSourceRequest command, CustomerListModel model,
            string[] searchCustomerRoleIds, string[] searchCustomerTagIds)
        {
            //we use own own binder for searchCustomerRoleIds property 
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

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
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = customers.Select(PrepareCustomerModelForList),
                Total = customers.TotalCount
            };

            return Json(gridModel);
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = new CustomerModel();
            PrepareCustomerModel(model, null, false);
            //default value
            model.Active = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]

        public IActionResult Create(CustomerModel model, bool continueEditing, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            if (!String.IsNullOrWhiteSpace(model.Email))
            {
                var cust2 = _customerService.GetCustomerByEmail(model.Email);
                if (cust2 != null)
                    ModelState.AddModelError("", "Email is already registered");
            }
            if (!String.IsNullOrWhiteSpace(model.Username) & _customerSettings.UsernamesEnabled)
            {
                var cust2 = _customerService.GetCustomerByUsername(model.Username);
                if (cust2 != null)
                    ModelState.AddModelError("", "Username is already registered");
            }

            //validate customer roles
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            var newCustomerRoles = new List<CustomerRole>();
            foreach (var customerRole in allCustomerRoles)
                if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    newCustomerRoles.Add(customerRole);
            var customerRolesError = ValidateCustomerRoles(newCustomerRoles);
            if (!String.IsNullOrEmpty(customerRolesError))
            {
                ModelState.AddModelError("", customerRolesError);
                ErrorNotification(customerRolesError, false);
            }

            if (ModelState.IsValid)
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
                var customerAttributes = ParseCustomCustomerAttributes(customer, form);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, customerAttributes);

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

                //password
                if (!String.IsNullOrWhiteSpace(model.Password))
                {
                    var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, model.Password);
                    var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
                    if (!changePassResult.Success)
                    {
                        foreach (var changePassError in changePassResult.Errors)
                            ErrorNotification(changePassError);
                    }
                }

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
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminCouldNotbeVendor"));
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
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
                }

                //tags
                SaveCustomerTags(customer, ParseCustomerTags(model.CustomerTags));

                //activity log
                _customerActivityService.InsertActivity("AddNewCustomer", customer.Id, _localizationService.GetResource("ActivityLog.AddNewCustomer"), customer.Id);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customer.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareCustomerModel(model, null, true);
            return View(model);

        }

        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null || customer.Deleted)
                //No customer found with the specified id
                return RedirectToAction("List");

            var model = new CustomerModel();
            PrepareCustomerModel(model, customer, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Edit(CustomerModel model, bool continueEditing, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null || customer.Deleted)
                //No customer found with the specified id
                return RedirectToAction("List");

            //validate customer roles
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            var newCustomerRoles = new List<CustomerRole>();
            foreach (var customerRole in allCustomerRoles)
                if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    newCustomerRoles.Add(customerRole);
            var customerRolesError = ValidateCustomerRoles(newCustomerRoles);
            if (!String.IsNullOrEmpty(customerRolesError))
            {
                ModelState.AddModelError("", customerRolesError);
                ErrorNotification(customerRolesError, false);
            }

            if (ModelState.IsValid)
            {
                try
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
                    var customerAttributes = ParseCustomCustomerAttributes(customer, form);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, customerAttributes);

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
                                customer.CustomerRoles.Remove(customerRole);
                        }
                    }
                    _customerService.UpdateCustomerinAdminPanel(customer);


                    //ensure that a customer with a vendor associated is not in "Administrators" role
                    //otherwise, he won't have access to the other functionality in admin area
                    if (customer.IsAdmin() && !String.IsNullOrEmpty(customer.VendorId))
                    {
                        customer.VendorId = "";
                        _customerService.UpdateCustomerinAdminPanel(customer);
                        ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminCouldNotbeVendor"));
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
                        ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
                    }

                    //tags
                    SaveCustomerTags(customer, ParseCustomerTags(model.CustomerTags));

                    //activity log
                    _customerActivityService.InsertActivity("EditCustomer", customer.Id, _localizationService.GetResource("ActivityLog.EditCustomer"), customer.Id);

                    SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Updated"));
                    if (continueEditing)
                    {
                        //selected tab
                        SaveSelectedTabIndex();

                        return RedirectToAction("Edit", new { id = customer.Id });
                    }
                    return RedirectToAction("List");
                }
                catch (Exception exc)
                {
                    ErrorNotification(exc.Message, false);
                }
            }


            //If we got this far, something failed, redisplay form
            PrepareCustomerModel(model, customer, true);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("changepassword")]
        public IActionResult ChangePassword(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var changePassRequest = new ChangePasswordRequest(model.Email,
                    false, _customerSettings.DefaultPasswordFormat, model.Password);
                var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
                if (changePassResult.Success)
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.PasswordChanged"));
                else
                    foreach (var error in changePassResult.Errors)
                        ErrorNotification(error);
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markVatNumberAsValid")]
        public IActionResult MarkVatNumberAsValid(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            _genericAttributeService.SaveAttribute(customer,
                SystemCustomerAttributeNames.VatNumberStatusId,
                (int)VatNumberStatus.Valid);

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markVatNumberAsInvalid")]
        public IActionResult MarkVatNumberAsInvalid(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            _genericAttributeService.SaveAttribute(customer,
                SystemCustomerAttributeNames.VatNumberStatusId,
                (int)VatNumberStatus.Invalid);

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("remove-affiliate")]
        public IActionResult RemoveAffiliate(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            customer.AffiliateId = "";
            _customerService.UpdateAffiliate(customer);
            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            try
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

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = customer.Id });
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("impersonate")]
        public IActionResult Impersonate(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AllowCustomerImpersonation))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            //ensure that a non-admin user cannot impersonate as an administrator
            //otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
            if (!_workContext.CurrentCustomer.IsAdmin() && customer.IsAdmin())
            {
                ErrorNotification("A non-admin user cannot impersonate as an administrator");
                return RedirectToAction("Edit", customer.Id);
            }


            _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.ImpersonatedCustomerId, customer.Id);

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("send-welcome-message")]
        public IActionResult SendWelcomeMessage(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

            SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.SendWelcomeMessage.Success"));

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("resend-activation-message")]
        public IActionResult ReSendActivationMessage(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            //email validation message
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AccountActivationToken, Guid.NewGuid().ToString());
            _workflowMessageService.SendCustomerEmailValidationMessage(customer, _workContext.WorkingLanguage.Id);

            SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.ReSendActivationMessage.Success"));

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        public IActionResult SendEmail(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            try
            {
                if (String.IsNullOrWhiteSpace(customer.Email))
                    throw new GrandException("Customer email is empty");
                if (!CommonHelper.IsValidEmail(customer.Email))
                    throw new GrandException("Customer email is not valid");
                if (String.IsNullOrWhiteSpace(model.SendEmail.Subject))
                    throw new GrandException("Email subject is empty");
                if (String.IsNullOrWhiteSpace(model.SendEmail.Body))
                    throw new GrandException("Email body is empty");

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
                    Subject = model.SendEmail.Subject,
                    Body = model.SendEmail.Body,
                    CreatedOnUtc = DateTime.UtcNow,
                    DontSendBeforeDateUtc = (model.SendEmail.SendImmediately || !model.SendEmail.DontSendBeforeDate.HasValue) ?
                        null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SendEmail.DontSendBeforeDate.Value)
                };
                _queuedEmailService.InsertQueuedEmail(email);
                _customerActivityService.InsertActivity("CustomerAdmin.SendEmail", "", _localizationService.GetResource("ActivityLog.SendEmailfromAdminPanel"), customer, model.SendEmail.Subject);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.SendEmail.Queued"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        public IActionResult SendPm(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            try
            {
                if (!_forumSettings.AllowPrivateMessages)
                    throw new GrandException("Private messages are disabled");
                if (customer.IsGuest())
                    throw new GrandException("Customer should be registered");
                if (String.IsNullOrWhiteSpace(model.SendPm.Subject))
                    throw new GrandException("PM subject is empty");
                if (String.IsNullOrWhiteSpace(model.SendPm.Message))
                    throw new GrandException("PM message is empty");


                var privateMessage = new PrivateMessage
                {
                    StoreId = _storeContext.CurrentStore.Id,
                    ToCustomerId = customer.Id,
                    FromCustomerId = _workContext.CurrentCustomer.Id,
                    Subject = model.SendPm.Subject,
                    Text = model.SendPm.Message,
                    IsDeletedByAuthor = false,
                    IsDeletedByRecipient = false,
                    IsRead = false,
                    CreatedOnUtc = DateTime.UtcNow
                };

                _forumService.InsertPrivateMessage(privateMessage);
                _customerActivityService.InsertActivity("CustomerAdmin.SendPM", "", _localizationService.GetResource("ActivityLog.SendPMfromAdminPanel"), customer, model.SendPm.Subject);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.SendPM.Sent"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        #endregion

        #region Reward points history

        [HttpPost]
        public IActionResult RewardPointsHistorySelect(string customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id");

            var model = new List<CustomerModel.RewardPointsHistoryModel>();
            foreach (var rph in _rewardPointsService.GetRewardPointsHistory(customer.Id, true))
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
            var gridModel = new DataSourceResult
            {
                Data = model,
                Total = model.Count
            };

            return Json(gridModel);
        }


        public IActionResult RewardPointsHistoryAdd(string customerId, string storeId, int addRewardPointsValue, string addRewardPointsMessage)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                return Json(new { Result = false });

            _rewardPointsService.AddRewardPointsHistory(customerId,
                addRewardPointsValue, storeId, addRewardPointsMessage);

            return Json(new { Result = true });
        }

        #endregion

        #region Addresses

        [HttpPost]
        public IActionResult AddressesSelect(string customerId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id", "customerId");

            var addresses = customer.Addresses.OrderByDescending(a => a.CreatedOnUtc).ThenByDescending(a => a.Id).ToList();
            var gridModel = new DataSourceResult
            {
                Data = addresses.Select(x =>
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
                }),
                Total = addresses.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult AddressDelete(string id, string customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id", "customerId");

            var address = customer.Addresses.FirstOrDefault(a => a.Id == id);
            if (address == null)
                //No customer found with the specified id
                return Content("No customer found with the specified id");
            customer.RemoveAddress(address);
            _customerService.UpdateCustomerinAdminPanel(customer);

            return new NullJsonResult();
        }

        public IActionResult AddressCreate(string customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            var model = new CustomerAddressModel();
            PrepareAddressModel(model, null, customer, false);

            return View(model);
        }

        [HttpPost]

        public IActionResult AddressCreate(CustomerAddressModel model, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.CustomerId);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            //custom address attributes
            var customAttributes = form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                customer.Addresses.Add(address);
                _customerService.UpdateCustomerinAdminPanel(customer);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Addresses.Added"));
                return RedirectToAction("AddressEdit", new { addressId = address.Id, customerId = model.CustomerId });
            }

            //If we got this far, something failed, redisplay form
            PrepareAddressModel(model, null, customer, true);
            return View(model);
        }

        public IActionResult AddressEdit(string addressId, string customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            var address = customer.Addresses.Where(x => x.Id == addressId).FirstOrDefault();
            if (address == null)
                //No address found with the specified id
                return RedirectToAction("Edit", new { id = customer.Id });

            var model = new CustomerAddressModel();
            PrepareAddressModel(model, address, customer, false);
            return View(model);
        }

        [HttpPost]

        public IActionResult AddressEdit(CustomerAddressModel model, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.CustomerId);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            var address = customer.Addresses.Where(x => x.Id == model.Address.Id).FirstOrDefault();
            if (address == null)
                //No address found with the specified id
                return RedirectToAction("Edit", new { id = customer.Id });

            //custom address attributes
            var customAttributes = form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;
                _customerService.UpdateCustomerinAdminPanel(customer);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Addresses.Updated"));
                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, customerId = model.CustomerId });
            }

            //If we got this far, something failed, redisplay form
            PrepareAddressModel(model, address, customer, true);

            return View(model);
        }

        #endregion

        #region Orders

        [HttpPost]
        public IActionResult OrderList(string customerId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var orders = _orderService.SearchOrders(customerId: customerId);

            var gridModel = new DataSourceResult
            {
                Data = orders.PagedForCommand(command)
                    .Select(order =>
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
                    }),
                Total = orders.Count
            };


            return Json(gridModel);
        }

        #endregion

        #region Reports

        public IActionResult Reports()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

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

            return View(model);
        }

        [HttpPost]
        public IActionResult ReportBestCustomersByOrderTotalList(DataSourceRequest command, BestCustomersReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Content("");

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;


            var items = _customerReportService.GetBestCustomersReport(startDateValue, endDateValue,
                orderStatus, paymentStatus, shippingStatus, 1, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x =>
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
                }),
                Total = items.TotalCount
            };

            return Json(gridModel);
        }
        [HttpPost]
        public IActionResult ReportBestCustomersByNumberOfOrdersList(DataSourceRequest command, BestCustomersReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Content("");

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;


            var items = _customerReportService.GetBestCustomersReport(startDateValue, endDateValue,
                orderStatus, paymentStatus, shippingStatus, 2, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x =>
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
                }),
                Total = items.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ReportRegisteredCustomersList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Content("");

            var model = GetReportRegisteredCustomersModel();
            var gridModel = new DataSourceResult
            {
                Data = model,
                Total = model.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ReportCustomerTimeChart(DataSourceRequest command, DateTime? startDate, DateTime? endDate)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Content("");

            var model = _customerReportService.GetCustomerByTimeReport(startDate, endDate);
            var gridModel = new DataSourceResult
            {
                Data = model
            };
            return Json(gridModel);
        }

        #endregion

        #region Current shopping cart/ wishlist

        [HttpPost]
        public IActionResult GetCartList(string customerId, int cartTypeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Content("");

            var customer = _customerService.GetCustomerById(customerId);
            var cart = customer.ShoppingCartItems.Where(x => x.ShoppingCartTypeId == cartTypeId).ToList();

            var gridModel = new DataSourceResult
            {
                Data = cart.Select(sci =>
                {
                    decimal taxRate;
                    var store = _storeService.GetStoreById(sci.StoreId);
                    var product = EngineContext.Current.Resolve<IProductService>().GetProductById(sci.ProductId);
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
                }),
                Total = cart.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult DeleteCart(string id, string customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id", "customerId");

            var cart = customer.ShoppingCartItems.FirstOrDefault(a => a.Id == id);
            if (cart == null)
                //No customer found with the specified id
                return Content("No customer found with the specified cart id");

            EngineContext.Current.Resolve<IShoppingCartService>()
                .DeleteShoppingCartItem(_workContext.CurrentCustomer, cart, ensureOnlyActiveCheckoutAttributes: true);

            _customerService.UpdateCustomerinAdminPanel(customer);

            return new NullJsonResult();
        }


        #endregion

        #region Customer Product Personalize / Price

        [HttpPost]
        public IActionResult ProductsPrice(DataSourceRequest command, string customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Content("");

            var productPrices = _customerService.GetProductsPriceByCustomer(customerId, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = productPrices.Select(x =>
                {
                    var m = new CustomerModel.ProductPriceModel
                    {
                        Id = x.Id,
                        Price = x.Price,
                        ProductId = x.ProductId,
                        ProductName = _productService.GetProductById(x.ProductId)?.Name
                    };
                    return m;

                }),
                Total = productPrices.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult PersonalizedProducts(DataSourceRequest command, string customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Content("");

            var productPrices = _customerService.GetProductsByCustomer(customerId, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = productPrices.Select(x =>
                {
                    var m = new CustomerModel.ProductModel
                    {
                        Id = x.Id,
                        DisplayOrder = x.DisplayOrder,
                        ProductId = x.ProductId,
                        ProductName = _productService.GetProductById(x.ProductId)?.Name
                    };
                    return m;

                }),
                Total = productPrices.TotalCount
            };

            return Json(gridModel);
        }

        public IActionResult ProductAddPopup(string customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

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

            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAddPopupList(DataSourceRequest command, CustomerModel.AddProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();
            var searchCategoryIds = new List<string>();
            if (!String.IsNullOrEmpty(model.SearchCategoryId))
                searchCategoryIds.Add(model.SearchCategoryId);

            var gridModel = new DataSourceResult();
            var products = _productService.SearchProducts(
                categoryIds: searchCategoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true
                );
            gridModel.Data = products.Select(x => x.ToModel());
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }
        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult ProductAddPopup(string customerId, bool personalized, CustomerModel.AddProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
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

            ViewBag.RefreshPage = true;
            return View(model);
        }
        public IActionResult UpdateProductPrice(CustomerModel.ProductPriceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var productPrice = _customerService.GetCustomerProductPriceById(model.Id);
            if (productPrice != null)
            {
                productPrice.Price = model.Price;
                _customerService.UpdateCustomerProductPrice(productPrice);
            }

            return new NullJsonResult();
        }
        public IActionResult DeleteProductPrice(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var productPrice = _customerService.GetCustomerProductPriceById(id);
            if (productPrice == null)
                throw new ArgumentException("No productPrice found with the specified id");

            _customerService.DeleteCustomerProductPrice(productPrice);

            return new NullJsonResult();
        }

        public IActionResult UpdatePersonalizedProduct(CustomerModel.ProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerproduct = _customerService.GetCustomerProduct(model.Id);
            if (customerproduct != null)
            {
                customerproduct.DisplayOrder = model.DisplayOrder;
                _customerService.UpdateCustomerProduct(customerproduct);
            }

            return new NullJsonResult();
        }

        public IActionResult DeletePersonalizedProduct(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerproduct = _customerService.GetCustomerProduct(id);
            if (customerproduct == null)
                throw new ArgumentException("No customerproduct found with the specified id");

            _customerService.DeleteCustomerProduct(customerproduct);

            return new NullJsonResult();
        }

        #endregion

        #region Activity log and message contact form

        [HttpPost]
        public IActionResult ListActivityLog(DataSourceRequest command, string customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Content("");

            var activityLog = _customerActivityService.GetAllActivities(null, null, customerId, "", null, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityLog.Select(x =>
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

                }),
                Total = activityLog.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ContactFormList(DataSourceRequest command, string customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageContactForm))
                return AccessDeniedView();


            string vendorId = "";
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            var contactform = _contactUsService.GetAllContactUs(
                storeId: "",
                vendorId: vendorId,
                customerId: customerId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = contactform.Select(x =>
                {
                    var store = _storeService.GetStoreById(x.StoreId);
                    var m = x.ToModel();
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    m.Enquiry = "";
                    m.Email = m.FullName + " - " + m.Email;
                    m.Store = store != null ? store.Name : "-empty-";
                    return m;
                }),
                Total = contactform.TotalCount
            };
            return Json(gridModel);
        }


        #endregion

        #region Back in stock subscriptions

        [HttpPost]
        public IActionResult BackInStockSubscriptionList(DataSourceRequest command, string customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Content("");

            var subscriptions = _backInStockSubscriptionService.GetAllSubscriptionsByCustomerId(customerId,
                "", command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = subscriptions.Select(x =>
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

                }),
                Total = subscriptions.TotalCount
            };

            return Json(gridModel);
        }

        #endregion

        #region Customer note

        [HttpPost]
        public IActionResult CustomerNotesSelect(string customerId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return Content("");

            //order notes
            var customerNoteModels = new List<CustomerModel.CustomerNote>();
            foreach (var customerNote in _customerService.GetCustomerNotes(customerId)
                .OrderByDescending(on => on.CreatedOnUtc))
            {
                var download = _downloadService.GetDownloadById(customerNote.DownloadId);
                customerNoteModels.Add(new CustomerModel.CustomerNote
                {
                    Id = customerNote.Id,
                    CustomerId = customer.Id,
                    DownloadId = String.IsNullOrEmpty(customerNote.DownloadId) ? "" : customerNote.DownloadId,
                    DownloadGuid = download != null ? download.DownloadGuid : Guid.Empty,
                    DisplayToCustomer = customerNote.DisplayToCustomer,
                    Title = customerNote.Title,
                    Note = customerNote.FormatCustomerNoteText(),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(customerNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }

            var gridModel = new DataSourceResult
            {
                Data = customerNoteModels,
                Total = customerNoteModels.Count
            };

            return Json(gridModel);
        }


        public IActionResult CustomerNoteAdd(string customerId, string downloadId, bool displayToCustomer, string title, string message)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                return Json(new { Result = false });

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return Json(new { Result = false });

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

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult CustomerNoteDelete(string id, string customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                throw new ArgumentException("AccessDenied");

            var customerNote = _customerService.GetCustomerNote(id);
            if (customerNote == null)
                throw new ArgumentException("No customer note found with the specified id");

            _customerService.DeleteCustomerNote(customerNote);

            return new NullJsonResult();
        }


        #endregion

        #region Export / Import

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public IActionResult ExportExcelAll(CustomerListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customers = _customerService.GetAllCustomers(
                customerRoleIds: model.SearchCustomerRoleIds.ToArray(),
                email: model.SearchEmail,
                username: model.SearchUsername,
                firstName: model.SearchFirstName,
                lastName: model.SearchLastName,
                company: model.SearchCompany,
                phone: model.SearchPhone,
                zipPostalCode: model.SearchZipPostalCode,
                loadOnlyWithShoppingCart: false);

            try
            {
                byte[] bytes = _exportManager.ExportCustomersToXlsx(customers);
                return File(bytes, "text/xls", "customers.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public IActionResult ExportExcelSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customers = new List<Customer>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                customers.AddRange(_customerService.GetCustomersByIds(ids));
            }

            byte[] bytes = _exportManager.ExportCustomersToXlsx(customers);
            return File(bytes, "text/xls", "customers.xlsx");
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportxml-all")]
        public IActionResult ExportXmlAll(CustomerListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customers = _customerService.GetAllCustomers(
                customerRoleIds: model.SearchCustomerRoleIds.ToArray(),
                email: model.SearchEmail,
                username: model.SearchUsername,
                firstName: model.SearchFirstName,
                lastName: model.SearchLastName,
                company: model.SearchCompany,
                phone: model.SearchPhone,
                zipPostalCode: model.SearchZipPostalCode,
                loadOnlyWithShoppingCart: false);

            try
            {
                var xml = _exportManager.ExportCustomersToXml(customers);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "customers.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public IActionResult ExportXmlSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customers = new List<Customer>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                customers.AddRange(_customerService.GetCustomersByIds(ids));
            }

            var xml = _exportManager.ExportCustomersToXml(customers);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "customers.xml");
        }

        #endregion
    }
}
