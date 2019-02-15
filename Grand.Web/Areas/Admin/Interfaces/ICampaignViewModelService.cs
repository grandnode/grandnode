using Grand.Core.Domain.Messages;
using Grand.Web.Areas.Admin.Models.Messages;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICampaignViewModelService
    {
        CampaignModel PrepareCampaignModel();
        CampaignModel PrepareCampaignModel(CampaignModel model);
        CampaignModel PrepareCampaignModel(Campaign campaign);
        (IEnumerable<CampaignModel> campaignModels, int totalCount) PrepareCampaignModels();
        Campaign InsertCampaignModel(CampaignModel model);
        Campaign UpdateCampaignModel(Campaign campaign, CampaignModel model);
    }
}
