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
using Microsoft.Extensions.DependencyInjection;
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
        public virtual void Register(IServiceCollection builder, ITypeFinder typeFinder, GrandConfig config)
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

        private void RegisterCore(IServiceCollection builder)
        {
            //web helper
            builder.AddScoped<IWebHelper, WebHelper>();
            //plugins
            builder.AddScoped<IPluginFinder, PluginFinder>();
        }

        private void RegisterDataLayer(IServiceCollection builder)
        {
            var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = dataSettingsManager.LoadSettings();
            if (string.IsNullOrEmpty(dataProviderSettings.DataConnectionString))
            {
                builder.AddTransient(c => dataSettingsManager.LoadSettings());
                builder.AddTransient<BaseDataProviderManager>(c=> new MongoDBDataProviderManager(c.GetRequiredService<DataSettings>()));
                builder.AddTransient<IDataProvider>(x => x.GetRequiredService<BaseDataProviderManager>().LoadDataProvider());
            }
            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                var connectionString = dataProviderSettings.DataConnectionString;
                var mongourl = new MongoUrl(connectionString);
                var databaseName = mongourl.DatabaseName;
                builder.AddScoped(c => new MongoClient(mongourl).GetDatabase(databaseName));
        
            }
            
            builder.AddScoped<IMongoDBContext, MongoDBContext>();
            //MongoDbRepository
            
            builder.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        }

        private void RegisterCache(IServiceCollection builder, GrandConfig config)
        {
            builder.AddSingleton<ICacheManager,MemoryCacheManager>();
            if (config.RedisPubSubEnabled)
            {
                var redis = ConnectionMultiplexer.Connect(config.RedisPubSubConnectionString);
                builder.AddSingleton<ISubscriber>(c => redis.GetSubscriber());
                builder.AddSingleton<IMessageBus, RedisMessageBus>();
                builder.AddSingleton<ICacheManager,RedisMessageCacheManager>();
            }
        }

        private void RegisterContextService(IServiceCollection builder)
        {
            //work context
            builder.AddScoped<IWorkContext, WebWorkContext>();
            //store context
            builder.AddScoped<IStoreContext, WebStoreContext>();
        }


        private void RegisterValidators(IServiceCollection builder, ITypeFinder typeFinder)
        {
            var validators = typeFinder.FindClassesOfType(typeof(IValidator)).ToList();
            foreach (var validator in validators)
            {
                builder.AddTransient(validator);
            }

            //validator consumers
            var validatorconsumers = typeFinder.FindClassesOfType(typeof(IValidatorConsumer<>)).ToList();
            foreach (var consumer in validatorconsumers)
            {
                var types = consumer.GetTypeInfo().FindInterfaces((type, criteria) =>
                 {
                     var isMatch = type.GetTypeInfo().IsGenericType && ((Type)criteria).IsAssignableFrom(type.GetGenericTypeDefinition());
                     return isMatch;
                 }, typeof(IValidatorConsumer<>));
                types.Select(c => builder.AddScoped(c, consumer));
                /*
                builder.RegisterType(consumer)
                    .As(consumer.GetTypeInfo().FindInterfaces((type, criteria) =>
                    {
                        var isMatch = type.GetTypeInfo().IsGenericType && ((Type)criteria).IsAssignableFrom(type.GetGenericTypeDefinition());
                        return isMatch;
                    }, typeof(IValidatorConsumer<>)))
                    .InstancePerLifetimeScope();
                */
            }
        }
        
        private void RegisterFramework(IServiceCollection builder)
        {
            builder.AddScoped<IPageHeadBuilder, PageHeadBuilder>();

            builder.AddScoped<IThemeProvider, ThemeProvider>();
            builder.AddScoped<IThemeContext, ThemeContext>();

            builder.AddSingleton<IRoutePublisher, RoutePublisher>();

            builder.AddScoped<SlugRouteTransformer>();

            builder.AddScoped<IResourceManager,ResourceManager>();

            //powered by
            builder.AddSingleton<IPoweredByMiddlewareOptions,PoweredByMiddlewareOptions>();
        }

    }

}
