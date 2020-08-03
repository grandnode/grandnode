using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Services.Customers;
using Grand.Services.Notifications.Customers;
using Grand.Services.Notifications.Orders;
using Grand.Services.Notifications.ShoppingCart;
using Grand.Services.Orders;
using Grand.Services.Payments;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Events.Extensions
{
    public static class WebEventsExtensions
    {
        public static async Task ShoppingCartWarningsAdd<T, U>(this IMediator eventPublisher, IList<T> warnings, IList<U> shoppingCartItems, string checkoutAttributesXml, bool validateCheckoutAttributes) where U : ShoppingCartItem
        {
            await eventPublisher.Publish(new ShoppingCartWarningsEvent<T, U>(warnings, shoppingCartItems, checkoutAttributesXml, validateCheckoutAttributes));
        }

        public static async Task ShoppingCartItemWarningsAdded<C, S, P>(this IMediator eventPublisher, IList<string> warnings, C customer, S shoppingcartItem, P product) where C : Customer where S : ShoppingCartItem where P : Product
        {
            await eventPublisher.Publish(new ShoppingCartItemWarningsEvent<C, S, P>(warnings, customer, shoppingcartItem, product));
        }

        public static async Task CustomerRegistrationEvent<C, R>(this IMediator eventPublisher, C result, R request) where C : CustomerRegistrationResult where R : CustomerRegistrationRequest
        {
            await eventPublisher.Publish(new CustomerRegistrationEvent<C, R>(result, request));
        }

        public static async Task PlaceOrderDetailsEvent<R, O>(this IMediator eventPublisher, R result, O order) where R : PlaceOrderResult where O : PlaceOrderContainter
        {
            await eventPublisher.Publish(new PlaceOrderDetailsEvent<R, O>(result, order));
        }
        public static async Task CaptureOrderDetailsEvent<R, C>(this IMediator eventPublisher, R result, C request) where R : CapturePaymentResult where C : CapturePaymentRequest
        {
            await eventPublisher.Publish(new CaptureOrderDetailsEvent<R, C>(result, request));
        }
        public static async Task VoidOrderDetailsEvent<R, C>(this IMediator eventPublisher, R result, C request) where R : VoidPaymentResult where C : VoidPaymentRequest
        {
            await eventPublisher.Publish(new VoidOrderDetailsEvent<R, C>(result, request));
        }
    }
}
