using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Vendors;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Interfaces;
using Grand.Web.Models.Common;
using Grand.Web.Models.Vendors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class AddressViewModelService : IAddressViewModelService
    {
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;

        private readonly AddressSettings _addressSettings;

        public AddressViewModelService(ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeFormatter addressAttributeFormatter,
            IWorkContext workContext,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            AddressSettings addressSettings
            )
        {
            _localizationService = localizationService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _addressAttributeService = addressAttributeService;
            _addressAttributeParser = addressAttributeParser;
            _addressAttributeFormatter = addressAttributeFormatter;
            _workContext = workContext;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
            _addressSettings = addressSettings;
        }

        public virtual AddressSettings AddressSettings()
        {
            return _addressSettings;
        }

        //address
        /// <summary>
        /// Prepare address model
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="address">Address</param>
        /// <param name="excludeProperties">A value indicating whether to exclude properties</param>
        /// <param name="loadCountries">A function to load countries  (used to prepare a select list). null to don't prepare the list.</param>
        /// <param name="prePopulateWithCustomerFields">A value indicating whether to pre-populate an address with customer fields entered during registration. It's used only when "address" parameter is set to "null"</param>
        /// <param name="customer">Customer record which will be used to pre-populate address. Used only when "prePopulateWithCustomerFields" is "true".</param>
        public virtual async Task PrepareModel(AddressModel model,
            Address address, bool excludeProperties,
            Func<IList<Country>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            string overrideAttributesXml = "")
        {
            //prepare address model
            await PrepareAddressModel(model, address, excludeProperties, loadCountries, prePopulateWithCustomerFields, customer, _addressSettings);

            //customer attribute services
            await PrepareCustomAddressAttributes(model, address, overrideAttributesXml);
            if (address != null)
            {
                model.FormattedCustomAddressAttributes = await _addressAttributeFormatter.FormatAttributes(address.CustomAttributes);
            }
        }

        public virtual async Task PrepareAddressModel(AddressModel model,
            Address address, bool excludeProperties,
            Func<IList<Country>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            AddressSettings addressSettings = null)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (!excludeProperties && address != null)
            {
                model.Id = address.Id;
                model.FirstName = address.FirstName;
                model.LastName = address.LastName;
                model.Email = address.Email;
                model.Company = address.Company;
                model.VatNumber = address.VatNumber;
                model.CountryId = address.CountryId;
                Country country = null;
                if (!String.IsNullOrEmpty(address.CountryId))
                    country = await _countryService.GetCountryById(address.CountryId);
                model.CountryName = country != null ? country.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id) : null;

                model.StateProvinceId = address.StateProvinceId;
                StateProvince state = null;
                if (!String.IsNullOrEmpty(address.StateProvinceId))
                    state = await _stateProvinceService.GetStateProvinceById(address.StateProvinceId);
                model.StateProvinceName = state != null ? state.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id) : null;

                model.City = address.City;
                model.Address1 = address.Address1;
                model.Address2 = address.Address2;
                model.ZipPostalCode = address.ZipPostalCode;
                model.PhoneNumber = address.PhoneNumber;
                model.FaxNumber = address.FaxNumber;
            }

            if (address == null && prePopulateWithCustomerFields)
            {
                if (customer == null)
                    throw new Exception("Customer cannot be null when prepopulating an address");
                model.Email = customer.Email;
                model.FirstName = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.FirstName);
                model.LastName = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.LastName);
                model.Company = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Company);
                model.VatNumber = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.VatNumber);
                model.Address1 = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StreetAddress);
                model.Address2 = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StreetAddress2);
                model.ZipPostalCode = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.ZipPostalCode);
                model.City = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.City);
                model.CountryId = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.CountryId);
                model.StateProvinceId = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StateProvinceId);
                model.PhoneNumber = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Phone);
                model.FaxNumber = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Fax);
            }

            //countries and states
            if (addressSettings.CountryEnabled && loadCountries != null)
            {

                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "" });
                foreach (var c in loadCountries())
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        Value = c.Id.ToString(),
                        Selected = !string.IsNullOrEmpty(model.CountryId) ? c.Id == model.CountryId : (c.Id == _storeContext.CurrentStore.DefaultCountryId)
                    });
                }

                if (addressSettings.StateProvinceEnabled)
                {
                    var languageId = _workContext.WorkingLanguage.Id;
                    var states = await _stateProvinceService
                        .GetStateProvincesByCountryId(!string.IsNullOrEmpty(model.CountryId) ? model.CountryId : _storeContext.CurrentStore.DefaultCountryId, languageId);
                        
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem
                            {
                                Text = s.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                                Value = s.Id.ToString(),
                                Selected = (s.Id == model.StateProvinceId)
                            });
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

            //form fields
            model.CompanyEnabled = addressSettings.CompanyEnabled;
            model.CompanyRequired = addressSettings.CompanyRequired;
            model.VatNumberEnabled = addressSettings.VatNumberEnabled;
            model.VatNumberRequired = addressSettings.VatNumberRequired;
            model.StreetAddressEnabled = addressSettings.StreetAddressEnabled;
            model.StreetAddressRequired = addressSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = addressSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = addressSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = addressSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = addressSettings.ZipPostalCodeRequired;
            model.CityEnabled = addressSettings.CityEnabled;
            model.CityRequired = addressSettings.CityRequired;
            model.CountryEnabled = addressSettings.CountryEnabled;
            model.StateProvinceEnabled = addressSettings.StateProvinceEnabled;
            model.PhoneEnabled = addressSettings.PhoneEnabled;
            model.PhoneRequired = addressSettings.PhoneRequired;
            model.FaxEnabled = addressSettings.FaxEnabled;
            model.FaxRequired = addressSettings.FaxRequired;
        }

        public virtual async Task PrepareVendorAddressModel(VendorAddressModel model,
            Address address, bool excludeProperties,
            Func<IList<Country>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            VendorSettings vendorSettings = null)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (!excludeProperties && address != null)
            {
                model.Company = address.Company;
                model.CountryId = address.CountryId;
                Country country = null;
                if (!String.IsNullOrEmpty(address.CountryId))
                    country = await _countryService.GetCountryById(address.CountryId);
                model.CountryName = country != null ? country.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id) : null;

                model.StateProvinceId = address.StateProvinceId;
                StateProvince state = null;
                if (!String.IsNullOrEmpty(address.StateProvinceId))
                    state = await _stateProvinceService.GetStateProvinceById(address.StateProvinceId);
                model.StateProvinceName = state != null ? state.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id) : null;

                model.City = address.City;
                model.Address1 = address.Address1;
                model.Address2 = address.Address2;
                model.ZipPostalCode = address.ZipPostalCode;
                model.PhoneNumber = address.PhoneNumber;
                model.FaxNumber = address.FaxNumber;
            }

            if (address == null && prePopulateWithCustomerFields)
            {
                if (customer == null)
                    throw new Exception("Customer cannot be null when prepopulating an address");
                model.Company = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Company);
                model.Address1 = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StreetAddress);
                model.Address2 = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StreetAddress2);
                model.ZipPostalCode = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.ZipPostalCode);
                model.City = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.City);
                model.PhoneNumber = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Phone);
                model.FaxNumber = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.Fax);

                if(vendorSettings.CountryEnabled)
                    model.CountryId = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.CountryId);

                if (vendorSettings.StateProvinceEnabled)
                    model.StateProvinceId = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.StateProvinceId);
            }

            //countries and states
            if (vendorSettings.CountryEnabled && loadCountries != null)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "" });
                foreach (var c in loadCountries())
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (vendorSettings.StateProvinceEnabled)
                {
                    var languageId = _workContext.WorkingLanguage.Id;
                    var states = await _stateProvinceService
                        .GetStateProvincesByCountryId(!String.IsNullOrEmpty(model.CountryId) ? model.CountryId : "", languageId);
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem
                            {
                                Text = s.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                                Value = s.Id.ToString(),
                                Selected = (s.Id == model.StateProvinceId)
                            });
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

            //form fields
            model.CompanyEnabled = vendorSettings.CompanyEnabled;
            model.CompanyRequired = vendorSettings.CompanyRequired;
            model.StreetAddressEnabled = vendorSettings.StreetAddressEnabled;
            model.StreetAddressRequired = vendorSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = vendorSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = vendorSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = vendorSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = vendorSettings.ZipPostalCodeRequired;
            model.CityEnabled = vendorSettings.CityEnabled;
            model.CityRequired = vendorSettings.CityRequired;
            model.CountryEnabled = vendorSettings.CountryEnabled;
            model.StateProvinceEnabled = vendorSettings.StateProvinceEnabled;
            model.PhoneEnabled = vendorSettings.PhoneEnabled;
            model.PhoneRequired = vendorSettings.PhoneRequired;
            model.FaxEnabled = vendorSettings.FaxEnabled;
            model.FaxRequired = vendorSettings.FaxRequired;
        }


        public virtual async Task PrepareCustomAddressAttributes(AddressModel model,
            Address address,
            string overrideAttributesXml = "")
        {
            
            var attributes = await _addressAttributeService.GetAllAddressAttributes();
            foreach (var attribute in attributes)
            {
                var attributeModel = new AddressAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = attribute.AddressAttributeValues; 
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new AddressAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                //set already selected attributes
                var selectedAddressAttributes = !String.IsNullOrEmpty(overrideAttributesXml) ?
                    overrideAttributesXml :
                    (address != null ? address.CustomAttributes : null);

                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                        {
                            if (!String.IsNullOrEmpty(selectedAddressAttributes))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues = await _addressAttributeParser.ParseAddressAttributeValues(selectedAddressAttributes);
                                foreach (var attributeValue in selectedValues)
                                    if (attributeModel.Id == attributeValue.AddressAttributeId)
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
                            if (!String.IsNullOrEmpty(selectedAddressAttributes))
                            {
                                var enteredText = _addressAttributeParser.ParseValues(selectedAddressAttributes, attribute.Id);
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

                model.CustomAddressAttributes.Add(attributeModel);
            }
        }

        public virtual async Task<string> ParseCustomAddressAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            string attributesXml = "";
            var attributes = await _addressAttributeService.GetAllAddressAttributes();
            foreach (var attribute in attributes)
            {
                string controlId = string.Format("address_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                attributesXml = _addressAttributeParser.AddAddressAttribute(attributesXml,
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
                                        attributesXml = _addressAttributeParser.AddAddressAttribute(attributesXml,
                                            attribute, item);
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = attribute.AddressAttributeValues;
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _addressAttributeParser.AddAddressAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
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
                                attributesXml = _addressAttributeParser.AddAddressAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported address attributes
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        public virtual async Task<IList<string>> GetAttributeWarnings(string attributesXml)
        {
            return await _addressAttributeParser.GetAttributeWarnings(attributesXml);
        }

    }
}