using System.Threading.Tasks;

namespace Grand.Core.Plugins
{
    /// <summary>
    /// Interface denoting plug-in attributes that are displayed throughout 
    /// the editing interface.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        string GetConfigurationPageUrl();

        /// <summary>
        /// Gets or sets the plugin descriptor
        /// </summary>
        PluginDescriptor PluginDescriptor { get; set; }

        /// <summary>
        /// Install plugin
        /// </summary>
        Task Install();

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        Task Uninstall();
    }
}
