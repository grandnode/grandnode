using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Web.Models.Catalog;
using MediatR;

namespace Grand.Web.Features.Models.Products
{
    public class GetProductReviews : IRequest<ProductReviewsModel>
    {
        public Product Product { get; set; }
        public Language Language { get; set; }
        public Store Store { get; set; }
        public Customer Customer { get; set; }
        public int Size { get; set; } = 0;
    }
}
