using System.Collections.Generic;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Localization;
using Grand.Domain.Orders;
using Grand.Domain.Stores;
using Grand.Web.Models.Checkout;
using MediatR;

namespace Grand.Web.Features.Models.Checkout
{
    public class GetPaymentMethod : IRequest<CheckoutPaymentMethodModel>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Currency Currency { get; set; }
        public Language Language { get; set; }
        public IList<ShoppingCartItem> Cart { get; set; } 
        public string FilterByCountryId { get; set; }
    }
}
