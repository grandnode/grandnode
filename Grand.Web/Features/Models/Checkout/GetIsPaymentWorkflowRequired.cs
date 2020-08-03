using Grand.Domain.Orders;
using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.Checkout
{
    public class GetIsPaymentWorkflowRequired : IRequest<bool>
    {
        public IList<ShoppingCartItem> Cart { get; set; }
        public bool? UseRewardPoints { get; set; } = null;
    }
}
