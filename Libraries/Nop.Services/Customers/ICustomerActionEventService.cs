
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
        void AddToCart(ShoppingCartItem cart, Product product, Customer customer);

        /// <summary>
        /// Run action add new order
        /// </summary>
        void AddOrder(Order order, Customer customer);

        /// <summary>
        /// Viewed
        /// </summary>
        void Viewed(Customer customer, string currentUrl, string previousUrl);

        /// <summary>
        /// Run action url
        /// </summary>
        void Url(Customer customer, string currentUrl, string previousUrl);


        /// <summary>
        /// Run action url
        /// </summary>
        void Registration(Customer customer);
    }
}
