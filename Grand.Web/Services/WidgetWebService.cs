using Grand.Core;
using Grand.Core.Caching;
using Grand.Services.Cms;
using Grand.Framework.Themes;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Cms;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

namespace Grand.Web.Services
{
    public partial class WidgetWebService : IWidgetWebService
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
            var cacheKey = string.Format(ModelCacheEventConsumer.WIDGET_MODEL_KEY,
                _storeContext.CurrentStore.Id, widgetZone, _themeContext.WorkingThemeName);

            //add widget zone to view component arguments
            additionalData = new RouteValueDictionary(additionalData)
            {
                { "widgetZone", widgetZone },
                { "additionalData", additionalData}
            };

            var cachedModel = _cacheManager.Get(cacheKey, () =>
            {
                //model
                var model = new List<RenderWidgetModel>();

                var widgets = _widgetService.LoadActiveWidgetsByWidgetZone(widgetZone, _storeContext.CurrentStore.Id);
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
                return model;
            });

            //"WidgetViewComponentArguments" property of widget models depends on "additionalData".
            //We need to clone the cached model before modifications (the updated one should not be cached)
            var clonedModel = new List<RenderWidgetModel>();

            foreach (var widgetModel in cachedModel)
            {
                var clonedWidgetModel = new RenderWidgetModel
                {
                    WidgetViewComponentName = widgetModel.WidgetViewComponentName
                };

                if (widgetModel.WidgetViewComponentArguments != null)
                    clonedWidgetModel.WidgetViewComponentArguments = new RouteValueDictionary(widgetModel.WidgetViewComponentArguments);

                if (additionalData != null)
                {
                    if (clonedWidgetModel.WidgetViewComponentArguments == null)
                        clonedWidgetModel.WidgetViewComponentArguments = new RouteValueDictionary();

                    clonedWidgetModel.WidgetViewComponentArguments = additionalData;
                }

                clonedModel.Add(clonedWidgetModel);
            }

            return clonedModel;
        }
    }
}