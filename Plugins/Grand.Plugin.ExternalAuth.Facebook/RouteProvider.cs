using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Plugin.ExternalAuth.Facebook
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapControllerRoute("Plugin.ExternalAuth.Facebook.SignInFacebook",
                 "fb-signin-failed",
                 new { controller = "FacebookAuthentication", action = "SignInFailed" }
            );
        }
        public int Priority
        {
            get
            {
                return 10;
            }
        }
    }
}
