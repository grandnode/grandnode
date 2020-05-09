using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Features.Models.Catalog;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Catalog
{
    public class GetFormatBasePriceHandler : IRequestHandler<GetFormatBasePrice, string>
    {
        private readonly ILocalizationService _localizationService;
        private readonly IMeasureService _measureService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;

        public GetFormatBasePriceHandler(
            ILocalizationService localizationService,
            IMeasureService measureService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter)
        {
            _localizationService = localizationService;
            _measureService = measureService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
        }

        public async Task<string> Handle(GetFormatBasePrice request, CancellationToken cancellationToken)
        {
            if (request.Product == null)
                throw new ArgumentNullException("product");

            if (!request.Product.BasepriceEnabled)
                return null;

            var productAmount = request.Product.BasepriceAmount;
            //Amount in product cannot be 0
            if (productAmount == 0)
                return null;
            var referenceAmount = request.Product.BasepriceBaseAmount;
            var productUnit = await _measureService.GetMeasureWeightById(request.Product.BasepriceUnitId);
            //measure weight cannot be loaded
            if (productUnit == null)
                return null;
            var referenceUnit = await _measureService.GetMeasureWeightById(request.Product.BasepriceBaseUnitId);
            //measure weight cannot be loaded
            if (referenceUnit == null)
                return null;

            request.ProductPrice = request.ProductPrice.HasValue ? request.ProductPrice.Value : request.Product.Price;

            decimal basePrice = request.ProductPrice.Value /
                //do not round. otherwise, it can cause issues
                await _measureService.ConvertWeight(productAmount, productUnit, referenceUnit, false) *
                referenceAmount;
            decimal basePriceInCurrentCurrency = await _currencyService.ConvertFromPrimaryStoreCurrency(basePrice, request.Currency);
            string basePriceStr = _priceFormatter.FormatPrice(basePriceInCurrentCurrency, true, false);

            var result = string.Format(_localizationService.GetResource("Products.BasePrice"),
                basePriceStr, referenceAmount.ToString("G29"), referenceUnit.Name);

            return result;
        }
    }
}
