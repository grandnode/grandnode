using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;
using Grand.Services.Seo;
using Grand.Services.Stores;
using System;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidBackInStockSubscription : Drop
    {
        private readonly BackInStockSubscription _backInStockSubscription;
        private readonly Product _product;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;

        public LiquidBackInStockSubscription(BackInStockSubscription backInStockSubscription,
            IStoreContext storeContext,
            IStoreService storeService)
        {
            this._backInStockSubscription = backInStockSubscription;
            this._product = EngineContext.Current.Resolve<IProductService>().GetProductById(_backInStockSubscription.ProductId);
            this._storeContext = storeContext;
            this._storeService = storeService;
        }

        public string ProductName
        {
            get { return _product.Name; }
        }

        public string ProductURL
        {
            get { return string.Format("{0}{1}", GetStoreUrl(_backInStockSubscription.StoreId), _product.GetSeName()); }
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