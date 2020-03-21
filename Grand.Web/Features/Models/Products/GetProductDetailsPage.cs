using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Web.Models.Catalog;
using MediatR;

namespace Grand.Web.Features.Models.Products
{
    public class GetProductDetailsPage : IRequest<ProductDetailsModel>
    {
        public Product Product { get; set; }
        public ShoppingCartItem UpdateCartItem { get; set; } = null;
        public bool IsAssociatedProduct { get; set; } = false;
    }
}
