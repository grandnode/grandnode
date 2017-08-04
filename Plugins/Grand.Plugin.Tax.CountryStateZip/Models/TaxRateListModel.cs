using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Plugin.Tax.CountryStateZip.Models
{
    public class TaxRateListModel : BaseGrandModel
    {
        public TaxRateListModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableTaxCategories = new List<SelectListItem>();
            TaxRates = new List<TaxRateModel>();
        }

        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Store")]
        public string AddStoreId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Country")]
        public string AddCountryId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.StateProvince")]
        public string AddStateProvinceId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Zip")]
        public string AddZip { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.TaxCategory")]
        public string AddTaxCategoryId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Percentage")]
        public decimal AddPercentage { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
        public IList<SelectListItem> AvailableTaxCategories { get; set; }

        public IList<TaxRateModel> TaxRates { get; set; }
        
    }
}