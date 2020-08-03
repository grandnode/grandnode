using Grand.Domain.News;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class NewsSettingsMappingExtensions
    {
        public static NewsSettingsModel ToModel(this NewsSettings entity)
        {
            return entity.MapTo<NewsSettings, NewsSettingsModel>();
        }
        public static NewsSettings ToEntity(this NewsSettingsModel model, NewsSettings destination)
        {
            return model.MapTo(destination);
        }
    }
}