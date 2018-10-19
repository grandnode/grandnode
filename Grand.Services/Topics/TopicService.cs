using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Security;
using Grand.Core.Domain.Topics;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Stores;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Key pattern to clear cache
        /// </summary>
        private const string TOPICS_PATTERN_KEY = "Grand.topics.";

        #endregion

        #region Fields

        private readonly IRepository<Topic> _topicRepository;
        private readonly IWorkContext _workContext;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        public TopicService(IRepository<Topic> topicRepository,
            IRepository<AclRecord> aclRepository,
            IWorkContext workContext,
            IStoreMappingService storeMappingService,
            CatalogSettings catalogSettings,
            IEventPublisher eventPublisher,
            ICacheManager cacheManager)
        {
            this._topicRepository = topicRepository;
            this._aclRepository = aclRepository;
            this._workContext = workContext;
            this._storeMappingService = storeMappingService;
            this._catalogSettings = catalogSettings;
            this._eventPublisher = eventPublisher;
            this._cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        public virtual void DeleteTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Delete(topic);

            //cache
            _cacheManager.RemoveByPattern(TOPICS_PATTERN_KEY);
            //event notification
            _eventPublisher.EntityDeleted(topic);
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="topicId">The topic identifier</param>
        /// <returns>Topic</returns>
        public virtual Topic GetTopicById(string topicId)
        {
            string key = string.Format(TOPICS_BY_ID_KEY, topicId);
            return _cacheManager.Get(key, () => _topicRepository.GetById(topicId));
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="systemName">The topic system name</param>
        /// <param name="storeId">Store identifier; pass 0 to ignore filtering by store and load the first one</param>
        /// <returns>Topic</returns>
        public virtual Topic GetTopicBySystemName(string systemName, string storeId = "")
        {
            if (String.IsNullOrEmpty(systemName))
                return null;

            var query = _topicRepository.Table;
            query = query.Where(t => t.SystemName.ToLower() == systemName.ToLower());
            query = query.OrderBy(t => t.Id);
            var topics = query.ToList();
            if (!String.IsNullOrEmpty(storeId))
            {
                topics = topics.Where(x => _storeMappingService.Authorize(x, storeId)).ToList();
            }
            return topics.FirstOrDefault();
        }

        /// <summary>
        /// Gets all topics
        /// </summary>
        /// <param name="storeId">Store identifier; pass "" to load all records</param>
        /// <returns>Topics</returns>
        public virtual IList<Topic> GetAllTopics(string storeId, bool ignorAcl = false)
        {
            string key = string.Format(TOPICS_ALL_KEY, storeId, ignorAcl);
            return _cacheManager.Get(key, () =>
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



                return query.ToList();
            });
        }

        /// <summary>
        /// Inserts a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        public virtual void InsertTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Insert(topic);

            //cache
            _cacheManager.RemoveByPattern(TOPICS_PATTERN_KEY);
            //event notification
            _eventPublisher.EntityInserted(topic);
        }

        /// <summary>
        /// Updates the topic
        /// </summary>
        /// <param name="topic">Topic</param>
        public virtual void UpdateTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Update(topic);

            //cache
            _cacheManager.RemoveByPattern(TOPICS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(topic);
        }

        #endregion
    }
}
