using Grand.Domain.Orders;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class OrderSettingsMappingExtensions
    {
        public static OrderSettingsModel ToModel(this OrderSettings entity)
        {
            return entity.MapTo<OrderSettings, OrderSettingsModel>();
        }
        public static OrderSettings ToEntity(this OrderSettingsModel model, OrderSettings destination)
        {
            return model.MapTo(destination);
        }
    }
}