namespace Grand.Services.Authentication
{
    public interface ITwoFactorAuthenticationService
    {
        bool AuthenticateTwoFactor(string secretKey, string token);

        TwoFactorQrCodeSetup GenerateQrCodeSetup(string secretKey, string email);
        
    }
}
