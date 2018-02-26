using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Core.Infrastructure;
using Grand.Services.Logging;
using Grand.Services.Tasks;
using Grand.Framework.FluentValidation;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Themes;
using Grand.Service.Authentication;
using FluentValidation.AspNetCore;
using FluentScheduler;
using System.Linq;
using Grand.Core.Plugins;
using Grand.Services.Authentication.External;
using Grand.Core;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Internal;
using Grand.Framework.Mvc.Routing;
using Grand.Core.Domain.Security;

namespace Grand.Framework.Infrastructure.Extensions
{
    /// <summary>
    /// Represents extensions of IServiceCollection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add services to the application and configure service provider
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration root of the application</param>
        /// <returns>Configured service provider</returns>
        public static IServiceProvider ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            //add GrandConfig configuration parameters
            services.ConfigureStartupConfig<GrandConfig>(configuration.GetSection("Grand"));
            //add hosting configuration parameters
            services.ConfigureStartupConfig<HostingConfig>(configuration.GetSection("Hosting"));
            //add accessor to HttpContext
            services.AddHttpContextAccessor();

            //create, initialize and configure the engine
            var engine = EngineContext.Create();
            engine.Initialize(services);
            var serviceProvider = engine.ConfigureServices(services, configuration);

            if (DataSettingsHelper.DatabaseIsInstalled())
            {
                //implement schedule tasks
                //database is already installed, so start scheduled tasks

                var scheduleTasks = ScheduleTaskManager.Instance.LoadScheduleTasks();       //load records from db to collection
                JobManager.Initialize(new RegistryGrandNode(scheduleTasks));                //init registry and start scheduled tasks

                //log application start
                EngineContext.Current.Resolve<ILogger>().Information("Application started", null, null);
            }

