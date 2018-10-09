using Grand.Core;
using Grand.Core.Domain.Stores;
using Grand.Services.Common;
using Grand.Services.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace Grand.Services.Helpers.Tests
{
    [TestClass()]
    public class DateTimeHelperTests {
        private IWorkContext _workContext;
        private IStoreContext _storeContext;
        private Mock<IGenericAttributeService> tempGenericAttributeService;
        private IGenericAttributeService _genericAttributeService;
        private ISettingService _settingService;
        private DateTimeSettings _dateTimeSettings;
        private IDateTimeHelper _dateTimeHelper;
        private Store _store;

        [TestInitialize()]
        public void TestInitialize() {
            tempGenericAttributeService = new Mock<IGenericAttributeService>();
            {
                _genericAttributeService = tempGenericAttributeService.Object;
            }

            _settingService = new Mock<ISettingService>().Object;
            _workContext = new Mock<IWorkContext>().Object;

            _store = new Store { Id = "1" };
            var tempStoreContext = new Mock<IStoreContext>();
            {
                tempStoreContext.Setup(x => x.CurrentStore).Returns(_store);
                _storeContext = tempStoreContext.Object;
            }

            _dateTimeSettings = new DateTimeSettings {
                AllowCustomersToSetTimeZone = false,
                DefaultStoreTimeZoneId = ""
            };

            _dateTimeHelper = new DateTimeHelper(_workContext, _genericAttributeService,
                _settingService, _dateTimeSettings);
        }

        [TestMethod()]
        public void Can_get_all_systemTimeZones() {
            var systemTimeZones = _dateTimeHelper.GetSystemTimeZones();
            Assert.IsNotNull(systemTimeZones);
            Assert.IsTrue(systemTimeZones.Count > 0);
        }

        [TestMethod()]
        public void Can_convert_dateTime_to_userTime() {
            var sourceDateTime = TimeZoneInfo.GetSystemTimeZones().Where(x => x.BaseUtcOffset.Hours == 2).FirstOrDefault(); //(GMT+02:00);
            Assert.IsNotNull(sourceDateTime);

            var destinationDateTime = TimeZoneInfo.GetSystemTimeZones().Where(x => x.BaseUtcOffset.Hours == 7).FirstOrDefault();//(GMT+07:00);
            Assert.IsNotNull(destinationDateTime);
            var ds = new DateTime(2010, 06, 01, 4, 0, 0);
            var dt = _dateTimeHelper.ConvertToUserTime(new DateTime(2010, 06, 01, 0, 0, 0), sourceDateTime, destinationDateTime);

            Assert.AreEqual(ds,dt);
        }
    }
}