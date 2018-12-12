using DotLiquid;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Orders;
using System.Collections.Generic;
using System.Linq;
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
        private string _languageId;

        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly CatalogSettings _catalogSettings;

        public LiquidShipmentItem(ShipmentItem shipmentItem, Shipment shipment, string languageId)
        {
            this._productService = EngineContext.Current.Resolve<IProductService>();
            this._orderService = EngineContext.Current.Resolve<IOrderService>();
            this._productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();
            this._catalogSettings = EngineContext.Current.Resolve<CatalogSettings>();

            this._shipmentItem = shipmentItem;
            this._languageId = languageId;
            this._shipment = shipment;
            this._order = _orderService.GetOrderById(_shipment.OrderId);
            this._orderItem = _order.OrderItems.Where(x => x.Id == _shipmentItem.OrderItemId).FirstOrDefault();
            this._product = _productService.GetProductById(_orderItem.ProductId);

            AdditionalTokens = new Dictionary<string, string>();
        }

        public bool ShowSkuOnProductDetailsPage
        {
            get
            {
                return _catalogSettings.ShowSkuOnProductDetailsPage;
            }
        }

        public string ProductName
        {
            get
            {
                string name = "";

                if (_product != null)
                    name = WebUtility.HtmlEncode(_product.GetLocalized(x => x.Name, _languageId));

                return name;
            }
        }

        public string ProductSku
        {
            get
            {
                string sku = "";

                if (_product != null)
                    sku = _product.FormatSku(_orderItem.AttributesXml, _productAttributeParser);

                return WebUtility.HtmlEncode(sku);
            }
        }

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
