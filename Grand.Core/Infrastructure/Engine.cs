using AutoMapper;
using Grand.Core.Configuration;
using Grand.Core.Extensions;
using Grand.Core.DependencyInjection;
using Grand.Core.Mapper;
using Grand.Core.Plugins;
using Grand.Core.Roslyn;
using Grand.Domain.MongoDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Grand.Core.Infrastructure
{
    /// <summary>
    /// Represents engine
    /// </summary>
    public static class Engine
    {
        #region Utilities

        /// <summary>
        /// Run startup tasks
        /// </summary>
        /// <param name="typeFinder">Type finder</param>
        private static void RunStartupTasks(ITypeFinder typeFinder)
        {
            //find startup tasks provided by other assemblies
            var startupTasks = typeFinder.FindClassesOfType<IStartupTask>();

            //create and sort instances of startup tasks
            var instances = startupTasks
                //.Where(startupTask => PluginManager.FindPlugin(startupTask).Return(plugin => plugin.Installed, true)) //ignore not installed plugins
                .Select(startupTask => (IStartupTask)Activator.CreateInstance(startupTask))
                .OrderBy(startupTask => startupTask.Order);

            //execute tasks
            foreach (var task in instances)
                task.Execute();
        }

        /// <summary>
        /// Register and configure AutoMapper
        /// </summary>
        /// <param name="typeFinder">Type finder</param>
        private static void AddAutoMapper(ITypeFinder typeFinder)
        {
            //find mapper configurations provided by other assemblies
            var mapperConfigurations = typeFinder.FindClassesOfType<IAutoMapperProfile>();

            //create and sort instances of mapper configurations
            var instances = mapperConfigurations
                .Where(mapperConfiguration => PluginManager.FindPlugin(mapperConfiguration)
                    .Return(plugin => plugin.Installed, true))
                .Select(mapperConfiguration => (IAutoMapperProfile)Activator.CreateInstance(mapperConfiguration))
                .OrderBy(mapperConfiguration => mapperConfiguration.Order);

            //create AutoMapper configuration
            var config = new MapperConfiguration(cfg =>
            {
                foreach (var instance in instances)
                {
                    cfg.AddProfile(instance.GetType());
                }
            });

            //register automapper
            AutoMapperConfig.Init(config);
        }

        /// <summary>
        /// Add attributes to convert some classes
        /// </summary>
        private static void AddTypeConverter(ITypeFinder typeFinder)
        {
            //find converters provided by other assemblies
            var converters = typeFinder.FindClassesOfType<ITypeConverter>();

            //create and sort instances of typeConverter 
            var instances = converters
                .Select(converter => (ITypeConverter)Activator.CreateInstance(converter))
                .OrderBy(converter => converter.Order);

            foreach (var item in instances)
                item.Register();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize engine
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public static void Initialize(IServiceCollection services, IConfiguration configuration)
        {
            //set base application path
            var provider = services.BuildServiceProvider();
            var hostingEnvironment = provider.GetRequiredService<IWebHostEnvironment>();
            var config = new GrandConfig();
            configuration.GetSection("Grand").Bind(config);

            CommonHelper.WebRootPath = hostingEnvironment.WebRootPath;
            CommonHelper.BaseDirectory = hostingEnvironment.ContentRootPath;
            CommonHelper.CacheTimeMinutes = config.DefaultCacheTimeMinutes;
            CommonHelper.CookieAuthExpires = config.CookieAuthExpires > 0 ? config.CookieAuthExpires : 24 * 365;
            CommonHelper.AllowToJsonResponse = config.AllowToJsonResponse;

            //register mongo mappings
            MongoDBMapperConfiguration.RegisterMongoDBMappings();

            //initialize plugins
            var mvcCoreBuilder = services.AddMvcCore();
            PluginManager.Initialize(mvcCoreBuilder, config);

            //initialize CTX sctipts
            RoslynCompiler.Initialize(mvcCoreBuilder.PartManager, config);

        }

        /// <summary>
        /// Add and configure services
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration root of the application</param>
        /// <returns>Service provider</returns>
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //find startup configurations provided by other assemblies
            var typeFinder = new WebAppTypeFinder();
            var startupConfigurations = typeFinder.FindClassesOfType<IGrandStartup>();

            //create and sort instances of startup configurations
            var instances = startupConfigurations
                //.Where(startup => PluginManager.FindPlugin(startup).Return(plugin => plugin.Installed, true)) //ignore not installed plugins
                .Select(startup => (IGrandStartup)Activator.CreateInstance(startup))
                .OrderBy(startup => startup.Order);

            //configure services
            foreach (var instance in instances)
                instance.ConfigureServices(services, configuration);

            //register mapper configurations
            AddAutoMapper(typeFinder);

            //Register custom type converters
            AddTypeConverter(typeFinder);

            var config = new GrandConfig();
            configuration.GetSection("Grand").Bind(config);

            //run startup tasks
            if (!config.IgnoreStartupTasks)
                RunStartupTasks(typeFinder);

        }

        /// <summary>
        /// Configure HTTP request pipeline
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        /// <param name="webHostEnvironment">WebHostEnvironment</param>
        public static void ConfigureRequestPipeline(IApplicationBuilder application, IWebHostEnvironment webHostEnvironment)
        {
            //find startup configurations provided by other assemblies
            var typeFinder = new WebAppTypeFinder();
            var startupConfigurations = typeFinder.FindClassesOfType<IGrandStartup>();

            //create and sort instances of startup configurations
            var instances = startupConfigurations
                //.Where(startup => PluginManager.FindPlugin(startup).Return(plugin => plugin.Installed, true)) //ignore not installed plugins
                .Select(startup => (IGrandStartup)Activator.CreateInstance(startup))
                .OrderBy(startup => startup.Order);

            //configure request pipeline
            foreach (var instance in instances)
                instance.Configure(application, webHostEnvironment);
        }

        /// <summary>
        /// ConfigureContainer is where you can register things directly
        /// with Autofac. This runs after ConfigureServices so the things
        /// here will override registrations made in ConfigureServices.
        /// </summary>
        /// <param name="serviceCollection">Service Collection</param>
        /// <param name="configuration">Configuration</param>
        public static void ConfigureContainer(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var typeFinder = new WebAppTypeFinder();

            //register type finder
            serviceCollection.AddSingleton<ITypeFinder>(typeFinder);

            //find dependency registrars provided by other assemblies
            var dependencyRegistrars = typeFinder.FindClassesOfType<IDependencyInjection>();

            //create and sort instances of dependency registrars
            var instances = dependencyRegistrars
                //.Where(startup => PluginManager.FindPlugin(startup).Return(plugin => plugin.Installed, true)) //ignore not installed plugins
                .Select(dependencyRegistrar => (IDependencyInjection)Activator.CreateInstance(dependencyRegistrar))
                .OrderBy(dependencyRegistrar => dependencyRegistrar.Order);

            var config = new GrandConfig();
            configuration.GetSection("Grand").Bind(config);

            //register all provided dependencies
            foreach (var dependencyRegistrar in instances)
                dependencyRegistrar.Register(serviceCollection, typeFinder, config);

        }
        #endregion

    }
}
