using Grand.Core.Domain.Customers;
using System.Collections.Generic;

namespace Grand.Services.Orders
{
    /// <summary>
    /// RewardPoints service interface
    /// </summary>
    public partial interface IRewardPointsService
    {

        /// <summary>
        /// Gets reward points balance
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="storeId">Store identifier; pass </param>
        /// <returns>Balance</returns>
        int GetRewardPointsBalance(string customerId, string storeId);

        /// <summary>
        /// Add reward points history record
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="points">Number of points to add</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="message">Message</param>
        /// <param name="usedWithOrder">the order for which points were redeemed as a payment</param>
        /// <param name="usedAmount">Used amount</param>
        RewardPointsHistory AddRewardPointsHistory(string customerId, int points, string storeId, string message = "",
           string usedWithOrderId = "", decimal usedAmount = 0M);

        /// <summary>
        /// Load reward point history records
        /// </summary>
        /// <param name="customerId">Customer identifier; "" to load all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records (filter by current store if possible)</param>
        /// <returns>Reward point history records</returns>
        IList<RewardPointsHistory> GetRewardPointsHistory(string customerId = "", bool showHidden = false);

    }
}
