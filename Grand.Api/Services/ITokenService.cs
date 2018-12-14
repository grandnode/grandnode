using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Api.Services
{
    public partial interface ITokenService
    {
        Task<string> GenerateToken(Dictionary<string, string> claims);
    }
}
