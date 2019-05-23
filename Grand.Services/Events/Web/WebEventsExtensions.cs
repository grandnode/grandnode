using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Services.Customers;
using Grand.Services.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Events.Web
{
    public static class WebEventsExtensions
    {
        public static async Task ShoppingCartWarningsAdd<T, U>(this IEventPublisher eventPublisher, IList<T> warnings, IList<U> shoppingCartItems, string checkoutAttributesXml, bool validateCheckoutAttributes) where U : ShoppingCartItem
        {
            await eventPublisher.PublishAsync(new ShoppingCartWarningsEvent<T, U>(warnings, shoppingCartItems, checkoutAttributesXml, validateCheckoutAttributes));
        }

        public static async Task ShoppingCartItemWarningsAdded<C, S, P>(this IEventPublisher eventPublisher, IList<string> warnings, C customer, S shoppingcartItem, P product) where C : Customer where S : ShoppingCartItem where P : Product
        {
            await eventPublisher.PublishAsync(new ShoppingCartItemWarningsEvent<C, S, P>(warnings, customer, shoppingcartItem, product));
        }

        public static async Task CustomerRegistrationEvent<C, R>(this IEventPublisher eventPublisher, C result, R request) where C : CustomerRegistrationResult where R : CustomerRegistrationRequest
        {
            await eventPublisher.PublishAsync(new CustomerRegistrationEvent<C, R>(result, request));
        }

        public static async Task PlaceOrderDetailsEvent<R, O>(this IEventPublisher eventPublisher, R result, O order) where R : PlaceOrderResult where O : PlaceOrderContainter
        {
            await eventPublisher.PublishAsync(new PlaceOrderDetailsEvent<R, O>(result, order));
        }

    }
}
