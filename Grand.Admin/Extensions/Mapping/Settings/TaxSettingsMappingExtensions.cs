using Grand.Domain.Tax;
using Grand.Admin.Models.Settings;

namespace Grand.Admin.Extensions
{
    public static class TaxSettingsMappingExtensions
    {
        public static TaxSettingsModel ToModel(this TaxSettings entity)
        {
            return entity.MapTo<TaxSettings, TaxSettingsModel>();
        }
        public static TaxSettings ToEntity(this TaxSettingsModel model, TaxSettings destination)
        {
            return model.MapTo(destination);
        }
    }
}