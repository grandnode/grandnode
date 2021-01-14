using Grand.Domain.Catalog;
using Grand.Admin.Models.Settings;

namespace Grand.Admin.Extensions
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