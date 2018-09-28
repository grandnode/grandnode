using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Configuration;
using Grand.Core.Infrastructure;
using Grand.Framework.Infrastructure.Extensions;
using Grand.Services.Configuration;
using Grand.Services.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Grand.Services.Tests.Configuration
{
    public class ConfigFileSettingService : SettingService
    {
        public ConfigFileSettingService(ICacheManager cacheManager,
            IEventPublisher eventPublisher,
            IRepository<Setting> settingRepository) :
            base(cacheManager, eventPublisher, settingRepository)
        {

        }
        public override Setting GetSettingById(string settingId)
        {
            throw new InvalidOperationException("Get setting by id is not supported");
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

        public override void DeleteSetting(Setting setting)
        {
            throw new InvalidOperationException("Deleting settings is not supported");
        }

        public override void SetSetting<T>(string key, T value, string storeId = "", bool clearCache = true)
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

        public override void ClearCache()
        {
        }
    }
}
