using DotLiquid;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidShoppingCart : Drop
    {
        private Customer _customer;
        private Language _language;
        private Store _store;

        private string _personalMessage;
        private string _customerEmail;

        public LiquidShoppingCart(Customer customer, Store store, Language language,
            string personalMessage = "", string customerEmail = "")
        {
            this._customer = customer;
            this._language = language;
            this._store = store;
            this._personalMessage = personalMessage;
            this._customerEmail = customerEmail;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string ShoppingCartProducts { get; set; }

        public string ShoppingCartProductsWithPictures { get; set; }

        public string WishlistProducts { get; set; }

        public string WishlistProductsWithPictures { get; set; }

        public string WishlistPersonalMessage
        {
            get { return _personalMessage; }
        }

        public string WishlistEmail
        {
            get { return _customerEmail; }
        }

        public string WishlistURLForCustomer
        {
            get
            {
                var wishlistUrl = string.Format("{0}wishlist/{1}", (_store.SslEnabled ? _store.SecureUrl : _store.Url), _customer.CustomerGuid);
                return wishlistUrl;
            }
        }
        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}