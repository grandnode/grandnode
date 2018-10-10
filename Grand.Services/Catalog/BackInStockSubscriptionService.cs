using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Infrastructure;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Messages;
using MongoDB.Driver.Linq;
using System;
using System.Linq;

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
        private readonly IEventPublisher _eventPublisher;

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
            IEventPublisher eventPublisher)
        {
            this._backInStockSubscriptionRepository = backInStockSubscriptionRepository;
            this._workflowMessageService = workflowMessageService;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a back in stock subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        public virtual void DeleteSubscription(BackInStockSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            _backInStockSubscriptionRepository.Delete(subscription);

            //event notification
            _eventPublisher.EntityDeleted(subscription);
        }

        /// <summary>
        /// Gets all subscriptions
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="storeId">Store identifier; pass "" to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Subscriptions</returns>
        public virtual IPagedList<BackInStockSubscription> GetAllSubscriptionsByCustomerId(string customerId,
            string storeId = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _backInStockSubscriptionRepository.Table;

            //customer
            query = query.Where(biss => biss.CustomerId == customerId);
            //store
            if (!String.IsNullOrEmpty(storeId))
                query = query.Where(biss => biss.StoreId == storeId);

            query = query.OrderByDescending(biss => biss.CreatedOnUtc);

            return new PagedList<BackInStockSubscription>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all subscriptions
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier; pass "" to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Subscriptions</returns>
        public virtual IPagedList<BackInStockSubscription> GetAllSubscriptionsByProductId(string productId, string warehouseId,
            string storeId = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _backInStockSubscriptionRepository.Table;
            //product
            query = query.Where(biss => biss.ProductId == productId);
            //store
            if (!String.IsNullOrEmpty(storeId))
                query = query.Where(biss => biss.StoreId == storeId);
            //warehouse
            if (!String.IsNullOrEmpty(warehouseId))
                query = query.Where(biss => biss.WarehouseId == warehouseId);

            query = query.OrderByDescending(biss => biss.CreatedOnUtc);
            return new PagedList<BackInStockSubscription>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all subscriptions
        /// </summary>
        /// <param name="customerId">Customer id</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Subscriptions</returns>
        public virtual BackInStockSubscription FindSubscription(string customerId, string productId, string storeId, string warehouseId)
        {
            var query = from biss in _backInStockSubscriptionRepository.Table
                        orderby biss.CreatedOnUtc descending
                        where biss.CustomerId == customerId &&
                              biss.ProductId == productId &&
                              biss.StoreId == storeId &&
                              biss.WarehouseId == warehouseId
                        select biss;

            var subscription = query.FirstOrDefault();
            return subscription;
        }

        /// <summary>
        /// Gets a subscription
        /// </summary>
        /// <param name="subscriptionId">Subscription identifier</param>
        /// <returns>Subscription</returns>
        public virtual BackInStockSubscription GetSubscriptionById(string subscriptionId)
        {
            var subscription = _backInStockSubscriptionRepository.GetById(subscriptionId);
            return subscription;
        }

        /// <summary>
        /// Inserts subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        public virtual void InsertSubscription(BackInStockSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            _backInStockSubscriptionRepository.Insert(subscription);

            //event notification
            _eventPublisher.EntityInserted(subscription);
        }

        /// <summary>
        /// Updates subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        public virtual void UpdateSubscription(BackInStockSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            _backInStockSubscriptionRepository.Update(subscription);

            //event notification
            _eventPublisher.EntityUpdated(subscription);
        }

        /// <summary>
        /// Send notification to subscribers
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>Number of sent email</returns>
        public virtual int SendNotificationsToSubscribers(Product product, string warehouse)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            int result = 0;
            var subscriptions = GetAllSubscriptionsByProductId(product.Id, warehouse);
            foreach (var subscription in subscriptions)
            {
                var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(subscription.CustomerId);
                //ensure that customer is registered (simple and fast way)
                if (customer != null && CommonHelper.IsValidEmail(customer.Email))
                {
                    var customerLanguageId = customer.GetAttribute<string>(SystemCustomerAttributeNames.LanguageId, subscription.StoreId);
                    _workflowMessageService.SendBackInStockNotification(subscription, customerLanguageId);
                    result++;
                }
            }
            for (int i = 0; i <= subscriptions.Count - 1; i++)
                DeleteSubscription(subscriptions[i]);
            return result;
        }
        
        #endregion
    }
}
