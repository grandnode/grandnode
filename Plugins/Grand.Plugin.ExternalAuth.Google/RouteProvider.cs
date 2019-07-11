using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Plugin.ExternalAuth.Google
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute("Plugin.ExternalAuth.Google.SignInGoogle",
                 "google-signin-failed",
                 new { controller = "ExternalAuthGoogle", action = "SignInFailed" }
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
