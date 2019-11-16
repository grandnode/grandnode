using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Core.Domain.Shipping;
using Grand.Services.Events;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Order service
    /// </summary>
    public partial class OrderService : IOrderService
    {
        #region Fields

        private static readonly Object _locker = new object();
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderNote> _orderNoteRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductDeleted> _productDeletedRepository;
        private readonly IRepository<ProductAlsoPurchased> _productAlsoPurchasedRepository;
        private readonly IRepository<RecurringPayment> _recurringPaymentRepository;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="orderRepository">Order repository</param>
        /// <param name="orderNoteRepository">Order note repository</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="recurringPaymentRepository">Recurring payment repository</param>
        /// <param name="mediator">Mediator</param>
        /// <param name="productAlsoPurchasedRepository">Product also purchased repository</param>
        public OrderService(IRepository<Order> orderRepository,
            IRepository<OrderNote> orderNoteRepository,
            IRepository<Product> productRepository,
            IRepository<RecurringPayment> recurringPaymentRepository,
            IMediator mediator,
            IRepository<ProductAlsoPurchased> productAlsoPurchasedRepository,
            IRepository<ProductDeleted> productDeletedRepository)
        {
            _orderRepository = orderRepository;
            _orderNoteRepository = orderNoteRepository;
            _productRepository = productRepository;
            _recurringPaymentRepository = recurringPaymentRepository;
            _mediator = mediator;
            _productAlsoPurchasedRepository = productAlsoPurchasedRepository;
            _productDeletedRepository = productDeletedRepository;
        }

        #endregion

        #region Methods

        #region Orders

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        /// <returns>Order</returns>
        public virtual Task<Order> GetOrderById(string orderId)
        {
            return _orderRepository.GetByIdAsync(orderId);
        }


        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderId">The order item identifier</param>
        /// <returns>Order</returns>
        public virtual Task<Order> GetOrderByOrderItemId(string orderItemId)
        {
            var query = from o in _orderRepository.Table
                        where o.OrderItems.Any(x => x.Id == orderItemId)
                        select o;

            return query.FirstOrDefaultAsync();
        }
        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderNumber">The order number</param>
        /// <returns>Order</returns>
        public virtual Task<Order> GetOrderByNumber(int orderNumber)
        {
            return _orderRepository.Table.FirstOrDefaultAsync(x => x.OrderNumber == orderNumber);
        }


        /// <summary>
        /// Get orders by identifiers
        /// </summary>
        /// <param name="orderIds">Order identifiers</param>
        /// <returns>Order</returns>
        public virtual async Task<IList<Order>> GetOrdersByIds(string[] orderIds)
        {
            if (orderIds == null || orderIds.Length == 0)
                return new List<Order>();

            var query = from o in _orderRepository.Table
                        where orderIds.Contains(o.Id)
                        select o;
            var orders = await query.ToListAsync();
            //sort by passed identifiers
            var sortedOrders = new List<Order>();
            foreach (string id in orderIds)
            {
                var order = orders.Find(x => x.Id == id);
                if (order != null)
                    sortedOrders.Add(order);
            }
            return sortedOrders;
        }

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderGuid">The order identifier</param>
        /// <returns>Order</returns>
        public virtual Task<Order> GetOrderByGuid(Guid orderGuid)
        {
            var query = from o in _orderRepository.Table
                        where o.OrderGuid == orderGuid
                        select o;
            return query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Deletes an order
        /// </summary>
        /// <param name="order">The order</param>
        public virtual async Task DeleteOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var filters = Builders<ProductAlsoPurchased>.Filter;
            var filter = filters.Where(x => x.OrderId == order.Id);
            await _productAlsoPurchasedRepository.Collection.DeleteManyAsync(filter);
            order.Deleted = true;
            await UpdateOrder(order);
        }

        /// <summary>
        /// Search orders
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="customerId">Customer identifier; 0 to load all orders</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="affiliateId">Affiliate identifier; 0 to load all orders</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier, only orders with products from a specified warehouse will be loaded; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="os">Order status; null to load all orders</param>
        /// <param name="ps">Order payment status; null to load all orders</param>
        /// <param name="ss">Order shipment status; null to load all orders</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <param name="orderGuid">Search by order GUID (Global unique identifier) or part of GUID. Leave empty to load all orders.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Orders</returns>
        public virtual async Task<IPagedList<Order>> SearchOrders(string storeId = "",
            string vendorId = "", string customerId = "",
            string productId = "", string affiliateId = "", string warehouseId = "",
            string billingCountryId = "", string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            OrderStatus? os = null, PaymentStatus? ps = null, ShippingStatus? ss = null,
            string billingEmail = null, string billingLastName = "", string orderGuid = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            int? orderStatusId = null;
            if (os.HasValue)
                orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue)
                paymentStatusId = (int)ps.Value;

            int? shippingStatusId = null;
            if (ss.HasValue)
                shippingStatusId = (int)ss.Value;

            var query = _orderRepository.Table;
            if (!String.IsNullOrEmpty(storeId))
                query = query.Where(o => o.StoreId == storeId);
            if (!String.IsNullOrEmpty(vendorId))
            {
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem => orderItem.VendorId == vendorId));
            }
            if (!String.IsNullOrEmpty(customerId))
                query = query.Where(o => o.CustomerId == customerId);
            if (!String.IsNullOrEmpty(productId))
            {
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem => orderItem.ProductId == productId));
            }
            if (!String.IsNullOrEmpty(warehouseId))
            {
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem =>
                        orderItem.WarehouseId == warehouseId
                        ));
            }
            if (!String.IsNullOrEmpty(billingCountryId))
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId);
            if (!String.IsNullOrEmpty(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);
            if (!String.IsNullOrEmpty(affiliateId))
                query = query.Where(o => o.AffiliateId == affiliateId);
            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.CreatedOnUtc);
            if (orderStatusId.HasValue)
                query = query.Where(o => orderStatusId.Value == o.OrderStatusId);
            if (paymentStatusId.HasValue)
                query = query.Where(o => paymentStatusId.Value == o.PaymentStatusId);
            if (shippingStatusId.HasValue)
                query = query.Where(o => shippingStatusId.Value == o.ShippingStatusId);
            if (!String.IsNullOrEmpty(billingEmail))
                query = query.Where(o => o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));
            if (!String.IsNullOrEmpty(billingLastName))
                query = query.Where(o => o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.LastName) && o.BillingAddress.LastName.Contains(billingLastName));

            query = query.Where(o => !o.Deleted);
            query = query.OrderByDescending(o => o.CreatedOnUtc);



            if (!String.IsNullOrEmpty(orderGuid))
            {
                //filter by GUID. Filter in BLL because EF doesn't support casting of GUID to string
                var orders = await query.ToListAsync();
                orders = orders.FindAll(o => o.OrderGuid.ToString().ToLowerInvariant().Contains(orderGuid.ToLowerInvariant()));
                return new PagedList<Order>(orders, pageIndex, pageSize);
            }

            //database layer paging
            return await PagedList<Order>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Inserts an order
        /// </summary>
        /// <param name="order">Order</param>
        public virtual async Task InsertOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            lock (_locker)
            {
                int orderExists = _orderRepository.Table.OrderByDescending(x => x.OrderNumber).Select(x => x.OrderNumber).FirstOrDefault();
                var orderNumber = orderExists != 0 ? orderExists + 1 : 1;
                order.OrderNumber = orderNumber;
            }

            await _orderRepository.InsertAsync(order);

            //event notification
            await _mediator.EntityInserted(order);
        }

        /// <summary>
        /// Inserts an product also purchased
        /// </summary>
        /// <param name="order">Order</param>
        public virtual async Task InsertProductAlsoPurchased(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            foreach (var item in order.OrderItems)
            {
                foreach (var it in order.OrderItems.Where(x => x.ProductId != item.ProductId))
                {
                    var productPurchase = new ProductAlsoPurchased();
                    productPurchase.ProductId = item.ProductId;
                    productPurchase.OrderId = order.Id;
                    productPurchase.CreatedOrderOnUtc = order.CreatedOnUtc;
                    productPurchase.Quantity = it.Quantity;
                    productPurchase.StoreId = order.StoreId;
                    productPurchase.ProductId2 = it.ProductId;
                    await _productAlsoPurchasedRepository.InsertAsync(productPurchase);
                }
            }
        }

        /// <summary>
        /// Updates the order
        /// </summary>
        /// <param name="order">The order</param>
        public virtual async Task UpdateOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            await _orderRepository.UpdateAsync(order);

            //event notification
            await _mediator.EntityUpdated(order);
        }

        /// <summary>
        /// Get an order by authorization transaction ID and payment method system name
        /// </summary>
        /// <param name="authorizationTransactionId">Authorization transaction ID</param>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>Order</returns>
        public virtual Task<Order> GetOrderByAuthorizationTransactionIdAndPaymentMethod(string authorizationTransactionId,
            string paymentMethodSystemName)
        {
            var query = _orderRepository.Table;

            if (!String.IsNullOrWhiteSpace(authorizationTransactionId))
                query = query.Where(o => o.AuthorizationTransactionId == authorizationTransactionId);

            if (!String.IsNullOrWhiteSpace(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);

            query = query.OrderByDescending(o => o.CreatedOnUtc);
            return query.FirstOrDefaultAsync();
        }

        #endregion

        #region Orders items

        /// <summary>
        /// Gets an item
        /// </summary>
        /// <param name="orderItemGuid">Order identifier</param>
        /// <returns>Order item</returns>
        public virtual Task<OrderItem> GetOrderItemByGuid(Guid orderItemGuid)
        {
            var query = from order in _orderRepository.Table
                        from orderItem in order.OrderItems
                        select orderItem;

            query = from orderItem in query
                    where orderItem.OrderItemGuid == orderItemGuid
                    select orderItem;

            return query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets all order items
        /// </summary>
        /// <param name="orderId">Order identifier; null to load all records</param>
        /// <param name="customerId">Customer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Order shipment status; null to load all records</param>
        /// <param name="loadDownloableProductsOnly">Value indicating whether to load downloadable products only</param>
        /// <returns>Orders</returns>
        public virtual async Task<IList<OrderItem>> GetAllOrderItems(string orderId,
            string customerId, DateTime? createdFromUtc, DateTime? createdToUtc,
            OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss,
            bool loadDownloableProductsOnly)
        {
            int? orderStatusId = null;
            if (os.HasValue)
                orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue)
                paymentStatusId = (int)ps.Value;

            int? shippingStatusId = null;
            if (ss.HasValue)
                shippingStatusId = (int)ss.Value;

            var builder = Builders<Order>.Filter;

            var filter = builder.Where(x => !x.Deleted);

            if (!String.IsNullOrEmpty(orderId))
                filter = filter & builder.Where(o => o.Id == orderId);

            if (!String.IsNullOrEmpty(customerId))
                filter = filter & builder.Where(o => o.CustomerId == customerId);

            if (orderStatusId.HasValue)
                filter = filter & builder.Where(o => o.OrderStatusId == orderStatusId.Value);

            if (paymentStatusId.HasValue)
                filter = filter & builder.Where(o => o.PaymentStatusId == paymentStatusId.Value);

            if (shippingStatusId.HasValue)
                filter = filter & builder.Where(o => o.ShippingStatusId == shippingStatusId.Value);

            if (createdFromUtc.HasValue)
                filter = filter & builder.Where(o => o.CreatedOnUtc >= createdFromUtc.Value);

            if (createdToUtc.HasValue)
                filter = filter & builder.Where(o => o.CreatedOnUtc <= createdToUtc.Value);

            var query = await _orderRepository.Collection.Aggregate().Match(filter).Unwind<Order, UnwindOrderItem>(x => x.OrderItems).ToListAsync();
            var items = new List<OrderItem>();
            foreach (var item in query)
            {
                if (loadDownloableProductsOnly)
                {
                    var product = await _productRepository.GetByIdAsync(item.OrderItems.ProductId);
                    if (product == null)
                        product = await _productDeletedRepository.GetByIdAsync(item.OrderItems.ProductId) as Product;
                    if (product != null)
                    {
                        if (product.IsDownload)
                        {
                            items.Add(item.OrderItems);
                        }
                    }
                }
                else
                    items.Add(item.OrderItems);
            }
            return items;
        }

        /// <summary>
        /// Delete an order item
        /// </summary>
        /// <param name="orderItem">The order item</param>
        public virtual async Task DeleteOrderItem(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException("orderItem");

            var order = await GetOrderByOrderItemId(orderItem.Id);

            var updatebuilder = Builders<Order>.Update;
            var updatefilter = updatebuilder.PullFilter(x => x.OrderItems, y => y.Id == orderItem.Id);
            await _orderRepository.Collection.UpdateOneAsync(new BsonDocument("_id", order.Id), updatefilter);

            var filters = Builders<ProductAlsoPurchased>.Filter;
            var filter = filters.Where(x => x.OrderId == order.Id && (x.ProductId == orderItem.ProductId || x.ProductId2 == orderItem.ProductId));
            await _productAlsoPurchasedRepository.Collection.DeleteManyAsync(filter);

            //event notification
            await _mediator.EntityDeleted(orderItem);
        }

        #endregion

        #region Orders notes

        /// <summary>
        /// Deletes an order note
        /// </summary>
        /// <param name="orderNote">The order note</param>
        public virtual async Task DeleteOrderNote(OrderNote orderNote)
        {
            if (orderNote == null)
                throw new ArgumentNullException("orderNote");

            await _orderNoteRepository.DeleteAsync(orderNote);

            //event notification
            await _mediator.EntityDeleted(orderNote);
        }

        /// <summary>
        /// Deletes an order note
        /// </summary>
        /// <param name="orderNote">The order note</param>
        public virtual async Task InsertOrderNote(OrderNote orderNote)
        {
            if (orderNote == null)
                throw new ArgumentNullException("orderNote");

            await _orderNoteRepository.InsertAsync(orderNote);

            //event notification
            await _mediator.EntityInserted(orderNote);
        }

        public virtual async Task<IList<OrderNote>> GetOrderNotes(string orderId)
        {
            var query = from orderNote in _orderNoteRepository.Table
                        where orderNote.OrderId == orderId
                        orderby orderNote.CreatedOnUtc descending
                        select orderNote;

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get ordernote by id
        /// </summary>
        /// <param name="ordernoteId">Order note identifier</param>
        /// <returns>OrderNote</returns>
        public virtual Task<OrderNote> GetOrderNote(string ordernoteId)
        {
            return _orderNoteRepository.Table.Where(x => x.Id == ordernoteId).FirstOrDefaultAsync();
        }


        #endregion

        #region Recurring payments

        /// <summary>
        /// Deletes a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual async Task DeleteRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            recurringPayment.Deleted = true;
            await UpdateRecurringPayment(recurringPayment);
        }

        /// <summary>
        /// Gets a recurring payment
        /// </summary>
        /// <param name="recurringPaymentId">The recurring payment identifier</param>
        /// <returns>Recurring payment</returns>
        public virtual Task<RecurringPayment> GetRecurringPaymentById(string recurringPaymentId)
        {
            return _recurringPaymentRepository.GetByIdAsync(recurringPaymentId);
        }

        /// <summary>
        /// Inserts a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual async Task InsertRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            await _recurringPaymentRepository.InsertAsync(recurringPayment);

            //event notification
            await _mediator.EntityInserted(recurringPayment);
        }

        /// <summary>
        /// Updates the recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual async Task UpdateRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            await _recurringPaymentRepository.UpdateAsync(recurringPayment);

            //event notification
            await _mediator.EntityUpdated(recurringPayment);
        }

        /// <summary>
        /// Search recurring payments
        /// </summary>
        /// <param name="storeId">The store identifier; "" to load all records</param>
        /// <param name="customerId">The customer identifier; "" to load all records</param>
        /// <param name="initialOrderId">The initial order identifier; "" to load all records</param>
        /// <param name="initialOrderStatus">Initial order status identifier; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Recurring payments</returns>
        public virtual async Task<IPagedList<RecurringPayment>> SearchRecurringPayments(string storeId = "",
            string customerId = "", string initialOrderId = "", OrderStatus? initialOrderStatus = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            int? initialOrderStatusId = null;
            if (initialOrderStatus.HasValue)
                initialOrderStatusId = (int)initialOrderStatus.Value;
            //TO DO
            var query1 = from rp in _recurringPaymentRepository.Table
                         where
                         (!rp.Deleted) &&
                         (showHidden || rp.IsActive) &&
                         (customerId == "" || rp.InitialOrder.CustomerId == customerId) &&
                         (storeId == "" || rp.InitialOrder.StoreId == storeId) &&
                         (initialOrderId == "" || rp.InitialOrder.Id == initialOrderId)
                         select rp.Id;
            var cc = query1.ToList();
            var query2 = from rp in _recurringPaymentRepository.Table
                         where cc.Contains(rp.Id)
                         orderby rp.StartDateUtc
                         select rp;
            return await PagedList<RecurringPayment>.Create(query2, pageIndex, pageSize);
        }

        #endregion

        #endregion

        #region UnwindOrderItem

        public class UnwindOrderItem
        {
            public OrderItem OrderItems { get; set; }
        }

        #endregion

    }
}
