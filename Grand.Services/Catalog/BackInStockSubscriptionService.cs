using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Messages;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Back in stock subscription service
    /// </summary>
    public partial class BackInStockSubscriptionService : IBackInStockSubscriptionService
    {
        #region Fields

        private readonly IRepository<BackInStockSubscription> _backInStockSubscriptionRepository;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="backInStockSubscriptionRepository">Back in stock subscription repository</param>
        /// <param name="workflowMessageService">Workflow message service</param>
        /// <param name="eventPublisher">Event publisher</param>
        public BackInStockSubscriptionService(IRepository<BackInStockSubscription> backInStockSubscriptionRepository,
            IWorkflowMessageService workflowMessageService,
            IMediator mediator,
            IServiceProvider serviceProvider)
        {
            _backInStockSubscriptionRepository = backInStockSubscriptionRepository;
            _workflowMessageService = workflowMessageService;
            _mediator = mediator;
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a back in stock subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        public virtual async Task DeleteSubscription(BackInStockSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            await _backInStockSubscriptionRepository.DeleteAsync(subscription);

            //event notification
            await _mediator.EntityDeleted(subscription);
        }

        /// <summary>
        /// Gets all subscriptions
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="storeId">Store identifier; pass "" to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Subscriptions</returns>
        public virtual async Task<IPagedList<BackInStockSubscription>> GetAllSubscriptionsByCustomerId(string customerId,
            string storeId = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _backInStockSubscriptionRepository.Table;

            //customer
            query = query.Where(biss => biss.CustomerId == customerId);
            //store
            if (!String.IsNullOrEmpty(storeId))
                query = query.Where(biss => biss.StoreId == storeId);

            query = query.OrderByDescending(biss => biss.CreatedOnUtc);

            return await PagedList<BackInStockSubscription>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all subscriptions
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier; pass "" to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Subscriptions</returns>
        public virtual async Task<IPagedList<BackInStockSubscription>> GetAllSubscriptionsByProductId(string productId, string attributeXml, string warehouseId,
            string storeId = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _backInStockSubscriptionRepository.Table;
            //product
            query = query.Where(biss => biss.ProductId == productId);
            //store
            if (!string.IsNullOrEmpty(storeId))
                query = query.Where(biss => biss.StoreId == storeId);
            //warehouse
            if (!string.IsNullOrEmpty(warehouseId))
                query = query.Where(biss => biss.WarehouseId == warehouseId);

            //warehouse
            if (!string.IsNullOrEmpty(attributeXml))
                query = query.Where(biss => biss.AttributeXml == attributeXml);

            query = query.OrderByDescending(biss => biss.CreatedOnUtc);
            return await PagedList<BackInStockSubscription>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all subscriptions
        /// </summary>
        /// <param name="customerId">Customer id</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="attributeXml">Attribute xml</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <returns>Subscriptions</returns>
        public virtual async Task<BackInStockSubscription> FindSubscription(string customerId, string productId, string attributeXml, string storeId, string warehouseId)
        {
            var query = from biss in _backInStockSubscriptionRepository.Table
                        orderby biss.CreatedOnUtc descending
                        where biss.CustomerId == customerId &&
                              biss.ProductId == productId &&
                              biss.StoreId == storeId &&
                              biss.WarehouseId == warehouseId
                        select biss;

            if (!string.IsNullOrEmpty(attributeXml))
            {
                query = query.Where(x => x.AttributeXml == attributeXml);
            }
            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a subscription
        /// </summary>
        /// <param name="subscriptionId">Subscription identifier</param>
        /// <returns>Subscription</returns>
        public virtual async Task<BackInStockSubscription> GetSubscriptionById(string subscriptionId)
        {
            var subscription = await _backInStockSubscriptionRepository.GetByIdAsync(subscriptionId);
            return subscription;
        }

        /// <summary>
        /// Inserts subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        public virtual async Task InsertSubscription(BackInStockSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            await _backInStockSubscriptionRepository.InsertAsync(subscription);

            //event notification
            await _mediator.EntityInserted(subscription);
        }

        /// <summary>
        /// Updates subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        public virtual async Task UpdateSubscription(BackInStockSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            await _backInStockSubscriptionRepository.UpdateAsync(subscription);

            //event notification
            await _mediator.EntityUpdated(subscription);
        }

        /// <summary>
        /// Send notification to subscribers
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="warehouse">Warehouse ident</param>
        /// <returns>Number of sent email</returns>
        public virtual async Task<int> SendNotificationsToSubscribers(Product product, string warehouse)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var customerService = _serviceProvider.GetRequiredService<ICustomerService>();

            int result = 0;
            var subscriptions = await GetAllSubscriptionsByProductId(product.Id, string.Empty, warehouse);
            foreach (var subscription in subscriptions)
            {
                var customer = await customerService.GetCustomerById(subscription.CustomerId);
                //ensure that customer is registered (simple and fast way)
                if (customer != null && CommonHelper.IsValidEmail(customer.Email))
                {
                    var customerLanguageId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LanguageId, subscription.StoreId);
                    await _workflowMessageService.SendBackInStockNotification(customer, product, subscription, customerLanguageId);
                    result++;
                }
            }
            for (int i = 0; i <= subscriptions.Count - 1; i++)
                await DeleteSubscription(subscriptions[i]);

            return result;
        }

        /// <summary>
        /// Send notification to subscribers
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributeXml">Attribute xml</param>
        /// <param name="warehouse">Warehouse ident</param>
        /// <returns>Number of sent email</returns>
        public virtual async Task<int> SendNotificationsToSubscribers(Product product, string attributeXml, string warehouse)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var customerService = _serviceProvider.GetRequiredService<ICustomerService>();

            int result = 0;
            var subscriptions = await GetAllSubscriptionsByProductId(product.Id, attributeXml, warehouse);
            foreach (var subscription in subscriptions)
            {
                var customer = await customerService.GetCustomerById(subscription.CustomerId);
                //ensure that customer is registered (simple and fast way)
                if (customer != null && CommonHelper.IsValidEmail(customer.Email))
                {
                    var customerLanguageId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LanguageId, subscription.StoreId);
                    await _workflowMessageService.SendBackInStockNotification(customer, product, subscription, customerLanguageId);
                    result++;
                }
            }
            for (int i = 0; i <= subscriptions.Count - 1; i++)
                await DeleteSubscription(subscriptions[i]);

            return result;
        }

        #endregion
    }
}
