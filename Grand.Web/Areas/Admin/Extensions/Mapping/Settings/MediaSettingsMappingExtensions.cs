using Grand.Domain.Media;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class MediaSettingsMappingExtensions
    {
        public static MediaSettingsModel ToModel(this MediaSettings entity)
        {
            return entity.MapTo<MediaSettings, MediaSettingsModel>();
        }
        public static MediaSettings ToEntity(this MediaSettingsModel model, MediaSettings destination)
        {
            return model.MapTo(destination);
        }
    }
}