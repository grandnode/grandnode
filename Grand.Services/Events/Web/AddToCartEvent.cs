using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using MediatR;

namespace Grand.Services.Events.Web
{
    public class AddToCartEvent : INotification
    {
        private readonly Product _product;
        private readonly Customer _customer;
        private readonly ShoppingCartItem _shoppingCartItem;

        public AddToCartEvent(Customer customer, ShoppingCartItem shoppingCartItem, Product product)
        {
            _customer = customer;
            _shoppingCartItem = shoppingCartItem;
            _product = product;
        }

        public Customer Customer { get { return _customer; } }
        public ShoppingCartItem ShoppingCartItem { get { return _shoppingCartItem; } }
        public Product Product { get { return _product; } }

    }
}
