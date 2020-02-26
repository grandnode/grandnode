namespace Grand.Services.Authentication
{
    public interface ITwoFactorAuthenticationService
    {
        bool AuthenticateTwoFactor(string secretKey, string token);

        QrCodeSetup GenerateQrCodeSetup(string secretKey);
        
    }
}
