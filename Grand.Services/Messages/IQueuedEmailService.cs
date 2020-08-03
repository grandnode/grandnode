using Grand.Domain;
using Grand.Domain.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    public partial interface IQueuedEmailService
    {
        /// <summary>
        /// Inserts a queued email
        /// </summary>
        /// <param name="queuedEmail">Queued email</param>
        Task InsertQueuedEmail(QueuedEmail queuedEmail);

        /// <summary>
        /// Updates a queued email
        /// </summary>
        /// <param name="queuedEmail">Queued email</param>
        Task UpdateQueuedEmail(QueuedEmail queuedEmail);

        /// <summary>
        /// Deleted a queued email
        /// </summary>
        /// <param name="queuedEmail">Queued email</param>
        Task DeleteQueuedEmail(QueuedEmail queuedEmail);

        /// <summary>
        /// Deleted a customer emails
        /// </summary>
        /// <param name="email">email</param>
        Task DeleteCustomerEmail(string email);

        /// <summary>
        /// Gets a queued email by identifier
        /// </summary>
        /// <param name="queuedEmailId">Queued email identifier</param>
        /// <returns>Queued email</returns>
        Task<QueuedEmail> GetQueuedEmailById(string queuedEmailId);

        /// <summary>
        /// Get queued emails by identifiers
        /// </summary>
        /// <param name="queuedEmailIds">queued email identifiers</param>
        /// <returns>Queued emails</returns>
        Task<IList<QueuedEmail>> GetQueuedEmailsByIds(string[] queuedEmailIds);

        /// <summary>
        /// Search queued emails
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
        /// <returns>Queued emails</returns>
        Task<IPagedList<QueuedEmail>> SearchEmails(string fromEmail,
            string toEmail, DateTime? createdFromUtc, DateTime? createdToUtc, 
            bool loadNotSentItemsOnly, bool loadOnlyItemsToBeSent, int maxSendTries,
            bool loadNewest, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Delete all queued emails
        /// </summary>
        Task DeleteAllEmails();
    }
}
