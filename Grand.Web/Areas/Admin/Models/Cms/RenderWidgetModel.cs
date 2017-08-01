using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Cms
{
    public partial class RenderWidgetModel : BaseGrandModel
    {
        public string WidgetViewComponentName { get; set; }
        public object WidgetViewComponentArguments { get; set; }
    }
}