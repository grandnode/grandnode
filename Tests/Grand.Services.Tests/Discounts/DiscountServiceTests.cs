using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Discounts;
using Grand.Core.Plugins;
using Grand.Services.Common;
using Grand.Services.Events;
using Grand.Services.Localization;
using Moq;
using MongoDB.Driver;
using Grand.Core.Domain.Catalog;
using System.Text;
using MongoDB.Bson.Serialization.Conventions;
using Grand.Services.Tests;
using Grand.Data;
using Grand.Core.Domain.Vendors;
using Grand.Core.Domain.Stores;

namespace Grand.Services.Discounts.Tests {
    [TestClass()] 
    public class DiscountServiceTests {
        private IRepository<Discount> _discountRepo;
        private IRepository<DiscountUsageHistory> _discountUsageHistoryRepo;
        private IEventPublisher _eventPublisher;
        private IGenericAttributeService _genericAttributeService;
        private ILocalizationService _localizationService;
        private IDiscountService _discountService;
        private IStoreContext _storeContext;
        //private IRepository<Vendor> _vendorRepo;
        //private IRepository<Store> _storeRepo;

        [TestInitialize()]
        public void TestInitialize() {
            //plugin initialization
            new Grand.Services.Tests.ServiceTest().PluginInitializator();

            var discount1 = new Discount {                
                DiscountType = DiscountType.AssignedToCategories,
                Name = "Discount 1",
                UsePercentage = true,       //!     (everything that I marked as "!" is variable that changes test output)
                DiscountPercentage = 10,    //!
                DiscountAmount = 0,
                DiscountLimitation = DiscountLimitationType.Unlimited,
                LimitationTimes = 0,
            };
            var discount2 = new Discount {
                DiscountType = DiscountType.AssignedToSkus,
                Name = "Discount 2",
                UsePercentage = false,      //!
                DiscountPercentage = 0,     //!
                DiscountAmount = 5,         //use amount instad of percentage (e.g. discount 100PLN instead of discount 10%) 
                RequiresCouponCode = true,
                CouponCode = "SecretCode",
                DiscountLimitation = DiscountLimitationType.NTimesPerCustomer, //only N times for this customer
                LimitationTimes = 3, // 3 times for this customer
            };
            _discountRepo = new MongoDBRepositoryTest<Discount>();
            _discountRepo.Insert(discount1);
            _discountRepo.Insert(discount2);


            var tempEventPublisher = new Mock<IEventPublisher>();
            {
                tempEventPublisher.Setup(x => x.Publish(It.IsAny<object>()));
                _eventPublisher = tempEventPublisher.Object;
            }
            _storeContext = new Mock<IStoreContext>().Object;

            _discountUsageHistoryRepo = new Mock<IRepository<DiscountUsageHistory>>().Object;
            var extraProductRepo = new Mock<IRepository<Product>>().Object;
            var extraCategoryRepo = new Mock<IRepository<Category>>().Object;
            var extraManufacturerRepo = new Mock<IRepository<Manufacturer>>().Object;
            var extraStoreRepo = new Mock<IRepository<Store>>().Object;
            var extraVendorRepo = new Mock<IRepository<Vendor>>().Object;

            _genericAttributeService = new Mock<IGenericAttributeService>().Object;
            _localizationService = new Mock<ILocalizationService>().Object;

            _discountService = new DiscountService(new NopNullCache(), _discountRepo,
                _discountUsageHistoryRepo, _localizationService, _storeContext, _genericAttributeService,
                new PluginFinder(), _eventPublisher, extraProductRepo, extraCategoryRepo, extraManufacturerRepo, extraVendorRepo, extraStoreRepo);
        }

        [TestMethod()]
        public void Can_get_all_discount() {
            var discounts = _discountService.GetAllDiscounts(null);
            Assert.IsNotNull(discounts);
            Assert.AreEqual(2, discounts.Count);
        }

        [TestMethod()]
        public void Can_load_discountRequirementRules() {
           
            var rules = _discountService.LoadAllDiscountRequirementRules();
            Assert.IsNotNull(rules);
            Assert.IsTrue(rules.Count > 0);
        }

        [TestMethod()]
        public void Can_load_discountRequirementRuleBySystemKeyword() {

            var rule = _discountService.LoadDiscountRequirementRuleBySystemName("TestDiscountRequirementRule");
            Assert.IsNotNull(rule);
        }

        [TestMethod()]
        public void Can_validate_discount_dateRange() {
           
            var discount = new Discount {
                DiscountType = DiscountType.AssignedToSkus,
                Name = "Discount 3",
                UsePercentage = false,
                DiscountPercentage = 0,
                DiscountAmount = 5,
                StartDateUtc = DateTime.UtcNow.AddDays(-1),
                EndDateUtc = DateTime.UtcNow.AddDays(1),
                RequiresCouponCode = false,
                DiscountLimitation = DiscountLimitationType.Unlimited,
            };

            var customer = new Customer {
                CustomerGuid = Guid.NewGuid(),
                AdminComment = "",
                Active = true,
                Deleted = false,
                CreatedOnUtc = new DateTime(2010, 01, 01),
                LastActivityDateUtc = new DateTime(2010, 01, 02)
            };

            Assert.IsTrue(_discountService.ValidateDiscount(discount, customer).IsValid);
            discount.StartDateUtc = DateTime.UtcNow.AddDays(1);
            Assert.IsFalse(_discountService.ValidateDiscount(discount, customer).IsValid);
        }
    }

}