            return serviceProvider;
        }

        /// <summary>
        /// Create, bind and register as service the specified configuration parameters 
        /// </summary>
        /// <typeparam name="TConfig">Configuration parameters</typeparam>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Set of key/value application configuration properties</param>
        /// <returns>Instance of configuration parameters</returns>
        public static TConfig ConfigureStartupConfig<TConfig>(this IServiceCollection services, IConfiguration configuration) where TConfig : class, new()
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            //create instance of config
            var config = new TConfig();

            //bind it to the appropriate section of configuration
            configuration.Bind(config);

            //and register it as a service
            services.AddSingleton(config);

            return config;
        }

        /// <summary>
        /// Register HttpContextAccessor
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public static void AddHttpContextAccessor(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        /// <summary>
        /// Adds services required for anti-forgery support
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public static void AddAntiForgery(this IServiceCollection services)
        {
            //override cookie name
            services.AddAntiforgery(options =>
            {
                options.Cookie = new CookieBuilder()
                {
                    Name = ".Grand.Antiforgery"
                };
                if (DataSettingsHelper.DatabaseIsInstalled())
                {
                    //whether to allow the use of anti-forgery cookies from SSL protected page on the other store pages which are not
                    options.Cookie.SecurePolicy = EngineContext.Current.Resolve<SecuritySettings>().ForceSslForAllPages
                    ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.None;
                }
            });
        }

        /// <summary>
        /// Adds services required for application session state
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public static void AddHttpSession(this IServiceCollection services)
        {
            services.AddSession(options =>
            {
                options.Cookie = new CookieBuilder()
                {
                    Name = ".Grand.Session",
                    HttpOnly = true,
                };
                if (DataSettingsHelper.DatabaseIsInstalled())
                {
                    //whether to allow the use of session values from SSL protected page on the other store pages which are not
                    options.Cookie.SecurePolicy = EngineContext.Current.Resolve<SecuritySettings>().ForceSslForAllPages
                        ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.None;
                }
            });
        }

        /// <summary>
        /// Adds services required for themes support
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public static void AddThemes(this IServiceCollection services)
        {
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            //themes support
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ThemeableViewLocationExpander());
            });
        }

        /// <summary>
        /// Adds data protection services
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public static void AddGrandDataProtection(this IServiceCollection services)
        {
            var dataProtectionKeysPath = CommonHelper.MapPath("~/App_Data/DataProtectionKeys");
            var dataProtectionKeysFolder = new DirectoryInfo(dataProtectionKeysPath);

            //configure the data protection system to persist keys to the specified directory
            services.AddDataProtection().PersistKeysToFileSystem(dataProtectionKeysFolder);
        }

        /// <summary>
        /// Adds authentication service
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public static void AddGrandAuthentication(this IServiceCollection services)
        {

            //set default authentication schemes
            var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = GrandCookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = GrandCookieAuthenticationDefaults.ExternalAuthenticationScheme;
            });

            //register external authentication plugins now
            var typeFinder = new WebAppTypeFinder();
            var externalAuthConfigurations = typeFinder.FindClassesOfType<IExternalAuthenticationRegistrar>();
            //create and sort instances of external authentication configurations
            var externalAuthInstances = externalAuthConfigurations
                .Where(x => PluginManager.FindPlugin(x)?.Installed ?? true) //ignore not installed plugins
                .Select(x => (IExternalAuthenticationRegistrar)Activator.CreateInstance(x))
                .OrderBy(x => x.Order);

            //configure services
            foreach (var instance in externalAuthInstances)
                instance.Configure(authenticationBuilder);

            //enable main cookie authentication
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = GrandCookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(GrandCookieAuthenticationDefaults.AuthenticationScheme,
                options =>
                {
                    options.ClaimsIssuer = GrandCookieAuthenticationDefaults.ClaimsIssuer;
                    options.Cookie = new CookieBuilder()
                    {
                        Name = GrandCookieAuthenticationDefaults.CookiePrefix + GrandCookieAuthenticationDefaults.AuthenticationScheme,
                        HttpOnly = true,
                    };
                    options.LoginPath = GrandCookieAuthenticationDefaults.LoginPath;
                    options.AccessDeniedPath = GrandCookieAuthenticationDefaults.AccessDeniedPath;
                    options.LogoutPath = GrandCookieAuthenticationDefaults.LogoutPath;
                    if (DataSettingsHelper.DatabaseIsInstalled())
                    {
                        //whether to allow the use of authentication cookies from SSL protected page on the other store pages which are not
                        options.Cookie.SecurePolicy = EngineContext.Current.Resolve<SecuritySettings>().ForceSslForAllPages
                        ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.None;
                    }
                }
            );

            //enable external authentication
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = GrandCookieAuthenticationDefaults.ExternalAuthenticationScheme;
            })
            .AddCookie(GrandCookieAuthenticationDefaults.ExternalAuthenticationScheme,
                options =>
                {
                    options.ClaimsIssuer = GrandCookieAuthenticationDefaults.ClaimsIssuer;
                    options.Cookie = new CookieBuilder()
                    {
                        Name = GrandCookieAuthenticationDefaults.CookiePrefix + GrandCookieAuthenticationDefaults.AuthenticationScheme,
                        HttpOnly = true,
                    };
                    options.LoginPath = GrandCookieAuthenticationDefaults.LoginPath;
                    options.AccessDeniedPath = GrandCookieAuthenticationDefaults.AccessDeniedPath;
                    options.LogoutPath = GrandCookieAuthenticationDefaults.LogoutPath;

                    // whether to allow the use of authentication cookies from SSL protected page on the other store pages which are not
                    options.Cookie.SecurePolicy = EngineContext.Current.Resolve<SecuritySettings>().ForceSslForAllPages
                     ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.None;
                }
            );
        }

        /// <summary>
        /// Add and configure MVC for the application
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <returns>A builder for configuring MVC services</returns>
        public static IMvcBuilder AddGrandMvc(this IServiceCollection services)
        {
            //add basic MVC feature
            var mvcBuilder = services.AddMvc();

            //MVC now serializes JSON with camel case names by default, use this code to avoid it
            mvcBuilder.AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            //add custom display metadata provider
            mvcBuilder.AddMvcOptions(options => options.ModelMetadataDetailsProviders.Add(new GrandMetadataProvider()));

            //add custom model binder provider (to the top of the provider list)
            mvcBuilder.AddMvcOptions(options => options.ModelBinderProviders.Insert(0, new GrandModelBinderProvider()));

            //add fluent validation
            mvcBuilder.AddFluentValidation(configuration => configuration.ValidatorFactoryType = typeof(GrandValidatorFactory));

            return mvcBuilder;
        }

        /// <summary>
        /// Register custom RedirectResultExecutor
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public static void AddGrandRedirectResultExecutor(this IServiceCollection services)
        {
            //we use custom redirect executor as a workaround to allow using non-ASCII characters in redirect URLs
            services.AddSingleton<RedirectResultExecutor, GrandRedirectResultExecutor>();
        }
    }
}
