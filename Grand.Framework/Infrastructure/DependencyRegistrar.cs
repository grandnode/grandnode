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
        /// <param name="ServiceCollection">Service Collection</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(IServiceCollection serviceCollection, ITypeFinder typeFinder, GrandConfig config)
        {
            RegisterDataLayer(serviceCollection);

            RegisterCache(serviceCollection, config);

            RegisterCore(serviceCollection);

            RegisterContextService(serviceCollection);

            RegisterValidators(serviceCollection, typeFinder);

            RegisterFramework(serviceCollection);
        }

        /// <summary>
        /// Gets order of this dependency registrar implementation
        /// </summary>
        public int Order {
            get { return 0; }
        }

        private void RegisterCore(IServiceCollection serviceCollection)
        {
            //web helper
            serviceCollection.AddScoped<IWebHelper, WebHelper>();
            //plugins
            serviceCollection.AddScoped<IPluginFinder, PluginFinder>();
        }

        private void RegisterDataLayer(IServiceCollection serviceCollection)
        {
            var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = dataSettingsManager.LoadSettings();
            if (string.IsNullOrEmpty(dataProviderSettings.DataConnectionString))
            {
                serviceCollection.AddTransient(c => dataSettingsManager.LoadSettings());
                serviceCollection.AddTransient<BaseDataProviderManager>(c=> new MongoDBDataProviderManager(c.GetRequiredService<DataSettings>()));
                serviceCollection.AddTransient<IDataProvider>(x => x.GetRequiredService<BaseDataProviderManager>().LoadDataProvider());
            }
            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                var connectionString = dataProviderSettings.DataConnectionString;
                var mongourl = new MongoUrl(connectionString);
                var databaseName = mongourl.DatabaseName;
                serviceCollection.AddScoped(c => new MongoClient(mongourl).GetDatabase(databaseName));
        
            }
            
            serviceCollection.AddScoped<IMongoDBContext, MongoDBContext>();
            //MongoDbRepository
            
            serviceCollection.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        }

        private void RegisterCache(IServiceCollection serviceCollection, GrandConfig config)
        {
            if (config.UseLazyCache)
            {
                serviceCollection.AddLazyCache();
                serviceCollection.AddSingleton<ICacheManager, LazyCacheManager>();
            }
            else
                serviceCollection.AddSingleton<ICacheManager, MemoryCacheManager>();

            if (config.RedisPubSubEnabled)
            {
                var redis = ConnectionMultiplexer.Connect(config.RedisPubSubConnectionString);
                serviceCollection.AddSingleton<ISubscriber>(c => redis.GetSubscriber());
                serviceCollection.AddSingleton<IMessageBus, RedisMessageBus>();
                serviceCollection.AddSingleton<ICacheManager,RedisMessageCacheManager>();
            }
        }

        private void RegisterContextService(IServiceCollection serviceCollection)
        {
            //work context
            serviceCollection.AddScoped<IWorkContext, WebWorkContext>();
            //store context
            serviceCollection.AddScoped<IStoreContext, WebStoreContext>();
        }


        private void RegisterValidators(IServiceCollection serviceCollection, ITypeFinder typeFinder)
        {
            var validators = typeFinder.FindClassesOfType(typeof(IValidator)).ToList();
            foreach (var validator in validators)
            {
                serviceCollection.AddTransient(validator);
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
                types.Select(c => serviceCollection.AddScoped(c, consumer));
            }
        }
        
        private void RegisterFramework(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IPageHeadBuilder, PageHeadBuilder>();

            serviceCollection.AddScoped<IThemeProvider, ThemeProvider>();
            serviceCollection.AddScoped<IThemeContext, ThemeContext>();

            serviceCollection.AddSingleton<IRoutePublisher, RoutePublisher>();

            serviceCollection.AddScoped<SlugRouteTransformer>();

            serviceCollection.AddScoped<IResourceManager,ResourceManager>();

            //powered by
            serviceCollection.AddSingleton<IPoweredByMiddlewareOptions,PoweredByMiddlewareOptions>();
        }

    }

}
