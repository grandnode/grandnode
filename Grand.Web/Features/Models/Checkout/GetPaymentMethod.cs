using System.Collections.Generic;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Stores;
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
