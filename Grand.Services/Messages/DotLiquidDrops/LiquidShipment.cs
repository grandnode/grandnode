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
        /// Convert a collection to a HTML table
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>HTML table of products</returns>
        protected virtual string ProductListToHtmlTable(Shipment shipment, string languageId)
        {
            string result;

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            #region Products
            sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Name", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Quantity", languageId)));
            sb.AppendLine("</tr>");

            var table = shipment.ShipmentItems.ToList();
            var order = _orderService.GetOrderById(shipment.OrderId);
            var productService = EngineContext.Current.Resolve<IProductService>();
            for (int i = 0; i <= table.Count - 1; i++)
            {
                var si = table[i];
                var orderItem = order.OrderItems.Where(x => x.Id == si.OrderItemId).FirstOrDefault();
                if (orderItem == null)
                    continue;

                var product = productService.GetProductByIdIncludeArch(orderItem.ProductId);
                if (product == null)
                    continue;

                sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                //product name
                string productName = product.GetLocalized(x => x.Name, languageId);

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));
                //attributes
                if (!String.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(orderItem.AttributeDescription);
                }
                //sku
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    var sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser);
                    if (!String.IsNullOrEmpty(sku))
                    {
                        sb.AppendLine("<br />");
                        sb.AppendLine(string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), WebUtility.HtmlEncode(sku)));
                    }
                }
                sb.AppendLine("</td>");

                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", si.Quantity));

                sb.AppendLine("</tr>");
            }
            #endregion

            sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
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
