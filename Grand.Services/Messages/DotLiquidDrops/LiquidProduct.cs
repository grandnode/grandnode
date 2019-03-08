using DotLiquid;
using Grand.Core.Domain.Catalog;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Services.Stores;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidProduct : Drop
    {
        private Product _product;
        private string _languageId;
        private string _storeId;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IStoreService _storeService;

        public LiquidProduct(Product product, string languageId, string storeId)
        {
            this._storeService = EngineContext.Current.Resolve<IStoreService>();
            this._currencyService = EngineContext.Current.Resolve<ICurrencyService>();
            this._priceFormatter = EngineContext.Current.Resolve<IPriceFormatter>();

            this._product = product;
            this._languageId = languageId;
            this._storeId = storeId;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string Id
        {
            get { return _product.Id.ToString(); }
        }

        public string Name
        {
            get { return _product.GetLocalized(x => x.Name, _languageId); }
        }

        public string ShortDescription
        {
            get { return _product.GetLocalized(x => x.ShortDescription, _languageId); }
        }

        public string SKU
        {
            get { return _product.Sku; }
        }

        public string StockQuantity
        {
            get { return _product.GetTotalStockQuantity().ToString(); }
        }

        public string Price
        {
            get
            {
                var defaultCurrency = _currencyService.GetPrimaryStoreCurrency();
                return _priceFormatter.FormatPrice(_product.Price, true, defaultCurrency);
            }
        }

        public string ProductURLForCustomer
        {
            get { return string.Format("{0}{1}", _storeService.GetStoreUrl(_storeId), _product.GetSeName()); }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}