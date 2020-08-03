using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Services.Catalog;
using Grand.Web.Features.Models.Products;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Catalog;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Products
{
    public class GetProductReviewOverviewHandler : IRequestHandler<GetProductReviewOverview, ProductReviewOverviewModel>
    {
        private readonly ICacheManager _cacheManager;
        private readonly IProductReviewService _productReviewService;
        private readonly CatalogSettings _catalogSettings;

        public GetProductReviewOverviewHandler(
            ICacheManager cacheManager,
            IProductReviewService productReviewService, 
            CatalogSettings catalogSettings)
        {
            _cacheManager = cacheManager;
            _productReviewService = productReviewService;
            _catalogSettings = catalogSettings;
        }

        public async Task<ProductReviewOverviewModel> Handle(GetProductReviewOverview request, CancellationToken cancellationToken)
        {
            ProductReviewOverviewModel productReview = null;

            if (_catalogSettings.ShowProductReviewsPerStore)
            {
                string cacheKey = string.Format(ModelCacheEventConst.PRODUCT_REVIEWS_MODEL_KEY, request.Product.Id, request.Store.Id);

                productReview = await _cacheManager.GetAsync(cacheKey, async () =>
                {
                    return new ProductReviewOverviewModel {
                        RatingSum = await _productReviewService.RatingSumProduct(request.Product.Id, _catalogSettings.ShowProductReviewsPerStore ? request.Store.Id : ""),
                        TotalReviews = await _productReviewService.TotalReviewsProduct(request.Product.Id, _catalogSettings.ShowProductReviewsPerStore ? request.Store.Id : ""),
                    };
                });
            }
            else
            {
                productReview = new ProductReviewOverviewModel() {
                    RatingSum = request.Product.ApprovedRatingSum,
                    TotalReviews = request.Product.ApprovedTotalReviews
                };
            }

            if (productReview != null)
            {
                productReview.ProductId = request.Product.Id;
                productReview.AllowCustomerReviews = request.Product.AllowCustomerReviews;
            }
            return productReview;
        }
    }
}
