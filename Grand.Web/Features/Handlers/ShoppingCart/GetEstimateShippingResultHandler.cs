using Grand.Domain.Common;
using Grand.Domain.Shipping;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Shipping;
using Grand.Services.Tax;
using Grand.Web.Features.Models.ShoppingCart;
using Grand.Web.Models.ShoppingCart;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.ShoppingCart
{
    public class GetEstimateShippingResultHandler : IRequestHandler<GetEstimateShippingResult, EstimateShippingResultModel>
    {
        private readonly IShippingService _shippingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceFormatter _priceFormatter;

        private readonly ShippingSettings _shippingSettings;

        public GetEstimateShippingResultHandler(
            IShippingService shippingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ITaxService taxService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IPriceFormatter priceFormatter,
            ShippingSettings shippingSettings)
        {
            _shippingService = shippingService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _taxService = taxService;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _priceFormatter = priceFormatter;
            _shippingSettings = shippingSettings;
        }

        public async Task<EstimateShippingResultModel> Handle(GetEstimateShippingResult request, CancellationToken cancellationToken)
        {
            var model = new EstimateShippingResultModel();

            if (request.Cart.RequiresShipping())
            {
                var address = new Address {
                    CountryId = request.CountryId,
                    StateProvinceId = request.StateProvinceId,
                    ZipPostalCode = request.ZipPostalCode,
                };
                GetShippingOptionResponse getShippingOptionResponse = await _shippingService
                    .GetShippingOptions(request.Customer, request.Cart, address, "", request.Store);
                if (!getShippingOptionResponse.Success)
                {
                    foreach (var error in getShippingOptionResponse.Errors)
                        model.Warnings.Add(error);
                }
                else
                {
                    if (getShippingOptionResponse.ShippingOptions.Any())
                    {
                        foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                        {
                            var soModel = new EstimateShippingResultModel.ShippingOptionModel {
                                Name = shippingOption.Name,
                                Description = shippingOption.Description,

                            };

                            //calculate discounted and taxed rate
                            var total = await _orderTotalCalculationService.AdjustShippingRate(shippingOption.Rate, request.Cart);
                            List<AppliedDiscount> appliedDiscounts = total.appliedDiscounts;
                            decimal shippingTotal = total.shippingRate;

                            decimal rateBase = (await _taxService.GetShippingPrice(shippingTotal, request.Customer)).shippingPrice;
                            decimal rate = await _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, request.Currency);
                            soModel.Price = _priceFormatter.FormatShippingPrice(rate, true);
                            model.ShippingOptions.Add(soModel);
                        }

                        //pickup in store?
                        if (_shippingSettings.AllowPickUpInStore)
                        {
                            var pickupPoints = await _shippingService.GetAllPickupPoints();
                            if (pickupPoints.Count > 0)
                            {
                                var soModel = new EstimateShippingResultModel.ShippingOptionModel {
                                    Name = _localizationService.GetResource("Checkout.PickUpInStore"),
                                    Description = _localizationService.GetResource("Checkout.PickUpInStore.Description"),
                                };

                                decimal shippingTotal = pickupPoints.Max(x => x.PickupFee);
                                decimal rateBase = (await _taxService.GetShippingPrice(shippingTotal, request.Customer)).shippingPrice;
                                decimal rate = await _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, request.Currency);
                                soModel.Price = _priceFormatter.FormatShippingPrice(rate, true);
                                model.ShippingOptions.Add(soModel);
                            }
                        }
                    }
                    else
                    {
                        model.Warnings.Add(_localizationService.GetResource("Checkout.ShippingIsNotAllowed"));
                    }
                }
            }
            return model;
        }
    }
}
