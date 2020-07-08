using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class HasAccess
    {
        public static bool HasAccessToProduct(this IWorkContext _workContext, Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (_workContext.CurrentVendor == null)
                //not a vendor; has access
                return true;

            var vendorId = _workContext.CurrentVendor.Id;
            return product.VendorId == vendorId;
        }

        public static bool HasAccessToOrder(this IWorkContext _workContext,Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (_workContext.CurrentVendor == null)
                //not a vendor; has access
                return true;

            var vendorId = _workContext.CurrentVendor.Id;
            var hasVendorProducts = order.OrderItems.Any(orderItem => orderItem.VendorId == vendorId);
            return hasVendorProducts;
        }

        public static bool HasAccessToOrderItem(this IWorkContext _workContext, OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException("orderItem");

            if (_workContext.CurrentVendor == null)
                //not a vendor; has access
                return true;

            var vendorId = _workContext.CurrentVendor.Id;
            return orderItem.VendorId == vendorId;
        }
        public static bool HasAccessToShipment(this IWorkContext _workContext, Order order, Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            if (_workContext.CurrentVendor == null)
                //not a vendor; has access
                return true;

            var hasVendorProducts = false;
            var vendorId = _workContext.CurrentVendor.Id;
            foreach (var shipmentItem in shipment.ShipmentItems)
            {
                var orderItem = order.OrderItems.Where(x => x.Id == shipmentItem.OrderItemId).FirstOrDefault();
                if (orderItem != null)
                {
                    if (orderItem.VendorId == vendorId)
                    {
                        hasVendorProducts = true;
                        break;
                    }
                }
            }
            return hasVendorProducts;
        }
    }
}
