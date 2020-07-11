using Grand.Domain.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    public partial interface IEmailAccountService
    {
        /// <summary>
        /// Inserts an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        Task InsertEmailAccount(EmailAccount emailAccount);

        /// <summary>
        /// Updates an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        Task UpdateEmailAccount(EmailAccount emailAccount);

        /// <summary>
        /// Deletes an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        Task DeleteEmailAccount(EmailAccount emailAccount);

        /// <summary>
        /// Gets an email account by identifier
        /// </summary>
        /// <param name="emailAccountId">The email account identifier</param>
        /// <returns>Email account</returns>
        Task<EmailAccount> GetEmailAccountById(string emailAccountId);

        /// <summary>
        /// Gets all email accounts
        /// </summary>
        /// <returns>Email accounts list</returns>
        Task<IList<EmailAccount>> GetAllEmailAccounts();
    }
}
