using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Orders;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IOrderViewModelService
    {
        Task<OrderListModel> PrepareOrderListModel(int? orderStatusId = null, int? paymentStatusId = null, int? shippingStatusId = null, DateTime? startDate = null, string storeId = null, string code = null);
        Task<(IEnumerable<OrderModel> orderModels, OrderAggreratorModel aggreratorModel, int totalCount)> PrepareOrderModel(OrderListModel model, int pageIndex, int pageSize);
        Task PrepareOrderDetailsModel(OrderModel model, Order order);
        Task<OrderModel.AddOrderProductModel> PrepareAddOrderProductModel(Order order);
        Task<OrderModel.AddOrderProductModel.ProductDetailsModel> PrepareAddProductToOrderModel(Order order, string productId);
        Task<OrderAddressModel> PrepareOrderAddressModel(Order order, Address address);
        Task LogEditOrder(string orderId);
        Task<IList<OrderModel.OrderNote>> PrepareOrderNotes(Order order);
        Task InsertOrderNote(Order order, string downloadId, bool displayToCustomer, string message);
        Task DeleteOrderNote(Order order, string id);
        Task<Address> UpdateOrderAddress(Order order, Address address, OrderAddressModel model, string customAttributes);
        Task<IList<string>> AddProductToOrderDetails(string orderId, string productId, IFormCollection form);
        Task EditCreditCardInfo(Order order, OrderModel model);
        Task<IList<Order>> PrepareOrders(OrderListModel model);
        Task SaveOrderTags(Order order, string tags);

    }
}
