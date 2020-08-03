using Grand.Domain.Customers;
using Grand.Domain.Localization;
using System.Threading.Tasks;

namespace Grand.Services.Authentication
{
    public interface ISMSVerificationService
    {
        Task<bool> Authenticate(string secretKey, string token, Customer customer);
        Task<TwoFactorCodeSetup> GenerateCode(string secretKey, Customer customer, Language language);
    }
}
