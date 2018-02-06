using System;
using System.IO;

namespace Grand.Core.Plugins
{
    public static class PluginExtensions
    {
        public static string GetLogoUrl(this PluginDescriptor pluginDescriptor, IWebHelper webHelper)
        {
            if (pluginDescriptor == null)
                throw new ArgumentNullException("pluginDescriptor");

            if (webHelper == null)
                throw new ArgumentNullException("webHelper");

            if (pluginDescriptor.OriginalAssemblyFile == null || pluginDescriptor.OriginalAssemblyFile.Directory == null)
            {
                return null;
            }

            var pluginDirectory = pluginDescriptor.OriginalAssemblyFile.Directory;
            var logoLocalPath = Path.Combine(pluginDirectory.FullName, "logo.jpg");
            if (!File.Exists(logoLocalPath))
            {
                return null;
            }

            string logoUrl = string.Format("{0}{1}/{2}/logo.jpg", webHelper.GetStoreLocation(), pluginDirectory.Parent.Name, pluginDirectory.Name);
            return logoUrl;
        }
    }
}
