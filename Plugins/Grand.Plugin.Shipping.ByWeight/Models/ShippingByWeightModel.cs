using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.Models;
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Plugin.Shipping.ByWeight.Models
{
    public class ShippingByWeightModel : BaseGrandEntityModel
    {
        public ShippingByWeightModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableShippingMethods = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Store")]
        public string StoreId { get; set; }
        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Store")]
        public string StoreName { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Warehouse")]
        public string WarehouseId { get; set; }
        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Warehouse")]
        public string WarehouseName { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Country")]
        public string CountryId { get; set; }
        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Country")]
        public string CountryName { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.StateProvince")]
        public string StateProvinceId { get; set; }
        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.StateProvince")]
        public string StateProvinceName { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Zip")]
        public string Zip { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.ShippingMethod")]
        public string ShippingMethodId { get; set; }
        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.ShippingMethod")]
        public string ShippingMethodName { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.From")]
        public decimal From { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.To")]
        public decimal To { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.AdditionalFixedCost")]
        public decimal AdditionalFixedCost { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.PercentageRateOfSubtotal")]
        public decimal PercentageRateOfSubtotal { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.RatePerWeightUnit")]
        public decimal RatePerWeightUnit { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.LowerWeightLimit")]
        public decimal LowerWeightLimit { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.DataHtml")]
        public string DataHtml { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }
        public string BaseWeightIn { get; set; }


        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
        public IList<SelectListItem> AvailableShippingMethods { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableWarehouses { get; set; }
    }
}