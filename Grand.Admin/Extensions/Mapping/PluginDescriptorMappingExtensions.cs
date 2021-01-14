using Grand.Core.Plugins;
using Grand.Admin.Models.Plugins;

namespace Grand.Admin.Extensions
{
    public static class PluginDescriptorMappingExtensions
    {
        public static PluginModel ToModel(this PluginDescriptor entity)
        {
            return entity.MapTo<PluginDescriptor, PluginModel>();
        }
    }
}