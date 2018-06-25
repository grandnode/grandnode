
namespace Grand.Core.Plugins
{
    /// <summary>
    /// Plugin uploaded event
    /// </summary>
    public class PluginUploadedEvent
    {
        /// <summary>
        /// Uploaded plugin
        /// </summary>
        public PluginDescriptor UploadedPlugin { get; private set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="uploadedPlugin">Uploaded plugin</param>
        public PluginUploadedEvent(PluginDescriptor uploadedPlugin)
        {
            this.UploadedPlugin = uploadedPlugin;
        }
    }

    /// <summary>
    /// Represents the plugin updated event
    /// </summary>
    public class PluginUpdatedEvent
    {
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="plugin">Updated plugin</param>
        public PluginUpdatedEvent(PluginDescriptor plugin)
        {
            this.Plugin = plugin;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Updated plugin
        /// </summary>
        public PluginDescriptor Plugin { get; private set; }

        #endregion
    }
}