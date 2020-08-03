using Grand.Core.Plugins;
using Grand.Web.Areas.Admin.Models.Plugins;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class PluginDescriptorMappingExtensions
    {
        public static PluginModel ToModel(this PluginDescriptor entity)
        {
            return entity.MapTo<PluginDescriptor, PluginModel>();
        }
    }
}