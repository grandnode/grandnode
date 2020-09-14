using Autofac;
using FluentValidation;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Caching.Message;
using Grand.Core.Caching.Redis;
using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Core.Plugins;
using Grand.Core.Routing;
using Grand.Core.Validators;
using Grand.Domain.Data;
using Grand.Framework.Middleware;
using Grand.Framework.Mvc.Routing;
using Grand.Framework.TagHelpers;
using Grand.Framework.Themes;
using Grand.Framework.UI;
using MongoDB.Driver;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Reflection;

namespace Grand.Framework.Infrastructure
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            RegisterDataLayer(builder);

            RegisterCache(builder, config);

            RegisterCore(builder);

            RegisterContextService(builder);

            RegisterValidators(builder, typeFinder);

            RegisterFramework(builder);
        }

        /// <summary>
        /// Gets order of this dependency registrar implementation
        /// </summary>
        public int Order {
            get { return 0; }
        }

        private void RegisterCore(ContainerBuilder builder)
        {
            //web helper
            builder.RegisterType<WebHelper>().As<IWebHelper>().InstancePerLifetimeScope();
            //plugins
            builder.RegisterType<PluginFinder>().As<IPluginFinder>().InstancePerLifetimeScope();
        }

        private void RegisterDataLayer(ContainerBuilder builder)
        {
            var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = dataSettingsManager.LoadSettings();
            if (string.IsNullOrEmpty(dataProviderSettings.DataConnectionString))
            {
                builder.Register(c => dataSettingsManager.LoadSettings()).As<DataSettings>();
                builder.Register(x => new MongoDBDataProviderManager(x.Resolve<DataSettings>())).As<BaseDataProviderManager>().InstancePerDependency();
                builder.Register(x => x.Resolve<BaseDataProviderManager>().LoadDataProvider()).As<IDataProvider>().InstancePerDependency();
            }
            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                var connectionString = dataProviderSettings.DataConnectionString;
                var mongourl = new MongoUrl(connectionString);
                var databaseName = mongourl.DatabaseName;
                builder.Register(c => new MongoClient(mongourl).GetDatabase(databaseName)).InstancePerLifetimeScope();
            }
            builder.RegisterType<MongoDBContext>().As<IMongoDBContext>().InstancePerLifetimeScope();

            //MongoDbRepository
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();

        }

        private void RegisterCache(ContainerBuilder builder, GrandConfig config)
        {
            builder.RegisterType<MemoryCacheManager>().As<ICacheManager>().SingleInstance();
            if (config.RedisPubSubEnabled)
            {
                var redis = ConnectionMultiplexer.Connect(config.RedisPubSubConnectionString);
                builder.Register(c => redis.GetSubscriber()).As<ISubscriber>().SingleInstance();
                builder.RegisterType<RedisMessageBus>().As<IMessageBus>().SingleInstance();
                builder.RegisterType<RedisMessageCacheManager>().As<ICacheManager>().SingleInstance();
            }
        }

        private void RegisterContextService(ContainerBuilder builder)
        {
            //work context
            builder.RegisterType<WebWorkContext>().As<IWorkContext>().InstancePerLifetimeScope();
            //store context
            builder.RegisterType<WebStoreContext>().As<IStoreContext>().InstancePerLifetimeScope();
        }


        private void RegisterValidators(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            var validators = typeFinder.FindClassesOfType(typeof(IValidator)).ToList();
            foreach (var validator in validators)
            {
                builder.RegisterType(validator);
            }

            //validator consumers
            var validatorconsumers = typeFinder.FindClassesOfType(typeof(IValidatorConsumer<>)).ToList();
            foreach (var consumer in validatorconsumers)
            {
                builder.RegisterType(consumer)
                    .As(consumer.GetTypeInfo().FindInterfaces((type, criteria) =>
                    {
                        var isMatch = type.GetTypeInfo().IsGenericType && ((Type)criteria).IsAssignableFrom(type.GetGenericTypeDefinition());
                        return isMatch;
                    }, typeof(IValidatorConsumer<>)))
                    .InstancePerLifetimeScope();
            }
        }
        
        private void RegisterFramework(ContainerBuilder builder)
        {
            builder.RegisterType<PageHeadBuilder>().As<IPageHeadBuilder>().InstancePerLifetimeScope();

            builder.RegisterType<ThemeProvider>().As<IThemeProvider>().InstancePerLifetimeScope();
            builder.RegisterType<ThemeContext>().As<IThemeContext>().InstancePerLifetimeScope();

            builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().SingleInstance();

            builder.RegisterType<SlugRouteTransformer>().InstancePerLifetimeScope();

            builder.RegisterType<ResourceManager>().As<IResourceManager>().InstancePerLifetimeScope();

            //powered by
            builder.RegisterType<PoweredByMiddlewareOptions>().As<IPoweredByMiddlewareOptions>().SingleInstance();
        }

    }

}
