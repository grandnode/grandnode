using Grand.Core;
using Grand.Core.Domain.Common;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Grand.Plugin.Shipping.ShippingPoint.Domain
{
    public class ShippingPointSerializable
    {
        public string Id { get; set; }
        public string ShippingPointName { get; set; }
        public string Description { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Address1 { get; set; }
        public string ZipPostalCode { get; set; }
        public decimal PickupFee { get; set; }
        public string OpeningHours { get; set; }
        public string StoreId { get; set; }
    }
}