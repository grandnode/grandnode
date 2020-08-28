using System.Collections.Generic;

namespace Grand.Core.Configuration
{
    /// <summary>
    /// Represents a GrandConfig
    /// </summary>
    public partial class GrandConfig 
    {
        public GrandConfig()
        {
            SupportedCultures = new List<string>();
        }
        /// <summary>
        /// Indicates whether we should ignore startup tasks
        /// </summary>
        public bool IgnoreStartupTasks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to clear /Plugins/bin directory on application startup
        /// </summary>
        public bool ClearPluginShadowDirectoryOnStartup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether copy dll plugin files to /Plugins/bin on application startup
        /// </summary>
        public bool PluginShadowCopy { get; set; }

        /// <summary>
        /// Enable the Publish/Subscribe messaging with redis to manage memory cache on every server
        /// </summary>
        public bool RedisPubSubEnabled { get; set; }

        /// <summary>
        /// Redis connection string. Used when Redis Publish/Subscribe is enabled
        /// </summary>
        public string RedisPubSubConnectionString { get; set; }

        /// <summary>
        /// Messages sent by other clients to these channels will be pushed by Redis to all the subscribed clients. It must me the same value on every server
        /// </summary>
        public string RedisPubSubChannel { get; set; }

        /// <summary>
        /// Indicates whether we should use Redis server for persist keys - required in farm scenario
        /// </summary>
        public bool PersistKeysToRedis { get; set; }

        /// <summary>
        /// Redis connection string. Used when PersistKeysToRedis is enabled
        /// </summary>
        public string PersistKeysToRedisUrl { get; set; }


        /// <summary>
        /// A value indicating whether the site is run on Windows Azure Web Apps
        /// </summary>
        public bool RunOnAzureWebApps { get; set; }

        /// <summary>
        /// Connection string for Azure BLOB storage
        /// </summary>
        public string AzureBlobStorageConnectionString { get; set; }

        /// <summary>
        /// Container name for Azure BLOB storage
        /// </summary>
        public string AzureBlobStorageContainerName { get; set; }
        /// <summary>
        /// End point for Azure BLOB storage
        /// </summary>
        public string AzureBlobStorageEndPoint { get; set; }

        /// <summary>
        /// Amazon Access Key
        /// </summary>
        public string AmazonAwsAccessKeyId { get; set; }

        /// <summary>
        /// Amazon Secret Access Key
        /// </summary>
        public string AmazonAwsSecretAccessKey { get; set; }

        /// <summary>
        /// Amazon Bucket Name using for identifying resources
        /// </summary>
        public string AmazonBucketName { get; set; }

        /// <summary>
        /// Amazon Domain name for cloudfront distribution
        /// </summary>
        public string AmazonDistributionDomainName { get; set; }

        /// <summary>
        /// Amazon Region 
        /// http://docs.amazonwebservices.com/AmazonS3/latest/BucketConfiguration.html#LocationSelection
        /// </summary>
        public string AmazonRegion { get; set; }

        /// <summary>
        /// A list of plugins ignored during installation
        /// </summary>
        public string PluginsIgnoredDuringInstallation { get; set; }

        /// <summary>
        /// A list of plugins to be ignored during start application - pattern
        /// </summary>
        public string PluginSkipLoadingPattern { get; set; }

        /// <summary>
        /// Enable scripting C# applications to execute code.
        /// </summary>
        public bool UseRoslynScripts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating for default cache time in minutes
        /// </summary>
        public int DefaultCacheTimeMinutes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating for cookie expires in hours - default 24 * 365 = 8760
        /// </summary>
        public int CookieAuthExpires { get; set; }

        /// <summary>
        /// Gets or sets a value for Cookie prefix
        /// </summary>
        public string CookiePrefix { get; set; }

        /// <summary>
        /// Gets or sets a value for Cookie claim issuer 
        /// </summary>
        public string CookieClaimsIssuer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mini profiler should be displayed in public store (used for debugging)
        /// </summary>
        public bool DisplayMiniProfilerInPublicStore { get; set; }

        /// <summary>
        /// A value indicating whether SEO friendly URLs with multiple languages are enabled
        /// </summary>
        public bool SeoFriendlyUrlsForLanguagesEnabled { get; set; }
        public string SeoFriendlyUrlsDefaultCode { get; set; } = "en";
        
        /// <summary>
        /// A value indicating whether to load all search engine friendly names (slugs) on application startup
        /// </summary>
        public bool LoadAllUrlRecordsOnStartup { get; set; }

        /// <summary>
        /// Enable minimal Progressive Web App.
        /// </summary>
        public bool EnableProgressiveWebApp { get; set; }
        public int ServiceWorkerStrategy { get; set; }

        /// <summary>
        /// Gets or sets a value of "Cache-Control" header value for static content
        /// </summary>
        public string StaticFilesCacheControl { get; set; }

        /// <summary>
        /// Gets or sets a value of "Cookie SecurePolicy Always"
        /// </summary>
        public bool CookieSecurePolicyAlways { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display the full error in production environment.
        /// It's ignored (always enabled) in development environment
        /// </summary>
        public bool DisplayFullErrorStack { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we compress response
        /// </summary>
        public bool UseResponseCompression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether use the default security headers for your application
        /// </summary>
        public bool UseDefaultSecurityHeaders { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable html minification
        /// </summary>
        public bool UseHtmlMinification { get; set; }

        public bool UseSessionStateTempDataProvider { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether we use url rewrite
        /// </summary>
        public bool UseUrlRewrite { get; set; }
        public bool UrlRewriteHttpsOptions { get; set; }
        public int UrlRewriteHttpsOptionsStatusCode { get; set; }
        public int UrlRewriteHttpsOptionsPort { get; set; }
        public bool UrlRedirectToHttpsPermanent { get; set; }

        /// <summary>
        /// HTTP Strict Transport Security Protocol
        /// isn't recommended in development because the HSTS header is highly cacheable by browsers
        /// </summary>
        public bool UseHsts { get; set; }

        /// <summary>
        /// Enforce HTTPS in ASP.NET Core
        /// </summary>
        public bool UseHttpsRedirection { get; set; }

        public int HttpsRedirectionRedirect { get; set; }
        public int? HttpsRedirectionHttpsPort { get; set; }

        /// <summary>
        /// Localization middleware
        /// </summary>
        public bool UseRequestLocalization { get; set; }
        public string DefaultRequestCulture { get; set; }
        public IList<string> SupportedCultures { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ignore InstallUrlMiddleware
        /// </summary>
        public bool IgnoreInstallUrlMiddleware { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ignore IgnoreUsePoweredByMiddleware
        /// </summary>
        public bool IgnoreUsePoweredByMiddleware { get; set; }

        /// <summary>
        /// Gets or sets a value indicating - (Serilog) use middleware for smarter HTTP request logging
        /// </summary>
        public bool UseSerilogRequestLogging { get; set; }
    }
}
