using System.Web.Routing;
using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.Cms
{
    public partial class RenderWidgetModel : BaseGrandModel
    {
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public RouteValueDictionary RouteValues { get; set; }
    }
}