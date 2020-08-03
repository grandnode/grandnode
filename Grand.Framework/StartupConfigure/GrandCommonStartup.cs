using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Framework.Infrastructure.Extensions;
using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections.Generic;
using System.Globalization;

namespace Grand.Framework.StartupConfigure
{
    /// <summary>
    /// Represents object for the configuring common features and middleware on application startup
    /// </summary>
    public class GrandCommonStartup : IGrandStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration root of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var config = new GrandConfig();
            configuration.GetSection("Grand").Bind(config);

            //add settings
            services.AddSettings();

            //compression
            services.AddResponseCompression();

            //add options feature
            services.AddOptions();
            
            //add HTTP sesion state feature
            services.AddHttpSession(config);

            //add anti-forgery
            services.AddAntiForgery(config);

            //add localization
            services.AddLocalization();

            //add theme support
            services.AddThemes();

            //add WebEncoderOptions
            services.AddWebEncoder();

            services.AddRouting(options =>
            {
                options.ConstraintMap["lang"] = typeof(LanguageParameterTransformer);
            });

        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        /// <param name="webHostEnvironment">WebHostEnvironment</param>
        public void Configure(IApplicationBuilder application, IWebHostEnvironment webHostEnvironment)
        {
            var serviceProvider = application.ApplicationServices;
            var grandConfig = serviceProvider.GetRequiredService<GrandConfig>();

            //add HealthChecks
            application.UseGrandHealthChecks();

            //default security headers
            if (grandConfig.UseDefaultSecurityHeaders)
            {
                application.UseDefaultSecurityHeaders();
            }

            //use hsts
            if (grandConfig.UseHsts)
            {
                application.UseHsts();
            }
            //enforce HTTPS in ASP.NET Core
            if (grandConfig.UseHttpsRedirection)
            {
                application.UseHttpsRedirection();
            }

            //compression
            if (grandConfig.UseResponseCompression)
            {
                //gzip by default
                application.UseResponseCompression();
            }

            //Add webMarkupMin
            if (grandConfig.UseHtmlMinification)
            {
                application.UseHtmlMinification();
            }

            //use request localization
            if (grandConfig.UseRequestLocalization)
            {
                var supportedCultures = new List<CultureInfo>();
                foreach (var culture in grandConfig.SupportedCultures)
                {
                    supportedCultures.Add(new CultureInfo(culture));
                }
                application.UseRequestLocalization(new RequestLocalizationOptions {
                    DefaultRequestCulture = new RequestCulture(grandConfig.DefaultRequestCulture),
                    SupportedCultures = supportedCultures,
                    SupportedUICultures = supportedCultures
                });
            }
            else
                //use default request localization
                application.UseRequestLocalization();

            //use static files feature
            application.UseGrandStaticFiles(grandConfig);

            //check whether database is installed
            if (!grandConfig.IgnoreInstallUrlMiddleware)
                application.UseInstallUrl();

            //use HTTP session
            application.UseSession();            

            //use powered by
            if (!grandConfig.IgnoreUsePoweredByMiddleware)
                application.UsePoweredBy();

            // Write streamlined request completion events, instead of the more verbose ones from the framework.
            // To use the default framework request logging instead, remove this line and set the "Microsoft"
            // level in appsettings.json to "Information".
            if (grandConfig.UseSerilogRequestLogging)
                application.UseSerilogRequestLogging();

            //use routing
            application.UseRouting();

        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order
        {
            //common services should be loaded after error handlers
            get { return 100; }
        }
    }
}
