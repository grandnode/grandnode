using Grand.Core.Domain.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    /// <summary>
    /// Message template service
    /// </summary>
    public partial interface IMessageTemplateService
    {
        /// <summary>
        /// Delete a message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        Task DeleteMessageTemplate(MessageTemplate messageTemplate);

        /// <summary>
        /// Inserts a message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        Task InsertMessageTemplate(MessageTemplate messageTemplate);

        /// <summary>
        /// Updates a message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        Task UpdateMessageTemplate(MessageTemplate messageTemplate);

        /// <summary>
        /// Gets a message template by identifier
        /// </summary>
        /// <param name="messageTemplateId">Message template identifier</param>
        /// <returns>Message template</returns>
        Task<MessageTemplate> GetMessageTemplateById(string messageTemplateId);

        /// <summary>
        /// Gets a message template by name
        /// </summary>
        /// <param name="messageTemplateName">Message template name</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Message template</returns>
        Task<MessageTemplate> GetMessageTemplateByName(string messageTemplateName, string storeId);

        /// <summary>
        /// Gets all message templates
        /// </summary>
        /// <param name="storeId">Store identifier; pass "" to load all records</param>
        /// <returns>Message template list</returns>
        Task<IList<MessageTemplate>> GetAllMessageTemplates(string storeId);

        /// <summary>
        /// Create a copy of message template with all depended data
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <returns>Message template copy</returns>
        Task<MessageTemplate> CopyMessageTemplate(MessageTemplate messageTemplate);
    }
}
