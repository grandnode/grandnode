using Grand.Core.Domain.Catalog;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Compare products service
    /// </summary>
    public partial class CompareProductsService : ICompareProductsService
    {
        #region Constants

        /// <summary>
        /// Compare products cookie name
        /// </summary>
        private const string COMPARE_PRODUCTS_COOKIE_NAME = "Grand.CompareProducts";

        #endregion
        
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor


        #region Utilities

        protected virtual void AddCompareProductsCookie(IEnumerable<string> comparedProductIds)
        {
            //delete current cookie if exists
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(COMPARE_PRODUCTS_COOKIE_NAME);

            //create cookie value
            var comparedProductIdsCookie = string.Join(",", comparedProductIds);

            //create cookie options 
            var cookieExpires = 24 * 10; //TODO make configurable
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddHours(cookieExpires),
                HttpOnly = true
            };

            //add cookie
            _httpContextAccessor.HttpContext.Response.Cookies.Append(COMPARE_PRODUCTS_COOKIE_NAME, comparedProductIdsCookie, cookieOptions);
        }

        #endregion

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        /// <param name="productService">Product service</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public CompareProductsService(IHttpContextAccessor httpContextAccessor, IProductService productService,
            CatalogSettings catalogSettings)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._productService = productService;
            this._catalogSettings = catalogSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets a "compare products" identifier list
        /// </summary>
        /// <returns>"compare products" identifier list</returns>
        protected virtual List<string> GetComparedProductIds()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.Request == null)
                return new List<string>();

            //try to get cookie
            if (!httpContext.Request.Cookies.TryGetValue(COMPARE_PRODUCTS_COOKIE_NAME, out string productIdsCookie) || string.IsNullOrEmpty(productIdsCookie))
                return new List<string>();

            //get array of string product identifiers from cookie
            var productIds = productIdsCookie.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            //return list of int product identifiers
            return productIds.Select(productId => productId).Distinct().ToList();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears a "compare products" list
        /// </summary>
        public virtual void ClearCompareProducts()
        {
            if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.Response == null)
                return;

            //sets an expired cookie
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(COMPARE_PRODUCTS_COOKIE_NAME);
        }

        /// <summary>
        /// Gets a "compare products" list
        /// </summary>
        /// <returns>"Compare products" list</returns>
        public virtual IList<Product> GetComparedProducts()
        {
            var products = new List<Product>();
            var productIds = GetComparedProductIds();
            foreach (string productId in productIds)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.Published)
                    products.Add(product);
            }
            return products;
        }

        /// <summary>
        /// Removes a product from a "compare products" list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        public virtual void RemoveProductFromCompareList(string productId)
        {
            if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.Response == null)
                return;

            //get list of compared product identifiers
            var comparedProductIds = GetComparedProductIds();

            //whether product identifier to remove exists
            if (!comparedProductIds.Contains(productId))
                return;

            //it exists, so remove it from list
            comparedProductIds.Remove(productId);

            //set cookie
            AddCompareProductsCookie(comparedProductIds);
        }

        /// <summary>
        /// Adds a product to a "compare products" list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        public virtual void AddProductToCompareList(string productId)
        {
            
            if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.Response == null)
                return;

            //get list of compared product identifiers
            var comparedProductIds = GetComparedProductIds();

            //whether product identifier to add already exist
            if (!comparedProductIds.Contains(productId))
                comparedProductIds.Insert(0, productId);

            //limit list based on the allowed number of products to be compared
            comparedProductIds = comparedProductIds.Take(_catalogSettings.CompareProductsNumber).ToList();

            //set cookie
            AddCompareProductsCookie(comparedProductIds);
        }

        #endregion
    }
}
