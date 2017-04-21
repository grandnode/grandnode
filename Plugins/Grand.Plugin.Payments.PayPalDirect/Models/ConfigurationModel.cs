using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Plugin.Payments.PayPalDirect.Models
{
    public class ConfigurationModel : BaseGrandModel
    {
        public string ActiveStoreScopeConfiguration { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.ClientId")]
        public string ClientId { get; set; }
        public bool ClientId_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.ClientSecret")]
        public string ClientSecret { get; set; }
        public bool ClientSecret_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.WebhookId")]
        public string WebhookId { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.PassPurchasedItems")]
        public bool PassPurchasedItems { get; set; }
        public bool PassPurchasedItems_OverrideForStore { get; set; }

        public int TransactModeId { get; set; }
        public bool TransactModeId_OverrideForStore { get; set; }
        [GrandResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.TransactMode")]
        public SelectList TransactModeValues { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}