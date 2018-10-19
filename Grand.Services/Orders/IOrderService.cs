using Grand.Core;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Core.Domain.Shipping;
using System;
using System.Collections.Generic;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Order service interface
    /// </summary>
    public partial interface IOrderService
    {
        #region Orders

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        /// <returns>Order</returns>
        Order GetOrderById(string orderId);

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderItemId">The order item identifier</param>
        /// <returns>Order</returns>
        Order GetOrderByOrderItemId(string orderItemId);

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderNumber">The order number</param>
        /// <returns>Order</returns>
        Order GetOrderByNumber(int orderNumber);

        /// <summary>
        /// Get orders by identifiers
        /// </summary>
        /// <param name="orderIds">Order identifiers</param>
        /// <returns>Order</returns>
        IList<Order> GetOrdersByIds(string[] orderIds);

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderGuid">The order identifier</param>
        /// <returns>Order</returns>
        Order GetOrderByGuid(Guid orderGuid);

        /// <summary>
        /// Deletes an order
        /// </summary>
        /// <param name="order">The order</param>
        void DeleteOrder(Order order);

        /// <summary>
        /// Search orders
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="customerId">Customer identifier; null to load all orders</param>
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
        /// <param name="orderGuid">Search by order GUID (Global unique identifier) or part of GUID. Leave empty to load all records.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Orders</returns>
        IPagedList<Order> SearchOrders(string storeId = "",
            string vendorId = "", string customerId = "",
            string productId = "", string affiliateId = "", string warehouseId = "",
            string billingCountryId = "", string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            OrderStatus? os = null, PaymentStatus? ps = null, ShippingStatus? ss = null,
            string billingEmail = null, string billingLastName = "", string orderGuid = null,
            int pageIndex = 0, int pageSize = int.MaxValue);
        
        /// <summary>
        /// Inserts an order
        /// </summary>
        /// <param name="order">Order</param>
        void InsertOrder(Order order);

        /// <summary>
        /// Inserts an product also purchased
        /// </summary>
        /// <param name="order">Order</param>
        void InsertProductAlsoPurchased(Order order);

        /// <summary>
        /// Updates the order
        /// </summary>
        /// <param name="order">The order</param>
        void UpdateOrder(Order order);

        /// <summary>
        /// Get an order by authorization transaction ID and payment method system name
        /// </summary>
        /// <param name="authorizationTransactionId">Authorization transaction ID</param>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>Order</returns>
        Order GetOrderByAuthorizationTransactionIdAndPaymentMethod(string authorizationTransactionId, string paymentMethodSystemName);
        
        #endregion

        #region Orders items
        
        /// <summary>
        /// Gets an order item
        /// </summary>
        /// <param name="orderItemGuid">Order item identifier</param>
        /// <returns>Order item</returns>
        OrderItem GetOrderItemByGuid(Guid orderItemGuid);

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
        /// <returns>Order items</returns>
        IList<OrderItem> GetAllOrderItems(string orderId,
           string customerId, DateTime? createdFromUtc, DateTime? createdToUtc,
           OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss,
           bool loadDownloableProductsOnly = false);

        /// <summary>
        /// Delete an order item
        /// </summary>
        /// <param name="orderItem">The order item</param>
        void DeleteOrderItem(OrderItem orderItem);

        #endregion

        #region Order notes

        /// <summary>
        /// Deletes an order note
        /// </summary>
        /// <param name="orderNote">The order note</param>
        void DeleteOrderNote(OrderNote orderNote);

        /// <summary>
        /// Insert an order note
        /// </summary>
        /// <param name="orderNote">The order note</param>
        void InsertOrderNote(OrderNote orderNote);


        /// <summary>
        /// Get ordernotes for order
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <returns>OrderNote</returns>
        IList<OrderNote> GetOrderNotes(string orderId);

        /// <summary>
        /// Get ordernote by id
        /// </summary>
        /// <param name="ordernoteId">Order note identifier</param>
        /// <returns>OrderNote</returns>
        OrderNote GetOrderNote(string ordernoteId);

        #endregion

        #region Recurring payments

        /// <summary>
        /// Deletes a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        void DeleteRecurringPayment(RecurringPayment recurringPayment);

        /// <summary>
        /// Gets a recurring payment
        /// </summary>
        /// <param name="recurringPaymentId">The recurring payment identifier</param>
        /// <returns>Recurring payment</returns>
        RecurringPayment GetRecurringPaymentById(string recurringPaymentId);

        /// <summary>
        /// Inserts a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        void InsertRecurringPayment(RecurringPayment recurringPayment);

        /// <summary>
        /// Updates the recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        void UpdateRecurringPayment(RecurringPayment recurringPayment);

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
        IPagedList<RecurringPayment> SearchRecurringPayments(string storeId = "",
            string customerId = "", string initialOrderId = "", OrderStatus? initialOrderStatus = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);



        #endregion
    }
}
