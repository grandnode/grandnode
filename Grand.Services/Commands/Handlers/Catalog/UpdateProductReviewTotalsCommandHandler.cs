﻿using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Services.Commands.Models.Catalog;
using MediatR;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;
using Grand.Services.Catalog;
using Grand.Core.Caching.Constants;

namespace Grand.Services.Commands.Handlers.Catalog
{
    public class UpdateProductReviewTotalsCommandHandler : IRequestHandler<UpdateProductReviewTotalsCommand, bool>
    {
        #region Fields

        private readonly IRepository<Product> _productRepository;
        private readonly IProductReviewService _productReviewService;
        private readonly ICacheManager _cacheManager;

        #endregion

        public UpdateProductReviewTotalsCommandHandler(IRepository<Product> productRepository, IProductReviewService productReviewService, ICacheManager cacheManager)
        {
            _productRepository = productRepository;
            _cacheManager = cacheManager;
            _productReviewService = productReviewService;
        }

        public async Task<bool> Handle(UpdateProductReviewTotalsCommand request, CancellationToken cancellationToken)
        {
            if (request.Product == null)
                throw new ArgumentNullException("product");

            int approvedRatingSum = 0;
            int notApprovedRatingSum = 0;
            int approvedTotalReviews = 0;
            int notApprovedTotalReviews = 0;

            var reviews = await _productReviewService.GetAllProductReviews(null, null, null, null, null,
                null, request.Product.Id, 0, int.MaxValue);
                        
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
            await _cacheManager.RemoveAsync(string.Format(CacheKey.PRODUCTS_BY_ID_KEY, request.Product.Id));

            return true;
        }
    }
}
