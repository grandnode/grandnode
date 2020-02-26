using Google.Authenticator;

namespace Grand.Services.Authentication
{
    public class TwoFactorAuthenticationService : ITwoFactorAuthenticationService
    {
        private TwoFactorAuthenticator _twoFactorAuthentication;
        public TwoFactorAuthenticationService()
        {
            _twoFactorAuthentication = new TwoFactorAuthenticator();
        }
        
        public bool AuthenticateTwoFactor(string secretKey, string token)
        {
            return  _twoFactorAuthentication.ValidateTwoFactorPIN(secretKey, token);
        }

        public QrCodeSetup GenerateQrCodeSetup(string secretKey)
        {
            var setupInfo = _twoFactorAuthentication.GenerateSetupCode("GrandNode", "GrandNode", secretKey, false, 3);

            return new QrCodeSetup 
            { 
                QrCodeImageUrl = setupInfo.QrCodeSetupImageUrl, 
                ManualEntryQrCode = setupInfo.ManualEntryKey
            }; 
        }
        
    }

    public class QrCodeSetup
    {
        public string QrCodeImageUrl { get; set; }
        public string ManualEntryQrCode { get; set; }
    }
}
