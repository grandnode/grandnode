using Grand.Core.Configuration;

namespace Grand.Core.Domain.Seo
{
    /// <summary>
    /// GoogleAnalytics settings
    /// </summary>
    public class GoogleAnalyticsSettings : ISettings
    {
        /// <summary>
        /// GoogleAnalytics PrivateKey
        /// </summary>
        public string gaprivateKey { get; set; }

        /// <summary>
        /// GoogleAnalytics ServiceAccountEmail
        /// </summary>
        public string gaserviceAccountEmail { get; set; }

        /// <summary>
        /// GoogleAnalytics ViewID
        /// </summary>
        public string gaviewID { get; set; }

    }
}