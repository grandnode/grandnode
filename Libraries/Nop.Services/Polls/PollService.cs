using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Polls;
using Nop.Services.Events;
using MongoDB.Driver.Linq;
using MongoDB.Driver;

namespace Nop.Services.Polls
{
    /// <summary>
    /// Poll service
    /// </summary>
    public partial class PollService : IPollService
    {
        #region Fields

        private readonly IRepository<Poll> _pollRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public PollService(IRepository<Poll> pollRepository, 
            IEventPublisher eventPublisher)
        {
            this._pollRepository = pollRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a poll
        /// </summary>
        /// <param name="pollId">The poll identifier</param>
        /// <returns>Poll</returns>
        public virtual Poll GetPollById(int pollId)
        {
            if (pollId == 0)
                return null;

            return _pollRepository.GetById(pollId);
        }

        /// <summary>
        /// Gets a poll
        /// </summary>
        /// <param name="systemKeyword">The poll system keyword</param>
        /// <param name="languageId">Language identifier. 0 if you want to get all polls</param>
        /// <returns>Poll</returns>
        public virtual Poll GetPollBySystemKeyword(string systemKeyword, int languageId)
        {
            if (String.IsNullOrWhiteSpace(systemKeyword))
                return null;

            var query = from p in _pollRepository.Table
                        where p.SystemKeyword == systemKeyword && p.LanguageId == languageId
                        select p;
            var poll = query.FirstOrDefault();
            return poll;
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
        public virtual IPagedList<Poll> GetPolls(int languageId = 0, bool loadShownOnHomePageOnly = false,
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
            if (loadShownOnHomePageOnly)
            {
                query = query.Where(p => p.ShowOnHomePage);
            }
            if (languageId > 0)
            {
                query = query.Where(p => p.LanguageId == languageId);
            }
            query = query.OrderBy(p => p.DisplayOrder);

            var polls = new PagedList<Poll>(query, pageIndex, pageSize);
            return polls;
        }

        /// <summary>
        /// Deletes a poll
        /// </summary>
        /// <param name="poll">The poll</param>
        public virtual void DeletePoll(Poll poll)
        {
            if (poll == null)
                throw new ArgumentNullException("poll");

            _pollRepository.Delete(poll);

            //event notification
            _eventPublisher.EntityDeleted(poll);
        }

        /// <summary>
        /// Inserts a poll
        /// </summary>
        /// <param name="poll">Poll</param>
        public virtual void InsertPoll(Poll poll)
        {
            if (poll == null)
                throw new ArgumentNullException("poll");

            _pollRepository.Insert(poll);

            //event notification
            _eventPublisher.EntityInserted(poll);
        }

        /// <summary>
        /// Updates the poll
        /// </summary>
        /// <param name="poll">Poll</param>
        public virtual void UpdatePoll(Poll poll)
        {
            if (poll == null)
                throw new ArgumentNullException("poll");

            _pollRepository.Update(poll);

            //event notification
            _eventPublisher.EntityUpdated(poll);
        }
        
        

        /// <summary>
        /// Gets a value indicating whether customer already vited for this poll
        /// </summary>
        /// <param name="pollId">Poll identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>Result</returns>
        public virtual bool AlreadyVoted(int pollId, int customerId)
        {
            if (pollId == 0 || customerId == 0)
                return false;

            var builder = Builders<Poll>.Filter;
            var filter = builder.Where(x => x.Id == pollId);
            filter = filter & builder.Where(x => x.PollAnswers.Any(y => y.PollVotingRecords.Any(z => z.CustomerId == customerId)));
            var query = _pollRepository.Collection.Find(filter).ToListAsync().Result;

            var result = (query.Count() > 0);            

            return result;
        }

        #endregion
    }
}
