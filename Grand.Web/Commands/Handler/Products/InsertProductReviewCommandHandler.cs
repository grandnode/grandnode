using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Localization;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Messages;
using Grand.Web.Commands.Models.Products;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Products
{
    public class InsertProductReviewCommandHandler : IRequestHandler<InsertProductReviewCommand, ProductReview>
    {
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IWorkflowMessageService _workflowMessageService;

        private readonly CatalogSettings _catalogSettings;
        private readonly LocalizationSettings _localizationSettings;

        public InsertProductReviewCommandHandler(
            IProductService productService,
            ICustomerService customerService,
            IWorkflowMessageService workflowMessageService,
            CatalogSettings catalogSettings,
            LocalizationSettings localizationSettings)
        {
            _productService = productService;
            _customerService = customerService;
            _workflowMessageService = workflowMessageService;
            _catalogSettings = catalogSettings;
            _localizationSettings = localizationSettings;
        }

        public async Task<ProductReview> Handle(InsertProductReviewCommand request, CancellationToken cancellationToken)
        {
            //save review
            int rating = request.Model.AddProductReview.Rating;
            if (rating < 1 || rating > 5)
                rating = _catalogSettings.DefaultProductRatingValue;
            bool isApproved = !_catalogSettings.ProductReviewsMustBeApproved;

            var productReview = new ProductReview {
                ProductId = request.Product.Id,
                StoreId = request.Store.Id,
                CustomerId = request.Customer.Id,
                Title = request.Model.AddProductReview.Title,
                ReviewText = request.Model.AddProductReview.ReviewText,
                Rating = rating,
                HelpfulYesTotal = 0,
                HelpfulNoTotal = 0,
                IsApproved = isApproved,
                CreatedOnUtc = DateTime.UtcNow,
            };
            await _productService.InsertProductReview(productReview);

            if (!request.Customer.HasContributions)
            {
                await _customerService.UpdateContributions(request.Customer);
            }

            //update product totals
            await _productService.UpdateProductReviewTotals(request.Product);

            //notify store owner
            if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                await _workflowMessageService.SendProductReviewNotificationMessage(request.Product, productReview, _localizationSettings.DefaultAdminLanguageId);

            return productReview;
        }
    }
}
