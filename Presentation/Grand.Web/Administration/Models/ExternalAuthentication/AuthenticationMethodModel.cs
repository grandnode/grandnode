using System.Web.Mvc;
using System.Web.Routing;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.ExternalAuthentication
{
    public partial class AuthenticationMethodModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.Configuration.ExternalAuthenticationMethods.Fields.FriendlyName")]
        [AllowHtml]
        public string FriendlyName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.ExternalAuthenticationMethods.Fields.SystemName")]
        [AllowHtml]
        public string SystemName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.ExternalAuthenticationMethods.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.ExternalAuthenticationMethods.Fields.IsActive")]
        public bool IsActive { get; set; }



        public string ConfigurationActionName { get; set; }
        public string ConfigurationControllerName { get; set; }
        public RouteValueDictionary ConfigurationRouteValues { get; set; }
    }
}