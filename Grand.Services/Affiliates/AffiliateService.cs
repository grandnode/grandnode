using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Affiliates;
using Grand.Domain.Orders;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Affiliates
{
    /// <summary>
    /// Affiliate service
    /// </summary>
    public partial class AffiliateService : IAffiliateService
    {
        #region Fields

        private readonly IRepository<Affiliate> _affiliateRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="affiliateRepository">Affiliate repository</param>
        /// <param name="orderRepository">Order repository</param>
        /// <param name="mediator">Mediator</param>
        public AffiliateService(IRepository<Affiliate> affiliateRepository,
            IRepository<Order> orderRepository,
            IMediator mediator)
        {
            _affiliateRepository = affiliateRepository;
            _orderRepository = orderRepository;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets an affiliate by affiliate identifier
        /// </summary>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <returns>Affiliate</returns>
        public virtual Task<Affiliate> GetAffiliateById(string affiliateId)
        {
            return _affiliateRepository.GetByIdAsync(affiliateId);
        }

        /// <summary>
        /// Gets an affiliate by friendly url name
        /// </summary>
        /// <param name="friendlyUrlName">Friendly url name</param>
        /// <returns>Affiliate</returns>
        public virtual Task<Affiliate> GetAffiliateByFriendlyUrlName(string friendlyUrlName)
        {
            var query = from a in _affiliateRepository.Table
                        orderby a.Id
                        where a.FriendlyUrlName.ToLower().Contains(friendlyUrlName.ToLower())
                        select a;
            var affiliate = query.FirstOrDefaultAsync();
            return affiliate;
        }

        /// <summary>
        /// Marks affiliate as deleted 
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        public virtual async Task DeleteAffiliate(Affiliate affiliate)
        {
            if (affiliate == null)
                throw new ArgumentNullException("affiliate");

            affiliate.Deleted = true;
            await UpdateAffiliate(affiliate);
        }

        /// <summary>
        /// Gets all affiliates
        /// </summary>
        /// <param name="friendlyUrlName">Friendly URL name; null to load all records</param>
        /// <param name="firstName">First name; null to load all records</param>
        /// <param name="lastName">Last name; null to load all records</param>
        /// <param name="loadOnlyWithOrders">Value indicating whether to load affiliates only with orders placed (by affiliated customers)</param>
        /// <param name="ordersCreatedFromUtc">Orders created date from (UTC); null to load all records. It's used only with "loadOnlyWithOrders" parameter st to "true".</param>
        /// <param name="ordersCreatedToUtc">Orders created date to (UTC); null to load all records. It's used only with "loadOnlyWithOrders" parameter st to "true".</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Affiliates</returns>
        public virtual async Task<IPagedList<Affiliate>> GetAllAffiliates(string friendlyUrlName = null,
            string firstName = null, string lastName = null,
            bool loadOnlyWithOrders = false,
            DateTime? ordersCreatedFromUtc = null, DateTime? ordersCreatedToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue,
            bool showHidden = false)
        {
            var query = _affiliateRepository.Table;

            if (!String.IsNullOrWhiteSpace(friendlyUrlName))
                query = query.Where(a => a.FriendlyUrlName != null && a.FriendlyUrlName.ToLower().Contains(friendlyUrlName.ToLower()));
            if (!String.IsNullOrWhiteSpace(firstName))
                query = query.Where(a => a.Address.FirstName != null && a.Address.FirstName.ToLower().Contains(firstName.ToLower()));
            if (!String.IsNullOrWhiteSpace(lastName))
                query = query.Where(a => a.Address.LastName != null && a.Address.LastName.ToLower().Contains(lastName.ToLower()));
            if (!showHidden)
                query = query.Where(a => a.Active);
            query = query.Where(a => !a.Deleted);

            if (loadOnlyWithOrders)
            {
                var ordersQuery = _orderRepository.Table;
                if (ordersCreatedFromUtc.HasValue)
                    ordersQuery = ordersQuery.Where(o => ordersCreatedFromUtc.Value <= o.CreatedOnUtc);
                if (ordersCreatedToUtc.HasValue)
                    ordersQuery = ordersQuery.Where(o => ordersCreatedToUtc.Value >= o.CreatedOnUtc);
                ordersQuery = ordersQuery.Where(o => !o.Deleted);
                var affOrder = ordersQuery.Select(x => x.AffiliateId).ToList();
                query = query.Where(x => affOrder.Contains(x.Id));
            }

            query = query.OrderByDescending(a => a.Id);

            return await PagedList<Affiliate>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Inserts an affiliate
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        public virtual async Task InsertAffiliate(Affiliate affiliate)
        {
            if (affiliate == null)
                throw new ArgumentNullException("affiliate");

            await _affiliateRepository.InsertAsync(affiliate);

            //event notification
            await _mediator.EntityInserted(affiliate);
        }

        /// <summary>
        /// Updates the affiliate
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        public virtual async Task UpdateAffiliate(Affiliate affiliate)
        {
            if (affiliate == null)
                throw new ArgumentNullException("affiliate");

            await _affiliateRepository.UpdateAsync(affiliate);

            //event notification
            await _mediator.EntityUpdated(affiliate);
        }

        #endregion
    }
}