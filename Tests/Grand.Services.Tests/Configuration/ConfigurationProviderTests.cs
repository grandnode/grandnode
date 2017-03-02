using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grand.Core.Configuration;
using Grand.Services.Configuration;
using Grand.Core.Caching;

namespace Grand.Services.Tests.Configuration {
    [TestClass()]
    public class ConfigurationProviderTests : ServiceTest {
        ISettingService _settingService;
        private ICacheManager _cacheManager;

        [TestInitialize()]
        public void SetUp() {
            _cacheManager = new NopNullCache();
            _settingService = new ConfigFileSettingService(_cacheManager, null, null);
        }

        [TestMethod()]
        public void Can_get_settings() {

            var settings = _settingService.LoadSetting<ArbitaryClassName>();
            Assert.AreEqual("Ruby", settings.ServerName);
            Assert.AreEqual("192.168.0.1", settings.Ip);
            Assert.AreEqual(21, settings.PortNumber);
            Assert.AreEqual("admin", settings.Username);
            Assert.AreEqual("password", settings.Password);
        }
    }

    public class ArbitaryClassName : ISettings {
        public string ServerName { get; set; }
        public string Ip { get; set; }
        public int PortNumber { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
