using System.Threading.Tasks;

namespace Grand.Core.Plugins
{
    public abstract class BasePlugin : IPlugin
    {
        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        /// <returns></returns>
        public virtual string GetConfigurationPageUrl()
        {
            return null;
        }
        /// <summary>
        /// Gets or sets the plugin descriptor
        /// </summary>
        public virtual PluginDescriptor PluginDescriptor { get; set; }

        /// <summary>
        /// Install plugin
        /// </summary>
        public virtual async Task Install() 
        {
            await PluginManager.MarkPluginAsInstalled(PluginDescriptor.SystemName);
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public virtual async Task Uninstall() 
        {
            await PluginManager.MarkPluginAsUninstalled(PluginDescriptor.SystemName);
        }

    }
}
