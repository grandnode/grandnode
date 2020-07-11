using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Services.Commands.Models.Customers;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Customers
{
    public partial class CustomerActionEventService : ICustomerActionEventService
    {
        #region Fields
        private const string CUSTOMER_ACTION_TYPE = "Grand.customer.action.type";

        private readonly IRepository<CustomerAction> _customerActionRepository;
        private readonly IRepository<CustomerActionHistory> _customerActionHistoryRepository;
        private readonly IRepository<CustomerActionType> _customerActionTypeRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IMediator _mediator;
        #endregion

        #region Ctor

        public CustomerActionEventService(
            IRepository<CustomerAction> customerActionRepository,
            IRepository<CustomerActionType> customerActionTypeRepository,
            IRepository<CustomerActionHistory> customerActionHistoryRepository,
            ICacheManager cacheManager,
            IMediator mediator)
        {
            _customerActionRepository = customerActionRepository;
            _customerActionTypeRepository = customerActionTypeRepository;
            _customerActionHistoryRepository = customerActionHistoryRepository;
            _cacheManager = cacheManager;
            _mediator = mediator;
        }

        #endregion

        #region Utilities

        protected async Task<IList<CustomerActionType>> GetAllCustomerActionType()
        {
            return await _cacheManager.GetAsync(CUSTOMER_ACTION_TYPE, () =>
            {
                return _customerActionTypeRepository.Table.ToListAsync();
            });
        }

        protected bool UsedAction(string actionId, string customerId)
        {
            var query = from u in _customerActionHistoryRepository.Table
                        where u.CustomerId == customerId && u.CustomerActionId == actionId
                        select u.Id;
            if (query.Count() > 0)
                return true;

            return false;
        }

        #endregion

        #region Methods

        public virtual async Task AddToCart(ShoppingCartItem cart, Product product, Customer customer)
        {
            var actiontypes = await GetAllCustomerActionType();
            var actionType = actiontypes.Where(x => x.SystemKeyword == CustomerActionTypeEnum.AddToCart.ToString()).FirstOrDefault();
            if (actionType?.Enabled == true)
            {
                var datetimeUtcNow = DateTime.UtcNow;
                var query = from a in _customerActionRepository.Table
                            where a.Active == true && a.ActionTypeId == actionType.Id
                                    && datetimeUtcNow >= a.StartDateTimeUtc && datetimeUtcNow <= a.EndDateTimeUtc
                            select a;

                foreach (var item in query.ToList())
                {
                    if (!UsedAction(item.Id, customer.Id))
                    {
                        if (await _mediator.Send(new CustomerActionEventConditionCommand() {
                            CustomerActionTypes = actiontypes,
                            Action = item,
                            ProductId = product.Id,
                            AttributesXml = cart.AttributesXml,
                            CustomerId = customer.Id
                        }))
                        {
                            await _mediator.Send(new CustomerActionEventReactionCommand() {
                                CustomerActionTypes = actiontypes,
                                Action = item,
                                CartItem = cart,
                                CustomerId = customer.Id
                            });
                        }
                    }
                }
            }
        }

        public virtual async Task AddOrder(Order order, CustomerActionTypeEnum customerActionType)
        {
            var actiontypes = await GetAllCustomerActionType();
            var actionType = actiontypes.Where(x => x.SystemKeyword == customerActionType.ToString()).FirstOrDefault();
            if (actionType?.Enabled == true)
            {
                var datetimeUtcNow = DateTime.UtcNow;
                var query = from a in _customerActionRepository.Table
                            where a.Active == true && a.ActionTypeId == actionType.Id
                                    && datetimeUtcNow >= a.StartDateTimeUtc && datetimeUtcNow <= a.EndDateTimeUtc
                            select a;

                foreach (var item in query.ToList())
                {
                    if (!UsedAction(item.Id, order.CustomerId))
                    {
                        foreach (var orderItem in order.OrderItems)
                        {
                            if (await _mediator.Send(new CustomerActionEventConditionCommand() {
                                CustomerActionTypes = actiontypes,
                                ProductId = orderItem.ProductId,
                                CustomerId = order.CustomerId,
                                AttributesXml = orderItem.AttributesXml
                            }))
                            {
                                await _mediator.Send(new CustomerActionEventReactionCommand() {
                                    CustomerActionTypes = actiontypes,
                                    Action = item,
                                    Order = order,
                                    CustomerId = order.CustomerId
                                });
                                break;
                            }
                        }
                    }
                }

            }

        }

        public virtual async Task Url(Customer customer, string currentUrl, string previousUrl)
        {
            if (!customer.IsSystemAccount)
            {
                var actiontypes = await GetAllCustomerActionType();
                var actionType = actiontypes.FirstOrDefault(x => x.SystemKeyword == CustomerActionTypeEnum.Url.ToString());
                if (actionType?.Enabled == true)
                {
                    var datetimeUtcNow = DateTime.UtcNow;
                    var query = from a in _customerActionRepository.Table
                                where a.Active == true && a.ActionTypeId == actionType.Id
                                        && datetimeUtcNow >= a.StartDateTimeUtc && datetimeUtcNow <= a.EndDateTimeUtc
                                select a;

                    foreach (var item in query.ToList())
                    {
                        if (!UsedAction(item.Id, customer.Id))
                        {
                            if (await _mediator.Send(new CustomerActionEventConditionCommand() {
                                CustomerActionTypes = actiontypes,
                                Action = item,
                                CustomerId = customer.Id,
                                CurrentUrl = currentUrl,
                                PreviousUrl = previousUrl
                            }))
                            {
                                await _mediator.Send(new CustomerActionEventReactionCommand() {
                                    CustomerActionTypes = actiontypes,
                                    Action = item,
                                    CustomerId = customer.Id
                                });
                            }
                        }
                    }
                }
            }
        }

        public virtual async Task Viewed(Customer customer, string currentUrl, string previousUrl)
        {
            if (!customer.IsSystemAccount)
            {
                var actiontypes = await GetAllCustomerActionType();
                var actionType = actiontypes.Where(x => x.SystemKeyword == CustomerActionTypeEnum.Viewed.ToString()).FirstOrDefault();
                if (actionType?.Enabled == true)
                {
                    var datetimeUtcNow = DateTime.UtcNow;
                    var query = from a in _customerActionRepository.Table
                                where a.Active == true && a.ActionTypeId == actionType.Id
                                        && datetimeUtcNow >= a.StartDateTimeUtc && datetimeUtcNow <= a.EndDateTimeUtc
                                select a;

                    foreach (var item in query.ToList())
                    {
                        if (!UsedAction(item.Id, customer.Id))
                        {
                            if (await _mediator.Send(new CustomerActionEventConditionCommand() {
                                CustomerActionTypes = actiontypes,
                                Action = item,
                                CustomerId = customer.Id,
                                CurrentUrl = currentUrl,
                                PreviousUrl = previousUrl
                            }))
                            {
                                await _mediator.Send(new CustomerActionEventReactionCommand() {
                                    CustomerActionTypes = actiontypes,
                                    Action = item,
                                    CustomerId = customer.Id
                                });
                            }
                        }
                    }

                }
            }

        }

        public virtual async Task Registration(Customer customer)
        {
            var actiontypes = await GetAllCustomerActionType();
            var actionType = actiontypes.Where(x => x.SystemKeyword == CustomerActionTypeEnum.Registration.ToString()).FirstOrDefault();
            if (actionType?.Enabled == true)
            {
                var datetimeUtcNow = DateTime.UtcNow;
                var query = from a in _customerActionRepository.Table
                            where a.Active == true && a.ActionTypeId == actionType.Id
                                    && datetimeUtcNow >= a.StartDateTimeUtc && datetimeUtcNow <= a.EndDateTimeUtc
                            select a;

                foreach (var item in query.ToList())
                {
                    if (!UsedAction(item.Id, customer.Id))
                    {
                        if (await _mediator.Send(new CustomerActionEventConditionCommand() {
                            CustomerActionTypes = actiontypes,
                            Action = item,
                            CustomerId = customer.Id
                        }))
                        {
                            await _mediator.Send(new CustomerActionEventReactionCommand() {
                                CustomerActionTypes = actiontypes,
                                Action = item,
                                CustomerId = customer.Id
                            });
                        }
                    }
                }

            }
        }

        #endregion
    }
}
