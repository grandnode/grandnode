using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grand.Domain.Directory;
using Grand.Domain.Shipping;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Web.Features.Models.ShoppingCart;
using Grand.Web.Models.ShoppingCart;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Grand.Web.Features.Handlers.ShoppingCart
{
    public class GetEstimateShippingHandler : IRequestHandler<GetEstimateShipping, EstimateShippingModel>
    {
        private readonly ILocalizationService _localizationService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;

        private readonly ShippingSettings _shippingSettings;

        public GetEstimateShippingHandler(
            ILocalizationService localizationService, 
            ICountryService countryService, 
            IStateProvinceService stateProvinceService, 
            ShippingSettings shippingSettings)
        {
            _localizationService = localizationService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _shippingSettings = shippingSettings;
        }

        public async Task<EstimateShippingModel> Handle(GetEstimateShipping request, CancellationToken cancellationToken)
        {
            var model = new EstimateShippingModel();
            model.Enabled = request.Cart.Any() && request.Cart.RequiresShipping() && _shippingSettings.EstimateShippingEnabled;
            if (model.Enabled)
            {
                //countries
                var defaultEstimateCountryId = (request.SetEstimateShippingDefaultAddress && request.Customer.ShippingAddress != null) ? request.Customer.ShippingAddress.CountryId : model.CountryId;
                if (string.IsNullOrEmpty(defaultEstimateCountryId))
                    defaultEstimateCountryId = request.Store.DefaultCountryId;

                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "" });
                foreach (var c in await _countryService.GetAllCountriesForShipping(request.Language.Id))
                    model.AvailableCountries.Add(new SelectListItem {
                        Text = c.GetLocalized(x => x.Name, request.Language.Id),
                        Value = c.Id.ToString(),
                        Selected = c.Id == defaultEstimateCountryId
                    });
                //states
                string defaultEstimateStateId = (request.SetEstimateShippingDefaultAddress && request.Customer.ShippingAddress != null) ? request.Customer.ShippingAddress.StateProvinceId : model.StateProvinceId;
                var states = !String.IsNullOrEmpty(defaultEstimateCountryId) ? await _stateProvinceService.GetStateProvincesByCountryId(defaultEstimateCountryId, request.Language.Id) : new List<StateProvince>();
                if (states.Any())
                    foreach (var s in states)
                        model.AvailableStates.Add(new SelectListItem {
                            Text = s.GetLocalized(x => x.Name, request.Language.Id),
                            Value = s.Id.ToString(),
                            Selected = s.Id == defaultEstimateStateId
                        });
                else
                    model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "" });

                if (request.SetEstimateShippingDefaultAddress && request.Customer.ShippingAddress != null)
                    model.ZipPostalCode = request.Customer.ShippingAddress.ZipPostalCode;
            }
            return model;
        }
    }
}
