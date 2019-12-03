using Grand.Core.Caching;
using Grand.Core.Domain.Cms;
using Grand.Core.Plugins;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Security.Authorization;
using Grand.Services.Cms;
using Grand.Services.Configuration;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Cms;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Widgets)]
    public partial class WidgetController : BaseAdminController
	{
		#region Fields
        private readonly IWidgetService _widgetService;
        private readonly ISettingService _settingService;
	    private readonly IPluginFinder _pluginFinder;
        private readonly ICacheManager _cacheManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly WidgetSettings _widgetSettings;
        #endregion

        #region Constructors

        public WidgetController(IWidgetService widgetService,
            ISettingService settingService,
            IPluginFinder pluginFinder,
            IEnumerable<ICacheManager> cacheManager,
            IServiceProvider serviceProvider,
            WidgetSettings widgetSettings)
		{
            _widgetService = widgetService;
            _widgetSettings = widgetSettings;
            _pluginFinder = pluginFinder;
            _cacheManager = cacheManager.First(o => o.GetType() == typeof(MemoryCacheManager));
            _serviceProvider = serviceProvider;
            _settingService = settingService;
        }

		#endregion 
        
        #region Methods
        
        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            var widgetsModel = new List<WidgetModel>();
            var widgets = _widgetService.LoadAllWidgets();
            foreach (var widget in widgets)
            {
                var tmp1 = widget.ToModel();
                tmp1.IsActive = widget.IsWidgetActive(_widgetSettings);
                tmp1.ConfigurationUrl = widget.PluginDescriptor.Instance(_serviceProvider).GetConfigurationPageUrl();
                tmp1.ConfigurationUrl = widget.GetConfigurationPageUrl();
                widgetsModel.Add(tmp1);
            }
            widgetsModel = widgetsModel.ToList();
            var gridModel = new DataSourceResult
            {
                Data = widgetsModel,
                Total = widgetsModel.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> WidgetUpdate(WidgetModel model)
        {
            var widget = _widgetService.LoadWidgetBySystemName(model.SystemName);
            if (widget.IsWidgetActive(_widgetSettings))
            {
                if (!model.IsActive)
                {
                    //mark as disabled
                    _widgetSettings.ActiveWidgetSystemNames.Remove(widget.PluginDescriptor.SystemName);
                    await _settingService.SaveSetting(_widgetSettings);
                }
            }
            else
            {
                if (model.IsActive)
                {
                    //mark as active
                    _widgetSettings.ActiveWidgetSystemNames.Add(widget.PluginDescriptor.SystemName);
                    await _settingService.SaveSetting(_widgetSettings);
                }
            }
            await _cacheManager.Clear();
            var pluginDescriptor = widget.PluginDescriptor;
            //display order
            pluginDescriptor.DisplayOrder = model.DisplayOrder;
            PluginFileParser.SavePluginDescriptionFile(pluginDescriptor);
            //reset plugin cache
            _pluginFinder.ReloadPlugins();

            return new NullJsonResult();
        }

        #endregion
    }
}
