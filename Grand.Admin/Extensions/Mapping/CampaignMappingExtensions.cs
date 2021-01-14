using Grand.Domain.Messages;
using Grand.Admin.Models.Messages;

namespace Grand.Admin.Extensions
{
    public static class CampaignMappingExtensions
    {
        public static CampaignModel ToModel(this Campaign entity)
        {
            return entity.MapTo<Campaign, CampaignModel>();
        }

        public static Campaign ToEntity(this CampaignModel model)
        {
            return model.MapTo<CampaignModel, Campaign>();
        }

        public static Campaign ToEntity(this CampaignModel model, Campaign destination)
        {
            return model.MapTo(destination);
        }
    }
}