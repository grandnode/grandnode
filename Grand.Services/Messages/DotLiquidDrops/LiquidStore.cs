using DotLiquid;
using Grand.Core.Domain;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Stores;
using Grand.Core.Infrastructure;
using Grand.Services.Localization;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidStore : Drop
    {
        private Store _store;
        private EmailAccount _emailAccount;

        private readonly StoreInformationSettings _storeInformationSettings;

        public LiquidStore(Store store, EmailAccount emailAccount = null)
        {
            this._storeInformationSettings = EngineContext.Current.Resolve<StoreInformationSettings>();

            this._store = store;
            this._emailAccount = emailAccount;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string Name
        {
            get { return _store.GetLocalized(x => x.Name); }
        }

        public string URL
        {
            get { return _store.SslEnabled ? _store.SecureUrl : _store.Url; }
        }

        public string Email
        {
            get { return _emailAccount.Email; }
        }

        public string CompanyName
        {
            get { return _store.CompanyName; }
        }

        public string CompanyAddress
        {
            get { return _store.CompanyAddress; }
        }

        public string CompanyPhoneNumber
        {
            get { return _store.CompanyPhoneNumber; }
        }

        public string CompanyEmail
        {
            get { return _store.CompanyEmail; }
        }

        public string CompanyHours
        {
            get { return _store.CompanyHours; }
        }

        public string CompanyVat
        {
            get { return _store.CompanyVat; }
        }

        public string TwitterLink
        {
            get { return _storeInformationSettings.TwitterLink; }
        }

        public string FacebookLink
        {
            get { return _storeInformationSettings.FacebookLink; }
        }

        public string YoutubeLink
        {
            get { return _storeInformationSettings.YoutubeLink; }
        }

        public string InstagramLink
        {
            get { return _storeInformationSettings.InstagramLink; }
        }

        public string LinkedInLink
        {
            get { return _storeInformationSettings.LinkedInLink; }
        }

        public string PinterestLink
        {
            get { return _storeInformationSettings.PinterestLink; }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}