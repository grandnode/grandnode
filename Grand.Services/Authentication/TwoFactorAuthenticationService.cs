using System;
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
        
        public bool AuthenticateTwoFactor(string userUniqueKey, string token)
        {
            return  _twoFactorAuthentication.ValidateTwoFactorPIN(userUniqueKey, token);
        }

        public QrCodeSetup GenerateQrCodeSetup(string userUniqueKey)
        {
            var setupInfo = _twoFactorAuthentication.GenerateSetupCode("GrandNode", "GrandNode", userUniqueKey, false, 3);
            return new QrCodeSetup 
            { 
                QrCodeImageUrl = setupInfo.QrCodeSetupImageUrl, 
                ManualEntryQrCode = setupInfo.ManualEntryKey
            }; 
        }

        public string GenerateSecurityCode()
        {
            return Guid.NewGuid().ToString();
        }
    }

    public class QrCodeSetup
    {
        public string QrCodeImageUrl { get; set; }
        public string ManualEntryQrCode { get; set; }
    }
}
