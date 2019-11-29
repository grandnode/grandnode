using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class WidgetViewComponent : BaseViewComponent
    {
        private readonly IWidgetViewModelService _widgetViewModelService;

        public WidgetViewComponent(IWidgetViewModelService widgetViewModelService)
        {
            _widgetViewModelService = widgetViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData = null)
        {
            var model = await _widgetViewModelService.PrepareRenderWidget(widgetZone, additionalData);
            if (!model.Any())
                return Content("");

            return View(model);
        }
    }
}