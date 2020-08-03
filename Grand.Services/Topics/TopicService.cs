using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Topics;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Stores;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Topics
{
    /// <summary>
    /// Topic service
    /// </summary>
    public partial class TopicService : ITopicService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// {1} : ignore ACL?
        /// </remarks>
        private const string TOPICS_ALL_KEY = "Grand.topics.all-{0}-{1}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : topic ID
        /// </remarks>
        private const string TOPICS_BY_ID_KEY = "Grand.topics.id-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : topic systemname
        /// {1} : store id
        /// </remarks>
        private const string TOPICS_BY_SYSTEMNAME = "Grand.topics.systemname-{0}-{1}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string TOPICS_PATTERN_KEY = "Grand.topics.";

        #endregion

        #region Fields

        private readonly IRepository<Topic> _topicRepository;
        private readonly IWorkContext _workContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IMediator _mediator;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        public TopicService(IRepository<Topic> topicRepository,
            IWorkContext workContext,
            IStoreMappingService storeMappingService,
            CatalogSettings catalogSettings,
            IMediator mediator,
            ICacheManager cacheManager)
        {
            _topicRepository = topicRepository;
            _workContext = workContext;
            _storeMappingService = storeMappingService;
            _catalogSettings = catalogSettings;
            _mediator = mediator;
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        public virtual async Task DeleteTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            await _topicRepository.DeleteAsync(topic);

            //cache
            await _cacheManager.RemoveByPrefix(TOPICS_PATTERN_KEY);
            //event notification
            await _mediator.EntityDeleted(topic);
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="topicId">The topic identifier</param>
        /// <returns>Topic</returns>
        public virtual Task<Topic> GetTopicById(string topicId)
        {
            string key = string.Format(TOPICS_BY_ID_KEY, topicId);
            return _cacheManager.GetAsync(key, () => _topicRepository.GetByIdAsync(topicId));
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="systemName">The topic system name</param>
        /// <param name="storeId">Store identifier; pass 0 to ignore filtering by store and load the first one</param>
        /// <returns>Topic</returns>
        public virtual async Task<Topic> GetTopicBySystemName(string systemName, string storeId = "")
        {
            if (string.IsNullOrEmpty(systemName))
                return null;

            string key = string.Format(TOPICS_BY_SYSTEMNAME, systemName, storeId);
            return await _cacheManager.GetAsync(key, async () =>
            {

                var query = _topicRepository.Table;
                query = query.Where(t => t.SystemName.ToLower() == systemName.ToLower());
                query = query.OrderBy(t => t.Id);
                var topics = await query.ToListAsync();
                if (!String.IsNullOrEmpty(storeId))
                {
                    topics = topics.Where(x => _storeMappingService.Authorize(x, storeId)).ToList();
                }
                return topics.FirstOrDefault();
            });
        }

        /// <summary>
        /// Gets all topics
        /// </summary>
        /// <param name="storeId">Store identifier; pass "" to load all records</param>
        /// <returns>Topics</returns>
        public virtual async Task<IList<Topic>> GetAllTopics(string storeId, bool ignorAcl = false)
        {
            string key = string.Format(TOPICS_ALL_KEY, storeId, ignorAcl);
            return await _cacheManager.GetAsync(key, () =>
            {
                var query = _topicRepository.Table;

                query = query.OrderBy(t => t.DisplayOrder).ThenBy(t => t.SystemName);

                if ((!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations) ||
                    (!ignorAcl && !_catalogSettings.IgnoreAcl))
                {
                    if (!ignorAcl && !_catalogSettings.IgnoreAcl)
                    {
                        var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                        query = from p in query
                                where !p.SubjectToAcl || allowedCustomerRolesIds.Any(x => p.CustomerRoles.Contains(x))
                                select p;
                    }
                    //Store mapping
                    if (!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)
                    {
                        query = from p in query
                                where !p.LimitedToStores || p.Stores.Contains(storeId)
                                select p;

                        query = query.OrderBy(t => t.SystemName);
                    }
                }
                return query.ToListAsync();
            });
        }

        /// <summary>
        /// Inserts a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        public virtual async Task InsertTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            await _topicRepository.InsertAsync(topic);

            //cache
            await _cacheManager.RemoveByPrefix(TOPICS_PATTERN_KEY);
            //event notification
            await _mediator.EntityInserted(topic);
        }

        /// <summary>
        /// Updates the topic
        /// </summary>
        /// <param name="topic">Topic</param>
        public virtual async Task UpdateTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            await _topicRepository.UpdateAsync(topic);

            //cache
            await _cacheManager.RemoveByPrefix(TOPICS_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(topic);
        }

        #endregion
    }
}
