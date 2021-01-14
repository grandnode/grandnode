using Grand.Domain.Blogs;
using Grand.Admin.Models.Settings;

namespace Grand.Admin.Extensions
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