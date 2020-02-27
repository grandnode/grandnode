using System.Collections.Generic;

namespace Grand.Services.Authentication
{
    public partial class TwoFactorCodeSetup
    {
        public TwoFactorCodeSetup()
        {
            CustomValues = new Dictionary<string, string>();
        }
        public IDictionary<string, string> CustomValues { get; set; }
        //public string QrCodeImageUrl { get; set; }
        //public string ManualEntryQrCode { get; set; }
    }
}
