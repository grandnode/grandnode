using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IShipmentViewModelService
    {
        ShipmentModel PrepareShipmentModel(Shipment shipment, bool prepareProducts, bool prepareShipmentEvent = false);
        int GetStockQty(Product product, string warehouseId);
        int GetPlannedQty(Product product, string warehouseId);
        int GetReservedQty(Product product, string warehouseId);
        void LogShipment(string shipmentId, string message);
        ShipmentListModel PrepareShipmentListModel();
        ShipmentModel PrepareShipmentModel(Order order);
        (Shipment shipment, decimal? totalWeight) PrepareShipment(Order order, IList<OrderItem> orderItems, IFormCollection form);
        (IEnumerable<Shipment> shipments, int totalCount) PrepareShipments(ShipmentListModel model, int pageIndex, int pageSize);
    }
}
