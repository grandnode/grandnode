using DotLiquid;
using Grand.Core.Domain;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Stores;
using Grand.Services.Localization;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidStore : Drop
    {
        private readonly Store _store;
        private readonly EmailAccount _emailAccount;

        private readonly StoreInformationSettings _storeInformationSettings;

        public LiquidStore(Store store, StoreInformationSettings storeInformationSettings, EmailAccount emailAccount = null)
        {
            this._store = store;
            this._storeInformationSettings = storeInformationSettings;
            this._emailAccount = emailAccount;
        }

        public string Name
        {
            get { return _store.GetLocalized(x => x.Name); }
        }

        public string Url
        {
            get { return _store.Url; }
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

        public string GooglePlusLink
        {
            get { return _storeInformationSettings.GooglePlusLink; }
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
    }
}