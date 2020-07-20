using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Plugin.Shipping.ShippingPoint.Models
{
    public class ShippingPointModel : BaseGrandEntityModel
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
        public string City { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ShippingPoint.Fields.Address1")]
        public string Address1 { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ShippingPoint.Fields.ZipPostalCode")]
        public string ZipPostalCode { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ShippingPoint.Fields.Country")]
        public string CountryId { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
    }


}