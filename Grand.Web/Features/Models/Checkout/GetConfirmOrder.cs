using Grand.Domain.Directory;
using Grand.Domain.Orders;
using Grand.Web.Models.Checkout;
using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.Checkout
{
    public class GetConfirmOrder : IRequest<CheckoutConfirmModel>
    {
        public IList<ShoppingCartItem> Cart { get; set; }
        public Currency Currency { get; set; }
    }
}
