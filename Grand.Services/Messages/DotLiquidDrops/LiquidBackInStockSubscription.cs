using DotLiquid;
using Grand.Domain.Catalog;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Services.Seo;
using Grand.Services.Stores;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidBackInStockSubscription : Drop
    {
        private readonly BackInStockSubscription _backInStockSubscription;
        private readonly Product _product;
        private readonly Store _store;
        private readonly Language _language;

        public LiquidBackInStockSubscription(Product product, BackInStockSubscription backInStockSubscription, Store store, Language language)
        {
            _backInStockSubscription = backInStockSubscription;
            _product = product;
            _store = store;
            _language = language;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string ProductName
        {
            get { return _product.Name; }
        }

        public string ProductUrl
        {
            get { return string.Format("{0}{1}", _store.SslEnabled ? _store.SecureUrl : _store.Url, _product.GetSeName(_language.Id)); }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}