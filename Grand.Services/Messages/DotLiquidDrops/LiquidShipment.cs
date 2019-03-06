using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Shipping;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using System;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidShipment : Drop
    {
        private Shipment _shipment;
        private string _languageId;
        private ICollection<LiquidShipmentItem> _shipmentItems;

        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IStoreService _storeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ShippingSettings _shippingSettings;
        private readonly MessageTemplatesSettings _templatesSettings;
        private readonly CatalogSettings _catalogSettings;

        public LiquidShipment(Shipment shipment, string languageId = "")
        {
            this._localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            this._orderService = EngineContext.Current.Resolve<IOrderService>();
            this._storeService = EngineContext.Current.Resolve<IStoreService>();
            this._productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();
            this._shippingSettings = EngineContext.Current.Resolve<ShippingSettings>();
            this._templatesSettings = EngineContext.Current.Resolve<MessageTemplatesSettings>();
            this._catalogSettings = EngineContext.Current.Resolve<CatalogSettings>();

            this._shipment = shipment;
            this._languageId = languageId;

            this._shipmentItems = new List<LiquidShipmentItem>();
            foreach (var shipmentItem in shipment.ShipmentItems)
            {
                this._shipmentItems.Add(new LiquidShipmentItem(shipmentItem, shipment, languageId));
            }
                       
            AdditionalTokens = new Dictionary<string, string>();
        }

        public ICollection<LiquidShipmentItem> ShipmentItems
        {
            get { return _shipmentItems; }
        }

        public string ShipmentNumber
        {
            get { return _shipment.ShipmentNumber.ToString(); }
        }

        public string TrackingNumber
        {
            get { return _shipment.TrackingNumber; }
        }

        public string TrackingNumberURL
        {
            get
            {
                var trackingNumberUrl = "";
                if (!String.IsNullOrEmpty(_shipment.TrackingNumber))
                {
                    //we cannot inject IShippingService into constructor because it'll cause circular references.
                    //that's why we resolve it here this way
                    var shippingService = EngineContext.Current.Resolve<IShippingService>();
                    var _order = EngineContext.Current.Resolve<IOrderService>().GetOrderById(_shipment.OrderId);
                    var srcm = shippingService.LoadShippingRateComputationMethodBySystemName(_order.ShippingRateComputationMethodSystemName);
                    if (srcm != null &&
                        srcm.PluginDescriptor.Installed &&
                        srcm.IsShippingRateComputationMethodActive(_shippingSettings))
                    {
                        var shipmentTracker = srcm.ShipmentTracker;
                        if (shipmentTracker != null)
                        {
                            trackingNumberUrl = shipmentTracker.GetUrl(_shipment.TrackingNumber);
                        }
                    }
                }

                return trackingNumberUrl;
            }
        }

        public string URLForCustomer
        {
            get
            {
                var order = EngineContext.Current.Resolve<IOrderService>().GetOrderById(_shipment.OrderId);
                return string.Format("{0}orderdetails/shipment/{1}", _storeService.GetStoreUrl(order.StoreId), _shipment.Id);
            }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}
