using Grand.Core;
using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Polls;
using Grand.Services.Customers;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Polls
{
    /// <summary>
    /// Poll service
    /// </summary>
    public partial class PollService : IPollService
    {
        #region Fields

        private readonly IRepository<Poll> _pollRepository;
        private readonly IMediator _mediator;
        private readonly IWorkContext _workContext;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        public PollService(IRepository<Poll> pollRepository, 
            IMediator mediator,
            IWorkContext workContext,
            CatalogSettings catalogSettings)
        {
            _pollRepository = pollRepository;
            _mediator = mediator;
            _workContext = workContext;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a poll
        /// </summary>
        /// <param name="pollId">The poll identifier</param>
        /// <returns>Poll</returns>
        public virtual Task<Poll> GetPollById(string pollId)
        {
            return _pollRepository.GetByIdAsync(pollId);
        }

        /// <summary>
        /// Gets a poll
        /// </summary>
        /// <param name="systemKeyword">The poll system keyword</param>
        /// <param name="languageId">Language identifier. 0 if you want to get all polls</param>
        /// <returns>Poll</returns>
        public virtual Task<Poll> GetPollBySystemKeyword(string systemKeyword, string storeId)
        {
            var query = from p in _pollRepository.Table
                        where p.SystemKeyword == systemKeyword 
                        select p;

            if (!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)
            {
                query = query.Where(b => b.Stores.Contains(storeId) || !b.LimitedToStores);
            }

            return query.FirstOrDefaultAsync();
        }
        
        /// <summary>
        /// Gets polls
        /// </summary>
        /// <param name="languageId">Language identifier. 0 if you want to get all polls</param>
        /// <param name="loadShownOnHomePageOnly">Retrieve only shown on home page polls</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Polls</returns>
        public virtual async Task<IPagedList<Poll>> GetPolls(string storeId = "", bool loadShownOnHomePageOnly = false,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = _pollRepository.Table;

            if (!showHidden)
            {
                var utcNow = DateTime.UtcNow;
                query = query.Where(p => p.Published);
                query = query.Where(p => !p.StartDateUtc.HasValue || p.StartDateUtc <= utcNow);
                query = query.Where(p => !p.EndDateUtc.HasValue || p.EndDateUtc >= utcNow);
            }
            if (!showHidden)
            {
                if (!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)
                {
                    query = query.Where(b => b.Stores.Contains(storeId) || !b.LimitedToStores);
                }
                if (!_catalogSettings.IgnoreAcl)
                {
                    //ACL (access control list)
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                    query = from p in query
                            where !p.SubjectToAcl || allowedCustomerRolesIds.Any(x => p.CustomerRoles.Contains(x))
                            select p;

                }
            }
            if (loadShownOnHomePageOnly)
            {
                query = query.Where(p => p.ShowOnHomePage);
            }
            query = query.OrderBy(p => p.DisplayOrder);
            return await PagedList<Poll>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Deletes a poll
        /// </summary>
        /// <param name="poll">The poll</param>
        public virtual async Task DeletePoll(Poll poll)
        {
            if (poll == null)
                throw new ArgumentNullException("poll");

            await _pollRepository.DeleteAsync(poll);

            //event notification
            await _mediator.EntityDeleted(poll);
        }

        /// <summary>
        /// Inserts a poll
        /// </summary>
        /// <param name="poll">Poll</param>
        public virtual async Task InsertPoll(Poll poll)
        {
            if (poll == null)
                throw new ArgumentNullException("poll");

            await _pollRepository.InsertAsync(poll);

            //event notification
            await _mediator.EntityInserted(poll);
        }

        /// <summary>
        /// Updates the poll
        /// </summary>
        /// <param name="poll">Poll</param>
        public virtual async Task UpdatePoll(Poll poll)
        {
            if (poll == null)
                throw new ArgumentNullException("poll");

            await _pollRepository.UpdateAsync(poll);

            //event notification
            await _mediator.EntityUpdated(poll);
        }
        
        

        /// <summary>
        /// Gets a value indicating whether customer already vited for this poll
        /// </summary>
        /// <param name="pollId">Poll identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>Result</returns>
        public virtual async Task<bool> AlreadyVoted(string pollId, string customerId)
        {
            if (String.IsNullOrEmpty(pollId) || String.IsNullOrEmpty(customerId))
                return false;

            var builder = Builders<Poll>.Filter;
            var filter = builder.Where(x => x.Id == pollId);
            filter = filter & builder.Where(x => x.PollAnswers.Any(y => y.PollVotingRecords.Any(z => z.CustomerId == customerId)));
            var query = await _pollRepository.Collection.Find(filter).ToListAsync();

            var result = (query.Count() > 0);            

            return result;
        }

        #endregion
    }
}
