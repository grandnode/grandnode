using System.Web.Mvc;
using System.Web.Routing;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Cms
{
    public partial class WidgetModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.ContentManagement.Widgets.Fields.FriendlyName")]
        [AllowHtml]
        public string FriendlyName { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Widgets.Fields.SystemName")]
        [AllowHtml]
        public string SystemName { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Widgets.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Widgets.Fields.IsActive")]
        public bool IsActive { get; set; }
        

        public string ConfigurationActionName { get; set; }
        public string ConfigurationControllerName { get; set; }
        public RouteValueDictionary ConfigurationRouteValues { get; set; }
    }
}