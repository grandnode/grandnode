using Grand.Services.Authentication.External;
using Grand.Admin.Models.ExternalAuthentication;

namespace Grand.Admin.Extensions
{
    public static class IExternalAuthenticationMethodMappingExtensions
    {
        public static AuthenticationMethodModel ToModel(this IExternalAuthenticationMethod entity)
        {
            return entity.MapTo<IExternalAuthenticationMethod, AuthenticationMethodModel>();
        }
    }
}