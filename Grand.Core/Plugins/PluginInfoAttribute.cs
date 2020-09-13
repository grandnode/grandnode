using System;

namespace Grand.Core.Plugins
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class PluginInfoAttribute : Attribute
    {
        public string Group { get; set; } = string.Empty;
        public string FriendlyName { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string SupportedVersion { get; set; }
        public string Version { get; set; }
    }
}
