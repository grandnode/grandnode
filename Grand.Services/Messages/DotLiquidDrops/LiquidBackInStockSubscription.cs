using DotLiquid;
using Grand.Core.Domain.Catalog;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;
using Grand.Services.Seo;
using Grand.Services.Stores;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidBackInStockSubscription : Drop
    {
        private readonly BackInStockSubscription _backInStockSubscription;
        private readonly Product _product;
        private readonly IStoreService _storeService;

        public LiquidBackInStockSubscription(BackInStockSubscription backInStockSubscription)
        {
            this._storeService = EngineContext.Current.Resolve<IStoreService>();
            this._backInStockSubscription = backInStockSubscription;
            this._product = EngineContext.Current.Resolve<IProductService>().GetProductById(_backInStockSubscription.ProductId);

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string ProductName
        {
            get { return _product.Name; }
        }

        public string ProductUrl
        {
            get { return string.Format("{0}{1}", _storeService.GetStoreUrl(_backInStockSubscription.StoreId), _product.GetSeName()); }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}