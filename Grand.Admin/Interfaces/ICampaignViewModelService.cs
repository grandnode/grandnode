using Grand.Domain.Messages;
using Grand.Admin.Models.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Admin.Interfaces
{
    public interface ICampaignViewModelService
    {
        Task<CampaignModel> PrepareCampaignModel();
        Task<CampaignModel> PrepareCampaignModel(CampaignModel model);
        Task<CampaignModel> PrepareCampaignModel(Campaign campaign);
        Task<(IEnumerable<CampaignModel> campaignModels, int totalCount)> PrepareCampaignModels();
        Task<Campaign> InsertCampaignModel(CampaignModel model);
        Task<Campaign> UpdateCampaignModel(Campaign campaign, CampaignModel model);
    }
}
