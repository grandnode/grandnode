using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Messages;
using Grand.Core.Infrastructure;
using Grand.Services.Stores;
using System;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidNewsLetterSubscription : Drop
    {
        private NewsLetterSubscription _subscription;

        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;

        public LiquidNewsLetterSubscription(NewsLetterSubscription subscription)
        {
            this._storeContext = EngineContext.Current.Resolve<IStoreContext>();
            this._storeService = EngineContext.Current.Resolve<IStoreService>();

            this._subscription = subscription;

            AdditionalTokens = new Dictionary<string, string>();
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
                var activationUrl = String.Format(urlFormat, _storeService.GetStoreUrl(_subscription.StoreId), _subscription.NewsLetterSubscriptionGuid, "true");
                return activationUrl;
            }
        }

        public string DeactivationUrl
        {
            get
            {
                string urlFormat = "{0}newsletter/subscriptionactivation/{1}/{2}";
                var deActivationUrl = String.Format(urlFormat, _storeService.GetStoreUrl(_subscription.StoreId), _subscription.NewsLetterSubscriptionGuid, "false");
                return deActivationUrl;
            }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}