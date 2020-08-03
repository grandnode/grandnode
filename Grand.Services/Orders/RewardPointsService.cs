using Grand.Domain.Data;
using Grand.Domain.Customers;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    /// <summary>
    /// RewardPoints service interface
    /// </summary>
    public partial class RewardPointsService: IRewardPointsService
    {
        #region Fields

        private readonly IRepository<RewardPointsHistory> _rphRepository;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="rphRepository">RewardPointsHistory repository</param>
        /// <param name="rewardPointsSettings">Reward points settings</param>
        /// <param name="mediator">Mediator</param>
        public RewardPointsService(IRepository<RewardPointsHistory> rphRepository,
            RewardPointsSettings rewardPointsSettings,
            IMediator mediator)
        {
            _rphRepository = rphRepository;
            _rewardPointsSettings = rewardPointsSettings;
            _mediator = mediator;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Get reward points for customer
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <returns>PointsBalance</returns>

        public virtual async Task<int> GetRewardPointsBalance(string customerId, string storeId)
        {
            var query = _rphRepository.Table;
            if (!String.IsNullOrEmpty(customerId))
                query = query.Where(rph => rph.CustomerId == customerId);
            if (!_rewardPointsSettings.PointsAccumulatedForAllStores)
                query = query.Where(rph => rph.StoreId == storeId);
            query = query.OrderByDescending(rph => rph.CreatedOnUtc).ThenByDescending(rph => rph.Id);

            var lastRph = await query.FirstOrDefaultAsync();
            return lastRph != null ? lastRph.PointsBalance : 0;

        }

        /// <summary>
        /// Add reward points
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="points">Points</param>
        /// <param name="message">Message</param>
        /// <param name="usedWithOrderId">Used with OrderId</param>
        /// <param name="usedAmount">Used amount</param>
        /// <returns>RewardPointsHistory</returns>

        public virtual async Task<RewardPointsHistory> AddRewardPointsHistory(string customerId, int points, string storeId,  string message = "",
           string usedWithOrderId = "", decimal usedAmount = 0M)
        {

            var rewardPointsHistory = new RewardPointsHistory
            {
                CustomerId = customerId,
                UsedWithOrderId = usedWithOrderId,
                Points = points,
                PointsBalance = await GetRewardPointsBalance(customerId, storeId) + points,
                UsedAmount = usedAmount,
                Message = message,
                StoreId = storeId,
                CreatedOnUtc = DateTime.UtcNow
            };
            await _rphRepository.InsertAsync(rewardPointsHistory);

            //event notification
            await _mediator.EntityInserted(rewardPointsHistory);

            return rewardPointsHistory;
        }

        public virtual async Task<IList<RewardPointsHistory>> GetRewardPointsHistory(string customerId = "", string storeId = "", bool showHidden = false)
        {
            var query = _rphRepository.Table;
            if (!string.IsNullOrEmpty(customerId))
                query = query.Where(rph => rph.CustomerId == customerId);
            if (!showHidden && !_rewardPointsSettings.PointsAccumulatedForAllStores)
            {
                //filter by store
                if(!string.IsNullOrEmpty(storeId))
                    query = query.Where(rph => rph.StoreId == storeId);
            }
            query = query.OrderByDescending(rph => rph.CreatedOnUtc).ThenByDescending(rph => rph.Id);

            return await query.ToListAsync();
        }

        #endregion
    }
}
