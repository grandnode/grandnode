using Autofac;
using Grand.Core;
using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Domain.Common;
using Grand.Core.Infrastructure;
using Grand.Framework.Middleware;
using Grand.Framework.Mvc.Routing;
using Grand.Services.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebMarkupMin.AspNetCore3;

namespace Grand.Framework.Infrastructure.Extensions
{
    /// <summary>
    /// Represents extensions of IApplicationBuilder
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Configure the application HTTP request pipeline
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        /// <param name="webHostEnvironment">Web Host Environment</param>
        public static void ConfigureRequestPipeline(this IApplicationBuilder application, IWebHostEnvironment webHostEnvironment)
        {
            EngineContext.Current.ConfigureRequestPipeline(application, webHostEnvironment);
        }

        /// <summary>
        /// Configure container
        /// </summary>
        /// <param name="container">ContainerBuilder from autofac</param>
        /// <param name="configuration">configuration</param>
        public static void ConfigureContainer(this ContainerBuilder container, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            EngineContext.Current.ConfigureContainer(container, configuration);
        }

        /// <summary>
        /// Add exception handling
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseGrandExceptionHandler(this IApplicationBuilder application)
        {
            var serviceProvider = application.ApplicationServices;
            var grandConfig = serviceProvider.GetRequiredService<GrandConfig>();
            var hostingEnvironment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            bool useDetailedExceptionPage = grandConfig.DisplayFullErrorStack || hostingEnvironment.IsDevelopment();
            if (useDetailedExceptionPage)
            {
                //get detailed exceptions for developing and testing purposes
                application.UseDeveloperExceptionPage();
            }
            else
            {
                //or use special exception handler
                application.UseExceptionHandler("/errorpage.htm");
            }

            //log errors
            application.UseExceptionHandler(handler =>
            {
                handler.Run(async context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    if (exception == null)
                        return;

                    string authHeader = context.Request.Headers["Authorization"];
                    var apirequest = authHeader != null && authHeader.Split(' ')[0] == "Bearer";
                    if (apirequest)
                    {
                        await context.Response.WriteAsync(exception.Message);
                        return;
                    }
                    try
                    {
                        //check whether database is installed
                        if (DataSettingsHelper.DatabaseIsInstalled())
                        {
                            var logger = context.RequestServices.GetRequiredService<ILogger>();
                            //get current customer
                            var workContext = context.RequestServices.GetRequiredService<IWorkContext>();
                            //log error
                            logger.Error(exception.Message, exception, workContext.CurrentCustomer);
                        }
                    }
                    finally
                    {
                        //rethrow the exception to show the error page
                        throw exception;
                    }
                });
            });
        }

        /// <summary>
        /// Adds a special handler that checks for responses with the 404 status code that do not have a body
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UsePageNotFound(this IApplicationBuilder application)
        {
            application.UseStatusCodePages(async context =>
            {
                //handle 404 Not Found
                if (context.HttpContext.Response.StatusCode == 404)
                {
                    string authHeader = context.HttpContext.Request.Headers["Authorization"];
                    var apirequest = authHeader != null && authHeader.Split(' ')[0] == "Bearer";

                    var webHelper = context.HttpContext.RequestServices.GetRequiredService<IWebHelper>();
                    if (!apirequest && !webHelper.IsStaticResource())
                    {
                        var location = "/page-not-found";
                        context.HttpContext.Response.Redirect(context.HttpContext.Request.PathBase + location);
                    }
                    var commonSettings = context.HttpContext.RequestServices.GetRequiredService<CommonSettings>();
                    if (commonSettings.Log404Errors)
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger>();
                        //get current customer
                        var workContext = context.HttpContext.RequestServices.GetRequiredService<IWorkContext>();
                        logger.Error($"Error 404. The requested page ({context.HttpContext.Request.Scheme}://{context.HttpContext.Request.Host}{context.HttpContext.Request.Path}) was not found",
                            customer: workContext.CurrentCustomer);
                    }
                }
                await Task.CompletedTask;
            });
        }

        /// <summary>
        /// Adds a special handler that checks for responses with the 400 status code (bad request)
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseBadRequestResult(this IApplicationBuilder application)
        {
            application.UseStatusCodePages(context =>
            {
                //handle 404 (Bad request)
                if (context.HttpContext.Response.StatusCode == StatusCodes.Status400BadRequest)
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger>();
                    var workContext = context.HttpContext.RequestServices.GetRequiredService<IWorkContext>();
                    logger.Error("Error 400. Bad request", null, customer: workContext.CurrentCustomer);
                }
                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// Congifure authentication
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseGrandAuthentication(this IApplicationBuilder application)
        {
            application.UseAuthentication();
            application.UseAuthorization();
        }

        /// <summary>
        /// Configure MVC endpoint
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseGrandEndpoints(this IApplicationBuilder application)
        {
            application.UseEndpoints(endpoints =>
            {
                endpoints.ServiceProvider.GetRequiredService<IRoutePublisher>().RegisterRoutes(endpoints);
            });
        }

        /// <summary>
        /// Configure static file serving
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseGrandStaticFiles(this IApplicationBuilder application, GrandConfig grandConfig)
        {
            //static files
            application.UseStaticFiles(new StaticFileOptions {

                OnPrepareResponse = ctx =>
                {
                    if (!String.IsNullOrEmpty(grandConfig.StaticFilesCacheControl))
                        ctx.Context.Response.Headers.Append(HeaderNames.CacheControl, grandConfig.StaticFilesCacheControl);
                }

            });

            //themes
            application.UseStaticFiles(new StaticFileOptions {
                FileProvider = new PhysicalFileProvider(CommonHelper.MapPath("Themes")),
                RequestPath = new PathString("/Themes"),
                OnPrepareResponse = ctx =>
                {
                    if (!String.IsNullOrEmpty(grandConfig.StaticFilesCacheControl))
                        ctx.Context.Response.Headers.Append(HeaderNames.CacheControl, grandConfig.StaticFilesCacheControl);
                }
            });
            //plugins
            application.UseStaticFiles(new StaticFileOptions {
                FileProvider = new PhysicalFileProvider(CommonHelper.MapPath("Plugins")),
                RequestPath = new PathString("/Plugins"),
                OnPrepareResponse = ctx =>
                {
                    if (!String.IsNullOrEmpty(grandConfig.StaticFilesCacheControl))
                        ctx.Context.Response.Headers.Append(HeaderNames.CacheControl, grandConfig.StaticFilesCacheControl);
                }
            });

        }

        /// <summary>
        /// Create and configure MiniProfiler service
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseProfiler(this IApplicationBuilder application)
        {
            //whether database is already installed
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            var grandConfig = application.ApplicationServices.GetRequiredService<GrandConfig>();
            //whether MiniProfiler should be displayed
            if (grandConfig.DisplayMiniProfilerInPublicStore)
            {
                application.UseMiniProfiler();
            }
        }

        /// <summary>
        /// Save log application started
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void LogApplicationStarted(this IApplicationBuilder application)
        {
            //whether database is already installed
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            var serviceProvider = application.ApplicationServices;
            var logger = serviceProvider.GetRequiredService<ILogger>();
            logger.Information("Application started", null, null);
        }

        /// <summary>
        /// Configure UseForwardedHeaders
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseGrandForwardedHeaders(this IApplicationBuilder application)
        {
            application.UseForwardedHeaders(new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
        }

        /// <summary>
        /// Configure Health checks
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseGrandHealthChecks(this IApplicationBuilder application)
        {
            application.UseHealthChecks("/health/live");
        }

        /// <summary>
        /// Configures the default security headers for your application.
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseDefaultSecurityHeaders(this IApplicationBuilder application)
        {
            var policyCollection = new HeaderPolicyCollection()
                .AddXssProtectionBlock()
                .AddContentTypeOptionsNoSniff()
                .AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: 60 * 60 * 24 * 365) // maxage = one year in seconds
                .AddReferrerPolicyStrictOriginWhenCrossOrigin()
                .AddContentSecurityPolicy(builder =>
                {
                    builder.AddUpgradeInsecureRequests();
                    builder.AddDefaultSrc().Self();
                    builder.AddConnectSrc().From("*");
                    builder.AddFontSrc().From("*");
                    builder.AddFrameAncestors().From("*");
                    builder.AddFrameSource().From("*");
                    builder.AddMediaSrc().From("*");
                    builder.AddImgSrc().From("*").Data();
                    builder.AddObjectSrc().From("*");
                    builder.AddScriptSrc().From("*").UnsafeInline().UnsafeEval();
                    builder.AddStyleSrc().From("*").UnsafeEval().UnsafeInline();
                })
                .RemoveServerHeader();

            application.UseSecurityHeaders(policyCollection);
        }

        /// <summary>
        /// Use WebMarkupMin for your application.
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseHtmlMinification(this IApplicationBuilder application)
        {
            application.UseWebMarkupMin();
        }

        /// <summary>
        /// Configure middleware checking whether database is installed
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseInstallUrl(this IApplicationBuilder application)
        {
            application.UseMiddleware<InstallUrlMiddleware>();
        }

        /// <summary>
        /// Configures wethere use or not the Header X-Powered-By and its value.
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UsePoweredBy(this IApplicationBuilder application)
        {
            application.UseMiddleware<PoweredByMiddleware>();
        }

    }
}
