using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Customers;
using Grand.Core.Tests.Caching;
using Grand.Services.Events;
using Grand.Services.Tests;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Customers.Tests
{
    [TestClass()]
    public class CustomerActionServiceTests
    {
        private IRepository<CustomerAction> _customerActionRepository;
        private IRepository<CustomerActionType> _customerActionTypeRepository;
        private IRepository<CustomerActionHistory> _customerActionHistoryRepository;
        private IMediator _eventPublisher;
        private ICustomerActionService _customerActionService;
        private string _Id_CustomerAction;
        private string _Id_CustomerActionType;

        [TestInitialize()]
        public void TestInitialize()
        {
            var eventPublisher = new Mock<IMediator>();
            _eventPublisher = eventPublisher.Object;

            _customerActionRepository = new MongoDBRepositoryTest<CustomerAction>();
            _customerActionTypeRepository = new MongoDBRepositoryTest<CustomerActionType>();
            _customerActionHistoryRepository = new MongoDBRepositoryTest<CustomerActionHistory>();
            
            _Id_CustomerActionType = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            var customerActionType = new List<CustomerActionType>()
            {
                new CustomerActionType()
                {
                    Id = _Id_CustomerActionType,
                    Name = "Add to cart",
                    SystemKeyword = "AddToCart",
                    Enabled = true,
                    ConditionType = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
                },
                new CustomerActionType()
                {
                    Name = "Add order",
                    SystemKeyword = "AddOrder",
                    Enabled = true,
                    ConditionType = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
                },
                new CustomerActionType()
                {
                    Name = "Viewed",
                    SystemKeyword = "Viewed",
                    Enabled = false,
                    ConditionType = {1, 2, 3, 7, 8, 9, 10}
                },
                new CustomerActionType()
                {
                    Name = "Url",
                    SystemKeyword = "Url",
                    Enabled = false,
                    ConditionType = {7, 8, 9, 10, 11, 12}
                },
                new CustomerActionType()
                {
                    Name = "Customer Registration",
                    SystemKeyword = "Registration",
                    Enabled = false,
                    ConditionType = {7, 8, 9, 10}
                }
            };
            _customerActionTypeRepository.Insert(customerActionType);
            _Id_CustomerAction = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            var customerActions = new List<CustomerAction>()
            {
                new CustomerAction()
                {
                    Id = _Id_CustomerAction,
                    Active = true,
                    StartDateTimeUtc = DateTime.UtcNow,
                    EndDateTimeUtc = DateTime.UtcNow.AddMonths(1),
                    Name = "Test action",
                    ReactionTypeId = (int)CustomerReactionTypeEnum.AssignToCustomerTag,
                    Condition = CustomerActionConditionEnum.OneOfThem,
                    ActionTypeId = _Id_CustomerActionType
                },
            };
            _customerActionRepository.Insert(customerActions);
            _customerActionService = new CustomerActionService(_customerActionRepository, _customerActionTypeRepository,
            _customerActionHistoryRepository, _eventPublisher, new TestMemoryCacheManager(new Mock<IMemoryCache>().Object, _eventPublisher));

        }

        [TestMethod()]
        public void GetCustomerActionByIdTest()
        {
            var action = _customerActionService.GetCustomerActionById(_Id_CustomerAction);
            Assert.IsNotNull(action);
        }

        [TestMethod()]
        public async Task GetCustomerActionsTest()
        {
            var actions = await _customerActionService.GetCustomerActions();
            Assert.IsNotNull(actions);
            Assert.IsTrue(actions.Count > 0);
        }
        [TestMethod()]
        public async Task InsertCustomerActionTest()
        {
            var customerAction = new CustomerAction() {
                Active = true,
                StartDateTimeUtc = DateTime.UtcNow,
                EndDateTimeUtc = DateTime.UtcNow.AddMonths(1),
                Name = "Test action",
                ReactionTypeId = (int)CustomerReactionTypeEnum.AssignToCustomerTag,
                Condition = CustomerActionConditionEnum.OneOfThem,
            };
            await _customerActionService.InsertCustomerAction(customerAction);
            Assert.IsTrue(!String.IsNullOrEmpty(customerAction.Id));
        }

        [TestMethod()]
        public async Task DeleteCustomerActionTest()
        {
            var action = await _customerActionService.GetCustomerActionById(_Id_CustomerAction);
            await _customerActionService.DeleteCustomerAction(action);
        }

        [TestMethod()]
        public async Task UpdateCustomerActionTest()
        {
            var action = await _customerActionService.GetCustomerActionById(_Id_CustomerAction);
            action.Name = "Update test 2";
            await _customerActionService.UpdateCustomerAction(action);
        }

        [TestMethod()]
        public async Task GetCustomerActionTypeTest()
        {
            var actionTypes = await _customerActionService.GetCustomerActionType();
            Assert.IsTrue(actionTypes.Count >0 );
        }

        [TestMethod()]
        public async Task GetAllCustomerActionHistoryTest()
        {
            var actionHistory = await _customerActionService.GetAllCustomerActionHistory("");
            Assert.IsTrue(actionHistory.Count == 0);
        }

        [TestMethod()]
        public async Task GetCustomerActionTypeByIdTest()
        {
            var customerActionType = await _customerActionService.GetCustomerActionTypeById(_Id_CustomerActionType);
            Assert.IsNotNull(customerActionType);
        }
        //TO DO
        //[TestMethod()]
        //public async Task UpdateCustomerActionTypeTest()
        //{
        //    var customerActionType = await _customerActionService.GetCustomerActionTypeById(_Id_CustomerActionType);
        //    customerActionType.Enabled = false;
        //    await _customerActionService.UpdateCustomerActionType(customerActionType);
        //    Assert.IsFalse(customerActionType.Enabled);
        //}
    }
}