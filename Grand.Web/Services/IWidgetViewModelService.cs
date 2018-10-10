using Grand.Web.Models.Cms;
using System.Collections.Generic;

namespace Grand.Web.Services
{
    public partial interface IWidgetViewModelService
    {
        List<RenderWidgetModel> PrepareRenderWidget(string widgetZone, object additionalData = null);
    }
}