using Grand.Services.Common;
using System.Threading.Tasks;

namespace Grand.Plugin.Widgets.GoogleAnalytics
{
    public class GoogleAnalyticsConsentCookie : IConsentCookie
    {
        private readonly GoogleAnalyticsEcommerceSettings _googleAnalyticsEcommerceSettings;

        public GoogleAnalyticsConsentCookie(GoogleAnalyticsEcommerceSettings googleAnalyticsEcommerceSettings)
        {
            _googleAnalyticsEcommerceSettings = googleAnalyticsEcommerceSettings;
        }

        public string SystemName => GoogleAnalyticConst.ConsentCookieSystemName;

        public bool AllowToDisable => _googleAnalyticsEcommerceSettings.AllowToDisableConsentCookie;

        public bool? DefaultState => _googleAnalyticsEcommerceSettings.ConsentDefaultState;

        public int DisplayOrder => 10;

        public Task<string> FullDescription()
        {
            return Task.FromResult(_googleAnalyticsEcommerceSettings.ConsentDescription);
        }

        public Task<string> Name()
        {
            return Task.FromResult(_googleAnalyticsEcommerceSettings.ConsentName);
        }
    }
}
