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
using System.Linq;
using System.Net;
using System.Text;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidShipment : Drop
    {
        private Shipment _shipment;
        private string _languageId;
        private ICollection<LiquidShipmentItem> _shipmentItems;

        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ShippingSettings _shippingSettings;
        private readonly MessageTemplatesSettings _templatesSettings;
        private readonly CatalogSettings _catalogSettings;

        public LiquidShipment(ILocalizationService localizationService,
            IOrderService orderService,
            IStoreContext storeContext,
            IStoreService storeService,
            IProductAttributeParser productAttributeParser,
            ShippingSettings shippingSettings,
            MessageTemplatesSettings templatesSettings,
            CatalogSettings catalogSettings)
        {
            this._localizationService = localizationService;
            this._orderService = orderService;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._shippingSettings = shippingSettings;
            this._templatesSettings = templatesSettings;
            this._catalogSettings = catalogSettings;
            this._productAttributeParser = productAttributeParser;
        }

        public void SetProperties(Shipment shipment, string languageId = "")
        {
            this._shipment = shipment;
            this._languageId = languageId;

            this._shipmentItems = new List<LiquidShipmentItem>();
            foreach (var shipmentItem in shipment.ShipmentItems)
            {
                var liquidShipmentItem = new LiquidShipmentItem();
                liquidShipmentItem.SetProperties(shipmentItem, shipment, languageId);
                this._shipmentItems.Add(liquidShipmentItem);
            }
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
                return string.Format("{0}orderdetails/shipment/{1}", GetStoreUrl(order.StoreId), _shipment.Id);
            }
        }

        /// <summary>
        /// Get store URL
        /// </summary>
        /// <param name="storeId">Store identifier; Pass 0 to load URL of the current store</param>
        /// <param name="useSsl">Use SSL</param>
        /// <returns></returns>
        protected virtual string GetStoreUrl(string storeId = "", bool useSsl = false)
        {
            var store = _storeService.GetStoreById(storeId) ?? _storeContext.CurrentStore;

            if (store == null)
                throw new Exception("No store could be loaded");

            return useSsl ? store.SecureUrl : store.Url;
        }
    }
}
