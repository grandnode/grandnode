using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Plugin.Payments.PayInStore.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [AllowHtml]
        [NopResourceDisplayName("Plugins.Payment.PayInStore.DescriptionText")]
        public string DescriptionText { get; set; }

        [NopResourceDisplayName("Plugins.Payment.PayInStore.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [NopResourceDisplayName("Plugins.Payment.PayInStore.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
    }
}