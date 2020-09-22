using Grand.Core.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Web.Areas.Admin.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder routeBuilder)
        {
            //admin index
            routeBuilder.MapControllerRoute("AdminIndex", $"admin/", new { controller = "Home", action = "Index", area = "Admin" });

            //admin login
            routeBuilder.MapControllerRoute("AdminLogin", $"admin/login/", new { controller = "Login", action = "Index", area = "Admin" });

        }
        public int Priority => 0;

    }
}
