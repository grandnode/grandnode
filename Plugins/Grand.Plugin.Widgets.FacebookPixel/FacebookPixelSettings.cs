using Grand.Core.Configuration;

namespace Grand.Plugin.Widgets.FacebookPixel
{
    public class FacebookPixelSettings : ISettings
    {
        public string PixelId { get; set; }
        public string PixelScript { get; set; }
        public string AddToCartScript { get; set; }
        public string DetailsOrderScript { get; set; }
    }
}