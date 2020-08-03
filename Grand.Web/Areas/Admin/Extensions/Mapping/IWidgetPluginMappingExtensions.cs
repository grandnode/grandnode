using Grand.Services.Cms;
using Grand.Web.Areas.Admin.Models.Cms;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class IWidgetPluginMappingExtensions
    {
        public static WidgetModel ToModel(this IWidgetPlugin entity)
        {
            return entity.MapTo<IWidgetPlugin, WidgetModel>();
        }
    }
}