using Grand.Domain.Customers;
using Grand.Domain.Localization;
using System.Threading.Tasks;

namespace Grand.Services.Authentication
{
    public interface ITwoFactorAuthenticationService
    {
        Task<bool> AuthenticateTwoFactor(string secretKey, string token, Customer customer, TwoFactorAuthenticationType twoFactorAuthenticationType);

        Task<TwoFactorCodeSetup> GenerateCodeSetup(string secretKey, Customer customer, Language language, TwoFactorAuthenticationType twoFactorAuthenticationType);
        
    }
}
