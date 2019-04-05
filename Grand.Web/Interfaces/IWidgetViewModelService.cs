using Grand.Web.Models.Cms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface IWidgetViewModelService
    {
        Task<List<RenderWidgetModel>> PrepareRenderWidget(string widgetZone, object additionalData = null);
    }
}