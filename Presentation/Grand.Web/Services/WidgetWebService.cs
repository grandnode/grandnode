using Grand.Core;
using Grand.Core.Caching;
using Grand.Services.Cms;
using Grand.Web.Framework.Themes;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Cms;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace Grand.Web.Services
{
    public partial class WidgetWebService: IWidgetWebService
    {
        private readonly IStoreContext _storeContext;
        private readonly ICacheManager _cacheManager;
        private readonly IWidgetService _widgetService;
        private readonly IThemeContext _themeContext;

        public WidgetWebService(IStoreContext storeContext, ICacheManager cacheManager,
            IWidgetService widgetService, IThemeContext themeContext)
        {
            this._storeContext = storeContext;
            this._cacheManager = cacheManager;
            this._widgetService = widgetService;
            this._themeContext = themeContext;
        }

        public virtual List<RenderWidgetModel> PrepareRenderWidget(string widgetZone, object additionalData = null)
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.WIDGET_MODEL_KEY, _storeContext.CurrentStore.Id, widgetZone, _themeContext.WorkingThemeName);
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                //model
                var model = new List<RenderWidgetModel>();

                var widgets = _widgetService.LoadActiveWidgetsByWidgetZone(widgetZone, _storeContext.CurrentStore.Id);
                foreach (var widget in widgets)
                {
                    var widgetModel = new RenderWidgetModel();

                    string actionName;
                    string controllerName;
                    RouteValueDictionary routeValues;
                    widget.GetDisplayWidgetRoute(widgetZone, out actionName, out controllerName, out routeValues);
                    widgetModel.ActionName = actionName;
                    widgetModel.ControllerName = controllerName;
                    widgetModel.RouteValues = routeValues;

                    model.Add(widgetModel);
                }
                return model;
            });

            //no data?
            if (!cacheModel.Any())
                return null;
                //return Content("");

            //"RouteValues" property of widget models depends on "additionalData".
            //We need to clone the cached model before modifications (the updated one should not be cached)
            var clonedModel = new List<RenderWidgetModel>();
            foreach (var widgetModel in cacheModel)
            {
                var clonedWidgetModel = new RenderWidgetModel();
                clonedWidgetModel.ActionName = widgetModel.ActionName;
                clonedWidgetModel.ControllerName = widgetModel.ControllerName;
                if (widgetModel.RouteValues != null)
                    clonedWidgetModel.RouteValues = new RouteValueDictionary(widgetModel.RouteValues);

                if (additionalData != null)
                {
                    if (clonedWidgetModel.RouteValues == null)
                        clonedWidgetModel.RouteValues = new RouteValueDictionary();
                    clonedWidgetModel.RouteValues.Add("additionalData", additionalData);
                }

                clonedModel.Add(clonedWidgetModel);
            }
            return cacheModel;
        }
    }
}