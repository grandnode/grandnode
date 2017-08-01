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
        public virtual void Install() 
        {
            PluginManager.MarkPluginAsInstalled(this.PluginDescriptor.SystemName);
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public virtual void Uninstall() 
        {
            PluginManager.MarkPluginAsUninstalled(this.PluginDescriptor.SystemName);
        }

    }
}
