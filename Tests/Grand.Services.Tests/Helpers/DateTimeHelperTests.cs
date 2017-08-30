using Grand.Core;
using Grand.Core.Domain.Stores;
using Grand.Services.Common;
using Grand.Services.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

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
        public void Can_find_systemTimeZone_by_id() {
            var timeZones = _dateTimeHelper.FindTimeZoneById("E. Europe Standard Time");
            Assert.IsNotNull(timeZones);
            Assert.AreEqual("E. Europe Standard Time", timeZones.Id);
        }

        [TestMethod()]
        public void Can_get_all_systemTimeZones() {
            var systemTimeZones = _dateTimeHelper.GetSystemTimeZones();
            Assert.IsNotNull(systemTimeZones);
            Assert.IsTrue(systemTimeZones.Count > 0);
        }

        [TestMethod()]
        public void Can_convert_dateTime_to_userTime() {
            var sourceDateTime = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time"); //(GMT+02:00) Minsk;
            Assert.IsNotNull(sourceDateTime);

            var destinationDateTime = TimeZoneInfo.FindSystemTimeZoneById("North Asia Standard Time"); //(GMT+07:00) Krasnoyarsk;
            Assert.IsNotNull(destinationDateTime);

            //summer time
            Assert.AreEqual(new DateTime(2010, 06, 01, 5, 0, 0), 
                _dateTimeHelper.ConvertToUserTime(new DateTime(2010, 06, 01, 0, 0, 0), sourceDateTime, destinationDateTime));

            //winter time
            Assert.AreEqual(new DateTime(2010, 01, 01, 5, 0, 0), 
                _dateTimeHelper.ConvertToUserTime(new DateTime(2010, 01, 01, 0, 0, 0), sourceDateTime, destinationDateTime));
        }

        [TestMethod()]
        public void Can_convert_dateTime_to_utc_dateTime() {

            var sourceDateTime = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time"); //(GMT+02:00) Minsk;
            Assert.IsNotNull(sourceDateTime);

            //summer time
            var dateTime1 = new DateTime(2010, 06, 01, 0, 0, 0);
            var convertedDateTime1 = _dateTimeHelper.ConvertToUtcTime(dateTime1, sourceDateTime);
            Assert.AreEqual(new DateTime(2010, 05, 31, 21, 0, 0), convertedDateTime1); //31th May 2010, 21:00

            
            //winter time
            var dateTime2 = new DateTime(2010, 01, 01, 0, 0, 0);
            var convertedDateTime2 = _dateTimeHelper.ConvertToUtcTime(dateTime2, sourceDateTime);
            Assert.AreEqual(new DateTime(2009, 12, 31, 22, 0, 0), convertedDateTime2); //31th December 2009, 22:00

        }
    }
}