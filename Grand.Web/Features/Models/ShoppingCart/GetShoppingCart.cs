using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Localization;
using Grand.Domain.Orders;
using Grand.Domain.Stores;
using Grand.Domain.Tax;
using Grand.Web.Models.ShoppingCart;
using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.ShoppingCart
{
    public class GetShoppingCart : IRequest<ShoppingCartModel>
    {
        public Customer Customer { get; set; }
        public Language Language { get; set; }
        public Currency Currency { get; set; }
        public Store Store { get; set; }
        public TaxDisplayType TaxDisplayType { get; set; }
        public IList<ShoppingCartItem> Cart { get; set; }
        public bool IsEditable { get; set; } = true;
        public bool ValidateCheckoutAttributes { get; set; } = false;
        public bool SetEstimateShippingDefaultAddress { get; set; } = true;
        public bool PrepareAndDisplayOrderReviewData { get; set; } = false;
    }
}
