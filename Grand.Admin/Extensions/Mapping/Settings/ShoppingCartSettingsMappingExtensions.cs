using Grand.Domain.Orders;
using Grand.Admin.Models.Settings;

namespace Grand.Admin.Extensions
{
    public static class ShoppingCartSettingsMappingExtensions
    {
        public static ShoppingCartSettingsModel ToModel(this ShoppingCartSettings entity)
        {
            return entity.MapTo<ShoppingCartSettings, ShoppingCartSettingsModel>();
        }
        public static ShoppingCartSettings ToEntity(this ShoppingCartSettingsModel model, ShoppingCartSettings destination)
        {
            return model.MapTo(destination);
        }
    }
}