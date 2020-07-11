using Grand.Domain.Catalog;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Web.Models.Catalog;
using MediatR;

namespace Grand.Web.Features.Models.Products
{
    public class GetProductReviewOverview : IRequest<ProductReviewOverviewModel>
    {
        public Product Product { get; set; }
        public Language Language { get; set; }
        public Store Store { get; set; }
    }
}
