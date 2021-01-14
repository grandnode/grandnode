using Grand.Services.Cms;
using Grand.Admin.Models.Cms;

namespace Grand.Admin.Extensions
{
    public static class IWidgetPluginMappingExtensions
    {
        public static WidgetModel ToModel(this IWidgetPlugin entity)
        {
            return entity.MapTo<IWidgetPlugin, WidgetModel>();
        }
    }
}