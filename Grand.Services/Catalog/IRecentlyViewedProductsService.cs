using Grand.Core.Domain.Catalog;
using System.Collections.Generic;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Recently viewed products service
    /// </summary>
    public partial interface IRecentlyViewedProductsService
    {
        /// <summary>
        /// Gets a "recently viewed products" list
        /// </summary>
        /// <param name="number">Number of products to load</param>
        /// <returns>"recently viewed products" list</returns>
        IList<Product> GetRecentlyViewedProducts(string customerId, int number);

        /// <summary>
        /// Adds a product to a recently viewed products list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        void AddProductToRecentlyViewedList(string customerId, string productId);
    }
}
