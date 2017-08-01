using Grand.Services.Cms;
using Grand.Web.Areas.Admin.Models.Cms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Components
{
    public class AdminWidgetViewComponent : ViewComponent
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

        public IViewComponentResult Invoke(string widgetZone)//original Action name: WidgetsByZone
        {
            //model
            var model = new List<RenderWidgetModel>();

            var widgets = _widgetService.LoadActiveWidgetsByWidgetZone(widgetZone);
            foreach (var widget in widgets)
            {
                var widgetModel = new RenderWidgetModel();


                //TODO ensure working
                widget.GetPublicViewComponent(out string viewComponentName);
                widgetModel.WidgetViewComponentName = viewComponentName;

         

                //string actionName;
                //string controllerName;
                //RouteValueDictionary routeValues;
                //widget.GetDisplayWidgetRoute(widgetZone, out actionName, out controllerName, out routeValues);
                //widgetModel.ActionName = actionName;
                //widgetModel.ControllerName = controllerName;
                //widgetModel.RouteValues = routeValues;

                model.Add(widgetModel);
            }
            return View(model);
        }

        #endregion
    }
}