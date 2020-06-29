namespace Grand.Core.Configuration
{
    public partial class HostingConfig
    {
        /// <summary>
        /// Gets or sets custom forwarded HTTP header (e.g. CF-Connecting-IP, X-FORWARDED-PROTO, etc)
        /// </summary>
        public string ForwardedHttpHeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use HTTP_CLUSTER_HTTPS
        /// </summary>
        public bool UseHttpClusterHttps { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use HTTP_X_FORWARDED_PROTO
        /// </summary>
        public bool UseHttpXForwardedProto { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use Forwards proxied headers onto current request
        /// </summary>
        public bool UseForwardedHeaders { get; set; }

        /// <summary>
        /// Gets or sets a value for allowedHosts, is used for host filtering to bind your app to specific hostnames
        /// </summary>
        public string AllowedHosts { get; set; }
    }
}
