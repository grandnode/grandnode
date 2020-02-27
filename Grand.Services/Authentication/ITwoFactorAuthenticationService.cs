namespace Grand.Services.Authentication
{
    public interface ITwoFactorAuthenticationService
    {
        bool AuthenticateTwoFactor(string secretKey, string token);

        TwoFactorCodeSetup GenerateCodeSetup(string secretKey, string email);
        
    }
}
