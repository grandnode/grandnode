using DotLiquid;
using Grand.Domain.Catalog;
using Grand.Domain.Localization;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Services.Localization;
using System.Collections.Generic;
using System.Net;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidShipmentItem : Drop
    {
        private ShipmentItem _shipmentItem;
        private Shipment _shipment;
        private Product _product;
        private Order _order;
        private OrderItem _orderItem;
        private Language _language;

        public LiquidShipmentItem(ShipmentItem shipmentItem, Shipment shipment, Order order, OrderItem orderItem, Product product, Language language)
        {
            _shipmentItem = shipmentItem;
            _language = language;
            _shipment = shipment;
            _order = order;
            _orderItem = orderItem;
            _product = product;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public bool ShowSkuOnProductDetailsPage { get; set; }

        public string ProductName
        {
            get
            {
                string name = "";
                if (_product != null)
                    name = WebUtility.HtmlEncode(_product.GetLocalized(x => x.Name, _language.Id));

                return name;
            }
        }

        public string ProductSku { get; set; }

        public string AttributeDescription
        {
            get
            {
                string attDesc = "";

                if (_orderItem != null)
                    attDesc = _orderItem.AttributeDescription;

                return attDesc;
            }
        }

        public string ShipmentId
        {
            get
            {
                return _shipmentItem.ShipmentId;
            }
        }

        public string OrderItemId
        {
            get
            {
                return _shipmentItem.OrderItemId;
            }
        }

        public string ProductId
        {
            get
            {
                return _shipmentItem.ProductId;
            }
        }

        public string AttributeXML
        {
            get
            {
                return _shipmentItem.AttributeXML;
            }
        }

        public int Quantity
        {
            get
            {
                return _shipmentItem.Quantity;
            }
        }

        public string WarehouseId
        {
            get
            {
                return _shipmentItem.WarehouseId;
            }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}
