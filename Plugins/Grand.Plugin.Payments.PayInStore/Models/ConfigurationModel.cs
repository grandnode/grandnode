using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Plugin.Payments.PayInStore.Models
{
    public class ConfigurationModel : BaseModel
    {
        [GrandResourceDisplayName("Plugins.Payment.PayInStore.DescriptionText")]
        public string DescriptionText { get; set; }

        [GrandResourceDisplayName("Plugins.Payment.PayInStore.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [GrandResourceDisplayName("Plugins.Payment.PayInStore.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
    }
}