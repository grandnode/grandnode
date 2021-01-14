using Grand.Domain.Customers;
using Grand.Admin.Models.Settings;

namespace Grand.Admin.Extensions
{
    public static class CustomerSettingsMappingExtensions
    {
        public static CustomerUserSettingsModel.CustomerSettingsModel ToModel(this CustomerSettings entity)
        {
            return entity.MapTo<CustomerSettings, CustomerUserSettingsModel.CustomerSettingsModel>();
        }
        public static CustomerSettings ToEntity(this CustomerUserSettingsModel.CustomerSettingsModel model, CustomerSettings destination)
        {
            return model.MapTo(destination);
        }
    }
}