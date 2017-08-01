using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;

using Grand.Framework;
using Grand.Framework.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Grand.Web.Areas.Admin.Models.Cms
{
    public partial class WidgetModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.ContentManagement.Widgets.Fields.FriendlyName")]
        
        public string FriendlyName { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Widgets.Fields.SystemName")]
        
        public string SystemName { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Widgets.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Widgets.Fields.IsActive")]
        public bool IsActive { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Widgets.Fields.Configure")]
        public string ConfigurationUrl { get; set; }
    }
}