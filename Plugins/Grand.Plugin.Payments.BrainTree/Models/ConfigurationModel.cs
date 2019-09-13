using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Plugin.Payments.BrainTree.Models
{
    public class ConfigurationModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Plugins.Payments.BrainTree.Fields.Use3DS")]
        public bool Use3DS { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.BrainTree.Fields.UseSandbox")]
        public bool UseSandBox { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.BrainTree.Fields.MerchantId")]
        public string MerchantId { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.BrainTree.Fields.PublicKey")]
        public string PublicKey { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.BrainTree.Fields.PrivateKey")]
        public string PrivateKey { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.BrainTree.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.BrainTree.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
    }
}