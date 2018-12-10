using DotLiquid;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Directory;
using Grand.Services.Catalog;
using Grand.Services.Directory;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidAuction : Drop
    {
        private Product _product;
        private Bid _bid;

        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;

        public LiquidAuction(IPriceFormatter priceFormatter,
            CurrencySettings currencySettings,
            ICurrencyService currencyService)
        {
            this._priceFormatter = priceFormatter;
            this._currencySettings = currencySettings;
            this._currencyService = currencyService;
        }

        public void SetProperties(Product product, Bid bid = null)
        {
            this._product = product;
            this._bid = bid;
        }

        public string ProductName
        {
            get { return _product.Name; }
        }

        public string Price
        {
            get
            {
                var defaultCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                return _priceFormatter.FormatPrice(_bid.Amount, true, defaultCurrency);
            }
        }

        public string EndTime
        {
            get { return _product.AvailableEndDateTimeUtc.ToString(); }
        }

        public string ProductSeName
        {
            get { return _product.SeName; }
        }
    }
}