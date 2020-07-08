using Grand.Domain.Common;
using Grand.Domain.Shipping;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Web.Features.Models.Checkout;
using Grand.Web.Features.Models.Common;
using Grand.Web.Models.Checkout;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Checkout
{
    public class GetShippingAddressHandler : IRequestHandler<GetShippingAddress, CheckoutShippingAddressModel>
    {
        private readonly IShippingService _shippingService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly ICountryService _countryService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IMediator _mediator;
        private readonly ShippingSettings _shippingSettings;

        public GetShippingAddressHandler(IShippingService shippingService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            ILocalizationService localizationService,
            ICountryService countryService,
            IStoreMappingService storeMappingService,
            IMediator mediator,
            ShippingSettings shippingSettings)
        {
            _shippingService = shippingService;
            _taxService = taxService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _localizationService = localizationService;
            _countryService = countryService;
            _storeMappingService = storeMappingService;
            _mediator = mediator;
            _shippingSettings = shippingSettings;
        }

        public async Task<CheckoutShippingAddressModel> Handle(GetShippingAddress request, CancellationToken cancellationToken)
        {
            var model = new CheckoutShippingAddressModel();
            //allow pickup in store?
            model.AllowPickUpInStore = _shippingSettings.AllowPickUpInStore;
            if (model.AllowPickUpInStore)
            {
                await PreparePickupPoints(model, request);
            }

            await PrepareAddresses(model, request);

            //new address
            model.NewAddress.CountryId = request.SelectedCountryId;
            var countries = await _countryService.GetAllCountriesForShipping();
            model.NewAddress = await _mediator.Send(new GetAddressModel() {
                Language = request.Language,
                Store = request.Store,
                Model = model.NewAddress,
                Address = null,
                ExcludeProperties = false,
                LoadCountries = () => countries,
                PrePopulateWithCustomerFields = request.PrePopulateNewAddressWithCustomerFields,
                Customer = request.Customer,
                OverrideAttributesXml = request.OverrideAttributesXml,
            });
            return model;
        }

        private async Task PreparePickupPoints(CheckoutShippingAddressModel model, GetShippingAddress request)
        {
            var pickupPoints = await _shippingService.LoadActivePickupPoints(request.Store.Id);

            if (pickupPoints.Any())
            {
                foreach (var pickupPoint in pickupPoints)
                {
                    var pickupPointModel = new CheckoutPickupPointModel() {
                        Id = pickupPoint.Id,
                        Name = pickupPoint.Name,
                        Description = pickupPoint.Description,
                        Address = pickupPoint.Address,
                    };
                    if (pickupPoint.PickupFee > 0)
                    {
                        var amount = (await _taxService.GetShippingPrice(pickupPoint.PickupFee, request.Customer)).shippingPrice;
                        amount = await _currencyService.ConvertFromPrimaryStoreCurrency(amount, request.Currency);
                        pickupPointModel.PickupFee = _priceFormatter.FormatShippingPrice(amount, true);
                    }
                    model.PickupPoints.Add(pickupPointModel);
                }
            }

            if (!(await _shippingService.LoadActiveShippingRateComputationMethods(request.Store.Id)).Any())
            {
                if (!pickupPoints.Any())
                {
                    model.Warnings.Add(_localizationService.GetResource("Checkout.ShippingIsNotAllowed"));
                    model.Warnings.Add(_localizationService.GetResource("Checkout.PickupPoints.NotAvailable"));
                }
                model.PickUpInStoreOnly = true;
                model.PickUpInStore = true;
            }
        }

        private async Task PrepareAddresses(CheckoutShippingAddressModel model, GetShippingAddress request)
        {
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
                if (country == null || (country.AllowsShipping && _storeMappingService.Authorize(country)))
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
        }
    }
}
