using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Messages;
using Grand.Services.Stores;
using System;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidNewsLetterSubscription : Drop
    {
        private NewsLetterSubscription _subscription;

        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;

        public LiquidNewsLetterSubscription(IStoreService storeService,
            IStoreContext storeContext)
        {
            this._storeContext = storeContext;
            this._storeService = storeService;
        }

        public void SetProperties(NewsLetterSubscription subscription)
        {
            this._subscription = subscription;
        }

        public string Email
        {
            get { return _subscription.Email; }
        }

        public string ActivationUrl
        {
            get
            {
                string urlFormat = "{0}newsletter/subscriptionactivation/{1}/{2}";
                var activationUrl = String.Format(urlFormat, GetStoreUrl(), _subscription.NewsLetterSubscriptionGuid, "true");
                return activationUrl;
            }
        }

        public string DeactivationUrl
        {
            get
            {
                string urlFormat = "{0}newsletter/subscriptionactivation/{1}/{2}";
                var deActivationUrl = String.Format(urlFormat, GetStoreUrl(), _subscription.NewsLetterSubscriptionGuid, "false");
                return deActivationUrl;
            }
        }

        /// <summary>
        /// Get store URL
        /// </summary>
        /// <param name="storeId">Store identifier; Pass 0 to load URL of the current store</param>
        /// <param name="useSsl">Use SSL</param>
        /// <returns></returns>
        protected virtual string GetStoreUrl(string storeId = "", bool useSsl = false)
        {
            var store = _storeService.GetStoreById(storeId) ?? _storeContext.CurrentStore;

            if (store == null)
                throw new Exception("No store could be loaded");

            return useSsl ? store.SecureUrl : store.Url;
        }
    }
}