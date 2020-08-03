using Grand.Core.Caching;
using Grand.Core.Tests.Caching;
using Grand.Services.Configuration;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Grand.Services.Tests.Configuration
{
    [TestClass()]
    public class ConfigFileSettingServiceTests /*: ServiceTest*/ {
        // requires following settings to exist in app.config
        // Setting1 : "SomeValue" : string
        // Setting2 : 25 : int
        // Setting3 : 25/12/2010 : Date

        ISettingService config;

        [TestInitialize()]
        public void TestInitialize()
        {
            var eventPublisher = new Mock<IMediator>();
            var cacheManager = new MemoryCacheManager(new Mock<IMemoryCache>().Object, eventPublisher.Object);
            config = new ConfigFileSettingService(cacheManager, null, null);
        }

        [TestMethod()]
        public void Can_get_all_settings()
       {
            var settings = config.GetAllSettings();
            Assert.IsNotNull(settings);
            Assert.IsTrue((settings.Count > 0));
        }

        [TestMethod()]
        public void Can_get_setting_by_key()
        {
            var setting = config.GetSettingByKey<string>("Setting1");
            Assert.AreEqual("SomeValue", setting);
        }

        //TO DO 
        //[TestMethod()]
        //public void Can_get_typed_setting_value_by_key()
        //{
        //    var setting = config.GetSettingByKey<DateTime>("Setting3");
        //    Assert.AreEqual(new DateTime(2010, 12, 25), setting);
        //}

        [TestMethod()]
        public void Default_value_returned_if_setting_does_not_exist()
        {
            var setting = config.GetSettingByKey("NonExistentKey", 100, loadSharedValueIfNotFound: true);
            Assert.AreEqual(100, setting);
        }
    }
}
