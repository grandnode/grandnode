using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Grand.Core.Domain.Catalog;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Recently viewed products service
    /// </summary>
    public partial class RecentlyViewedProductsService : IRecentlyViewedProductsService
    {
        #region Fields

        private readonly HttpContextBase _httpContext;
        private readonly IProductService _productService;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        /// <param name="productService">Product service</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public RecentlyViewedProductsService(HttpContextBase httpContext, IProductService productService,
            CatalogSettings catalogSettings)
        {
            this._httpContext = httpContext;
            this._productService = productService;
            this._catalogSettings = catalogSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets a "recently viewed products" identifier list
        /// </summary>
        /// <returns>"recently viewed products" list</returns>
        protected IList<string> GetRecentlyViewedProductsIds()
        {
            return GetRecentlyViewedProductsIds(int.MaxValue);
        }

        /// <summary>
        /// Gets a "recently viewed products" identifier list
        /// </summary>
        /// <param name="number">Number of products to load</param>
        /// <returns>"recently viewed products" list</returns>
        protected IList<string> GetRecentlyViewedProductsIds(int number)
        {
            var productIds = new List<string>();
            var recentlyViewedCookie = _httpContext.Request.Cookies.Get("Grandnode.RecentlyViewedProducts");
            if (recentlyViewedCookie == null)
                return productIds;
            string[] values = recentlyViewedCookie.Values.GetValues("RecentlyViewedProductIds");
            if (values == null)
                return productIds;
            foreach (string productId in values)
            {
                if (!productIds.Contains(productId))
                {
                    productIds.Add(productId);
                    if (productIds.Count >= number)
                        break;
                }

            }

            return productIds;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Gets a "recently viewed products" list
        /// </summary>
        /// <param name="number">Number of products to load</param>
        /// <returns>"recently viewed products" list</returns>
        public virtual IList<Product> GetRecentlyViewedProducts(int number)
        {
            var products = new List<Product>();
            var productIds = GetRecentlyViewedProductsIds(number);
            foreach (var product in _productService.GetProductsByIds(productIds.ToArray()))
                if (product.Published)
                    products.Add(product);
            return products;
        }

        /// <summary>
        /// Adds a product to a recently viewed products list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        public virtual void AddProductToRecentlyViewedList(string productId)
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return;

            var oldProductIds = GetRecentlyViewedProductsIds();
            var newProductIds = new List<string>();
            newProductIds.Add(productId);
            foreach (string oldProductId in oldProductIds)
                if (oldProductId != productId)
                    newProductIds.Add(oldProductId);

            var recentlyViewedCookie = _httpContext.Request.Cookies.Get("Grandnode.RecentlyViewedProducts");
            if (recentlyViewedCookie == null)
            {
                recentlyViewedCookie = new HttpCookie("Grandnode.RecentlyViewedProducts");
                recentlyViewedCookie.HttpOnly = true;
            }
            recentlyViewedCookie.Values.Clear();
            int maxProducts = _catalogSettings.RecentlyViewedProductsNumber;
            if (maxProducts <= 0)
                maxProducts = 10;
            int i = 1;
            foreach (string newProductId in newProductIds)
            {
                recentlyViewedCookie.Values.Add("RecentlyViewedProductIds", newProductId.ToString());
                if (i == maxProducts)
                    break;
                i++;
            }
            recentlyViewedCookie.Expires = DateTime.Now.AddDays(10.0);
            _httpContext.Response.Cookies.Set(recentlyViewedCookie);
        }
        
        #endregion
    }
}
