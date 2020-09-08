using Grand.Domain.Customers;
using Grand.Domain.Stores;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Common
{
    public class CookiePreference : ICookiePreference
    {
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IEnumerable<IConsentCookie> _consentCookies;

        public CookiePreference(IGenericAttributeService genericAttributeService,
            IEnumerable<IConsentCookie> consentCookies)
        {
            _genericAttributeService = genericAttributeService;
            _consentCookies = consentCookies;
        }

        public virtual IList<IConsentCookie> GetConsentCookies()
        {
            return _consentCookies.OrderBy(x => x.DisplayOrder).ToList();
        }


        public virtual async Task<bool?> IsEnable(Customer customer, Store store, string cookieSystemName)
        {
            bool? result = default(bool?);
            var savedCookiesConsent = await _genericAttributeService.GetAttributesForEntity<Dictionary<string, bool>>(customer, SystemCustomerAttributeNames.ConsentCookies, store.Id);
            if (savedCookiesConsent != null)
                if (savedCookiesConsent.ContainsKey(cookieSystemName))
                    result = savedCookiesConsent[cookieSystemName];

            return result;
        }
    }
}
