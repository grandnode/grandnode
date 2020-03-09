using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Plugin.Tax.FixedRate.Models
{
    public class FixedTaxRateModel
    {
        public string TaxCategoryId { get; set; }

        [GrandResourceDisplayName("Plugins.Tax.FixedRate.Fields.TaxCategoryName")]
        public string TaxCategoryName { get; set; }

        [GrandResourceDisplayName("Plugins.Tax.FixedRate.Fields.Rate")]
        public decimal Rate { get; set; }
    }
}