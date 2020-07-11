using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Framework.Security.Captcha;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Products;
using Grand.Web.Models.Catalog;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Products
{
    public class GetProductReviewsHandler : IRequestHandler<GetProductReviews, ProductReviewsModel>
    {
        private readonly ICustomerService _customerService;
        private readonly IProductReviewService _productReviewService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly CatalogSettings _catalogSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly CaptchaSettings _captchaSettings;

        public GetProductReviewsHandler(
            ICustomerService customerService,
            IProductReviewService productReviewService,
            IDateTimeHelper dateTimeHelper,
            CatalogSettings catalogSettings,
            CustomerSettings customerSettings,
            CaptchaSettings captchaSettings)
        {
            _customerService = customerService;
            _productReviewService = productReviewService;
            _dateTimeHelper = dateTimeHelper;
            _catalogSettings = catalogSettings;
            _customerSettings = customerSettings;
            _captchaSettings = captchaSettings;
        }

        public async Task<ProductReviewsModel> Handle(GetProductReviews request, CancellationToken cancellationToken)
        {
            if (request.Product == null)
                throw new ArgumentNullException("product");

            var model = new ProductReviewsModel();

            model.ProductId = request.Product.Id;
            model.ProductName = request.Product.GetLocalized(x => x.Name, request.Language.Id);
            model.ProductSeName = request.Product.GetSeName(request.Language.Id);
            var productReviews = await _productReviewService.GetAllProductReviews("", true, null, null, "", _catalogSettings.ShowProductReviewsPerStore ? request.Store.Id : "", request.Product.Id, request.Size);
            foreach (var pr in productReviews)
            {
                var customer = await _customerService.GetCustomerById(pr.CustomerId);
                model.Items.Add(new ProductReviewModel {
                    Id = pr.Id,
                    CustomerId = pr?.CustomerId,
                    CustomerName = customer?.FormatUserName(_customerSettings.CustomerNameFormat),
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !customer.IsGuest(),
                    Title = pr.Title,
                    ReviewText = pr.ReviewText,
                    ReplyText = pr.ReplyText,
                    Signature = pr.Signature,
                    Rating = pr.Rating,
                    Helpfulness = new ProductReviewHelpfulnessModel {
                        ProductId = request.Product.Id,
                        ProductReviewId = pr.Id,
                        HelpfulYesTotal = pr.HelpfulYesTotal,
                        HelpfulNoTotal = pr.HelpfulNoTotal,
                    },
                    WrittenOnStr = _dateTimeHelper.ConvertToUserTime(pr.CreatedOnUtc, DateTimeKind.Utc).ToString("g"),
                });
            }

            model.AddProductReview.CanCurrentCustomerLeaveReview = _catalogSettings.AllowAnonymousUsersToReviewProduct || !request.Customer.IsGuest();
            model.AddProductReview.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage;

            return model;
        }
    }
}
