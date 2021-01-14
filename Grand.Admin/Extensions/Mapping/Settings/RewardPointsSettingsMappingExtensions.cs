using Grand.Domain.Customers;
using Grand.Admin.Models.Settings;

namespace Grand.Admin.Extensions
{
    public static class RewardPointsSettingsMappingExtensions
    {
        public static RewardPointsSettingsModel ToModel(this RewardPointsSettings entity)
        {
            return entity.MapTo<RewardPointsSettings, RewardPointsSettingsModel>();
        }
        public static RewardPointsSettings ToEntity(this RewardPointsSettingsModel model, RewardPointsSettings destination)
        {
            return model.MapTo(destination);
        }
    }
}