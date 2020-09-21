using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Models.ShoppingCart
{
    public partial class EstimateShippingModel : BaseModel
    {
        public EstimateShippingModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
        }

        public bool Enabled { get; set; }

        [GrandResourceDisplayName("ShoppingCart.EstimateShipping.Country")]
        public string CountryId { get; set; }
        [GrandResourceDisplayName("ShoppingCart.EstimateShipping.StateProvince")]
        public string StateProvinceId { get; set; }
        [GrandResourceDisplayName("ShoppingCart.EstimateShipping.ZipPostalCode")]
        public string ZipPostalCode { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
    }

    public partial class EstimateShippingResultModel : BaseModel
    {
        public EstimateShippingResultModel()
        {
            ShippingOptions = new List<ShippingOptionModel>();
            Warnings = new List<string>();
        }

        public IList<ShippingOptionModel> ShippingOptions { get; set; }

        public IList<string> Warnings { get; set; }

        #region Nested Classes

        public partial class ShippingOptionModel : BaseModel
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public string Price { get; set; }
        }

        #endregion
    }
}