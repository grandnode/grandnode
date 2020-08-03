using Grand.Core;
using Grand.Domain.Data;
using Grand.Domain.Messages;
using Grand.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MediatR;
using Grand.Core.Caching;

namespace Grand.Services.Messages
{
    public partial class EmailAccountService : IEmailAccountService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : email account ID
        /// </remarks>
        private const string EMAILACCOUNT_BY_ID_KEY = "Grand.emailaccount.id-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// </remarks>
        private const string EMAILACCOUNT_ALL_KEY = "Grand.emailaccount.all";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string EMAILACCOUNT_PATTERN_KEY = "Grand.emailaccount.";

        #endregion

        private readonly IRepository<EmailAccount> _emailAccountRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IMediator _mediator;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="emailAccountRepository">Email account repository</param>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="mediator">Mediator</param>
        public EmailAccountService(
            IRepository<EmailAccount> emailAccountRepository,
            ICacheManager cacheManager,
            IMediator mediator)
        {
            _emailAccountRepository = emailAccountRepository;
            _cacheManager = cacheManager;
            _mediator = mediator;
        }

        /// <summary>
        /// Inserts an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        public virtual async Task InsertEmailAccount(EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            emailAccount.Email = CommonHelper.EnsureNotNull(emailAccount.Email);
            emailAccount.DisplayName = CommonHelper.EnsureNotNull(emailAccount.DisplayName);
            emailAccount.Host = CommonHelper.EnsureNotNull(emailAccount.Host);
            emailAccount.Username = CommonHelper.EnsureNotNull(emailAccount.Username);
            emailAccount.Password = CommonHelper.EnsureNotNull(emailAccount.Password);

            emailAccount.Email = emailAccount.Email.Trim();
            emailAccount.DisplayName = emailAccount.DisplayName.Trim();
            emailAccount.Host = emailAccount.Host.Trim();
            emailAccount.Username = emailAccount.Username.Trim();
            emailAccount.Password = emailAccount.Password.Trim();

            emailAccount.Email = CommonHelper.EnsureMaximumLength(emailAccount.Email, 255);
            emailAccount.DisplayName = CommonHelper.EnsureMaximumLength(emailAccount.DisplayName, 255);
            emailAccount.Host = CommonHelper.EnsureMaximumLength(emailAccount.Host, 255);
            emailAccount.Username = CommonHelper.EnsureMaximumLength(emailAccount.Username, 255);
            emailAccount.Password = CommonHelper.EnsureMaximumLength(emailAccount.Password, 255);

            await _emailAccountRepository.InsertAsync(emailAccount);

            //clear cache
            await _cacheManager.RemoveByPrefix(EMAILACCOUNT_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(emailAccount);
        }

        /// <summary>
        /// Updates an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        public virtual async Task UpdateEmailAccount(EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            emailAccount.Email = CommonHelper.EnsureNotNull(emailAccount.Email);
            emailAccount.DisplayName = CommonHelper.EnsureNotNull(emailAccount.DisplayName);
            emailAccount.Host = CommonHelper.EnsureNotNull(emailAccount.Host);
            emailAccount.Username = CommonHelper.EnsureNotNull(emailAccount.Username);
            emailAccount.Password = CommonHelper.EnsureNotNull(emailAccount.Password);

            emailAccount.Email = emailAccount.Email.Trim();
            emailAccount.DisplayName = emailAccount.DisplayName.Trim();
            emailAccount.Host = emailAccount.Host.Trim();
            emailAccount.Username = emailAccount.Username.Trim();
            emailAccount.Password = emailAccount.Password.Trim();

            emailAccount.Email = CommonHelper.EnsureMaximumLength(emailAccount.Email, 255);
            emailAccount.DisplayName = CommonHelper.EnsureMaximumLength(emailAccount.DisplayName, 255);
            emailAccount.Host = CommonHelper.EnsureMaximumLength(emailAccount.Host, 255);
            emailAccount.Username = CommonHelper.EnsureMaximumLength(emailAccount.Username, 255);
            emailAccount.Password = CommonHelper.EnsureMaximumLength(emailAccount.Password, 255);

            await _emailAccountRepository.UpdateAsync(emailAccount);

            //clear cache
            await _cacheManager.RemoveByPrefix(EMAILACCOUNT_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(emailAccount);
        }

        /// <summary>
        /// Deletes an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        public virtual async Task DeleteEmailAccount(EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");
            var emailAccounts = await GetAllEmailAccounts();
            if (emailAccounts.Count == 1)
                throw new GrandException("You cannot delete this email account. At least one account is required.");

            await _emailAccountRepository.DeleteAsync(emailAccount);

            //clear cache
            await _cacheManager.RemoveByPrefix(EMAILACCOUNT_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(emailAccount);
        }

        /// <summary>
        /// Gets an email account by identifier
        /// </summary>
        /// <param name="emailAccountId">The email account identifier</param>
        /// <returns>Email account</returns>
        public virtual async Task<EmailAccount> GetEmailAccountById(string emailAccountId)
        {
            string key = string.Format(EMAILACCOUNT_BY_ID_KEY, emailAccountId);
            return await _cacheManager.GetAsync(key, () =>
            {
                return _emailAccountRepository.GetByIdAsync(emailAccountId);
            });

        }

        /// <summary>
        /// Gets all email accounts
        /// </summary>
        /// <returns>Email accounts list</returns>
        public virtual async Task<IList<EmailAccount>> GetAllEmailAccounts()
        {
            return await _cacheManager.GetAsync(EMAILACCOUNT_ALL_KEY, () =>
            {
                var query = from ea in _emailAccountRepository.Table
                            orderby ea.Id
                            select ea;
                return query.ToListAsync();
            });
        }
    }
}
