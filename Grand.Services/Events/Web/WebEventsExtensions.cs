using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Services.Customers;
using Grand.Services.Orders;
using System.Collections.Generic;

namespace Grand.Services.Events.Web
{
    public static class WebEventsExtensions
    {
        public static void ShoppingCartWarningsAdd<T, U>(this IEventPublisher eventPublisher, IList<T> warnings, IList<U> shoppingCartItems, string checkoutAttributesXml, bool validateCheckoutAttributes) where U : ShoppingCartItem
        {
            eventPublisher.Publish(new ShoppingCartWarningsEvent<T, U>(warnings, shoppingCartItems, checkoutAttributesXml, validateCheckoutAttributes));
        }

        public static void ShoppingCartItemWarningsAdded<C, S, P>(this IEventPublisher eventPublisher, IList<string> warnings, C customer, S shoppingcartItem, P product) where C : Customer where S : ShoppingCartItem where P : Product
        {
            eventPublisher.Publish(new ShoppingCartItemWarningsEvent<C, S, P>(warnings, customer, shoppingcartItem, product));
        }

        public static void CustomerRegistrationEvent<C, R>(this IEventPublisher eventPublisher, C result, R request) where C : CustomerRegistrationResult where R : CustomerRegistrationRequest
        {
            eventPublisher.Publish(new CustomerRegistrationEvent<C, R>(result, request));
        }

        public static void PlaceOrderDetailsEvent<R, O>(this IEventPublisher eventPublisher, R result, O order) where R : PlaceOrderResult where O : PlaceOrderContainter
        {
            eventPublisher.Publish(new PlaceOrderDetailsEvent<R, O>(result, order));
        }

    }
}
