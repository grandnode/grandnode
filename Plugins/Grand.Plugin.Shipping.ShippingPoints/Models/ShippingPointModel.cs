using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Plugin.Shipping.ShippingPoint.Validators;

namespace Grand.Plugin.Shipping.ShippingPoint.Models
{
    [Validator(typeof(ShippingPointValidator))]
    public class ShippingPointModel : BaseNopEntityModel
    {
        public ShippingPointModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Plugins.Shipping.ShippingPoint.Fields.ShippingPointName")]
        public string ShippingPointName { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ShippingPoint.Fields.Description")]
        public string Description { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ShippingPoint.Fields.OpeningHours")]
        public string OpeningHours { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ShippingPoint.Fields.PickupFee")]
        public decimal PickupFee { get; set; }


        public List<SelectListItem> AvailableStores { get; set; }
        [GrandResourceDisplayName("Plugins.Shipping.ShippingPoint.Fields.Store")]
        public string StoreId { get; set; }

        public string StoreName { get; set; }


        [GrandResourceDisplayName("Plugins.Shipping.ShippingPoint.Fields.City")]
        [AllowHtml]
        public string City { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ShippingPoint.Fields.Address1")]
        [AllowHtml]
        public string Address1 { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ShippingPoint.Fields.ZipPostalCode")]
        [AllowHtml]
        public string ZipPostalCode { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ShippingPoint.Fields.Country")]
        public string CountryId { get; set; }
        
        public IList<SelectListItem> AvailableCountries { get; set; }
    }

    
}