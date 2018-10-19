using Grand.Core.Domain.Orders;
using System.Collections.Generic;

namespace Grand.Services.Events.Web
{
    public class ShoppingCartWarningsEvent<T, U> where U : ShoppingCartItem
    {
        private readonly IList<T> _warnings;
        private readonly IList<U> _shoppingCartItems;
        private readonly string _checkoutAttributesXml;
        private readonly bool _validateCheckoutAttributes;

        public ShoppingCartWarningsEvent(IList<T> warnings, IList<U> shoppingCartItems, string checkoutAttributesXml, bool validateCheckoutAttributes)
        {
            _warnings = warnings;
            _shoppingCartItems = shoppingCartItems;
            _checkoutAttributesXml = checkoutAttributesXml;
            _validateCheckoutAttributes = validateCheckoutAttributes;
        }
        public IList<T> Warnings { get { return _warnings; } }
        public IList<U> ShoppingCartItems { get { return _shoppingCartItems; } }

        public string CheckoutAttributesXml { get { return _checkoutAttributesXml; } }

        public bool ValidateCheckoutAttributes { get { return _validateCheckoutAttributes; } }

    }
}
