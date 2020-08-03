using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace Grand.Services.Catalog
{
    /// <summary>
    /// Product review service
    /// </summary>
    public class ProductReviewService : IProductReviewService
    {

        #region Fields

        private readonly IRepository<ProductReview> _productReviewRepository;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        public ProductReviewService(IRepository<ProductReview> productReviewRepository, IMediator mediator)
        {
            _productReviewRepository = productReviewRepository;
            _mediator = mediator;
        }

        #endregion

        /// <summary>
        /// Gets all product reviews
        /// </summary>
        /// <param name="customerId">Customer identifier; "" to load all records</param>
        /// <param name="approved">A value indicating whether to content is approved; null to load all records</param> 
        /// <param name="fromUtc">Item creation from; null to load all records</param>
        /// <param name="toUtc">Item item creation to; null to load all records</param>
        /// <param name="message">Search title or review text; null to load all records</param>
        /// <returns>Reviews</returns>
        public virtual async Task<IPagedList<ProductReview>> GetAllProductReviews(string customerId, bool? approved,
            DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = null, string storeId = "", string productId = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from p in _productReviewRepository.Table
                        select p;

            if (approved.HasValue)
                query = query.Where(c => c.IsApproved == approved.Value);
            if (!String.IsNullOrEmpty(customerId))
                query = query.Where(c => c.CustomerId == customerId);
            if (fromUtc.HasValue)
                query = query.Where(c => fromUtc.Value <= c.CreatedOnUtc);
            if (toUtc.HasValue)
                query = query.Where(c => toUtc.Value >= c.CreatedOnUtc);
            if (!String.IsNullOrEmpty(message))
                query = query.Where(c => c.Title.Contains(message) || c.ReviewText.Contains(message));
            if (!String.IsNullOrEmpty(storeId))
                query = query.Where(c => c.StoreId == storeId || c.StoreId == "");
            if (!String.IsNullOrEmpty(productId))
                query = query.Where(c => c.ProductId == productId);

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            return await PagedList<ProductReview>.Create(query, pageIndex, pageSize);
        }

        public virtual async Task<int> RatingSumProduct(string productId, string storeId)
        {
            var query = from p in _productReviewRepository.Table
                        where p.ProductId == productId && p.IsApproved && (p.StoreId == storeId || p.StoreId == "")
                        group p by true into g
                        select new { Sum = g.Sum(x => x.Rating) };
            var content = await query.ToListAsync();
            return content.Count > 0 ? content.FirstOrDefault().Sum : 0;
        }

        public virtual async Task<int> TotalReviewsProduct(string productId, string storeId)
        {
            var query = from p in _productReviewRepository.Table
                        where p.ProductId == productId && p.IsApproved && (p.StoreId == storeId || p.StoreId == "")
                        group p by true into g
                        select new { Count = g.Count() };
            var content = await query.ToListAsync();
            return content.Count > 0 ? content.FirstOrDefault().Count : 0;
        }


        /// <summary>
        /// Inserts a product review
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual async Task InsertProductReview(ProductReview productReview)
        {
            if (productReview == null)
                throw new ArgumentNullException("productPicture");

            await _productReviewRepository.InsertAsync(productReview);

            //event notification
            await _mediator.EntityInserted(productReview);
        }
        public virtual async Task UpdateProductReview(ProductReview productreview)
        {
            if (productreview == null)
                throw new ArgumentNullException("productreview");

            var builder = Builders<ProductReview>.Filter;
            var filter = builder.Eq(x => x.Id, productreview.Id);
            var update = Builders<ProductReview>.Update
                .Set(x => x.Title, productreview.Title)
                .Set(x => x.ReviewText, productreview.ReviewText)
                .Set(x => x.ReplyText, productreview.ReplyText)
                .Set(x => x.Signature, productreview.Signature)
                .Set(x => x.UpdatedOnUtc, DateTime.UtcNow)
                .Set(x => x.IsApproved, productreview.IsApproved)
                .Set(x => x.HelpfulNoTotal, productreview.HelpfulNoTotal)
                .Set(x => x.HelpfulYesTotal, productreview.HelpfulYesTotal)
                .Set(x => x.ProductReviewHelpfulnessEntries, productreview.ProductReviewHelpfulnessEntries);

            await _productReviewRepository.Collection.UpdateManyAsync(filter, update);

            //event notification
            await _mediator.EntityUpdated(productreview);
        }


        /// <summary>
        /// Deletes a product review
        /// </summary>
        /// <param name="productReview">Product review</param>
        public virtual async Task DeleteProductReview(ProductReview productReview)
        {
            if (productReview == null)
                throw new ArgumentNullException("productReview");

            await _productReviewRepository.DeleteAsync(productReview);

            //event notification
            await _mediator.EntityDeleted(productReview);
        }

        /// <summary>
        /// Gets product review
        /// </summary>
        /// <param name="productReviewId">Product review identifier</param>
        /// <returns>Product review</returns>
        public virtual Task<ProductReview> GetProductReviewById(string productReviewId)
        {
            return _productReviewRepository.GetByIdAsync(productReviewId);
        }

    }
}
