namespace Grand.Services.Authentication
{
    public interface ITwoFactorAuthenticationService
    {
        bool AuthenticateTwoFactor(string secretKey, string token);

        TwoFactorCodeSetup GenerateQrCodeSetup(string secretKey, string email);
        
    }
}
