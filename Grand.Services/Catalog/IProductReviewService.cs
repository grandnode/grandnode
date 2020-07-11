using Grand.Domain;
using Grand.Domain.Catalog;
using System;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Product review service
    /// </summary>
    public partial interface IProductReviewService
    {
        /// <summary>
        /// Insert product review 
        /// </summary>
        /// <param name="product">Product</param>
        Task InsertProductReview(ProductReview productreview);

        /// <summary>
        /// Update product review 
        /// </summary>
        /// <param name="product">Product</param>
        Task UpdateProductReview(ProductReview productreview);

        /// <summary>
        /// Gets all product reviews
        /// </summary>
        /// <param name="customerId">Customer identifier; "" to load all records</param>
        /// <param name="approved">A value indicating whether to content is approved; null to load all records</param> 
        /// <param name="fromUtc">Item creation from; null to load all records</param>
        /// <param name="toUtc">Item item creation to; null to load all records</param>
        /// <param name="message">Search title or review text; null to load all records</param>
        /// <param name="storeId">Store identifier; "" to load all records</param>
        /// <param name="productId">Product identifier; "" to load all records</param>
        /// <returns>Reviews</returns>
        Task<IPagedList<ProductReview>> GetAllProductReviews(string customerId, bool? approved = null,
            DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = null, string storeId = "", string productId = "", int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get rating sum for product
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier, "" to load all records</param> 
        /// <returns>Sum</returns>
        Task<int> RatingSumProduct(string productId, string storeId);

        /// <summary>
        /// Total reviews for product
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier, "" to load all records</param> 
        /// <returns>Sum</returns>
        Task<int> TotalReviewsProduct(string productId, string storeId);

        /// <summary>
        /// Gets product review
        /// </summary>
        /// <param name="productReviewId">Product review identifier</param>
        /// <returns>Product review</returns>
        Task<ProductReview> GetProductReviewById(string productReviewId);

        /// <summary>
        /// Deletes a product review
        /// </summary>
        /// <param name="productReview">Product review</param>
        Task DeleteProductReview(ProductReview productReview);

    }
}
