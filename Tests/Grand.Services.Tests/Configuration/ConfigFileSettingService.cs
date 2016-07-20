using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Configuration;
using Grand.Services.Configuration;
using Grand.Services.Events;

namespace Grand.Services.Tests.Configuration {
    public class ConfigFileSettingService : SettingService {
        public ConfigFileSettingService(ICacheManager cacheManager,
            IEventPublisher eventPublisher,
            IRepository<Setting> settingRepository) :
            base(cacheManager, eventPublisher, settingRepository) {

        }
        public override Setting GetSettingById(string settingId) {
            throw new InvalidOperationException("Get setting by id is not supported");
        }

        public override T GetSettingByKey<T>(string key, T defaultValue = default(T),
            string storeId = "", bool loadSharedValueIfNotFound = false) {

            if (String.IsNullOrEmpty(key))
                return defaultValue;

            var settings = GetAllSettings();
            key = key.Trim().ToLowerInvariant();

            var setting = settings.FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase) &&
                x.StoreId == storeId);

            if (setting == null && !String.IsNullOrEmpty(storeId) && loadSharedValueIfNotFound) {
                setting = settings.FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase) &&
                    x.StoreId == "");
            }

            if (setting != null)
                return CommonHelper.To<T>(setting.Value);

            return defaultValue;
        }

        public override void DeleteSetting(Setting setting) {
            throw new InvalidOperationException("Deleting settings is not supported");
        }

        public override void SetSetting<T>(string key, T value, string storeId = "", bool clearCache = true) {
            throw new NotImplementedException();
        }

        public override IList<Setting> GetAllSettings() {
            var settings = new List<Setting>();
            var appSettings = ConfigurationManager.AppSettings;
            foreach (var setting in appSettings.AllKeys) {
                settings.Add(new Setting {
                    Name = setting.ToLowerInvariant(),
                    Value = appSettings[setting]      ,
                    StoreId = "",
                });
            }

            return settings;
        }

        public override void ClearCache() {
        }
    }
}
