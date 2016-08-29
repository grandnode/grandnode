using Grand.Web.Framework;

namespace Grand.Plugin.Tax.FixedRate.Models
{
    public class FixedTaxRateModel
    {
        public string TaxCategoryId { get; set; }

        [NopResourceDisplayName("Plugins.Tax.FixedRate.Fields.TaxCategoryName")]
        public string TaxCategoryName { get; set; }

        [NopResourceDisplayName("Plugins.Tax.FixedRate.Fields.Rate")]
        public decimal Rate { get; set; }
    }
}