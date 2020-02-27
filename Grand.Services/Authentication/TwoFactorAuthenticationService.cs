using Google.Authenticator;
using Grand.Core;

namespace Grand.Services.Authentication
{
    public class TwoFactorAuthenticationService : ITwoFactorAuthenticationService
    {
        private readonly IStoreContext _storeContext;

        private TwoFactorAuthenticator _twoFactorAuthentication;
        
        public TwoFactorAuthenticationService(IStoreContext storeContext)
        {
            _twoFactorAuthentication = new TwoFactorAuthenticator();
            _storeContext = storeContext;
        }
        
        public virtual bool AuthenticateTwoFactor(string secretKey, string token)
        {
            return  _twoFactorAuthentication.ValidateTwoFactorPIN(secretKey, token);
        }

        public virtual TwoFactorCodeSetup GenerateCodeSetup(string secretKey, string email)
        {
            var setupInfo = _twoFactorAuthentication.GenerateSetupCode(_storeContext.CurrentStore.CompanyName, email, secretKey, false, 3);
            var model = new TwoFactorCodeSetup();
            model.CustomValues.Add("QrCodeImageUrl", setupInfo.QrCodeSetupImageUrl);
            model.CustomValues.Add("ManualEntryQrCode", setupInfo.ManualEntryKey);
            return model;
        }
    }
}
