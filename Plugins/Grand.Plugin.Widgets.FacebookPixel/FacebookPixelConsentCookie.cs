using Grand.Services.Common;
using System.Threading.Tasks;

namespace Grand.Plugin.Widgets.FacebookPixel
{
    public class FacebookPixelConsentCookie : IConsentCookie
    {
        private readonly FacebookPixelSettings _facebookPixelSettings;

        public FacebookPixelConsentCookie(FacebookPixelSettings facebookPixelSettings)
        {
            _facebookPixelSettings = facebookPixelSettings;
        }

        public string SystemName => FacebookPixelConst.ConsentCookieSystemName;

        public bool AllowToDisable => _facebookPixelSettings.AllowToDisableConsentCookie;

        public bool? DefaultState => _facebookPixelSettings.ConsentDefaultState;

        public int DisplayOrder => 10;

        public Task<string> FullDescription()
        {
            return Task.FromResult(_facebookPixelSettings.ConsentDescription);
        }

        public Task<string> Name()
        {
            return Task.FromResult(_facebookPixelSettings.ConsentName);
        }
    }
}
