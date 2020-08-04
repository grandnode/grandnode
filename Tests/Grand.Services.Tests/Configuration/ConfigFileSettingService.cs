using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Configuration;
using Grand.Core.Infrastructure;
using Grand.Framework.Infrastructure.Extensions;
using Grand.Services.Configuration;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Grand.Services.Tests.Configuration
{
    public class ConfigFileSettingService : SettingService
    {
        private readonly ICacheManager _cacheManager;
        public ConfigFileSettingService(ICacheManager cacheManager,
            IMediator eventPublisher,
            IRepository<Setting> settingRepository) :
            base(cacheManager, eventPublisher, settingRepository)
        {
            _cacheManager = cacheManager;
        }
        
        public override T GetSettingByKey<T>(string key, T defaultValue = default(T),
            string storeId = "", bool loadSharedValueIfNotFound = false)
        {

            if (String.IsNullOrEmpty(key))
                return defaultValue;

            var settings = GetAllSettings();
            key = key.Trim().ToLowerInvariant();

            var setting = settings.FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase) &&
                x.StoreId == storeId);

            if (setting == null && !String.IsNullOrEmpty(storeId) && loadSharedValueIfNotFound)
            {
                setting = settings.FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase) &&
                    x.StoreId == "");
            }

            if (setting != null)
                return CommonHelper.To<T>(setting.Value);

            return defaultValue;
        }

        public override Task DeleteSetting(Setting setting)
        {
            throw new InvalidOperationException("Deleting settings is not supported");
        }

        public override Task SetSetting<T>(string key, T value, string storeId = "", bool clearCache = true)
        {
            throw new NotImplementedException();
        }

        public override IList<Setting> GetAllSettings()
        {
            string directory = new WebAppTypeFinder().GetBinDirectory();
            var configurationBasePath = "";
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                configurationBasePath = directory.Substring(0, directory.IndexOf("\\Tests\\Grand.Services.Tests\\") + 27);
            else
                configurationBasePath = directory.Substring(0, directory.IndexOf("/Tests/Grand.Services.Tests/") + 27);

            var configuration = new ConfigurationBuilder()
           .SetBasePath(configurationBasePath)
           .AddJsonFile("appsettingstest.json", optional: false, reloadOnChange: true)
           .Build();

            var settings = new List<Setting>();
            var settingObject = new ServiceCollection().ConfigureStartupConfig<ApplicationSettings>(configuration.GetSection("ApplicationSettingsSection"));
            var properties = settingObject.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = settingObject.GetType().GetProperty(property.Name).GetValue(settingObject, null);
                settings.Add(new Setting
                {
                    Name = property.Name.ToLowerInvariant(),
                    Value = value.ToString(),
                    StoreId = ""
                });
            }

            return settings;
        }

        public override Task ClearCache()
        {
            return Task.CompletedTask;
        }
    }
}
