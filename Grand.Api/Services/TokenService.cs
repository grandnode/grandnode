using Grand.Api.Infrastructure.Extensions;
using Grand.Api.Interfaces;
using Grand.Api.Jwt;
using Grand.Core.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Api.Services
{
    public partial class TokenService : ITokenService
    {
        private readonly ApiConfig _apiConfig;

        public TokenService(ApiConfig apiConfig)
        {
            _apiConfig = apiConfig;
        }

        public virtual Task<string> GenerateToken(Dictionary<string, string> claims)
        {
            var token = new JwtTokenBuilder();
            token.AddSecurityKey(JwtSecurityKey.Create(_apiConfig.SecretKey));

            if (_apiConfig.ValidateIssuer)
                token.AddIssuer(_apiConfig.ValidIssuer);
            if (_apiConfig.ValidateAudience)
                token.AddAudience(_apiConfig.ValidAudience);

            token.AddClaims(claims);
            token.AddExpiry(_apiConfig.ExpiryInMinutes);
            token.Build();

            return Task.FromResult(token.Build().Value);
        }
    }
}
