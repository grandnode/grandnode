using Grand.Framework.Components;
using Grand.Services.Cms;
using Grand.Web.Areas.Admin.Models.Cms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Components
{
    public class AdminWidgetViewComponent : BaseViewComponent
    {
        #region Fields

        private readonly IWidgetService _widgetService;
        #endregion

        #region Constructors

        public AdminWidgetViewComponent(IWidgetService widgetService)
        {
            this._widgetService = widgetService;
        }

        #endregion

        #region Invoker

        public IViewComponentResult Invoke(string widgetZone, object additionalData = null)
        {
            var model = new List<RenderWidgetModel>();

            //add widget zone to view component arguments
            additionalData = new RouteValueDictionary(additionalData)
            {
                { "widgetZone", widgetZone }
            };

            var widgets = _widgetService.LoadActiveWidgetsByWidgetZone(widgetZone);
            foreach (var widget in widgets)
            {
                widget.GetPublicViewComponent(widgetZone, out string viewComponentName);

                var widgetModel = new RenderWidgetModel
                {
                    WidgetViewComponentName = viewComponentName,
                    WidgetViewComponentArguments = additionalData
                };

                model.Add(widgetModel);
            }

            //no data?
            if (!model.Any())
                return Content("");

            return View(model);
        }

        #endregion
    }
}