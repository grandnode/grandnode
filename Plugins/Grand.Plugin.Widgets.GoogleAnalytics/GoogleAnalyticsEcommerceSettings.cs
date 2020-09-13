using Grand.Domain.Configuration;

namespace Grand.Plugin.Widgets.GoogleAnalytics
{
    public class GoogleAnalyticsEcommerceSettings : ISettings
    {
        public string GoogleId { get; set; }
        public string TrackingScript { get; set; }
        public string EcommerceScript { get; set; }
        public string EcommerceDetailScript { get; set; }
        public bool IncludingTax { get; set; }
        public bool AllowToDisableConsentCookie { get; set; }
        public bool ConsentDefaultState { get; set; }
        public string ConsentName { get; set; }
        public string ConsentDescription { get; set; }
    }
}