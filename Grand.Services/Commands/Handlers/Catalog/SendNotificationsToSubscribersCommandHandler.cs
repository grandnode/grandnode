using Grand.Core;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Services.Commands.Models.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Messages;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Catalog
{
    public class SendNotificationsToSubscribersCommandHandler : IRequestHandler<SendNotificationsToSubscribersCommand, IList<BackInStockSubscription>>
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IRepository<BackInStockSubscription> _backInStockSubscriptionRepository;

        public SendNotificationsToSubscribersCommandHandler(
            ICustomerService customerService,
            IWorkflowMessageService workflowMessageService,
            IRepository<BackInStockSubscription> backInStockSubscriptionRepository)
        {
            _customerService = customerService;
            _workflowMessageService = workflowMessageService;
            _backInStockSubscriptionRepository = backInStockSubscriptionRepository;
        }

        public async Task<IList<BackInStockSubscription>> Handle(SendNotificationsToSubscribersCommand request, CancellationToken cancellationToken)
        {
            if (request.Product == null)
                throw new ArgumentNullException("product");

            int result = 0;
            var subscriptions = await GetAllSubscriptionsByProductId(request.Product.Id, request.AttributeXml, request.Warehouse);
            foreach (var subscription in subscriptions)
            {
                var customer = await _customerService.GetCustomerById(subscription.CustomerId);
                //ensure that customer is registered (simple and fast way)
                if (customer != null && CommonHelper.IsValidEmail(customer.Email))
                {
                    var customerLanguageId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LanguageId, subscription.StoreId);
                    await _workflowMessageService.SendBackInStockNotification(customer, request.Product, subscription, customerLanguageId);
                    result++;
                }
            }

            return subscriptions;

        }

        /// <summary>
        /// Gets all subscriptions
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier; pass "" to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Subscriptions</returns>
        private async Task<IList<BackInStockSubscription>> GetAllSubscriptionsByProductId(string productId, string attributeXml, string warehouseId)
        {
            var query = _backInStockSubscriptionRepository.Table;
            //product
            query = query.Where(biss => biss.ProductId == productId);

            //warehouse
            if (!string.IsNullOrEmpty(warehouseId))
                query = query.Where(biss => biss.WarehouseId == warehouseId);

            //warehouse
            if (!string.IsNullOrEmpty(attributeXml))
                query = query.Where(biss => biss.AttributeXml == attributeXml);

            query = query.OrderByDescending(biss => biss.CreatedOnUtc);
            return await query.ToListAsync();
        }
    }
}
