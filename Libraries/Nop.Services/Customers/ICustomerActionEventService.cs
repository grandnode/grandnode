
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

        /// <summary>
        /// Run action add new order
        /// </summary>
        void AddOrder(Order order);

        /// <summary>
        /// Viewed
        /// </summary>
        void Viewed(string customerId, string currentUrl, string previousUrl);

        /// <summary>
        /// Run action url
        /// </summary>
        void Url(string customerId, string currentUrl, string previousUrl);


        /// <summary>
        /// Run action url
        /// </summary>
        void Registration(string customerId);
    }
}
