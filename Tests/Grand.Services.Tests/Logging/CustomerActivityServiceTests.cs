﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grand.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Logging;
using Moq;
using MongoDB.Driver;
using Grand.Services.Tests;

namespace Grand.Services.Logging.Tests {
    [TestClass()]
    public class CustomerActivityServiceTests {
        private ICacheManager _cacheManager;
        private IRepository<ActivityLog> _activityLogRepository;
        private IRepository<ActivityLogType> _activityLogTypeRepository;
        private IWorkContext _workContext;
        private ICustomerActivityService _customerActivityService;
        private ActivityLogType _activityType1, _activityType2;
        private ActivityLog _activity1, _activity2;
        private Customer _customer1, _customer2;

        [TestInitialize()]
        public void TestInitialize() {

            _activityType1 = new ActivityLogType {
                Id = "1",
                SystemKeyword = "TestKeyword1",
                Enabled = true,
                Name = "Test name1"
            };
            _activityType2 = new ActivityLogType {
                Id = "2",
                SystemKeyword = "TestKeyword2",
                Enabled = true,
                Name = "Test name2"
            };

            _customer1 = new Customer {
                Id = "1",
                Email = "test1@teststore1.com",
                Username = "TestUser1",
                Deleted = false,
            };
            _customer2 = new Customer {
                Id = "2",
                Email = "test2@teststore2.com",
                Username = "TestUser2",
                Deleted = false,
            };

            _activity1 = new ActivityLog {
                Id = "1",
                ActivityLogType = _activityType1,
                CustomerId = _customer1.Id,
            };
            _activity2 = new ActivityLog {
                Id = "2",
                ActivityLogType = _activityType1,
                CustomerId = _customer2.Id,
            };

            _cacheManager = new NopNullCache();
            _workContext = new Mock<IWorkContext>().Object;

            _activityLogRepository = new MongoDBRepositoryTest<ActivityLog>();
            _activityLogRepository.Insert(_activity1);
            _activityLogRepository.Insert(_activity2);

            _activityLogTypeRepository = new MongoDBRepositoryTest<ActivityLogType>();
            _activityLogTypeRepository.Insert(_activityType1);
            _activityLogTypeRepository.Insert(_activityType2);

            _customerActivityService = new CustomerActivityService(
                _cacheManager, _activityLogRepository,
                _activityLogTypeRepository, _workContext, 
                null, null, null);
        }

        [TestMethod()]
        public void InsertActivityTypeTest() {
            var activities = _customerActivityService.GetAllActivities();
            Assert.IsTrue(activities.Contains(_activity1));

            activities = _customerActivityService.GetAllActivities();
            Assert.IsTrue(activities.Contains(_activity1));

            var tempActivity3 = new ActivityLog();
            activities = _customerActivityService.GetAllActivities();
            Assert.IsFalse(activities.Contains(tempActivity3));
        }
    }
}