using Grand.Domain.Catalog;
using Grand.Domain.Data;
using Grand.Services.Commands.Handlers.Catalog;
using Grand.Services.Commands.Models.Catalog;
using Grand.Services.Customers;
using Grand.Services.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Tests.Commands.Handlers.Catalog
{
    [TestClass()]
    public class SendNotificationsToSubscribersCommandHandlerTest
    {
        private Mock<ICustomerService> _mockCustomerService;
        private IRepository<BackInStockSubscription> _backInStockSubscriptionRepository;
        private List<BackInStockSubscription> _expected;
        private Mock<IWorkflowMessageService> _mockWorkflowMessageService;
        private SendNotificationsToSubscribersCommandHandler _handler;
        private SendNotificationsToSubscribersCommand _sendNotificationsToSubscribersCommand;
        
        [TestInitialize()]
        public void Init()
        {
            _expected = new List<BackInStockSubscription>
            {
                new BackInStockSubscription { WarehouseId = "11", ProductId = "11" },
                new BackInStockSubscription { WarehouseId = "11", ProductId = "11" }
            };
            
            _backInStockSubscriptionRepository = new MongoDBRepositoryTest<BackInStockSubscription>();
            _backInStockSubscriptionRepository.Insert(_expected);
            
            _mockWorkflowMessageService = new Mock<IWorkflowMessageService>();
            _sendNotificationsToSubscribersCommand = new SendNotificationsToSubscribersCommand { Product = new Product { Id = "11"}, Warehouse = "11" };
            _mockCustomerService = new Mock<ICustomerService>();
            _handler = new SendNotificationsToSubscribersCommandHandler(_mockCustomerService.Object, _mockWorkflowMessageService.Object, _backInStockSubscriptionRepository);
        }

        [TestMethod()]
        public async Task GetAllSubscriptionsWithCorrectParams()
        {
            var result = await _handler.Handle(_sendNotificationsToSubscribersCommand, default);
            var resultList = result.ToList();
            Assert.AreEqual(resultList.Count, _expected.Count);
            AssertSubscriptions(resultList, _expected);
        }

        private static void AssertListEquals<TE, TA>(Action<TE, TA> asserter, IEnumerable<TE> expected, IEnumerable<TA> actual)
        {
            IList<TA> actualList = actual.ToList();
            IList<TE> expectedList = expected.ToList();

            for (var i = 0; i < expectedList.Count; i++)
            {
                try
                {
                    asserter.Invoke(expectedList[i], actualList[i]);
                }
                catch (Exception e)
                {
                    Assert.IsTrue(false, $"Assertion failed because: {e.Message}");
                }
            }
        }

        private void AssertSubscriptions(IEnumerable<BackInStockSubscription> expected, IEnumerable<BackInStockSubscription> actual)
        {
            AssertListEquals(
                (e, a) => AssertSubscription(e, a),
                expected,
                actual);
        }

        private void AssertSubscription(BackInStockSubscription expected, BackInStockSubscription actual)
        {
            Assert.AreEqual(expected.ProductId, actual.ProductId);
            Assert.AreEqual(expected.WarehouseId, actual.WarehouseId);
        }
    }
}