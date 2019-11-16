﻿using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Messages;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Forums
{
    /// <summary>
    /// Forum service
    /// </summary>
    public partial class ForumService : IForumService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string FORUMGROUP_ALL_KEY = "Grand.forumgroup.all";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : forum group ID
        /// </remarks>
        private const string FORUM_ALLBYFORUMGROUPID_KEY = "Grand.forum.allbyforumgroupid-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string FORUMGROUP_PATTERN_KEY = "Grand.forumgroup.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string FORUM_PATTERN_KEY = "Grand.forum.";

        #endregion

        #region Fields
        private readonly IRepository<ForumGroup> _forumGroupRepository;
        private readonly IRepository<Forum> _forumRepository;
        private readonly IRepository<ForumTopic> _forumTopicRepository;
        private readonly IRepository<ForumPost> _forumPostRepository;
        private readonly IRepository<PrivateMessage> _forumPrivateMessageRepository;
        private readonly IRepository<ForumSubscription> _forumSubscriptionRepository;
        private readonly IRepository<ForumPostVote> _forumPostVoteRepository;
        private readonly ForumSettings _forumSettings;
        private readonly ICacheManager _cacheManager;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="forumGroupRepository">Forum group repository</param>
        /// <param name="forumRepository">Forum repository</param>
        /// <param name="forumTopicRepository">Forum topic repository</param>
        /// <param name="forumPostRepository">Forum post repository</param>
        /// <param name="forumPrivateMessageRepository">Private message repository</param>
        /// <param name="forumSubscriptionRepository">Forum subscription repository</param>
        /// <param name="forumSettings">Forum settings</param>
        /// <param name="genericAttributeService">Generic attribute service</param>
        /// <param name="customerService">Customer service</param>
        /// <param name="workContext">Work context</param>
        /// <param name="workflowMessageService">Workflow message service</param>
        /// <param name="mediator">Mediator</param>
        public ForumService(ICacheManager cacheManager,
            IRepository<ForumGroup> forumGroupRepository,
            IRepository<Forum> forumRepository,
            IRepository<ForumTopic> forumTopicRepository,
            IRepository<ForumPost> forumPostRepository,
            IRepository<PrivateMessage> forumPrivateMessageRepository,
            IRepository<ForumSubscription> forumSubscriptionRepository,
            IRepository<ForumPostVote> forumPostVoteRepository,
            ForumSettings forumSettings,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            IMediator mediator
            )
        {
            _cacheManager = cacheManager;
            _forumGroupRepository = forumGroupRepository;
            _forumRepository = forumRepository;
            _forumTopicRepository = forumTopicRepository;
            _forumPostRepository = forumPostRepository;
            _forumPrivateMessageRepository = forumPrivateMessageRepository;
            _forumSubscriptionRepository = forumSubscriptionRepository;
            _forumPostVoteRepository = forumPostVoteRepository;
            _forumSettings = forumSettings;
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _mediator = mediator;
        }
        #endregion

        #region Utilities

        /// <summary>
        /// Update forum stats
        /// </summary>
        /// <param name="forumId">The forum identifier</param>
        private async Task UpdateForumStats(string forumId)
        {
            var forum = await GetForumById(forumId);
            if (forum == null)
            {
                return;
            }

            //number of topics
            var queryNumTopics = from ft in _forumTopicRepository.Table
                                 where ft.ForumId == forumId
                                 select ft.Id;
            int numTopics = queryNumTopics.Count();

            //number of posts
            var queryNumPosts = from fp in _forumPostRepository.Table
                                where fp.ForumId == forumId
                                select fp.Id;
            int numPosts = queryNumPosts.Count();

            //last values
            string lastTopicId = "";
            string lastPostId = "";
            string lastPostCustomerId = "";
            DateTime? lastPostTime = null;
            var queryLastValues = from ft in _forumTopicRepository.Table
                                  where ft.ForumId == forumId
                                  orderby ft.CreatedOnUtc descending
                                  select new
                                  {
                                      LastTopicId = ft.Id,
                                  };
            var lastValues = queryLastValues.FirstOrDefault();

            var queryLastValuesPost = from ft in _forumPostRepository.Table                                      
                                      where ft.ForumId == forumId
                                      orderby ft.CreatedOnUtc descending
                                      select new
                                      {
                                          LastPostId =  ft.Id,
                                          LastPostCustomerId = ft.CustomerId,
                                          LastPostTime = ft.CreatedOnUtc
                                      };
            var lastValuesPost = queryLastValuesPost.FirstOrDefault();


            if (lastValues != null)
            {
                lastTopicId = lastValues.LastTopicId;
            }

            if (lastValuesPost != null)
            {
                lastPostId = lastValuesPost.LastPostId;
                lastPostCustomerId = lastValuesPost.LastPostCustomerId;
                lastPostTime = lastValuesPost.LastPostTime;
            }

            //update forum
            forum.NumTopics = numTopics;
            forum.NumPosts = numPosts;
            forum.LastTopicId = lastTopicId;
            forum.LastPostId = lastPostId;
            forum.LastPostCustomerId = lastPostCustomerId;
            forum.LastPostTime = lastPostTime;
            await UpdateForum(forum);
        }

        /// <summary>
        /// Update forum topic stats
        /// </summary>
        /// <param name="forumTopicId">The forum topic identifier</param>
        private async Task UpdateForumTopicStats(string forumTopicId)
        {
            var forumTopic = await GetTopicById(forumTopicId);
            if (forumTopic == null)
            {
                return;
            }

            //number of posts
            var queryNumPosts = from fp in _forumPostRepository.Table
                                where fp.TopicId == forumTopicId
                                select fp.Id;
            int numPosts = queryNumPosts.Count();

            //last values
            string lastPostId = "";
            string lastPostCustomerId = "";
            DateTime? lastPostTime = null;
            var queryLastValues = from fp in _forumPostRepository.Table
                                  where fp.TopicId == forumTopicId
                                  orderby fp.CreatedOnUtc descending
                                  select new
                                  {
                                      LastPostId = fp.Id,
                                      LastPostCustomerId = fp.CustomerId,
                                      LastPostTime = fp.CreatedOnUtc
                                  };
            var lastValues = queryLastValues.FirstOrDefault();
            if (lastValues != null)
            {
                lastPostId = lastValues.LastPostId;
                lastPostCustomerId = lastValues.LastPostCustomerId;
                lastPostTime = lastValues.LastPostTime;
            }

            //update topic
            forumTopic.NumPosts = numPosts;
            forumTopic.LastPostId = lastPostId;
            forumTopic.LastPostCustomerId = lastPostCustomerId;
            forumTopic.LastPostTime = lastPostTime;
            await UpdateTopic(forumTopic);
        }

        /// <summary>
        /// Update customer stats
        /// </summary>
        /// <param name="customerId">The customer identifier</param>
        private async Task UpdateCustomerStats(string customerId)
        {
            var customer = await _customerService.GetCustomerById(customerId);

            if (customer == null)
            {
                return;
            }

            var query = from fp in _forumPostRepository.Table
                        where fp.CustomerId == customerId
                        select fp.Id;
            int numPosts = query.Count();

            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ForumPostCount, numPosts);
        }

        private bool IsForumModerator(Customer customer)
        {
            if (customer.IsForumModerator())
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a forum group
        /// </summary>
        /// <param name="forumGroup">Forum group</param>
        public virtual async Task DeleteForumGroup(ForumGroup forumGroup)
        {
            if (forumGroup == null)
            {
                throw new ArgumentNullException("forumGroup");
            }

            await _forumGroupRepository.DeleteAsync(forumGroup);

            await _cacheManager.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(FORUM_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(forumGroup);
        }

        /// <summary>
        /// Gets a forum group
        /// </summary>
        /// <param name="forumGroupId">The forum group identifier</param>
        /// <returns>Forum group</returns>
        public virtual Task<ForumGroup> GetForumGroupById(string forumGroupId)
        {
            return _forumGroupRepository.GetByIdAsync(forumGroupId);
        }

        /// <summary>
        /// Gets all forum groups
        /// </summary>
        /// <returns>Forum groups</returns>
        public virtual async Task<IList<ForumGroup>> GetAllForumGroups()
        {
            string key = string.Format(FORUMGROUP_ALL_KEY);
            return await _cacheManager.GetAsync(key, () =>
            {
                var query = from fg in _forumGroupRepository.Table
                            orderby fg.DisplayOrder
                            select fg;
                return query.ToListAsync();
            });
        }

        /// <summary>
        /// Inserts a forum group
        /// </summary>
        /// <param name="forumGroup">Forum group</param>
        public virtual async Task InsertForumGroup(ForumGroup forumGroup)
        {
            if (forumGroup == null)
            {
                throw new ArgumentNullException("forumGroup");
            }

            await _forumGroupRepository.InsertAsync(forumGroup);

            //cache
            await _cacheManager.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(FORUM_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(forumGroup);
        }

        /// <summary>
        /// Updates the forum group
        /// </summary>
        /// <param name="forumGroup">Forum group</param>
        public virtual async Task UpdateForumGroup(ForumGroup forumGroup)
        {
            if (forumGroup == null)
            {
                throw new ArgumentNullException("forumGroup");
            }

            await _forumGroupRepository.UpdateAsync(forumGroup);

            //cache
            await _cacheManager.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(FORUM_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(forumGroup);
        }

        /// <summary>
        /// Deletes a forum
        /// </summary>
        /// <param name="forum">Forum</param>
        public virtual async Task DeleteForum(Forum forum)
        {            
            if (forum == null)
            {
                throw new ArgumentNullException("forum");
            }

            //delete forum subscriptions (topics)
            var queryTopicIds = await (from ft in _forumTopicRepository.Table
                                 where ft.ForumId == forum.Id
                                 select ft.Id).ToListAsync();

            var queryFs1 = from fs in _forumSubscriptionRepository.Table
                           where queryTopicIds.Contains(fs.TopicId)
                           select fs;

            foreach (var fs in queryFs1.ToList())
            {
                await _forumSubscriptionRepository.DeleteAsync(fs);
                //event notification
                await _mediator.EntityDeleted(fs);
            }

            //delete forum subscriptions (forum)
            var queryFs2 = from fs in _forumSubscriptionRepository.Table
                           where fs.ForumId == forum.Id
                           select fs;
            foreach (var fs2 in await queryFs2.ToListAsync())
            {
                await _forumSubscriptionRepository.DeleteAsync(fs2);
                //event notification
                await _mediator.EntityDeleted(fs2);
            }

            //delete forum
            await _forumRepository.DeleteAsync(forum);

            await _cacheManager.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(FORUM_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(forum);
        }

        /// <summary>
        /// Gets a forum
        /// </summary>
        /// <param name="forumId">The forum identifier</param>
        /// <returns>Forum</returns>
        public virtual Task<Forum> GetForumById(string forumId)
        {
            return _forumRepository.GetByIdAsync(forumId);
        }

        /// <summary>
        /// Gets forums by forum group identifier
        /// </summary>
        /// <param name="forumGroupId">The forum group identifier</param>
        /// <returns>Forums</returns>
        public virtual async Task<IList<Forum>> GetAllForumsByGroupId(string forumGroupId)
        {
            string key = string.Format(FORUM_ALLBYFORUMGROUPID_KEY, forumGroupId);
            return await _cacheManager.GetAsync(key, () =>
            {
                var query = from f in _forumRepository.Table
                            orderby f.DisplayOrder
                            where f.ForumGroupId == forumGroupId
                            select f;
                return query.ToListAsync();
            });
        }

        /// <summary>
        /// Inserts a forum
        /// </summary>
        /// <param name="forum">Forum</param>
        public virtual async Task InsertForum(Forum forum)
        {
            if (forum == null)
            {
                throw new ArgumentNullException("forum");
            }

            await _forumRepository.InsertAsync(forum);

            await _cacheManager.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(FORUM_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(forum);
        }

        /// <summary>
        /// Updates the forum
        /// </summary>
        /// <param name="forum">Forum</param>
        public virtual async Task UpdateForum(Forum forum)
        {
            if (forum == null)
            {
                throw new ArgumentNullException("forum");
            }

            await _forumRepository.UpdateAsync(forum);
            
            await _cacheManager.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(FORUM_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(forum);
        }

        /// <summary>
        /// Deletes a forum topic
        /// </summary>
        /// <param name="forumTopic">Forum topic</param>
        public virtual async Task DeleteTopic(ForumTopic forumTopic)
        {            
            if (forumTopic == null)
            {                
                throw new ArgumentNullException("forumTopic");
            }                

            string customerId = forumTopic.CustomerId;
            string forumId = forumTopic.ForumId;

            //delete topic
            await _forumTopicRepository.DeleteAsync(forumTopic);

            //delete posts
            var queryPosts = _forumPostRepository.Table.Where(x => x.TopicId == forumTopic.Id).ToList();
            foreach (var post in queryPosts)
            {
                await _forumPostRepository.DeleteAsync(post);
            }

            //delete forum subscriptions
            var queryFs = from ft in _forumSubscriptionRepository.Table
                          where ft.TopicId == forumTopic.Id
                          select ft;
            var forumSubscriptions = await queryFs.ToListAsync();
            foreach (var fs in forumSubscriptions)
            {
                await _forumSubscriptionRepository.DeleteAsync(fs);
                //event notification
                await _mediator.EntityDeleted(fs);
            }

            //update stats
            await UpdateForumStats(forumId);
            await UpdateCustomerStats(customerId);

            await _cacheManager.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(FORUM_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(forumTopic);
        }

        /// <summary>
        /// Gets a forum topic
        /// </summary>
        /// <param name="forumTopicId">The forum topic identifier</param>
        /// <returns>Forum Topic</returns>
        public virtual async Task<ForumTopic> GetTopicById(string forumTopicId)
        {
            return await GetTopicById(forumTopicId, false);
        }

        /// <summary>
        /// Gets a forum topic
        /// </summary>
        /// <param name="forumTopicId">The forum topic identifier</param>
        /// <param name="increaseViews">The value indicating whether to increase forum topic views</param>
        /// <returns>Forum Topic</returns>
        public virtual async Task<ForumTopic> GetTopicById(string forumTopicId, bool increaseViews)
        {
            var forumTopic = await _forumTopicRepository.GetByIdAsync(forumTopicId);
            if (forumTopic == null)
                return null;

            if (increaseViews)
            {
                forumTopic.Views = ++forumTopic.Views;
                await UpdateTopic(forumTopic);
            }

            return forumTopic;
        }

        /// <summary>
        /// Gets all forum topics
        /// </summary>
        /// <param name="forumId">The forum identifier</param>
        /// <param name="customerId">The customer identifier</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchType">Search type</param>
        /// <param name="limitDays">Limit by the last number days; 0 to load all topics</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Forum Topics</returns>
        public virtual async Task<IPagedList<ForumTopic>> GetAllTopics(string forumId = "",
            string customerId = "", string keywords = "", ForumSearchType searchType = ForumSearchType.All,
            int limitDays = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            DateTime? limitDate = null;
            if (limitDays > 0)
            {
                limitDate = DateTime.UtcNow.AddDays(-limitDays);
            }

            if (customerId == null)
                customerId = "";
            if (forumId == null)
                forumId = "";

            bool searchKeywords = !String.IsNullOrEmpty(keywords);
            bool searchTopicTitles = searchType == ForumSearchType.All || searchType == ForumSearchType.TopicTitlesOnly;
            bool searchPostText = searchType == ForumSearchType.All || searchType == ForumSearchType.PostTextOnly;
            if (!limitDate.HasValue)
                limitDate = DateTime.MinValue;

            List<string> topicIds = new List<string>();
            if (searchKeywords && searchPostText)
            {
                var query1 = (from fp in _forumPostRepository.Table
                              where
                              (forumId == "" || fp.ForumId == forumId) &&
                              (customerId == "" || fp.CustomerId == customerId) &&
                              (fp.Text.ToLower().Contains(keywords.ToLower()))
                              select fp.TopicId).ToList();
                topicIds.AddRange(query1);
            }

            var query2 = await (from ft in _forumTopicRepository.Table
                                where
                                (forumId == "" || ft.ForumId == forumId) &&
                                (customerId == "" || ft.CustomerId == customerId) &&
                                (
                                   !searchKeywords ||
                                   (searchTopicTitles && ft.Subject.ToLower().Contains(keywords.ToLower()))
                                )
                                &&
                                (limitDate.Value <= ft.LastPostTime)
                                select ft.Id).ToListAsync();

            topicIds.AddRange(query2);
            var _topicIds = topicIds.Distinct();
            var query3 = from ft in _forumTopicRepository.Table
                         where _topicIds.Contains(ft.Id)
                         orderby ft.TopicTypeId descending, ft.LastPostTime descending, ft.Id descending
                         select ft;

            return await PagedList<ForumTopic>.Create(query3, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets active forum topics
        /// </summary>
        /// <param name="forumId">The forum identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Forum Topics</returns>
        public virtual async Task<IPagedList<ForumTopic>> GetActiveTopics(string forumId = "", 
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query1 = await (from ft in _forumTopicRepository.Table
                                where
                                (forumId == "" || ft.ForumId == forumId) &&
                                (ft.LastPostTime.HasValue)
                                select ft.Id).ToListAsync();

            var query2 = from ft in _forumTopicRepository.Table
                         where query1.Contains(ft.Id)
                         orderby ft.LastPostTime descending
                         select ft;

            return await PagedList<ForumTopic>.Create(query2, pageIndex, pageSize);

        }

        /// <summary>
        /// Inserts a forum topic
        /// </summary>
        /// <param name="forumTopic">Forum topic</param>
        /// <param name="sendNotifications">A value indicating whether to send notifications to subscribed customers</param>
        public virtual async Task InsertTopic(ForumTopic forumTopic, bool sendNotifications)
        {
            if (forumTopic == null)
            {
                throw new ArgumentNullException("forumTopic");
            }

            await _forumTopicRepository.InsertAsync(forumTopic);

            //update stats
            await UpdateForumStats(forumTopic.ForumId);

            //cache            
            await _cacheManager.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(FORUM_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(forumTopic);

            //send notifications
            if (sendNotifications)
            {
                var forum = await _forumRepository.GetByIdAsync(forumTopic.ForumId);
                var subscriptions = await GetAllSubscriptions(forumId: forum.Id);
                var languageId = _workContext.WorkingLanguage.Id;

                foreach (var subscription in subscriptions)
                {
                    if (subscription.CustomerId == forumTopic.CustomerId)
                    {
                        continue;
                    }

                    if (subscription.CustomerId!="")
                    {
                        var customer = await _customerService.GetCustomerById(subscription.CustomerId);
                        await _workflowMessageService.SendNewForumTopicMessage(customer, forumTopic,
                            forum, languageId);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the forum topic
        /// </summary>
        /// <param name="forumTopic">Forum topic</param>
        public virtual async Task UpdateTopic(ForumTopic forumTopic)
        {
            if (forumTopic == null)
            {
                throw new ArgumentNullException("forumTopic");
            }

            await _forumTopicRepository.UpdateAsync(forumTopic);

            await _cacheManager.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(FORUM_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(forumTopic);
        }

        /// <summary>
        /// Moves the forum topic
        /// </summary>
        /// <param name="forumTopicId">The forum topic identifier</param>
        /// <param name="newForumId">New forum identifier</param>
        /// <returns>Moved forum topic</returns>
        public virtual async Task<ForumTopic> MoveTopic(string forumTopicId, string newForumId)
        {
            var forumTopic = await GetTopicById(forumTopicId);
            if (forumTopic == null)
                return null;

            if (IsCustomerAllowedToMoveTopic(_workContext.CurrentCustomer, forumTopic))
            {
                string previousForumId = forumTopic.ForumId;
                var newForum = await GetForumById(newForumId);

                if (newForum != null)
                {
                    if (previousForumId != newForumId)
                    {
                        forumTopic.ForumId = newForum.Id;
                        forumTopic.UpdatedOnUtc = DateTime.UtcNow;
                        await UpdateTopic(forumTopic);

                        //update forum stats
                        await UpdateForumStats(previousForumId);
                        await UpdateForumStats(newForumId);
                    }
                }
            }
            return forumTopic;
        }

        /// <summary>
        /// Deletes a forum post
        /// </summary>
        /// <param name="forumPost">Forum post</param>
        public virtual async Task DeletePost(ForumPost forumPost)
        {            
            if (forumPost == null)
            {
                throw new ArgumentNullException("forumPost");
            }

            string forumTopicId = forumPost.TopicId;
            string customerId = forumPost.CustomerId;
            var forumTopic = await GetTopicById(forumTopicId);
            string forumId = forumTopic.ForumId;

            //delete topic if it was the first post
            bool deleteTopic = false;
            ForumPost firstPost = await forumTopic.GetFirstPost(this);
            if (firstPost != null && firstPost.Id == forumPost.Id)
            {
                deleteTopic = true;
            }

            //delete forum post
            await _forumPostRepository.DeleteAsync(forumPost);

            //delete topic
            if (deleteTopic)
            {
                await DeleteTopic(forumTopic);
            }

            //update stats
            if (!deleteTopic)
            {
                await UpdateForumTopicStats(forumTopicId);
            }
            await UpdateForumStats(forumId);
            await UpdateCustomerStats(customerId);

            //clear cache            
            await _cacheManager.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(FORUM_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(forumPost);

        }

        /// <summary>
        /// Gets a forum post
        /// </summary>
        /// <param name="forumPostId">The forum post identifier</param>
        /// <returns>Forum Post</returns>
        public virtual Task<ForumPost> GetPostById(string forumPostId)
        {
            return _forumPostRepository.GetByIdAsync(forumPostId);
        }

        /// <summary>
        /// Gets all forum posts
        /// </summary>
        /// <param name="forumTopicId">The forum topic identifier</param>
        /// <param name="customerId">The customer identifier</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Posts</returns>
        public virtual async Task<IPagedList<ForumPost>> GetAllPosts(string forumTopicId = "",
            string customerId = "", string keywords = "", 
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return await GetAllPosts(forumTopicId, customerId, keywords, true,
                pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all forum posts
        /// </summary>
        /// <param name="forumTopicId">The forum topic identifier</param>
        /// <param name="customerId">The customer identifier</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="ascSort">Sort order</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Forum Posts</returns>
        public virtual async Task<IPagedList<ForumPost>> GetAllPosts(string forumTopicId = "", string customerId = "",
            string keywords = "", bool ascSort = false, 
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _forumPostRepository.Table;
            if (!String.IsNullOrEmpty(forumTopicId))
            {
                query = query.Where(fp => forumTopicId == fp.TopicId);
            }
            if (!String.IsNullOrEmpty(customerId))
            {
                query = query.Where(fp => customerId == fp.CustomerId);
            }
            if (!String.IsNullOrEmpty(keywords))
            {
                query = query.Where(fp => fp.Text.Contains(keywords));
            }
            if (ascSort)
            {
                query = query.OrderBy(fp => fp.CreatedOnUtc).ThenBy(fp => fp.Id);
            }
            else
            {
                query = query.OrderByDescending(fp => fp.CreatedOnUtc).ThenBy(fp => fp.Id);
            }
            return await PagedList<ForumPost>.Create(query, pageIndex, pageSize);

        }

        /// <summary>
        /// Inserts a forum post
        /// </summary>
        /// <param name="forumPost">The forum post</param>
        /// <param name="sendNotifications">A value indicating whether to send notifications to subscribed customers</param>
        public virtual async Task InsertPost(ForumPost forumPost, bool sendNotifications)
        {
            if (forumPost == null)
            {
                throw new ArgumentNullException("forumPost");
            }

            await _forumPostRepository.InsertAsync(forumPost);

            //update stats
            string customerId = forumPost.CustomerId;
            var forumTopic = await GetTopicById(forumPost.TopicId);
            string forumId = forumTopic.ForumId;
            await UpdateForumTopicStats(forumPost.TopicId);
            await UpdateForumStats(forumId);
            await UpdateCustomerStats(customerId);

            //clear cache            
            await _cacheManager.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(FORUM_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(forumPost);

            //notifications
            if (sendNotifications)
            {
                var forum = await _forumRepository.GetByIdAsync(forumTopic.ForumId);
                var subscriptions = await GetAllSubscriptions(topicId: forumTopic.Id);

                var languageId = _workContext.WorkingLanguage.Id;

                int friendlyTopicPageIndex = await CalculateTopicPageIndex(forumPost.TopicId,
                    _forumSettings.PostsPageSize > 0 ? _forumSettings.PostsPageSize : 10, 
                    forumPost.Id) + 1;

                foreach (ForumSubscription subscription in subscriptions)
                {
                    if (subscription.CustomerId == forumPost.CustomerId)
                    {
                        continue;
                    }

                    if ((subscription.CustomerId!=""))
                    {
                        var customer = await _customerService.GetCustomerById(subscription.CustomerId);
                        await _workflowMessageService.SendNewForumPostMessage(customer, forumPost,
                            forumTopic, forum, friendlyTopicPageIndex, languageId);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the forum post
        /// </summary>
        /// <param name="forumPost">Forum post</param>
        public virtual async Task UpdatePost(ForumPost forumPost)
        {
            //validation
            if (forumPost == null)
            {
                throw new ArgumentNullException("forumPost");
            }

            await _forumPostRepository.UpdateAsync(forumPost);

            await _cacheManager.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(FORUM_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(forumPost);
        }

        /// <summary>
        /// Deletes a private message
        /// </summary>
        /// <param name="privateMessage">Private message</param>
        public virtual async Task DeletePrivateMessage(PrivateMessage privateMessage)
        {
            if (privateMessage == null)
            {
                throw new ArgumentNullException("privateMessage");
            }

            await _forumPrivateMessageRepository.DeleteAsync(privateMessage);

            //event notification
            await _mediator.EntityDeleted(privateMessage);
        }

        /// <summary>
        /// Gets a private message
        /// </summary>
        /// <param name="privateMessageId">The private message identifier</param>
        /// <returns>Private message</returns>
        public virtual Task<PrivateMessage> GetPrivateMessageById(string privateMessageId)
        {
            return _forumPrivateMessageRepository.GetByIdAsync(privateMessageId);
        }

        /// <summary>
        /// Gets private messages
        /// </summary>
        /// <param name="storeId">The store identifier; pass "" to load all messages</param>
        /// <param name="fromCustomerId">The customer identifier who sent the message</param>
        /// <param name="toCustomerId">The customer identifier who should receive the message</param>
        /// <param name="isRead">A value indicating whether loaded messages are read. false - to load not read messages only, 1 to load read messages only, null to load all messages</param>
        /// <param name="isDeletedByAuthor">A value indicating whether loaded messages are deleted by author. false - messages are not deleted by author, null to load all messages</param>
        /// <param name="isDeletedByRecipient">A value indicating whether loaded messages are deleted by recipient. false - messages are not deleted by recipient, null to load all messages</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Private messages</returns>
        public virtual async Task<IPagedList<PrivateMessage>> GetAllPrivateMessages(string storeId, string fromCustomerId,
            string toCustomerId, bool? isRead, bool? isDeletedByAuthor, bool? isDeletedByRecipient,
            string keywords, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _forumPrivateMessageRepository.Table;

            if (!String.IsNullOrEmpty(storeId))
                query = query.Where(pm => storeId == pm.StoreId);
            if (!String.IsNullOrEmpty(fromCustomerId))
                query = query.Where(pm => fromCustomerId == pm.FromCustomerId);
            if (!String.IsNullOrEmpty(toCustomerId))
                query = query.Where(pm => toCustomerId == pm.ToCustomerId);
            if (isRead.HasValue)
                query = query.Where(pm => isRead.Value == pm.IsRead);
            if (isDeletedByAuthor.HasValue)
                query = query.Where(pm => isDeletedByAuthor.Value == pm.IsDeletedByAuthor);
            if (isDeletedByRecipient.HasValue)
                query = query.Where(pm => isDeletedByRecipient.Value == pm.IsDeletedByRecipient);
            if (!String.IsNullOrEmpty(keywords))
            {
                query = query.Where(pm => pm.Subject.Contains(keywords));
                query = query.Where(pm => pm.Text.Contains(keywords));
            }
            query = query.OrderByDescending(pm => pm.CreatedOnUtc);

            return await PagedList<PrivateMessage>.Create(query, pageIndex, pageSize);

        }

        /// <summary>
        /// Inserts a private message
        /// </summary>
        /// <param name="privateMessage">Private message</param>
        public virtual async Task InsertPrivateMessage(PrivateMessage privateMessage)
        {
            if (privateMessage == null)
            {
                throw new ArgumentNullException("privateMessage");
            }

            await _forumPrivateMessageRepository.InsertAsync(privateMessage);

            //event notification
            await _mediator.EntityInserted(privateMessage);

            var customerTo = await _customerService.GetCustomerById(privateMessage.ToCustomerId);
            if (customerTo == null)
            {
                throw new GrandException("Recipient could not be loaded");
            }

            //UI notification
            await _genericAttributeService.SaveAttribute(customerTo, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, false, privateMessage.StoreId);

            //Email notification
            if (_forumSettings.NotifyAboutPrivateMessages)
            {
                await _workflowMessageService.SendPrivateMessageNotification(privateMessage, _workContext.WorkingLanguage.Id);                
            }
        }

        /// <summary>
        /// Updates the private message
        /// </summary>
        /// <param name="privateMessage">Private message</param>
        public virtual async Task UpdatePrivateMessage(PrivateMessage privateMessage)
        {
            if (privateMessage == null)
                throw new ArgumentNullException("privateMessage");

            if (privateMessage.IsDeletedByAuthor && privateMessage.IsDeletedByRecipient)
            {
                await _forumPrivateMessageRepository.DeleteAsync(privateMessage);
                //event notification
                await _mediator.EntityDeleted(privateMessage);
            }
            else
            {
                await _forumPrivateMessageRepository.UpdateAsync(privateMessage);
                //event notification
                await _mediator.EntityUpdated(privateMessage);
            }
        }

        /// <summary>
        /// Deletes a forum subscription
        /// </summary>
        /// <param name="forumSubscription">Forum subscription</param>
        public virtual async Task DeleteSubscription(ForumSubscription forumSubscription)
        {
            if (forumSubscription == null)
            {
                throw new ArgumentNullException("forumSubscription");
            }

            await _forumSubscriptionRepository.DeleteAsync(forumSubscription);

            //event notification
            await _mediator.EntityDeleted(forumSubscription);
        }

        /// <summary>
        /// Gets a forum subscription
        /// </summary>
        /// <param name="forumSubscriptionId">The forum subscription identifier</param>
        /// <returns>Forum subscription</returns>
        public virtual Task<ForumSubscription> GetSubscriptionById(string forumSubscriptionId)
        {
            return _forumSubscriptionRepository.GetByIdAsync(forumSubscriptionId);
        }

        /// <summary>
        /// Gets forum subscriptions
        /// </summary>
        /// <param name="customerId">The customer identifier</param>
        /// <param name="forumId">The forum identifier</param>
        /// <param name="topicId">The topic identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Forum subscriptions</returns>
        public virtual async Task<IPagedList<ForumSubscription>> GetAllSubscriptions(string customerId = "", string forumId = "",
            string topicId = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var fsQuery = await (from fs in _forumSubscriptionRepository.Table
                                where
                                (customerId == "" || fs.CustomerId == customerId) &&
                                (forumId == "" || fs.ForumId == forumId) &&
                                (topicId == "" || fs.TopicId == topicId)
                                select fs.SubscriptionGuid).ToListAsync();

            var query = from fs in _forumSubscriptionRepository.Table
                        where fsQuery.Contains(fs.SubscriptionGuid)
                        orderby fs.CreatedOnUtc descending, fs.SubscriptionGuid descending
                        select fs;

            return await PagedList<ForumSubscription>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Inserts a forum subscription
        /// </summary>
        /// <param name="forumSubscription">Forum subscription</param>
        public virtual async Task InsertSubscription(ForumSubscription forumSubscription)
        {
            if (forumSubscription == null)
            {
                throw new ArgumentNullException("forumSubscription");
            }

            await _forumSubscriptionRepository.InsertAsync(forumSubscription);

            //event notification
            await _mediator.EntityInserted(forumSubscription);
        }

        /// <summary>
        /// Updates the forum subscription
        /// </summary>
        /// <param name="forumSubscription">Forum subscription</param>
        public virtual async Task UpdateSubscription(ForumSubscription forumSubscription)
        {
            if (forumSubscription == null)
            {
                throw new ArgumentNullException("forumSubscription");
            }
            
            await _forumSubscriptionRepository.UpdateAsync(forumSubscription);

            //event notification
            await _mediator.EntityUpdated(forumSubscription);
        }

        /// <summary>
        /// Check whether customer is allowed to create new topics
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="forum">Forum</param>
        /// <returns>True if allowed, otherwise false</returns>
        public virtual bool IsCustomerAllowedToCreateTopic(Customer customer, Forum forum)
        {
            if (forum == null)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest() && !_forumSettings.AllowGuestsToCreateTopics)
            {
                return false;
            }

            if (IsForumModerator(customer))
            {
                return true;
            }

            return true;
        }

        /// <summary>
        /// Check whether customer is allowed to edit topic
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="topic">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        public virtual bool IsCustomerAllowedToEditTopic(Customer customer, ForumTopic topic)
        {
            if (topic == null)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest())
            {
                return false;
            }

            if (IsForumModerator(customer))
            {
                return true;
            }

            if (_forumSettings.AllowCustomersToEditPosts)
            {
                bool ownTopic = customer.Id == topic.CustomerId;
                return ownTopic;
            }

            return false;
        }

        /// <summary>
        /// Check whether customer is allowed to move topic
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="topic">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        public virtual bool IsCustomerAllowedToMoveTopic(Customer customer, ForumTopic topic)
        {
            if (topic == null)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest())
            {
                return false;
            }

            if (IsForumModerator(customer))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check whether customer is allowed to delete topic
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="topic">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        public virtual bool IsCustomerAllowedToDeleteTopic(Customer customer, ForumTopic topic)
        {
            if (topic == null)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest())
            {
                return false;
            }

            if (IsForumModerator(customer))
            {
                return true;
            }

            if (_forumSettings.AllowCustomersToDeletePosts)
            {
                bool ownTopic = customer.Id == topic.CustomerId;
                return ownTopic;
            }

            return false;
        }

        /// <summary>
        /// Check whether customer is allowed to create new post
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="topic">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        public virtual bool IsCustomerAllowedToCreatePost(Customer customer, ForumTopic topic)
        {
            if (topic == null)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest() && !_forumSettings.AllowGuestsToCreatePosts)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check whether customer is allowed to edit post
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="post">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        public virtual bool IsCustomerAllowedToEditPost(Customer customer, ForumPost post)
        {
            if (post == null)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest())
            {
                return false;
            }

            if (IsForumModerator(customer))
            {
                return true;
            }

            if (_forumSettings.AllowCustomersToEditPosts)
            {
                bool ownPost = customer.Id == post.CustomerId;
                return ownPost;
            }

            return false;
        }
        
        /// <summary>
        /// Check whether customer is allowed to delete post
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="post">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        public virtual bool IsCustomerAllowedToDeletePost(Customer customer, ForumPost post)
        {
            if (post == null)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest())
            {
                return false;
            }

            if (IsForumModerator(customer))
            {
                return true;
            }

            if (_forumSettings.AllowCustomersToDeletePosts)
            {
                bool ownPost = customer.Id == post.CustomerId;
                return ownPost;
            }

            return false;
        }

        /// <summary>
        /// Check whether customer is allowed to set topic priority
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>True if allowed, otherwise false</returns>
        public virtual bool IsCustomerAllowedToSetTopicPriority(Customer customer)
        {
            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest())
            {
                return false;
            }

            if (IsForumModerator(customer))
            {
                return true;
            }            

            return false;
        }

        /// <summary>
        /// Check whether customer is allowed to watch topics
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>True if allowed, otherwise false</returns>
        public virtual bool IsCustomerAllowedToSubscribe(Customer customer)
        {
            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates topic page index by post identifier
        /// </summary>
        /// <param name="forumTopicId">Forum topic identifier</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="postId">Post identifier</param>
        /// <returns>Page index</returns>
        public virtual async Task<int> CalculateTopicPageIndex(string forumTopicId, int pageSize, string postId)
        {
            int pageIndex = 0;
            var forumPosts = await GetAllPosts(forumTopicId: forumTopicId, ascSort: true);

            for (int i = 0; i < forumPosts.TotalCount; i++)
            {
                if (forumPosts[i].Id == postId)
                {
                    if (pageSize > 0)
                    {
                        pageIndex = i / pageSize;
                    }
                }
            }
            return pageIndex;
        }


        /// <summary>
        /// Get a post vote 
        /// </summary>
        /// <param name="postId">Post identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>Post vote</returns>
        public virtual async Task<ForumPostVote> GetPostVote(string postId, string customerId)
        {
            if (String.IsNullOrEmpty(customerId))
                return null;

            return await _forumPostVoteRepository.Table.FirstOrDefaultAsync(pv => pv.ForumPostId == postId && pv.CustomerId == customerId);
        }

        /// <summary>
        /// Get post vote made since the parameter date
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="сreatedFromUtc">Date</param>
        /// <returns>Post votes count</returns>
        public virtual async Task<int> GetNumberOfPostVotes(string customerId, DateTime сreatedFromUtc)
        {
            if (String.IsNullOrEmpty(customerId))
                return 0;

            return await _forumPostVoteRepository.Table.CountAsync(pv => pv.CustomerId == customerId && pv.CreatedOnUtc > сreatedFromUtc);
        }

        /// <summary>
        /// Insert a post vote
        /// </summary>
        /// <param name="postVote">Post vote</param>
        public virtual async Task InsertPostVote(ForumPostVote postVote)
        {
            if (postVote == null)
                throw new ArgumentNullException("postVote");

            await _forumPostVoteRepository.InsertAsync(postVote);

            //update post
            var post = await GetPostById(postVote.ForumPostId);
            post.VoteCount = postVote.IsUp ? ++post.VoteCount : --post.VoteCount;
            await UpdatePost(post);

            //event notification
            await _mediator.EntityInserted(postVote);
        }

        /// <summary>
        /// Update a post vote
        /// </summary>
        /// <param name="postVote">Post vote</param>
        public virtual async Task UpdatePostVote(ForumPostVote postVote)
        {
            if (postVote == null)
                throw new ArgumentNullException("postVote");

            await _forumPostVoteRepository.UpdateAsync(postVote);

            //event notification
            await _mediator.EntityUpdated(postVote);
        }

        /// <summary>
        /// Delete a post vote
        /// </summary>
        /// <param name="postVote">Post vote</param>
        public virtual async Task DeletePostVote(ForumPostVote postVote)
        {
            if (postVote == null)
                throw new ArgumentNullException("postVote");

            await _forumPostVoteRepository.DeleteAsync(postVote);

            // update post
            var post = await GetPostById(postVote.ForumPostId);
            post.VoteCount = postVote.IsUp ? --post.VoteCount : ++post.VoteCount;
            await UpdatePost(post);

            //event notification
            await _mediator.EntityDeleted(postVote);
        }

        #endregion
    }
}
