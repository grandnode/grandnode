using Grand.Framework.Mvc.Models;

namespace Grand.Plugin.Shipping.ShippingPoint.Models
{
    public class PointModel : BaseGrandEntityModel
    {
        public string ShippingPointName { get; set; }
        public string Description { get; set; }
        public string OpeningHours { get; set; }
        public string PickupFee { get; set; }
        public string City { get; set; }
        public string Address1 { get; set; }
        public string ZipPostalCode { get; set; }
        public string CountryName { get; set; }
    }
    
}