using Grand.Core.Domain.Messages;
using Grand.Web.Areas.Admin.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
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
