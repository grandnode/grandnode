
using Grand.Core.Configuration;

namespace Grand.Plugin.Widgets.GoogleAnalytics
{
    public class GoogleAnalyticsEcommerceSettings : ISettings
    {
        public string GoogleId { get; set; }
        public string TrackingScript { get; set; }
        public string EcommerceScript { get; set; }
        public string EcommerceDetailScript { get; set; }
        public bool IncludingTax { get; set; }
    }
}