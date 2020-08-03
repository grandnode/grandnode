using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Features.Models.Common;
using Grand.Web.Models.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Grand.Web.Features.Handlers.Common
{
    public class GetAddressModelHandler : IRequestHandler<GetAddressModel, AddressModel>
    {
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILocalizationService _localizationService;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeParser _addressAttributeParser;

        private readonly AddressSettings _addressSettings;

        public GetAddressModelHandler(
            IAddressAttributeFormatter addressAttributeFormatter, 
            ICountryService countryService, 
            IStateProvinceService stateProvinceService, 
            ILocalizationService localizationService, 
            IAddressAttributeService addressAttributeService, 
            IAddressAttributeParser addressAttributeParser, 
            AddressSettings addressSettings)
        {
            _addressAttributeFormatter = addressAttributeFormatter;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _localizationService = localizationService;
            _addressAttributeService = addressAttributeService;
            _addressAttributeParser = addressAttributeParser;
            _addressSettings = addressSettings;
        }

        public async Task<AddressModel> Handle(GetAddressModel request, CancellationToken cancellationToken)
        {
            var model = request.Model ?? new AddressModel();

            //prepare address model
            await PrepareAddressModel(model, request.Address, request.ExcludeProperties, 
                request.LoadCountries, request.PrePopulateWithCustomerFields, request.Customer, request.Language, request.Store);

            //customer attribute services
            await PrepareCustomAddressAttributes(model, request.Address, request.Language,
                request.OverrideAttributesXml);

            if (request.Address != null)
            {
                model.FormattedCustomAddressAttributes = await _addressAttributeFormatter.FormatAttributes(request.Address.CustomAttributes);
            }
            return model;
        }

        private async Task PrepareAddressModel(AddressModel model,
            Address address, bool excludeProperties,
            Func<IList<Country>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            Language language = null,
            Store store = null)
        {
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
                model.CountryName = country != null ? country.GetLocalized(x => x.Name, language.Id) : null;

                model.StateProvinceId = address.StateProvinceId;
                StateProvince state = null;
                if (!String.IsNullOrEmpty(address.StateProvinceId))
                    state = await _stateProvinceService.GetStateProvinceById(address.StateProvinceId);
                model.StateProvinceName = state != null ? state.GetLocalized(x => x.Name, language.Id) : null;

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
                model.FirstName = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.FirstName);
                model.LastName = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastName);
                model.Company = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Company);
                model.VatNumber = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.VatNumber);
                model.Address1 = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress);
                model.Address2 = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress2);
                model.ZipPostalCode = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.ZipPostalCode);
                model.City = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.City);
                model.CountryId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CountryId);
                model.StateProvinceId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StateProvinceId);
                model.PhoneNumber = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Phone);
                model.FaxNumber = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Fax);
            }

            //countries and states
            if (_addressSettings.CountryEnabled && loadCountries != null)
            {

                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "" });
                foreach (var c in loadCountries())
                {
                    model.AvailableCountries.Add(new SelectListItem {
                        Text = c.GetLocalized(x => x.Name, language.Id),
                        Value = c.Id.ToString(),
                        Selected = !string.IsNullOrEmpty(model.CountryId) ? c.Id == model.CountryId : (c.Id == store.DefaultCountryId)
                    });
                }

                if (_addressSettings.StateProvinceEnabled)
                {
                    var states = await _stateProvinceService
                        .GetStateProvincesByCountryId(!string.IsNullOrEmpty(model.CountryId) ? model.CountryId : store.DefaultCountryId, language.Id);

                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem {
                                Text = s.GetLocalized(x => x.Name, language.Id),
                                Value = s.Id.ToString(),
                                Selected = (s.Id == model.StateProvinceId)
                            });
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

            //form fields
            model.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.CompanyRequired = _addressSettings.CompanyRequired;
            model.VatNumberEnabled = _addressSettings.VatNumberEnabled;
            model.VatNumberRequired = _addressSettings.VatNumberRequired;
            model.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.CityEnabled = _addressSettings.CityEnabled;
            model.CityRequired = _addressSettings.CityRequired;
            model.CountryEnabled = _addressSettings.CountryEnabled;
            model.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.PhoneRequired = _addressSettings.PhoneRequired;
            model.FaxEnabled = _addressSettings.FaxEnabled;
            model.FaxRequired = _addressSettings.FaxRequired;
        }

        private async Task PrepareCustomAddressAttributes(AddressModel model, Address address,
            Language language,
            string overrideAttributesXml = "")
        {

            var attributes = await _addressAttributeService.GetAllAddressAttributes();
            foreach (var attribute in attributes)
            {
                var attributeModel = new AddressAttributeModel {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name, language.Id),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = attribute.AddressAttributeValues;
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new AddressAttributeValueModel {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name, language.Id),
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                //set already selected attributes
                var selectedAddressAttributes = !string.IsNullOrEmpty(overrideAttributesXml) ?
                    overrideAttributesXml :
                    (address != null ? address.CustomAttributes : null);

                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                        {
                            if (!string.IsNullOrEmpty(selectedAddressAttributes))
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
                            if (!string.IsNullOrEmpty(selectedAddressAttributes))
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
    }
}
