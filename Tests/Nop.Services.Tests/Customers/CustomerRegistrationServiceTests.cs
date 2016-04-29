using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using MongoDB.Driver;
using Rhino.Mocks;
using Nop.Data;

namespace Nop.Services.Customers.Tests {
    [TestClass()]
    public class CustomerRegistrationServiceTests {
        private IRepository<Customer> _customerRepo;
        private IRepository<CustomerRole> _customerRoleRepo;
        private IRepository<CustomerRoleProduct> _customerRoleProductRepo; 
        private IRepository<Order> _orderRepo;
        private IRepository<ForumPost> _forumPostRepo;
        private IRepository<ForumTopic> _forumTopicRepo;
        private IGenericAttributeService _genericAttributeService;
        private IEncryptionService _encryptionService;
        private ICustomerService _customerService;
        private ICustomerRegistrationService _customerRegistrationService;
        private ILocalizationService _localizationService;
        private CustomerSettings _customerSettings;
        private INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private IEventPublisher _eventPublisher;
        private IStoreService _storeService;
        private RewardPointsSettings _rewardPointsSettings;
        private SecuritySettings _securitySettings;

        //this method just help to get rid of repetitive code below
        private void AddCustomerToRegisteredRole(Customer customer) {
            customer.CustomerRoles.Add(new CustomerRole {
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Registered
            });
        }

        [TestInitialize()]
        public void TestInitialize() {             
            _securitySettings = new SecuritySettings {
                EncryptionKey = "273ece6f97dd844d"
            };
            _rewardPointsSettings = new RewardPointsSettings {
                Enabled = false,
            };

            _encryptionService = new EncryptionService(_securitySettings);

            var customer1 = new Customer {
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

            var customer2 = new Customer {
                Username = "test@test.com",
                Email = "test@test.com",
                PasswordFormat = PasswordFormat.Clear,
                Password = "password",
                Active = true
            };
            AddCustomerToRegisteredRole(customer2);

            var customer3 = new Customer {
                Username = "user@test.com",
                Email = "user@test.com",
                PasswordFormat = PasswordFormat.Encrypted,
                Password = _encryptionService.EncryptText("password"),
                Active = true
            };
            AddCustomerToRegisteredRole(customer3);

            var customer4 = new Customer {
                Username = "registered@test.com",
                Email = "registered@test.com",
                PasswordFormat = PasswordFormat.Clear,
                Password = "password",
                Active = true
            };
            AddCustomerToRegisteredRole(customer4);

            var customer5 = new Customer {
                Username = "notregistered@test.com",
                Email = "notregistered@test.com",
                PasswordFormat = PasswordFormat.Clear,
                Password = "password",
                Active = true
            };

            _eventPublisher = MockRepository.GenerateMock<IEventPublisher>();
            _eventPublisher.Expect(x => x.Publish(Arg<object>.Is.Anything));
            _storeService = MockRepository.GenerateMock<IStoreService>();
            
            _customerRepo = new Nop.Services.Tests.MongoDBRepositoryTest<Customer>();
            _customerRepo.Insert(customer1);
            _customerRepo.Insert(customer2);
            _customerRepo.Insert(customer3);
            _customerRepo.Insert(customer4);
            _customerRepo.Insert(customer5);

            _customerRoleRepo = MockRepository.GenerateMock<IRepository<CustomerRole>>();
            _orderRepo = MockRepository.GenerateMock<IRepository<Order>>();
            _forumPostRepo = MockRepository.GenerateMock<IRepository<ForumPost>>();
            _forumTopicRepo = MockRepository.GenerateMock<IRepository<ForumTopic>>();

            _genericAttributeService = MockRepository.GenerateMock<IGenericAttributeService>();
            _newsLetterSubscriptionService = MockRepository.GenerateMock<INewsLetterSubscriptionService>();
            _localizationService = MockRepository.GenerateMock<ILocalizationService>();
            _customerRoleProductRepo = MockRepository.GenerateMock<IRepository<CustomerRoleProduct>>();
            _customerSettings = new CustomerSettings();

            _customerService = new CustomerService(new NopNullCache(), _customerRepo, _customerRoleRepo,
                _customerRoleProductRepo, _orderRepo, _forumPostRepo, _forumTopicRepo,
                null, null, _genericAttributeService, null, 
                _eventPublisher, _customerSettings, null);

            _customerRegistrationService = new CustomerRegistrationService(
                _customerService,
                _encryptionService,
                _newsLetterSubscriptionService,
                _localizationService,
                _storeService,
                _rewardPointsSettings,
                _customerSettings,
                null);
        }

        [TestMethod()]
        public void Ensure_only_registered_customers_can_login() {
            Assert.AreEqual(
                CustomerLoginResults.Successful, 
                _customerRegistrationService.ValidateCustomer("registered@test.com", "password"));

            Assert.AreEqual(
                CustomerLoginResults.NotRegistered,
                _customerRegistrationService.ValidateCustomer("notregistered@test.com", "password"));
        }
    }
}