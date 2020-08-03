using DotLiquid;
using Grand.Domain.Localization;
using Grand.Domain.Messages;
using Grand.Domain.Stores;
using Grand.Services.Localization;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidStore : Drop
    {
        private Store _store;
        private EmailAccount _emailAccount;
        private Language _language;

        public LiquidStore(Store store, Language language, EmailAccount emailAccount = null)
        {
            _store = store;
            _language = language;
            _emailAccount = emailAccount;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string Name
        {
            get { return _store.GetLocalized(x => x.Name, _language.Id); }
        }

        public string Shortcut {
            get { return _store.GetLocalized(x => x.Shortcut, _language.Id); }
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

        public string TwitterLink { get; set; }

        public string FacebookLink { get; set; }

        public string YoutubeLink { get; set; }

        public string InstagramLink { get; set; }

        public string LinkedInLink { get; set; }

        public string PinterestLink { get; set; }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}