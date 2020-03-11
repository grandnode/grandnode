using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Stores;
using Grand.Web.Models.Checkout;
using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.Checkout
{
    public class GetShippingMethod : IRequest<CheckoutShippingMethodModel>
    {
        public IList<ShoppingCartItem> Cart { get; set; }
        public Address ShippingAddress { get; set; }
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Currency Currency { get; set; }
        public Language Language { get; set; }

    }
}
