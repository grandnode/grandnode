using Grand.Domain.Customers;
using Grand.Domain.Shipping;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Orders;
using Grand.Services.Shipping;
using Grand.Services.Tax;
using Grand.Web.Features.Models.Checkout;
using Grand.Web.Models.Checkout;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Checkout
{
    public class GetShippingMethodHandler : IRequestHandler<GetShippingMethod, CheckoutShippingMethodModel>
    {
        private readonly IShippingService _shippingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;

        private readonly ShippingSettings _shippingSettings;

        public GetShippingMethodHandler(IShippingService shippingService, 
            IGenericAttributeService genericAttributeService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            ShippingSettings shippingSettings)
        {
            _shippingService = shippingService;
            _genericAttributeService = genericAttributeService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _taxService = taxService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _shippingSettings = shippingSettings;
        }

        public async Task<CheckoutShippingMethodModel> Handle(GetShippingMethod request, CancellationToken cancellationToken)
        {
            var model = new CheckoutShippingMethodModel();

            var getShippingOptionResponse = await _shippingService
                .GetShippingOptions(request.Customer, request.Cart, request.ShippingAddress,
                "", request.Store);

            if (getShippingOptionResponse.Success)
            {
                //performance optimization. cache returned shipping options.
                //we'll use them later (after a customer has selected an option).
                await _genericAttributeService.SaveAttribute(request.Customer,
                                                       SystemCustomerAttributeNames.OfferedShippingOptions,
                                                       getShippingOptionResponse.ShippingOptions,
                                                       request.Store.Id);

                foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                {
                    var soModel = new CheckoutShippingMethodModel.ShippingMethodModel {
                        Name = shippingOption.Name,
                        Description = shippingOption.Description,
                        ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName,
                        ShippingOption = shippingOption,
                    };

                    //adjust rate
                    var shippingTotal = (await _orderTotalCalculationService.AdjustShippingRate(
                        shippingOption.Rate, request.Cart)).shippingRate;

                    decimal rateBase = (await _taxService.GetShippingPrice(shippingTotal, request.Customer)).shippingPrice;
                    decimal rate = await _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, request.Currency);
                    soModel.Fee = _priceFormatter.FormatShippingPrice(rate, true);

                    model.ShippingMethods.Add(soModel);
                }

                //find a selected (previously) shipping method
                var selectedShippingOption = request.Customer.GetAttributeFromEntity<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, request.Store.Id);
                if (selectedShippingOption != null)
                {
                    var shippingOptionToSelect = model.ShippingMethods.ToList()
                        .Find(so =>
                           !String.IsNullOrEmpty(so.Name) &&
                           so.Name.Equals(selectedShippingOption.Name, StringComparison.OrdinalIgnoreCase) &&
                           !String.IsNullOrEmpty(so.ShippingRateComputationMethodSystemName) &&
                           so.ShippingRateComputationMethodSystemName.Equals(selectedShippingOption.ShippingRateComputationMethodSystemName, StringComparison.OrdinalIgnoreCase));
                    if (shippingOptionToSelect != null)
                    {
                        shippingOptionToSelect.Selected = true;
                    }
                }
                //if no option has been selected, let's do it for the first one
                if (model.ShippingMethods.FirstOrDefault(so => so.Selected) == null)
                {
                    var shippingOptionToSelect = model.ShippingMethods.FirstOrDefault();
                    if (shippingOptionToSelect != null)
                    {
                        shippingOptionToSelect.Selected = true;
                    }
                }

                //notify about shipping from multiple locations
                if (_shippingSettings.NotifyCustomerAboutShippingFromMultipleLocations)
                {
                    model.NotifyCustomerAboutShippingFromMultipleLocations = getShippingOptionResponse.ShippingFromMultipleLocations;
                }
            }
            else
            {
                foreach (var error in getShippingOptionResponse.Errors)
                    model.Warnings.Add(error);
            }

            return model;
        }
    }
}
