using Grand.Web.Models.Cms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Services
{
    public partial interface IWidgetWebService
    {
        List<RenderWidgetModel> PrepareRenderWidget(string widgetZone, object additionalData = null);
    }
}