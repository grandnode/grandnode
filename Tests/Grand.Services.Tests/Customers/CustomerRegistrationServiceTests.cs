using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Security;
using Grand.Services.Common;
using Grand.Services.Events;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Stores;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Grand.Services.Customers.Tests
{
    [TestClass()]
    public class CustomerRegistrationServiceTests
    {
        private IRepository<Customer> _customerRepo;
        private IRepository<CustomerHistoryPassword> _customerHistoryRepo;
        private IRepository<CustomerRole> _customerRoleRepo;
        private IRepository<CustomerProductPrice> _customerProductPriceRepo;
        private IRepository<CustomerProduct> _customerProductRepo;
        private IRepository<CustomerNote> _customerNoteRepo;
        private IRepository<CustomerRoleProduct> _customerRoleProductRepo;
        private IRepository<Order> _orderRepo;
        private IRepository<ForumPost> _forumPostRepo;
        private IRepository<ForumTopic> _forumTopicRepo;
        private IGenericAttributeService _genericAttributeService;
        private IEncryptionService _encryptionService;
        private ICustomerService _customerService;
        private ICustomerRegistrationService _customerRegistrationService;
        private ILocalizationService _localizationService;
        private IRewardPointsService _rewardPointsService;
        private CustomerSettings _customerSettings;
        private INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private IEventPublisher _eventPublisher;
        private IStoreService _storeService;
        private RewardPointsSettings _rewardPointsSettings;
        private SecuritySettings _securitySettings;
        private CommonSettings _commonSettings;

        //this method just help to get rid of repetitive code below
        private void AddCustomerToRegisteredRole(Customer customer)
        {
            customer.CustomerRoles.Add(new CustomerRole
            {
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Registered
            });
        }

        [TestInitialize()]
        public void TestInitialize()
        {
            _securitySettings = new SecuritySettings
            {
                EncryptionKey = "273ece6f97dd844d97dd8f4d"
            };
            _rewardPointsSettings = new RewardPointsSettings
            {
                Enabled = false,
            };

            _encryptionService = new EncryptionService(_securitySettings);

            var customer1 = new Customer
            {
                Username = "a@b.com",
                Email = "a@b.com",
                PasswordFormat = PasswordFormat.Hashed,
                Active = true
            };

            string saltKey = _encryptionService.CreateSaltKey(5);
            string password = _encryptionService.CreatePasswordHash("password", saltKey);
            customer1.PasswordSalt = saltKey;
            customer1.Password = password;
            AddCustomerToRegisteredRole(customer1);

            var customer2 = new Customer
            {
                Username = "test@test.com",
                Email = "test@test.com",
                PasswordFormat = PasswordFormat.Clear,
                Password = "password",
                Active = true
            };
            AddCustomerToRegisteredRole(customer2);

            var customer3 = new Customer
            {
                Username = "user@test.com",
                Email = "user@test.com",
                PasswordFormat = PasswordFormat.Encrypted,
                Password = _encryptionService.EncryptText("password"),
                Active = true
            };
            AddCustomerToRegisteredRole(customer3);

            var customer4 = new Customer
            {
                Username = "registered@test.com",
                Email = "registered@test.com",
                PasswordFormat = PasswordFormat.Clear,
                Password = "password",
                Active = true
            };
            AddCustomerToRegisteredRole(customer4);

            var customer5 = new Customer
            {
                Username = "notregistered@test.com",
                Email = "notregistered@test.com",
                PasswordFormat = PasswordFormat.Clear,
                Password = "password",
                Active = true
            };

            //trying to recreate

            var eventPublisher = new Mock<IEventPublisher>();
            eventPublisher.Setup(x => x.Publish(new object()));
            _eventPublisher = eventPublisher.Object;

            _storeService = new Mock<IStoreService>().Object;

            _customerRepo = new Grand.Services.Tests.MongoDBRepositoryTest<Customer>();
            _customerRepo.Insert(customer1);
            _customerRepo.Insert(customer2);
            _customerRepo.Insert(customer3);
            _customerRepo.Insert(customer4);
            _customerRepo.Insert(customer5);

            _customerRoleRepo = new Mock<IRepository<CustomerRole>>().Object;
            _orderRepo = new Mock<IRepository<Order>>().Object;
            _forumPostRepo = new Mock<IRepository<ForumPost>>().Object;
            _forumTopicRepo = new Mock<IRepository<ForumTopic>>().Object;
            _customerProductPriceRepo = new Mock<IRepository<CustomerProductPrice>>().Object;
            _customerProductRepo = new Mock<IRepository<CustomerProduct>>().Object;
            _customerHistoryRepo = new Mock<IRepository<CustomerHistoryPassword>>().Object;
            _customerNoteRepo = new Mock<IRepository<CustomerNote>>().Object;

            _genericAttributeService = new Mock<IGenericAttributeService>().Object;
            _newsLetterSubscriptionService = new Mock<INewsLetterSubscriptionService>().Object;
            _localizationService = new Mock<ILocalizationService>().Object;
            _rewardPointsService = new Mock<IRewardPointsService>().Object;
            _customerRoleProductRepo = new Mock<IRepository<CustomerRoleProduct>>().Object;

            _customerSettings = new CustomerSettings();
            _commonSettings = new CommonSettings();
            _customerService = new CustomerService(new GrandNullCache(), _customerRepo, _customerRoleRepo, _customerProductRepo, _customerProductPriceRepo,
                _customerHistoryRepo, _customerRoleProductRepo, _customerNoteRepo, _orderRepo, _forumPostRepo, _forumTopicRepo, null, null, _genericAttributeService, null,
                _eventPublisher, _customerSettings, _commonSettings);

            _customerRegistrationService = new CustomerRegistrationService(
                _customerService,
                _encryptionService,
                _newsLetterSubscriptionService,
                _localizationService,
                _storeService,
                _eventPublisher,
                _rewardPointsSettings,
                _customerSettings,
                _rewardPointsService);
        }

        [TestMethod()]
        public void Ensure_only_registered_customers_can_login()
        {
            Assert.AreEqual(
                CustomerLoginResults.Successful,
                _customerRegistrationService.ValidateCustomer("registered@test.com", "password"));

            Assert.AreEqual(
                CustomerLoginResults.NotRegistered,
                _customerRegistrationService.ValidateCustomer("notregistered@test.com", "password"));
        }
    }
}