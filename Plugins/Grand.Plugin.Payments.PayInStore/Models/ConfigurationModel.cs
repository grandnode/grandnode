using Grand.Framework;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Plugin.Payments.PayInStore.Models
{
    public class ConfigurationModel : BaseGrandModel
    {        
        [GrandResourceDisplayName("Plugins.Payment.PayInStore.DescriptionText")]
        public string DescriptionText { get; set; }

        [GrandResourceDisplayName("Plugins.Payment.PayInStore.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [GrandResourceDisplayName("Plugins.Payment.PayInStore.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
    }
}