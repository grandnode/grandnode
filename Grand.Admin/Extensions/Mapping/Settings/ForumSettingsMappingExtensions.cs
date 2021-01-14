using Grand.Domain.Forums;
using Grand.Admin.Models.Settings;

namespace Grand.Admin.Extensions
{
    public static class ForumSettingsMappingExtensions
    {
        public static ForumSettingsModel ToModel(this ForumSettings entity)
        {
            return entity.MapTo<ForumSettings, ForumSettingsModel>();
        }
        public static ForumSettings ToEntity(this ForumSettingsModel model, ForumSettings destination)
        {
            return model.MapTo(destination);
        }
    }
}