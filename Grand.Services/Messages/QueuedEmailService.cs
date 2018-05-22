using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Messages;
using Grand.Services.Events;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Grand.Services.Messages
{
    public partial class QueuedEmailService : IQueuedEmailService
    {
        private readonly IRepository<QueuedEmail> _queuedEmailRepository;
        private readonly IDataProvider _dataProvider;
        private readonly CommonSettings _commonSettings;
        private readonly IEventPublisher _eventPublisher;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="queuedEmailRepository">Queued email repository</param>
        /// <param name="eventPublisher">Event published</param>
        /// <param name="dbContext">DB context</param>
        /// <param name="dataProvider">WeData provider</param>
        /// <param name="commonSettings">Common settings</param>
        public QueuedEmailService(IRepository<QueuedEmail> queuedEmailRepository,
            IEventPublisher eventPublisher,
            IDataProvider dataProvider, 
            CommonSettings commonSettings)
        {
            _queuedEmailRepository = queuedEmailRepository;
            _eventPublisher = eventPublisher;
            this._dataProvider = dataProvider;
            this._commonSettings = commonSettings;
        }

        /// <summary>
        /// Inserts a queued email
        /// </summary>
        /// <param name="queuedEmail">Queued email</param>        
        public virtual void InsertQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
                throw new ArgumentNullException("queuedEmail");

            _queuedEmailRepository.Insert(queuedEmail);

            //event notification
            _eventPublisher.EntityInserted(queuedEmail);
        }

        /// <summary>
        /// Updates a queued email
        /// </summary>
        /// <param name="queuedEmail">Queued email</param>
        public virtual void UpdateQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
                throw new ArgumentNullException("queuedEmail");

            _queuedEmailRepository.Update(queuedEmail);

            //event notification
            _eventPublisher.EntityUpdated(queuedEmail);
        }

        /// <summary>
        /// Deleted a queued email
        /// </summary>
        /// <param name="queuedEmail">Queued email</param>
        public virtual void DeleteQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
                throw new ArgumentNullException("queuedEmail");

            _queuedEmailRepository.Delete(queuedEmail);

            //event notification
            _eventPublisher.EntityDeleted(queuedEmail);
        }

        /// <summary>
        /// Deleted a customer emails
        /// </summary>
        /// <param name="email">email</param>
        public virtual void DeleteCustomerEmail(string email)
        {
            if (email == null)
                throw new ArgumentNullException("email");

            var builder = Builders<QueuedEmail>.Filter;
            var filter = builder.Eq(x => x.To, email);
            _queuedEmailRepository.Collection.DeleteMany(filter);
        }

        /// <summary>
        /// Gets a queued email by identifier
        /// </summary>
        /// <param name="queuedEmailId">Queued email identifier</param>
        /// <returns>Queued email</returns>
        public virtual QueuedEmail GetQueuedEmailById(string queuedEmailId)
        {
            return _queuedEmailRepository.GetById(queuedEmailId);

        }

        /// <summary>
        /// Get queued emails by identifiers
        /// </summary>
        /// <param name="queuedEmailIds">queued email identifiers</param>
        /// <returns>Queued emails</returns>
        public virtual IList<QueuedEmail> GetQueuedEmailsByIds(string[] queuedEmailIds)
        {
            if (queuedEmailIds == null || queuedEmailIds.Length == 0)
                return new List<QueuedEmail>();

            var query = from qe in _queuedEmailRepository.Table
                        where queuedEmailIds.Contains(qe.Id)
                        select qe;
            var queuedEmails = query.ToList();
            //sort by passed identifiers
            var sortedQueuedEmails = new List<QueuedEmail>();
            foreach (string id in queuedEmailIds)
            {
                var queuedEmail = queuedEmails.Find(x => x.Id == id);
                if (queuedEmail != null)
                    sortedQueuedEmails.Add(queuedEmail);
            }
            return sortedQueuedEmails;
        }

        /// <summary>
        /// Gets all queued emails
        /// </summary>
        /// <param name="fromEmail">From Email</param>
        /// <param name="toEmail">To Email</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="loadNotSentItemsOnly">A value indicating whether to load only not sent emails</param>
        /// <param name="maxSendTries">Maximum send tries</param>
        /// <param name="loadNewest">A value indicating whether we should sort queued email descending; otherwise, ascending.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Email item list</returns>
        public virtual IPagedList<QueuedEmail> SearchEmails(string fromEmail,
            string toEmail, DateTime? createdFromUtc, DateTime? createdToUtc, 
            bool loadNotSentItemsOnly, bool loadOnlyItemsToBeSent, int maxSendTries,
            bool loadNewest, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            fromEmail = (fromEmail ?? String.Empty).Trim();
            toEmail = (toEmail ?? String.Empty).Trim();

            var query = _queuedEmailRepository.Table;

            if (!String.IsNullOrEmpty(fromEmail))
                query = query.Where(qe => qe.From.ToLower().Contains(fromEmail.ToLower()));
            if (!String.IsNullOrEmpty(toEmail))
                query = query.Where(qe => qe.To.ToLower().Contains(toEmail.ToLower()));
            if (createdFromUtc.HasValue)
                query = query.Where(qe => qe.CreatedOnUtc >= createdFromUtc.Value);
            if (createdToUtc.HasValue)
                query = query.Where(qe => qe.CreatedOnUtc <= createdToUtc.Value);
            if (loadNotSentItemsOnly)
                query = query.Where(qe => !qe.SentOnUtc.HasValue);

            if (loadOnlyItemsToBeSent)
            {
                var nowUtc = DateTime.UtcNow;
                query = query.Where(qe => !qe.DontSendBeforeDateUtc.HasValue || qe.DontSendBeforeDateUtc.Value <= nowUtc);
            }

            query = query.Where(qe => qe.SentTries < maxSendTries);
            if (loadNewest)
            {
                //load the newest records
                query = query.OrderByDescending(qe => qe.CreatedOnUtc);
            }
            else
            {
                //load by priority
                query = query.OrderByDescending(qe => qe.PriorityId).ThenBy(qe => qe.CreatedOnUtc);
            }

            var queuedEmails = new PagedList<QueuedEmail>(query, pageIndex, pageSize);
            return queuedEmails;
        }

        /// <summary>
        /// Delete all queued emails
        /// </summary>
        public virtual void DeleteAllEmails()
        {
                var queuedEmails = _queuedEmailRepository.Table.ToList();
                foreach (var qe in queuedEmails)
                    _queuedEmailRepository.Delete(qe);
        }
    }
}
