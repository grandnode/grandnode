using DotLiquid;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Directory;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidAuction : Drop
    {
        private Product _product;
        private Bid _bid;

        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;

        public LiquidAuction(Product product, Bid bid = null)
        {
            this._priceFormatter = EngineContext.Current.Resolve<IPriceFormatter>();
            this._currencyService = EngineContext.Current.Resolve<ICurrencyService>();
            this._currencySettings = EngineContext.Current.Resolve<CurrencySettings>();

            this._product = product;
            this._bid = bid;

            AdditionalTokens = new Dictionary<string, string>();
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

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}