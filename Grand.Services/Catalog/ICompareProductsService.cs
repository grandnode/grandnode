using Grand.Core.Domain.Catalog;
using System.Collections.Generic;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Compare products service interface
    /// </summary>
    public partial interface ICompareProductsService
    {
        /// <summary>
        /// Clears a "compare products" list
        /// </summary>
        void ClearCompareProducts();

        /// <summary>
        /// Gets a "compare products" list
        /// </summary>
        /// <returns>"Compare products" list</returns>
        IList<Product> GetComparedProducts();

        /// <summary>
        /// Removes a product from a "compare products" list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        void RemoveProductFromCompareList(string productId);

        /// <summary>
        /// Adds a product to a "compare products" list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        void AddProductToCompareList(string productId);
    }
}
