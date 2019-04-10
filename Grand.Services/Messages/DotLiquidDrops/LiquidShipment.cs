using DotLiquid;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Stores;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidShipment : Drop
    {
        private Shipment _shipment;
        private Order _order;
        private Store _store;
        private Language _language;

        private ICollection<LiquidShipmentItem> _shipmentItems;


        public LiquidShipment(Shipment shipment, Order order, Store store, Language language)
        {
            this._shipment = shipment;
            this._language = language;
            this._store = store;
            this._order = order;
            this._shipmentItems = new List<LiquidShipmentItem>();
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

        public string AdminComment
        {
            get { return _shipment.AdminComment; }
        }

        public string URLForCustomer
        {
            get
            {
                return string.Format("{0}orderdetails/shipment/{1}", (_store.SslEnabled ? _store.SecureUrl : _store.Url), _shipment.Id);
            }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}
