using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Grand.Core.Infrastructure;
using Grand.Core.Data;
using Microsoft.AspNetCore.Rewrite;
using System.IO;
using Grand.Core.Configuration;

namespace Grand.Framework.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring/load url rewrite rules from external file on application startup
    /// </summary>
    public class UrlRewriteStartup : IGrandStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration root of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
            var grandConfig = EngineContext.Current.Resolve<GrandConfig>();
            if (grandConfig.UseUrlRewrite)
            {
                var urlRewriteOptions = new RewriteOptions();
                if (File.Exists("App_Data/UrlRewrite.xml"))
                {
                    using (var streamReader = File.OpenText("App_Data/UrlRewrite.xml"))
                    {
                        urlRewriteOptions.AddIISUrlRewrite(streamReader);
                    }
                }
                if (grandConfig.UrlRewriteHttpsOptions)
                {
                    urlRewriteOptions.AddRedirectToHttps(grandConfig.UrlRewriteHttpsOptionsStatusCode, grandConfig.UrlRewriteHttpsOptionsPort);
                }
                if (grandConfig.UrlRedirectToHttpsPermanent)
                {
                    urlRewriteOptions.AddRedirectToHttpsPermanent();
                }
                application.UseRewriter(urlRewriteOptions);
            }
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order
        {
            get { return 0; }
        }
    }
}
