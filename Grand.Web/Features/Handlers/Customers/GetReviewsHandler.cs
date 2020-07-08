using Grand.Domain.Customers;
using Grand.Services.Catalog;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Customers;
using Grand.Web.Models.Customer;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Customers
{
    public class GetReviewsHandler : IRequestHandler<GetReviews, CustomerProductReviewsModel>
    {
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IProductReviewService _productReviewService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public GetReviewsHandler(
            ILocalizationService localizationService,
            IProductService productService,
            IProductReviewService productReviewService,
            IDateTimeHelper dateTimeHelper)
        {
            _localizationService = localizationService;
            _productService = productService;
            _productReviewService = productReviewService;
            _dateTimeHelper = dateTimeHelper;
        }

        public async Task<CustomerProductReviewsModel> Handle(GetReviews request, CancellationToken cancellationToken)
        {
            var reviewsModel = new CustomerProductReviewsModel();

            reviewsModel.CustomerId = request.Customer.Id;
            reviewsModel.CustomerInfo = request.Customer != null ? request.Customer.IsRegistered() ? request.Customer.Email : _localizationService.GetResource("Admin.Customers.Guest") : "";

            var productReviews = await _productReviewService.GetAllProductReviews(request.Customer.Id);
            foreach (var productReview in productReviews)
            {
                var product = await _productService.GetProductById(productReview.ProductId);

                var reviewModel = new CustomerProductReviewModel();

                reviewModel.Id = productReview.Id;
                reviewModel.ProductId = productReview.ProductId;
                reviewModel.ProductName = product.Name;
                reviewModel.ProductSeName = product.GetSeName(request.Language.Id);
                reviewModel.Rating = productReview.Rating;
                reviewModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(productReview.CreatedOnUtc, DateTimeKind.Utc);
                reviewModel.Signature = productReview.Signature;
                reviewModel.ReviewText = productReview.ReviewText;
                reviewModel.ReplyText = productReview.ReplyText;
                reviewModel.IsApproved = productReview.IsApproved;

                reviewsModel.Reviews.Add(reviewModel);
            }

            return reviewsModel;
        }
    }
}
