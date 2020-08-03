using Grand.Domain.Blogs;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class BlogSettingsMappingExtensions
    {
        public static BlogSettingsModel ToModel(this BlogSettings entity)
        {
            return entity.MapTo<BlogSettings, BlogSettingsModel>();
        }
        public static BlogSettings ToEntity(this BlogSettingsModel model, BlogSettings destination)
        {
            return model.MapTo(destination);
        }
    }
}