using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Web.Areas.Admin.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //push notifications
            routeBuilder.MapRoute(
               "PushNotifications.Send",
               "Admin/PushNotifications/Send",
            new { controller = "PushNotifications", action = "Send" });

            routeBuilder.MapRoute(
                "PushNotifications.Messages",
                "Admin/PushNotifications/Messages",
            new { controller = "PushNotifications", action = "Messages" });

            routeBuilder.MapRoute(
               "PushNotifications.Receivers",
               "Admin/PushNotifications/Receivers",
            new { controller = "PushNotifications", action = "Receivers" });

            routeBuilder.MapRoute(
                "PushNotifications.DeleteReceiver",
                "Admin/PushNotifications/DeleteReceiver",
                new { controller = "PushNotifications", action = "DeleteReceiver" });

            routeBuilder.MapRoute(
                "PushNotifications.Configure",
                "Admin/PushNotifications/Configure",
                new { controller = "PushNotifications", action = "Configure" });

            routeBuilder.MapRoute(
                "PushNotifications.PushMessagesList",
                "Admin/PushNotifications/PushMessagesList",
            new { controller = "PushNotifications", action = "PushMessagesList" });

            routeBuilder.MapRoute(
                "PushNotifications.PushReceiversList",
                "Admin/PushNotifications/PushReceiversList",
            new { controller = "PushNotifications", action = "PushReceiversList" });

        }
        public int Priority => 0;


    }
}
