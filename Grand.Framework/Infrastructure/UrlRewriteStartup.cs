using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

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
            var urlRewriteOptions = new RewriteOptions();
            var rewriteOptions = false;
            if (grandConfig.UseUrlRewrite)
            {
                if (File.Exists("App_Data/UrlRewrite.xml"))
                {
                    using (var streamReader = File.OpenText("App_Data/UrlRewrite.xml"))
                    {
                        rewriteOptions = true;
                        urlRewriteOptions.AddIISUrlRewrite(streamReader);
                    }
                }
            }
            if (grandConfig.UrlRewriteHttpsOptions)
            {
                rewriteOptions = true;
                urlRewriteOptions.AddRedirectToHttps(grandConfig.UrlRewriteHttpsOptionsStatusCode, grandConfig.UrlRewriteHttpsOptionsPort);
            }
            if (grandConfig.UrlRedirectToHttpsPermanent)
            {
                rewriteOptions = true;
                urlRewriteOptions.AddRedirectToHttpsPermanent();
            }
            if(rewriteOptions)
                application.UseRewriter(urlRewriteOptions);
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
