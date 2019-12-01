using Autofac;
using AutoMapper.Configuration;
using Grand.Core;
using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Core.Domain;
using Grand.Core.Http;
using Grand.Core.Infrastructure;
using Grand.Framework.Middleware;
using Grand.Framework.Mvc.Routing;
using Grand.Services.Authentication;
using Grand.Services.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.Memory;
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
        public static void ConfigureRequestPipeline(this IApplicationBuilder application, IWebHostEnvironment webHostEnvironment)
        {
            EngineContext.Current.ConfigureRequestPipeline(application);
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
                handler.Run(context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    if (exception == null)
                        return Task.CompletedTask;

                    string authHeader = context.Request.Headers["Authorization"];
                    var apirequest = authHeader != null && authHeader.Split(' ')[0] == "Bearer";
                    if (apirequest)
                    {
                        context.Response.WriteAsync(exception.Message).Wait();
                        return Task.CompletedTask;
                    }
                    try
                    {
                        //check whether database is installed
                        if (DataSettingsHelper.DatabaseIsInstalled())
                        {
                            //get current customer
                            var currentCustomer = serviceProvider.GetRequiredService<IWorkContext>().CurrentCustomer;

                            //log error
                            serviceProvider.GetRequiredService<ILogger>().Error(exception.Message, exception, currentCustomer);
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
            var serviceProvider = application.ApplicationServices;
            application.UseStatusCodePages(async context =>
            {
                string authHeader = context.HttpContext.Request.Headers["Authorization"];
                var apirequest = authHeader != null && authHeader.Split(' ')[0] == "Bearer";

                //handle 404 Not Found
                if (!apirequest && context.HttpContext.Response.StatusCode == 404)
                {
                    var webHelper = serviceProvider.GetRequiredService<IWebHelper>();
                    if (!webHelper.IsStaticResource())
                    {
                        //get original path and query
                        var originalPath = context.HttpContext.Request.Path;
                        var originalQueryString = context.HttpContext.Request.QueryString;

                        //store the original paths in special feature, so we can use it later
                        context.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(new StatusCodeReExecuteFeature()
                        {
                            OriginalPathBase = context.HttpContext.Request.PathBase.Value,
                            OriginalPath = originalPath.Value,
                            OriginalQueryString = originalQueryString.HasValue ? originalQueryString.Value : null,
                        });

                        //get new path
                        context.HttpContext.Request.Path = "/page-not-found";
                        context.HttpContext.Request.QueryString = QueryString.Empty;

                        try
                        {
                            //re-execute request with new path
                            await context.Next(context.HttpContext);
                        }
                        finally
                        {
                            //return original path to request
                            context.HttpContext.Request.QueryString = originalQueryString;
                            context.HttpContext.Request.Path = originalPath;
                            context.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(null);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Adds a special handler that checks for responses with the 400 status code (bad request)
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseBadRequestResult(this IApplicationBuilder application)
        {
            var serviceProvider = application.ApplicationServices;
            application.UseStatusCodePages(context =>
            {
                //handle 404 (Bad request)
                if (context.HttpContext.Response.StatusCode == StatusCodes.Status400BadRequest)
                {
                    var logger = serviceProvider.GetRequiredService<ILogger>();
                    var workContext = serviceProvider.GetRequiredService<IWorkContext>();
                    logger.Error("Error 400. Bad request", null, customer: workContext.CurrentCustomer);
                }

                return Task.CompletedTask;
            });

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
        /// Congifure authentication
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseGrandAuthentication(this IApplicationBuilder application)
        {
            //application.UseMiddleware<AuthenticationMiddleware>();
            //TO DO AuthenticationMiddleware should be remove and use this
            application.UseAuthentication();
            application.UseAuthorization();
        }

        /// <summary>
        /// Configure MVC routing
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseGrandMvc(this IApplicationBuilder application)
        {
            application.UseMvc(routeBuilder =>
            {
                //register all routes
                routeBuilder.ServiceProvider.GetRequiredService<IRoutePublisher>().RegisterRoutes(routeBuilder);
            });
        }

        /// <summary>
        /// Configure static file serving
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseGrandStaticFiles(this IApplicationBuilder application, GrandConfig grandConfig)
        {
            //static files
            application.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    if (!String.IsNullOrEmpty(grandConfig.StaticFilesCacheControl))
                        ctx.Context.Response.Headers.Append(HeaderNames.CacheControl, grandConfig.StaticFilesCacheControl);
                }
            });

            //themes
            application.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(CommonHelper.MapPath("Themes")),
                RequestPath = new PathString("/Themes"),
                OnPrepareResponse = ctx =>
                {
                    if (!String.IsNullOrEmpty(grandConfig.StaticFilesCacheControl))
                        ctx.Context.Response.Headers.Append(HeaderNames.CacheControl, grandConfig.StaticFilesCacheControl);
                }
            });
            //plugins
            application.UseStaticFiles(new StaticFileOptions
            {
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

            var serviceProvider = application.ApplicationServices;
            //whether MiniProfiler should be displayed
            if (serviceProvider.GetRequiredService<StoreInformationSettings>().DisplayMiniProfilerInPublicStore)
            {
                application.UseMiniProfiler();
            }
        }

        /// <summary>
        /// Configure UseForwardedHeaders
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseGrandForwardedHeaders(this IApplicationBuilder application)
        {
            application.UseForwardedHeaders(new ForwardedHeadersOptions
            {
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
        /// Configures wethere use or not the Header X-Powered-By and its value.
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UsePoweredBy(this IApplicationBuilder application)
        {
            application.UseMiddleware<PoweredByMiddleware>();
        }

    }
}
