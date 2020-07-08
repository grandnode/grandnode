using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Messages;
using Grand.Services.Events;
using Grand.Services.Localization;
using Grand.Services.Stores;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    public partial class MessageTemplateService : IMessageTemplateService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
        private const string MESSAGETEMPLATES_ALL_KEY = "Grand.messagetemplate.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : template name
        /// {1} : store ID
        /// </remarks>
        private const string MESSAGETEMPLATES_BY_NAME_KEY = "Grand.messagetemplate.name-{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string MESSAGETEMPLATES_PATTERN_KEY = "Grand.messagetemplate.";

        #endregion

        #region Fields

        private readonly IRepository<MessageTemplate> _messageTemplateRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IMediator _mediator;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="messageTemplateRepository">Message template repository</param>
        /// <param name="catalogSettings">Catalog settings</param>
        /// <param name="mediator">Mediator</param>
        public MessageTemplateService(ICacheManager cacheManager,
            IStoreMappingService storeMappingService,
            IRepository<MessageTemplate> messageTemplateRepository,
            CatalogSettings catalogSettings,
            IMediator mediator)
        {
            _cacheManager = cacheManager;
            _storeMappingService = storeMappingService;
            _messageTemplateRepository = messageTemplateRepository;
            _catalogSettings = catalogSettings;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        public virtual async Task DeleteMessageTemplate(MessageTemplate messageTemplate)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException("messageTemplate");

            await _messageTemplateRepository.DeleteAsync(messageTemplate);

            await _cacheManager.RemoveByPrefix(MESSAGETEMPLATES_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(messageTemplate);
        }

        /// <summary>
        /// Inserts a message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        public virtual async Task InsertMessageTemplate(MessageTemplate messageTemplate)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException("messageTemplate");

            await _messageTemplateRepository.InsertAsync(messageTemplate);

            await _cacheManager.RemoveByPrefix(MESSAGETEMPLATES_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(messageTemplate);
        }

        /// <summary>
        /// Updates a message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        public virtual async Task UpdateMessageTemplate(MessageTemplate messageTemplate)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException("messageTemplate");

            await _messageTemplateRepository.UpdateAsync(messageTemplate);

            await _cacheManager.RemoveByPrefix(MESSAGETEMPLATES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(messageTemplate);
        }

        /// <summary>
        /// Gets a message template
        /// </summary>
        /// <param name="messageTemplateId">Message template identifier</param>
        /// <returns>Message template</returns>
        public virtual Task<MessageTemplate> GetMessageTemplateById(string messageTemplateId)
        {
            return _messageTemplateRepository.GetByIdAsync(messageTemplateId);
        }

        /// <summary>
        /// Gets a message template
        /// </summary>
        /// <param name="messageTemplateName">Message template name</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Message template</returns>
        public virtual async Task<MessageTemplate> GetMessageTemplateByName(string messageTemplateName, string storeId)
        {
            if (string.IsNullOrWhiteSpace(messageTemplateName))
                throw new ArgumentException("messageTemplateName");

            string key = string.Format(MESSAGETEMPLATES_BY_NAME_KEY, messageTemplateName, storeId);
            return await _cacheManager.GetAsync(key, async () =>
            {
                var query = _messageTemplateRepository.Table;

                query = query.Where(t => t.Name == messageTemplateName);
                query = query.OrderBy(t => t.Id);
                var templates = await query.ToListAsync();

                //store mapping
                if (!String.IsNullOrEmpty(storeId))
                {
                    templates = templates
                        .Where(t => _storeMappingService.Authorize(t, storeId))
                        .ToList();
                }

                return templates.FirstOrDefault();
            });

        }

        /// <summary>
        /// Gets all message templates
        /// </summary>
        /// <param name="storeId">Store identifier; pass "" to load all records</param>
        /// <returns>Message template list</returns>
        public virtual async Task<IList<MessageTemplate>> GetAllMessageTemplates(string storeId)
        {
            string key = string.Format(MESSAGETEMPLATES_ALL_KEY, storeId);
            return await _cacheManager.GetAsync(key, () =>
            {
                var query = _messageTemplateRepository.Table;

                query = query.OrderBy(t => t.Name);

                //Store mapping
                if (!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)
                {
                    query = from p in query
                            where !p.LimitedToStores || p.Stores.Contains(storeId)
                            select p;
                    query = query.OrderBy(t => t.Name);
                }
                return query.ToListAsync();
            });
        }

        /// <summary>
        /// Create a copy of message template with all depended data
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <returns>Message template copy</returns>
        public virtual async Task<MessageTemplate> CopyMessageTemplate(MessageTemplate messageTemplate)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException("messageTemplate");

            var mtCopy = new MessageTemplate
            {
                Name = messageTemplate.Name,
                BccEmailAddresses = messageTemplate.BccEmailAddresses,
                Subject = messageTemplate.Subject,
                Body = messageTemplate.Body,
                IsActive = messageTemplate.IsActive,
                AttachedDownloadId = messageTemplate.AttachedDownloadId,
                EmailAccountId = messageTemplate.EmailAccountId,
                LimitedToStores = messageTemplate.LimitedToStores,
                Locales = messageTemplate.Locales,
                Stores = messageTemplate.Stores,
                DelayBeforeSend = messageTemplate.DelayBeforeSend,
                DelayPeriod = messageTemplate.DelayPeriod
            };

            await InsertMessageTemplate(mtCopy);

            return mtCopy;
        }

        #endregion
    }
}
