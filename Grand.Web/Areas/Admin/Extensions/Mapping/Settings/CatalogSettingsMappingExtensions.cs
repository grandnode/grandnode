using Grand.Domain.Catalog;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class CatalogSettingsMappingExtensions
    {
        public static CatalogSettingsModel ToModel(this CatalogSettings entity)
        {
            return entity.MapTo<CatalogSettings, CatalogSettingsModel>();
        }
        public static CatalogSettings ToEntity(this CatalogSettingsModel model, CatalogSettings destination)
        {
            return model.MapTo(destination);
        }
    }
}