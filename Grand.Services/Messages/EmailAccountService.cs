using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Messages;
using Grand.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MediatR;

namespace Grand.Services.Messages
{
    public partial class EmailAccountService : IEmailAccountService
    {
        private readonly IRepository<EmailAccount> _emailAccountRepository;
        private readonly IMediator _mediator;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="emailAccountRepository">Email account repository</param>
        /// <param name="mediator">Mediator</param>
        public EmailAccountService(IRepository<EmailAccount> emailAccountRepository,
            IMediator mediator)
        {
            _emailAccountRepository = emailAccountRepository;
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

            //event notification
            await _mediator.EntityDeleted(emailAccount);
        }

        /// <summary>
        /// Gets an email account by identifier
        /// </summary>
        /// <param name="emailAccountId">The email account identifier</param>
        /// <returns>Email account</returns>
        public virtual Task<EmailAccount> GetEmailAccountById(string emailAccountId)
        {
            return _emailAccountRepository.GetByIdAsync(emailAccountId);
        }

        /// <summary>
        /// Gets all email accounts
        /// </summary>
        /// <returns>Email accounts list</returns>
        public virtual async Task<IList<EmailAccount>> GetAllEmailAccounts()
        {
            var query = from ea in _emailAccountRepository.Table
                        orderby ea.Id
                        select ea;
            return await query.ToListAsync();
        }
    }
}
