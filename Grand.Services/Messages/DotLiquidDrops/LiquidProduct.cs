using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Directory;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Services.Stores;
using System;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidProduct : Drop
    {
        private Product _product;
        private string _languageId;

        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly CurrencySettings _currencySettings;

        public LiquidProduct(IStoreContext storeContext,
            IStoreService storeService,
            CurrencySettings currencySettings,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter)
        {
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._currencySettings = currencySettings;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
        }

        public void SetProperties(Product product, string languageId)
        {
            this._product = product;
            this._languageId = languageId;
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
                var defaultCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                return _priceFormatter.FormatPrice(_product.Price, true, defaultCurrency);
            }
        }

        public string ProductURLForCustomer
        {
            get { return string.Format("{0}{1}", GetStoreUrl(), _product.GetSeName()); }
        }

        /// <summary>
        /// Get store URL
        /// </summary>
        /// <param name="storeId">Store identifier; Pass 0 to load URL of the current store</param>
        /// <param name="useSsl">Use SSL</param>
        /// <returns></returns>
        protected virtual string GetStoreUrl(string storeId = "", bool useSsl = false)
        {
            var store = _storeService.GetStoreById(storeId) ?? _storeContext.CurrentStore;

            if (store == null)
                throw new Exception("No store could be loaded");

            return useSsl ? store.SecureUrl : store.Url;
        }
    }
}