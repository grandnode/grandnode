using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Grand.Framework.Security.Authorization
{
    public class ApiAuthorizationSchemeRequirement : IAuthorizationRequirement
    {
        public bool IsValid(IHeaderDictionary headers)
        {
            if (headers != null && headers.ContainsKey("Authorization") && headers["Authorization"].ToString().Contains(JwtBearerDefaults.AuthenticationScheme))
            {
                return true;
            }
            return false;
        }
    }
}
