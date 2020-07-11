using Grand.Domain.Configuration;

namespace Grand.Plugin.Payments.BrainTree
{
    public class BrainTreePaymentSettings : ISettings
    {
        public bool Use3DS { get; set; }
        public bool UseSandBox { get; set; }
        public string MerchantId { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFeePercentage { get; set; }
    }
}
