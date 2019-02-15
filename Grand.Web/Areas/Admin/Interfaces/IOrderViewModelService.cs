using Grand.Core.Domain.Common;
using Grand.Core.Domain.Orders;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IOrderViewModelService
    {
        OrderListModel PrepareOrderListModel(int? orderStatusId = null, int? paymentStatusId = null, int? shippingStatusId = null, DateTime? startDate = null);
        (IEnumerable<OrderModel> orderModels, OrderAggreratorModel aggreratorModel, int totalCount) PrepareOrderModel(OrderListModel model, int pageIndex, int pageSize);
        void PrepareOrderDetailsModel(OrderModel model, Order order);
        OrderModel.AddOrderProductModel PrepareAddOrderProductModel(Order order);
        OrderModel.AddOrderProductModel.ProductDetailsModel PrepareAddProductToOrderModel(string orderId, string productId);
        OrderAddressModel PrepareOrderAddressModel(Order order, Address address);
        void LogEditOrder(string orderId);
        IList<OrderModel.OrderNote> PrepareOrderNotes(Order order);
        void InsertOrderNote(Order order, string downloadId, bool displayToCustomer, string message);
        void DeleteOrderNote(Order order, string id);
        Address UpdateOrderAddress(Order order, Address address, OrderAddressModel model, string customAttributes);
        IList<string> AddProductToOrderDetails(string orderId, string productId, IFormCollection form);
        void EditCreditCardInfo(Order order, OrderModel model);
        IList<Order> PrepareOrders(OrderListModel model);
    }
}
