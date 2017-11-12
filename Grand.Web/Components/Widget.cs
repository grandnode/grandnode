using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using System.Linq;

namespace Grand.Web.ViewComponents
{
    public class WidgetViewComponent : ViewComponent
    {
        private readonly IWidgetWebService _widgetWebService;

        public WidgetViewComponent(IWidgetWebService widgetWebService)
        {
            this._widgetWebService = widgetWebService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData = null)
        {
            var model = _widgetWebService.PrepareRenderWidget(widgetZone, additionalData);

            if (!model.Any())
                return Content("");

            return View(model);
        }
    }
}