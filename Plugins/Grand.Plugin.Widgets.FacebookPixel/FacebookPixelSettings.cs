using Grand.Domain.Configuration;

namespace Grand.Plugin.Widgets.FacebookPixel
{
    public class FacebookPixelSettings : ISettings
    {
        public string PixelId { get; set; }
        public string PixelScript { get; set; }
        public string AddToCartScript { get; set; }
        public string DetailsOrderScript { get; set; }
        public bool AllowToDisableConsentCookie { get; set; }
        public bool ConsentDefaultState { get; set; }
        public string ConsentName { get; set; }
        public string ConsentDescription { get; set; }
    }
}