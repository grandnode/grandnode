using Grand.Core.Routing;
using Grand.Admin.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Admin.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder routeBuilder)
        {
            //admin index
            routeBuilder.MapControllerRoute("AdminIndex", $"admin/", new { controller = "Home", action = "Index", area = Constants.AreaAdmin });

            //admin login
            routeBuilder.MapControllerRoute("AdminLogin", $"admin/login/", new { controller = "Login", action = "Index", area = Constants.AreaAdmin });

        }
        public int Priority => 0;

    }
}
