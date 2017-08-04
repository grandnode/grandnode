using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Plugin.Tax.CountryStateZip.Models
{
    public class TaxRateModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Store")]
        public string StoreId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Store")]
        public string StoreName { get; set; }

        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.TaxCategory")]
        public string TaxCategoryId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.TaxCategory")]
        public string TaxCategoryName { get; set; }

        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Country")]
        public string CountryId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Country")]
        public string CountryName { get; set; }

        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.StateProvince")]
        public string StateProvinceId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.StateProvince")]
        public string StateProvinceName { get; set; }

        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Zip")]
        public string Zip { get; set; }

        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Percentage")]
        public decimal Percentage { get; set; }
    }
}