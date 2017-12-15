using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Services.Events;
using Grand.Services.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Grand.Services.Customers.Tests
{
    [TestClass()]
    public class CustomerActionServiceTests
    {
        private IRepository<CustomerAction> _customerActionRepository;
        private IRepository<CustomerActionType> _customerActionTypeRepository;
        private IRepository<CustomerActionHistory> _customerActionHistoryRepository;
        private IEventPublisher _eventPublisher;
        private ICustomerActionService _customerActionService;
        private string _Id_CustomerAction;
        private string _Id_CustomerActionType;

        [TestInitialize()]
        public void TestInitialize()
        {
            var tempEventPublisher = new Mock<IEventPublisher>();
            {
                tempEventPublisher.Setup(x => x.Publish(It.IsAny<object>()));
                _eventPublisher = tempEventPublisher.Object;
            }

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
            _customerActionHistoryRepository, _eventPublisher, new GrandNullCache());

        }

        [TestMethod()]
        public void GetCustomerActionByIdTest()
        {
            var action = _customerActionService.GetCustomerActionById(_Id_CustomerAction);
            Assert.IsNotNull(action);
        }

        [TestMethod()]
        public void GetCustomerActionsTest()
        {
            var actions = _customerActionService.GetCustomerActions();
            Assert.IsNotNull(actions);
            Assert.IsTrue(actions.Count > 0);
        }

        [TestMethod()]
        public void InsertCustomerActionTest()
        {
            var customerAction = new CustomerAction()
            {
                Active = true,
                StartDateTimeUtc = DateTime.UtcNow,
                EndDateTimeUtc = DateTime.UtcNow.AddMonths(1),
                Name = "Test action",
                ReactionTypeId = (int)CustomerReactionTypeEnum.AssignToCustomerTag,
                Condition = CustomerActionConditionEnum.OneOfThem,
            };
            _customerActionService.InsertCustomerAction(customerAction);
            Assert.IsTrue(!String.IsNullOrEmpty(customerAction.Id));
        }

        [TestMethod()]
        public void DeleteCustomerActionTest()
        {
            var action = _customerActionService.GetCustomerActionById(_Id_CustomerAction);
            _customerActionService.DeleteCustomerAction(action);
        }

        [TestMethod()]
        public void UpdateCustomerActionTest()
        {
            var action = _customerActionService.GetCustomerActionById(_Id_CustomerAction);
            action.Name = "Update test 2";
            _customerActionService.UpdateCustomerAction(action);
        }

        [TestMethod()]
        public void GetCustomerActionTypeTest()
        {
            var actionTypes = _customerActionService.GetCustomerActionType();
            Assert.IsTrue(actionTypes.Count >0 );
        }

        [TestMethod()]
        public void GetAllCustomerActionHistoryTest()
        {
            var actionHistory = _customerActionService.GetAllCustomerActionHistory("");
            Assert.IsTrue(actionHistory.Count == 0);
        }

        [TestMethod()]
        public void GetCustomerActionTypeByIdTest()
        {
            var customerActionType = _customerActionService.GetCustomerActionTypeById(_Id_CustomerActionType);
            Assert.IsNotNull(customerActionType);
        }

        [TestMethod()]
        public void UpdateCustomerActionTypeTest()
        {
            var customerActionType = _customerActionService.GetCustomerActionTypeById(_Id_CustomerActionType);
            customerActionType.Enabled = false;
            _customerActionService.UpdateCustomerActionType(customerActionType);
            Assert.IsFalse(customerActionType.Enabled);
        }
    }
}