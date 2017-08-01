using System.Collections.Generic;
using Grand.Core.Domain.Messages;
using Grand.Core;

namespace Grand.Services.Messages
{
    public partial interface ICampaignService
    {
        /// <summary>
        /// Inserts a campaign
        /// </summary>
        /// <param name="campaign">Campaign</param>        
        void InsertCampaign(Campaign campaign);

        /// <summary>
        /// Inserts a campaign history
        /// </summary>
        /// <param name="campaign">Campaign</param>        
        void InsertCampaignHistory(CampaignHistory campaignhistory);
        /// <summary>
        /// Updates a campaign
        /// </summary>
        /// <param name="campaign">Campaign</param>
        void UpdateCampaign(Campaign campaign);

        /// <summary>
        /// Deleted a queued email
        /// </summary>
        /// <param name="campaign">Campaign</param>
        void DeleteCampaign(Campaign campaign);

        /// <summary>
        /// Gets a campaign by identifier
        /// </summary>
        /// <param name="campaignId">Campaign identifier</param>
        /// <returns>Campaign</returns>
        Campaign GetCampaignById(string campaignId);

        /// <summary>
        /// Gets all campaigns
        /// </summary>
        /// <returns>Campaigns</returns>
        IList<Campaign> GetAllCampaigns();

        /// <summary>
        /// Gets campaign history 
        /// </summary>
        /// <returns>Campaigns</returns>
        IPagedList<CampaignHistory> GetCampaignHistory(Campaign campaign, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets customer subscriptions
        /// </summary>
        /// <param name="Campaign">Campaign</param>
        /// <returns>CampaignCustomerSubscription</returns>
        IPagedList<NewsLetterSubscription> CustomerSubscriptions(Campaign campaign, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Sends a campaign to specified emails
        /// </summary>
        /// <param name="campaign">Campaign</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="subscriptions">Subscriptions</param>
        /// <returns>Total emails sent</returns>
        int SendCampaign(Campaign campaign, EmailAccount emailAccount,
            IEnumerable<NewsLetterSubscription> subscriptions);

        /// <summary>
        /// Sends a campaign to specified email
        /// </summary>
        /// <param name="campaign">Campaign</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="email">Email</param>
        void SendCampaign(Campaign campaign, EmailAccount emailAccount, string email);
    }
}
