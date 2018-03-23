using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Core.Domain.Cms;
using Grand.Core.Plugins;
using Grand.Core.Caching;

namespace Grand.Services.Cms
{
    /// <summary>
    /// Widget service
    /// </summary>
    public partial class WidgetService : IWidgetService
    {
        #region Fields

        private readonly IPluginFinder _pluginFinder;
        private readonly ICacheManager _cacheManager;
        private readonly WidgetSettings _widgetSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="pluginFinder">Plugin finder</param>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="widgetSettings">Widget settings</param>
        public WidgetService(IPluginFinder pluginFinder, ICacheManager cacheManager,
            WidgetSettings widgetSettings)
        {
            this._pluginFinder = pluginFinder;
            this._cacheManager = cacheManager;
            this._widgetSettings = widgetSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load active widgets
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <returns>Widgets</returns>
        public virtual IList<IWidgetPlugin> LoadActiveWidgets(string storeId = "")
        {
            return LoadAllWidgets(storeId)
                   .Where(x => _widgetSettings.ActiveWidgetSystemNames.Contains(x.PluginDescriptor.SystemName, StringComparer.OrdinalIgnoreCase))
                   .ToList();
        }

        /// <summary>
        /// Load active widgets
        /// </summary>
        /// <param name="widgetZone">Widget zone</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <returns>Widgets</returns>
        public virtual IList<IWidgetPlugin> LoadActiveWidgetsByWidgetZone(string  widgetZone, string storeId = "")
        {
            if (String.IsNullOrWhiteSpace(widgetZone))
                return new List<IWidgetPlugin>();

            return LoadActiveWidgets(storeId)
                   .Where(x => x.GetWidgetZones().Contains(widgetZone, StringComparer.OrdinalIgnoreCase))
                   .ToList();
        }

        /// <summary>
        /// Load widget by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found widget</returns>
        public virtual IWidgetPlugin LoadWidgetBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IWidgetPlugin>(systemName);
            if (descriptor != null)
                return descriptor.Instance<IWidgetPlugin>();

            return null;
        }

        /// <summary>
        /// Load all widgets
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <returns>Widgets</returns>
        public virtual IList<IWidgetPlugin> LoadAllWidgets(string storeId = "")
        {
            var cacheKey = string.Format("Grand.pres.widget-store-{0}", storeId);
            return _cacheManager.Get(cacheKey, () =>
            {
                return _pluginFinder.GetPlugins<IWidgetPlugin>(storeId: storeId).ToList();
            });
        }
        
        #endregion
    }
}
