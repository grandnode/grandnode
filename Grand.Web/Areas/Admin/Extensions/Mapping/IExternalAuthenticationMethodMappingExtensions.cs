using Grand.Services.Authentication.External;
using Grand.Web.Areas.Admin.Models.ExternalAuthentication;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class IExternalAuthenticationMethodMappingExtensions
    {
        public static AuthenticationMethodModel ToModel(this IExternalAuthenticationMethod entity)
        {
            return entity.MapTo<IExternalAuthenticationMethod, AuthenticationMethodModel>();
        }
    }
}