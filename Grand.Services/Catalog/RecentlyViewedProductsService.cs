using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Recently viewed products service
    /// </summary>
    public partial class RecentlyViewedProductsService : IRecentlyViewedProductsService
    {

        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer id
        /// {1} : number
        /// </remarks>
        private const string RECENTLY_VIEW_PRODUCTS_KEY = "Grand.recentlyviewedproducts-{0}-{1}";

        /// <summary>
        /// Key pattern to clear cache
        /// {0} customer id
        /// </summary>
        private const string RECENTLY_VIEW_PRODUCTS_PATTERN_KEY = "Grand.recentlyviewedproducts-{0}";

        #endregion

        #region Fields

        private readonly IProductService _productService;
        private readonly ICacheManager _cacheManager;
        private readonly CatalogSettings _catalogSettings;
        private readonly IRepository<RecentlyViewedProduct> _recentlyViewedProducts;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="productService">Product service</param>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="catalogSettings">Catalog settings</param>
        /// <param name="recentlyViewedProducts">Collection recentlyViewedProducts</param>
        public RecentlyViewedProductsService(
            IProductService productService,
            ICacheManager cacheManager,
            CatalogSettings catalogSettings, 
            IRepository<RecentlyViewedProduct> recentlyViewedProducts)
        {
            _productService = productService;
            _cacheManager = cacheManager;
            _catalogSettings = catalogSettings;
            _recentlyViewedProducts = recentlyViewedProducts;
        }

        #endregion

        #region Utilities

        
        protected IList<RecentlyViewedProduct> GetRecentlyViewedProducts(string customerId)
        {
            var query = from p in _recentlyViewedProducts.Table
                        where p.CustomerId == customerId
                        orderby p.CreatedOnUtc descending
                        select p;

            return query.ToList();
        }
        /// <summary>
        /// Gets a "recently viewed products" identifier list
        /// </summary>
        /// <param name="number">Number of products to load</param>
        /// <returns>"recently viewed products" list</returns>
        protected async Task<IList<string>> GetRecentlyViewedProductsIds(string customerId, int number)
        {
            string key = string.Format(RECENTLY_VIEW_PRODUCTS_KEY, customerId, number);
            return await _cacheManager.GetAsync(key, async () =>
            {
                var query = from p in _recentlyViewedProducts.Table
                             where p.CustomerId == customerId
                             orderby p.CreatedOnUtc descending
                             select p.ProductId;
                return await query.Take(number).ToListAsync();
            });            
        }

        #endregion

        #region Methods


        /// <summary>
        /// Gets a "recently viewed products" list
        /// </summary>
        /// <param name="number">Number of products to load</param>
        /// <returns>"recently viewed products" list</returns>
        public virtual async Task<IList<Product>> GetRecentlyViewedProducts(string customerId, int number)
        {
            var products = new List<Product>();
            var productIds = await GetRecentlyViewedProductsIds(customerId, number);
            foreach (var product in await _productService.GetProductsByIds(productIds.ToArray()))
                if (product.Published)
                    products.Add(product);
            return products;
        }

        /// <summary>
        /// Adds a product to a recently viewed products list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        public virtual async Task AddProductToRecentlyViewedList(string customerId, string productId)
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return;

            var recentlyViewedProducts = GetRecentlyViewedProducts(customerId);
            var recentlyViewedProduct = recentlyViewedProducts.FirstOrDefault(x => x.ProductId == productId);
            if (recentlyViewedProduct == null)
            {
                await _recentlyViewedProducts.InsertAsync(new RecentlyViewedProduct() { CustomerId = customerId, ProductId = productId, CreatedOnUtc = DateTime.UtcNow });
            }
            else
            {
                recentlyViewedProduct.CreatedOnUtc = DateTime.UtcNow;
                await _recentlyViewedProducts.UpdateAsync(recentlyViewedProduct);
            }
            int maxProducts = _catalogSettings.RecentlyViewedProductsNumber;
            if (maxProducts <= 0)
                maxProducts = 10;

            if (recentlyViewedProducts.Count > _catalogSettings.RecentlyViewedProductsNumber)
            {
                await _recentlyViewedProducts.DeleteAsync(recentlyViewedProducts.OrderBy(x => x.CreatedOnUtc).Take(recentlyViewedProducts.Count - _catalogSettings.RecentlyViewedProductsNumber));
            }

            //Clear cache
            await _cacheManager.RemoveByPrefixAsync(string.Format(RECENTLY_VIEW_PRODUCTS_PATTERN_KEY, customerId));

        }

        #endregion
    }
}
