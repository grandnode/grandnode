namespace Grand.Services.Authentication
{
    public partial class TwoFactorQrCodeSetup
    {
        public string QrCodeImageUrl { get; set; }
        public string ManualEntryQrCode { get; set; }
    }
}
