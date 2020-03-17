using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Stores;
using Grand.Web.Models.Catalog;
using MediatR;

namespace Grand.Web.Commands.Models.Products
{
    public class InsertProductReviewCommand : IRequest<ProductReview>
    {
        public Store Store { get; set; }
        public Customer Customer { get; set; }
        public Product Product { get; set; }
        public ProductReviewsModel Model { get; set; }
    }
}
