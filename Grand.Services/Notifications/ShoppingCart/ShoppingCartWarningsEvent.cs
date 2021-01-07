using Grand.Domain.Common;
using Grand.Domain.Orders;
using MediatR;
using System.Collections.Generic;

namespace Grand.Services.Notifications.ShoppingCart
{
    public class ShoppingCartWarningsEvent<T, U> : INotification where U : ShoppingCartItem
    {
        private readonly IList<T> _warnings;
        private readonly IList<U> _shoppingCartItems;
        private readonly IList<CustomAttribute> _checkoutAttributes;
        private readonly bool _validateCheckoutAttributes;

        public ShoppingCartWarningsEvent(IList<T> warnings, IList<U> shoppingCartItems, IList<CustomAttribute> checkoutAttributes, bool validateCheckoutAttributes)
        {
            _warnings = warnings;
            _shoppingCartItems = shoppingCartItems;
            _checkoutAttributes = checkoutAttributes;
            _validateCheckoutAttributes = validateCheckoutAttributes;
        }
        public IList<T> Warnings { get { return _warnings; } }
        public IList<U> ShoppingCartItems { get { return _shoppingCartItems; } }

        public IList<CustomAttribute> CheckoutAttributes { get { return _checkoutAttributes; } }

        public bool ValidateCheckoutAttributes { get { return _validateCheckoutAttributes; } }

    }
}
