using Grand.Core.Domain.Orders;
using System.Collections.Generic;

namespace Grand.Services.Events.Web
{
    public static class WebEventsExtensions
    {
        public static void ShoppingCartWarningsAdd<T, U>(this IEventPublisher eventPublisher, IList<T> warnings, IList<U> shoppingCartItems, string checkoutAttributesXml, bool validateCheckoutAttributes) where U : ShoppingCartItem
        {
            eventPublisher.Publish(new ShoppingCartWarningsEvent<T, U>(warnings, shoppingCartItems, checkoutAttributesXml, validateCheckoutAttributes));
        }
    }
}
