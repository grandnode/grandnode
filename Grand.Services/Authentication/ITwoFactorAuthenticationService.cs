using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using System.Threading.Tasks;

namespace Grand.Services.Authentication
{
    public interface ITwoFactorAuthenticationService
    {
        Task<bool> AuthenticateTwoFactor(string secretKey, string token, Customer customer, TwoFactorAuthenticationType twoFactorAuthenticationType);

        Task<TwoFactorCodeSetup> GenerateCodeSetup(string secretKey, Customer customer, Language language, TwoFactorAuthenticationType twoFactorAuthenticationType);
        
    }
}
