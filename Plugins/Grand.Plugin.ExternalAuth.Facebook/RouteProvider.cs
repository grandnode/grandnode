using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Plugin.ExternalAuth.Facebook
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute("Plugin.ExternalAuth.Facebook.SignInFacebook",
                 "signin-failed",
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
