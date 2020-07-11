using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Vendors;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Features.Models.Common;
using Grand.Web.Models.Vendors;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Common
{
    public class GetVendorAddressHandler : IRequestHandler<GetVendorAddress, VendorAddressModel>
    {
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILocalizationService _localizationService;

        private readonly VendorSettings _vendorSettings;

        public GetVendorAddressHandler(
            ICountryService countryService, 
            IStateProvinceService stateProvinceService, 
            ILocalizationService localizationService, 
            VendorSettings vendorSettings)
        {
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _localizationService = localizationService;
            _vendorSettings = vendorSettings;
        }

        public async Task<VendorAddressModel> Handle(GetVendorAddress request, CancellationToken cancellationToken)
        {
            var model = request.Model ?? new VendorAddressModel();

            if (!request.ExcludeProperties && request.Address != null)
            {
                model.Company = request.Address.Company;
                model.CountryId = request.Address.CountryId;
                Country country = null;
                if (!String.IsNullOrEmpty(request.Address.CountryId))
                    country = await _countryService.GetCountryById(request.Address.CountryId);
                model.CountryName = country != null ? country.GetLocalized(x => x.Name, request.Language.Id) : null;

                model.StateProvinceId = request.Address.StateProvinceId;
                StateProvince state = null;
                if (!String.IsNullOrEmpty(request.Address.StateProvinceId))
                    state = await _stateProvinceService.GetStateProvinceById(request.Address.StateProvinceId);
                model.StateProvinceName = state != null ? state.GetLocalized(x => x.Name, request.Language.Id) : null;

                model.City = request.Address.City;
                model.Address1 = request.Address.Address1;
                model.Address2 = request.Address.Address2;
                model.ZipPostalCode = request.Address.ZipPostalCode;
                model.PhoneNumber = request.Address.PhoneNumber;
                model.FaxNumber = request.Address.FaxNumber;
            }

            if (request.Address == null && request.PrePopulateWithCustomerFields)
            {
                if (request.Customer == null)
                    throw new Exception("Customer cannot be null when prepopulating an address");
                model.Company = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Company);
                model.Address1 = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress);
                model.Address2 = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress2);
                model.ZipPostalCode = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.ZipPostalCode);
                model.City = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.City);
                model.PhoneNumber = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Phone);
                model.FaxNumber = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Fax);

                if (_vendorSettings.CountryEnabled)
                    model.CountryId = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CountryId);

                if (_vendorSettings.StateProvinceEnabled)
                    model.StateProvinceId = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StateProvinceId);
            }

            //countries and states
            if (_vendorSettings.CountryEnabled && request.LoadCountries != null)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "" });
                foreach (var c in request.LoadCountries())
                {
                    model.AvailableCountries.Add(new SelectListItem {
                        Text = c.GetLocalized(x => x.Name, request.Language.Id),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_vendorSettings.StateProvinceEnabled)
                {
                    var states = await _stateProvinceService
                        .GetStateProvincesByCountryId(!String.IsNullOrEmpty(model.CountryId) ? model.CountryId : "", request.Language.Id);
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem {
                                Text = s.GetLocalized(x => x.Name, request.Language.Id),
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
            model.CompanyEnabled = _vendorSettings.CompanyEnabled;
            model.CompanyRequired = _vendorSettings.CompanyRequired;
            model.StreetAddressEnabled = _vendorSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _vendorSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _vendorSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _vendorSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _vendorSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _vendorSettings.ZipPostalCodeRequired;
            model.CityEnabled = _vendorSettings.CityEnabled;
            model.CityRequired = _vendorSettings.CityRequired;
            model.CountryEnabled = _vendorSettings.CountryEnabled;
            model.StateProvinceEnabled = _vendorSettings.StateProvinceEnabled;
            model.PhoneEnabled = _vendorSettings.PhoneEnabled;
            model.PhoneRequired = _vendorSettings.PhoneRequired;
            model.FaxEnabled = _vendorSettings.FaxEnabled;
            model.FaxRequired = _vendorSettings.FaxRequired;

            return model;
        }
    }
}
