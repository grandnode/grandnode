using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Services.Commands.Models.Catalog;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Catalog
{
    public class UpdateProductReviewTotalsCommandHandler : IRequestHandler<UpdateProductReviewTotalsCommand, bool>
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        private const string PRODUCTS_BY_ID_KEY = "Grand.product.id-{0}";

        #endregion

        #region Fields

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductReview> _productReviewRepository;
        private readonly ICacheManager _cacheManager;

        #endregion

        public UpdateProductReviewTotalsCommandHandler(IRepository<Product> productRepository, IRepository<ProductReview> productReviewRepository, ICacheManager cacheManager)
        {
            _productRepository = productRepository;
            _productReviewRepository = productReviewRepository;
            _cacheManager = cacheManager;
        }

        public async Task<bool> Handle(UpdateProductReviewTotalsCommand request, CancellationToken cancellationToken)
        {
            if (request.Product == null)
                throw new ArgumentNullException("product");

            int approvedRatingSum = 0;
            int notApprovedRatingSum = 0;
            int approvedTotalReviews = 0;
            int notApprovedTotalReviews = 0;
            var reviews = await _productReviewRepository.Collection.Find(new BsonDocument("ProductId", request.Product.Id)).ToListAsync();
            foreach (var pr in reviews)
            {
                if (pr.IsApproved)
                {
                    approvedRatingSum += pr.Rating;
                    approvedTotalReviews++;
                }
                else
                {
                    notApprovedRatingSum += pr.Rating;
                    notApprovedTotalReviews++;
                }
            }

            request.Product.ApprovedRatingSum = approvedRatingSum;
            request.Product.NotApprovedRatingSum = notApprovedRatingSum;
            request.Product.ApprovedTotalReviews = approvedTotalReviews;
            request.Product.NotApprovedTotalReviews = notApprovedTotalReviews;

            var filter = Builders<Product>.Filter.Eq("Id", request.Product.Id);
            var update = Builders<Product>.Update
                    .Set(x => x.ApprovedRatingSum, request.Product.ApprovedRatingSum)
                    .Set(x => x.NotApprovedRatingSum, request.Product.NotApprovedRatingSum)
                    .Set(x => x.ApprovedTotalReviews, request.Product.ApprovedTotalReviews)
                    .Set(x => x.NotApprovedTotalReviews, request.Product.NotApprovedTotalReviews);

            await _productRepository.Collection.UpdateOneAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, request.Product.Id));

            return true;
        }
    }
}
