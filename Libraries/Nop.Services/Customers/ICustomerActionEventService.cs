
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;

namespace Nop.Services.Customers
{
    public partial interface ICustomerActionEventService
    {
        /// <summary>
        /// Run action add to cart 
        /// </summary>
        void AddToCart(ShoppingCartItem cart, Product product);
    }
}
