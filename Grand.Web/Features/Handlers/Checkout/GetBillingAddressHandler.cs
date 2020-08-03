using Grand.Domain.Common;
using Grand.Domain.Shipping;
using Grand.Services.Directory;
using Grand.Services.Orders;
using Grand.Services.Stores;
using Grand.Web.Features.Models.Checkout;
using Grand.Web.Features.Models.Common;
using Grand.Web.Models.Checkout;
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
        private readonly IMediator _mediator;

        public GetBillingAddressHandler(ShippingSettings shippingSettings,
            ICountryService countryService,
            IStoreMappingService storeMappingService,
            IMediator mediator)
        {
            _shippingSettings = shippingSettings;
            _countryService = countryService;
            _storeMappingService = storeMappingService;
            _mediator = mediator;
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
                var addressModel = await _mediator.Send(new GetAddressModel() {
                    Language = request.Language,
                    Store = request.Store,
                    Model = null,
                    Address = address,
                    ExcludeProperties = false,
                });
                model.ExistingAddresses.Add(addressModel);
            }

            //new address
            model.NewAddress.CountryId = request.SelectedCountryId;
            var countries = await _countryService.GetAllCountriesForBilling(request.Language.Id);

            model.NewAddress = await _mediator.Send(new GetAddressModel() {
                Language = request.Language,
                Store = request.Store,
                Model = model.NewAddress,
                Address = null,
                ExcludeProperties = false,
                PrePopulateWithCustomerFields = request.PrePopulateNewAddressWithCustomerFields,
                LoadCountries = () => countries,
                Customer = request.Customer,
                OverrideAttributesXml = request.OverrideAttributesXml
            });

            return model;
        }
    }
}
