using Grand.Core.Domain.Common;
using Grand.Core.Domain.Shipping;
using Grand.Services.Directory;
using Grand.Services.Orders;
using Grand.Services.Stores;
using Grand.Web.Features.Models.Checkout;
using Grand.Web.Interfaces;
using Grand.Web.Models.Checkout;
using Grand.Web.Models.Common;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Checkout
{
    public class GetBillingAddressHandler : IRequestHandler<GetBillingAddress, CheckoutBillingAddressModel>
    {
        private readonly ShippingSettings _shippingSettings;
        private readonly ICountryService _countryService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAddressViewModelService _addressViewModelService;


        public GetBillingAddressHandler(ShippingSettings shippingSettings,
            ICountryService countryService,
            IStoreMappingService storeMappingService,
            IAddressViewModelService addressViewModelService)
        {
            _shippingSettings = shippingSettings;
            _countryService = countryService;
            _storeMappingService = storeMappingService;
            _addressViewModelService = addressViewModelService;
        }

        public async Task<CheckoutBillingAddressModel> Handle(GetBillingAddress request, CancellationToken cancellationToken)
        {
            var model = new CheckoutBillingAddressModel();
            model.ShipToSameAddressAllowed = _shippingSettings.ShipToSameAddress && request.Cart.RequiresShipping();
            model.ShipToSameAddress = true;

            //existing addresses
            var addresses = new List<Address>();
            foreach (var item in request.Customer.Addresses)
            {
                if (string.IsNullOrEmpty(item.CountryId))
                {
                    addresses.Add(item);
                    continue;
                }
                var country = await _countryService.GetCountryById(item.CountryId);
                if (country == null || (country.AllowsBilling && _storeMappingService.Authorize(country)))
                {
                    addresses.Add(item);
                    continue;
                }
            }

            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                await _addressViewModelService.PrepareModel(model: addressModel, address: address, excludeProperties: false);
                model.ExistingAddresses.Add(addressModel);
            }

            //new address
            model.NewAddress.CountryId = request.SelectedCountryId;
            var countries = await _countryService.GetAllCountriesForBilling(request.Language.Id);
            await _addressViewModelService.PrepareModel(model: model.NewAddress, address: null, excludeProperties: false,
                loadCountries: () => countries,
                prePopulateWithCustomerFields: request.PrePopulateNewAddressWithCustomerFields,
                customer: request.Customer,
                overrideAttributesXml: request.OverrideAttributesXml
                );

            return model;
        }
    }
}
