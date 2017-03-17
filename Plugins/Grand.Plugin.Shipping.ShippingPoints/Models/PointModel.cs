using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Plugin.Shipping.ShippingPoint.Validators;

namespace Grand.Plugin.Shipping.ShippingPoint.Models
{
    public class PointModel : BaseNopEntityModel
    {
        public string ShippingPointName { get; set; }
        public string Description { get; set; }
        public string OpeningHours { get; set; }
        public string PickupFee { get; set; }
        [AllowHtml]
        public string City { get; set; }
        [AllowHtml]
        public string Address1 { get; set; }
        [AllowHtml]
        public string ZipPostalCode { get; set; }
        public string CountryName { get; set; }
    }
    
}