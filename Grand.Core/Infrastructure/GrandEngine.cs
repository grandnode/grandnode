using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Grand.Core.Configuration;
using Grand.Core.Extensions;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Core.Infrastructure.Mapper;
using Grand.Core.Infrastructure.MongoDB;
using Grand.Core.Plugins;
using Grand.Core.Roslyn;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Grand.Core.Infrastructure
{
    /// <summary>
    /// Represents Grand engine
    /// </summary>
    public class GrandEngine : IEngine
    {
        #region Utilities

        /// <summary>
        /// Run startup tasks
        /// </summary>
        /// <param name="typeFinder">Type finder</param>
        protected virtual void RunStartupTasks(ITypeFinder typeFinder)
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
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="typeFinder">Type finder</param>
        protected virtual void AddAutoMapper(IServiceCollection services, ITypeFinder typeFinder)
        {
            //find mapper configurations provided by other assemblies
            var mapperConfigurations = typeFinder.FindClassesOfType<IMapperProfile>();

            //create and sort instances of mapper configurations
            var instances = mapperConfigurations
                .Where(mapperConfiguration => PluginManager.FindPlugin(mapperConfiguration)
                    .Return(plugin => plugin.Installed, true)) //ignore not installed plugins
                .Select(mapperConfiguration => (IMapperProfile)Activator.CreateInstance(mapperConfiguration))
                .OrderBy(mapperConfiguration => mapperConfiguration.Order);

            //create AutoMapper configuration
            var config = new MapperConfiguration(cfg =>
            {
                foreach (var instance in instances)
                {
                    cfg.AddProfile(instance.GetType());
                }
            });

            //register
            AutoMapperConfiguration.Init(config);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize engine
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public void Initialize(IServiceCollection services, IConfiguration configuration)
        {
            //set base application path
            var provider = services.BuildServiceProvider();
            var hostingEnvironment = provider.GetRequiredService<IWebHostEnvironment>();
            var config = new GrandConfig();
            configuration.GetSection("Grand").Bind(config);

            CommonHelper.HostingEnvironment = hostingEnvironment;

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
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //find startup configurations provided by other assemblies
            var typeFinder = new WebAppTypeFinder();
            var startupConfigurations = typeFinder.FindClassesOfType<IGrandStartup>();

            //create and sort instances of startup configurations
            var instances = startupConfigurations
                .Where(startup => PluginManager.FindPlugin(startup).Return(plugin => plugin.Installed, true)) //ignore not installed plugins
                .Select(startup => (IGrandStartup)Activator.CreateInstance(startup))
                .OrderBy(startup => startup.Order);

            //configure services
            foreach (var instance in instances)
                instance.ConfigureServices(services, configuration);

            //register mapper configurations
            AddAutoMapper(services, typeFinder);

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
        public void ConfigureRequestPipeline(IApplicationBuilder application)
        {
            //find startup configurations provided by other assemblies
            var typeFinder = new WebAppTypeFinder();
            var startupConfigurations = typeFinder.FindClassesOfType<IGrandStartup>();

            //create and sort instances of startup configurations
            var instances = startupConfigurations
                .Where(startup => PluginManager.FindPlugin(startup).Return(plugin => plugin.Installed, true)) //ignore not installed plugins
                .Select(startup => (IGrandStartup)Activator.CreateInstance(startup))
                .OrderBy(startup => startup.Order);

            //configure request pipeline
            foreach (var instance in instances)
                instance.Configure(application);
        }

        /// <summary>
        /// ConfigureContainer is where you can register things directly
        /// with Autofac. This runs after ConfigureServices so the things
        /// here will override registrations made in ConfigureServices.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        public void ConfigureContainer(ContainerBuilder builder, IConfiguration configuration)
        {
            var typeFinder = new WebAppTypeFinder();

            //register type finder
            builder.RegisterInstance(typeFinder).As<ITypeFinder>().SingleInstance();

            //find dependency registrars provided by other assemblies
            var dependencyRegistrars = typeFinder.FindClassesOfType<IDependencyRegistrar>();

            //create and sort instances of dependency registrars
            var instances = dependencyRegistrars
                //.Where(startup => PluginManager.FindPlugin(startup).Return(plugin => plugin.Installed, true)) //ignore not installed plugins
                .Select(dependencyRegistrar => (IDependencyRegistrar)Activator.CreateInstance(dependencyRegistrar))
                .OrderBy(dependencyRegistrar => dependencyRegistrar.Order);

            var config = new GrandConfig();
            configuration.GetSection("Grand").Bind(config);

            //register all provided dependencies
            foreach (var dependencyRegistrar in instances)
                dependencyRegistrar.Register(builder, typeFinder, config);

        }
        #endregion

    }
}
