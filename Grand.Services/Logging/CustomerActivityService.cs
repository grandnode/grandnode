using Grand.Core;
using Grand.Domain;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Customers;
using Grand.Domain.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Logging
{
    /// <summary>
    /// Customer activity service
    /// </summary>
    public class CustomerActivityService : ICustomerActivityService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string ACTIVITYTYPE_ALL_KEY = "Grand.activitytype.all";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string ACTIVITYTYPE_PATTERN_KEY = "Grand.activitytype.";

        #endregion

        #region Fields

        /// <summary>
        /// Cache manager
        /// </summary>
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<ActivityLog> _activityLogRepository;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IActivityKeywordsProvider _activityKeywordsProvider;
        #endregion

        #region Ctor
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager list</param>
        /// <param name="activityLogRepository">Activity log repository</param>
        /// <param name="activityLogTypeRepository">Activity log type repository</param>
        /// <param name="workContext">Work context</param>
        /// <param name="webHelper">Web helper</param>
        /// <param name="activityKeywordsProvider">Activity Keywords provider</param>
        public CustomerActivityService(
            ICacheManager cacheManager,
            IRepository<ActivityLog> activityLogRepository,
            IRepository<ActivityLogType> activityLogTypeRepository,
            IWorkContext workContext,
            IWebHelper webHelper,
            IActivityKeywordsProvider activityKeywordsProvider)
        {
            _cacheManager = cacheManager;
            _activityLogRepository = activityLogRepository;
            _activityLogTypeRepository = activityLogTypeRepository;
            _workContext = workContext;
            _webHelper = webHelper;
            _activityKeywordsProvider = activityKeywordsProvider;
        }

        #endregion

        #region Nested classes

        [Serializable]
        public class ActivityLogTypeForCaching
        {
            public string Id { get; set; }
            public string SystemKeyword { get; set; }
            public string Name { get; set; }
            public bool Enabled { get; set; }
        }

        #endregion

        #region Utitlies

        /// <summary>
        /// Gets all activity log types (class for caching)
        /// </summary>
        /// <returns>Activity log types</returns>
        protected virtual async Task<IList<ActivityLogTypeForCaching>> GetAllActivityTypesCached()
        {
            //cache
            string key = string.Format(ACTIVITYTYPE_ALL_KEY);
            return await _cacheManager.GetAsync(key, async () =>
            {
                var result = new List<ActivityLogTypeForCaching>();
                var activityLogTypes = await GetAllActivityTypes();
                foreach (var alt in activityLogTypes)
                {
                    var altForCaching = new ActivityLogTypeForCaching
                    {
                        Id = alt.Id,
                        SystemKeyword = alt.SystemKeyword,
                        Name = alt.Name,
                        Enabled = alt.Enabled
                    };
                    result.Add(altForCaching);
                }
                return result;
            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type item</param>
        public virtual async Task InsertActivityType(ActivityLogType activityLogType)
        {
            if (activityLogType == null)
                throw new ArgumentNullException("activityLogType");

            await _activityLogTypeRepository.InsertAsync(activityLogType);
            await _cacheManager.RemoveByPrefix(ACTIVITYTYPE_PATTERN_KEY);
        }

        /// <summary>
        /// Updates an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type item</param>
        public virtual async Task UpdateActivityType(ActivityLogType activityLogType)
        {
            if (activityLogType == null)
                throw new ArgumentNullException("activityLogType");

            await _activityLogTypeRepository.UpdateAsync(activityLogType);
            await _cacheManager.RemoveByPrefix(ACTIVITYTYPE_PATTERN_KEY);
        }

        /// <summary>
        /// Deletes an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type</param>
        public virtual async Task DeleteActivityType(ActivityLogType activityLogType)
        {
            if (activityLogType == null)
                throw new ArgumentNullException("activityLogType");

            await _activityLogTypeRepository.DeleteAsync(activityLogType);
            await _cacheManager.RemoveByPrefix(ACTIVITYTYPE_PATTERN_KEY);
        }

        /// <summary>
        /// Gets all activity log type items
        /// </summary>
        /// <returns>Activity log type items</returns>
        public virtual async Task<IList<ActivityLogType>> GetAllActivityTypes()
        {
            var query = from alt in _activityLogTypeRepository.Table
                        orderby alt.Name
                        select alt;
            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets an activity log type item
        /// </summary>
        /// <param name="activityLogTypeId">Activity log type identifier</param>
        /// <returns>Activity log type item</returns>
        public virtual Task<ActivityLogType> GetActivityTypeById(string activityLogTypeId)
        {
            return _activityLogTypeRepository.GetByIdAsync(activityLogTypeId);
        }

        /// <summary>
        /// Inserts an activity log item
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Activity log item</returns>
        public virtual async Task InsertActivity(string systemKeyword, string entityKeyId,
            string comment, params object[] commentParams)
        {
            await InsertActivity(systemKeyword, entityKeyId, comment, _workContext.CurrentCustomer, commentParams);
        }

        /// <summary>
        /// Inserts an activity log item
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="customer">The customer</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Activity log item</returns>
        public virtual async Task<ActivityLog> InsertActivity(string systemKeyword, string entityKeyId,
            string comment, Customer customer, params object[] commentParams)
        {
            if (customer == null)
                return null;

            var activityTypes = await GetAllActivityTypesCached();
            var activityType = activityTypes.ToList().Find(at => at.SystemKeyword == systemKeyword);
            if (activityType == null || !activityType.Enabled)
                return null;

            comment = CommonHelper.EnsureNotNull(comment);
            comment = string.Format(comment, commentParams);
            comment = CommonHelper.EnsureMaximumLength(comment, 4000);

            var activity = new ActivityLog();
            activity.ActivityLogTypeId = activityType.Id;
            activity.CustomerId = customer.Id;
            activity.EntityKeyId = entityKeyId;
            activity.Comment = comment;
            activity.CreatedOnUtc = DateTime.UtcNow;
            activity.IpAddress = _webHelper.GetCurrentIpAddress();
            await _activityLogRepository.InsertAsync(activity);

            return activity;
        }

        /// <summary>
        /// Inserts an activity log item async
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="entityKeyId">Entity Key</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="customerId">The customer</param>
        /// <param name="addressIp">IP Address</param>
        /// <returns>Activity log item</returns>
        public virtual async Task InsertActivityAsync(string systemKeyword, string entityKeyId,
            string comment, string customerId, string addressIp)
        {
            var activityTypes = await GetAllActivityTypesCached();
            var activityType = activityTypes.ToList().Find(at => at.SystemKeyword == systemKeyword);
            if (activityType == null || !activityType.Enabled)
                return;

            comment = CommonHelper.EnsureNotNull(comment);
            comment = CommonHelper.EnsureMaximumLength(comment, 4000);

            var activity = new ActivityLog();
            activity.ActivityLogTypeId = activityType.Id;
            activity.CustomerId = customerId;
            activity.EntityKeyId = entityKeyId;
            activity.Comment = comment;
            activity.CreatedOnUtc = DateTime.UtcNow;
            activity.IpAddress = addressIp;
            await _activityLogRepository.InsertAsync(activity);
        }

        /// <summary>
        /// Deletes an activity log item
        /// </summary>
        /// <param name="activityLog">Activity log type</param>
        public virtual async Task DeleteActivity(ActivityLog activityLog)
        {
            if (activityLog == null)
                throw new ArgumentNullException("activityLog");

            await _activityLogRepository.DeleteAsync(activityLog);
        }

        /// <summary>
        /// Gets all activity log items
        /// </summary>
        /// <param name="comment">Log item message text or text part; null or empty string to load all</param>
        /// <param name="createdOnFrom">Log item creation from; null to load all customers</param>
        /// <param name="createdOnTo">Log item creation to; null to load all customers</param>
        /// <param name="customerId">Customer identifier; null to load all customers</param>
        /// <param name="activityLogTypeId">Activity log type identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Activity log items</returns>
        public virtual async Task<IPagedList<ActivityLog>> GetAllActivities(string comment = "",
            DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null, string customerId = "", string activityLogTypeId = "",
            string ipAddress = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _activityLogRepository.Table;
            if (!String.IsNullOrEmpty(comment))
                query = query.Where(al => al.Comment != null && al.Comment.ToLower().Contains(comment.ToLower()));
            if (createdOnFrom.HasValue)
                query = query.Where(al => createdOnFrom.Value <= al.CreatedOnUtc);
            if (createdOnTo.HasValue)
                query = query.Where(al => createdOnTo.Value >= al.CreatedOnUtc);
            if (!String.IsNullOrEmpty(activityLogTypeId))
                query = query.Where(al => activityLogTypeId == al.ActivityLogTypeId);
            if (!String.IsNullOrEmpty(customerId))
                query = query.Where(al => customerId == al.CustomerId);
            if (!String.IsNullOrEmpty(ipAddress))
                query = query.Where(al => ipAddress == al.IpAddress);

            query = query.OrderByDescending(al => al.CreatedOnUtc);
            return await PagedList<ActivityLog>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets stats activity log items
        /// </summary>
        /// <param name="createdOnFrom">Log item creation from; null to load all records</param>
        /// <param name="createdOnTo">Log item creation to; null to load all records</param>
        /// <param name="activityLogTypeId">Activity log type identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Activity log items</returns>
        public virtual async Task<IPagedList<ActivityStats>> GetStatsActivities(DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null, string activityLogTypeId = "",
            int pageIndex = 0, int pageSize = int.MaxValue)
        {

            var builder = Builders<ActivityLog>.Filter;
            var filter = builder.Where(x => true);
            if (createdOnFrom.HasValue)
                filter = filter & builder.Where(al => createdOnFrom.Value <= al.CreatedOnUtc);
            if (createdOnTo.HasValue)
                filter = filter & builder.Where(al => createdOnTo.Value >= al.CreatedOnUtc);
            if (!String.IsNullOrEmpty(activityLogTypeId))
                filter = filter & builder.Where(al => activityLogTypeId == al.ActivityLogTypeId);

            var query = _activityLogRepository.Collection
                    .Aggregate()
                    .Match(filter)
                    .Group(
                        key => new { key.ActivityLogTypeId, key.EntityKeyId },
                            g => new
                            {
                                Id = g.Key,
                                Count = g.Count()
                            })
                    .Project(x => new ActivityStats
                    {
                        ActivityLogTypeId = x.Id.ActivityLogTypeId,
                        EntityKeyId = x.Id.EntityKeyId,
                        Count = x.Count
                    })
                    .SortByDescending(x => x.Count);

            return await PagedList<ActivityStats>.Create(query, pageIndex, pageSize);
        }
        /// <summary>
        /// Gets category activity log items
        /// </summary>
        /// <param name="createdOnFrom">Log item creation from; null to load all records</param>
        /// <param name="createdOnTo">Log item creation to; null to load all records</param>
        /// <param name="categoryId">Category identifier</param>        
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Activity log items</returns>
        public virtual async Task<IPagedList<ActivityLog>> GetCategoryActivities(DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null, string categoryId = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _activityLogRepository.Table;
            if (createdOnFrom.HasValue)
                query = query.Where(al => createdOnFrom.Value <= al.CreatedOnUtc);
            if (createdOnTo.HasValue)
                query = query.Where(al => createdOnTo.Value >= al.CreatedOnUtc);

            var activityTypes = await GetAllActivityTypesCached();
            var activityTypeIds = activityTypes.ToList().Where(at => _activityKeywordsProvider.GetCategorySystemKeywords().Contains(at.SystemKeyword)).Select(x => x.Id);

            query = query.Where(al => activityTypeIds.Contains(al.ActivityLogTypeId));

            query = query.Where(al => al.EntityKeyId == categoryId);
            query = query.OrderByDescending(al => al.CreatedOnUtc);
            return await PagedList<ActivityLog>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets knowledgebase category activity log items
        /// </summary>
        /// <param name="createdOnFrom">Log item creation from; null to load all records</param>
        /// <param name="createdOnTo">Log item creation to; null to load all records</param>
        /// <param name="categoryId">Category identifier</param>        
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Activity log items</returns>
        public virtual async Task<IPagedList<ActivityLog>> GetKnowledgebaseCategoryActivities(DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null, string categoryId = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _activityLogRepository.Table;
            if (createdOnFrom.HasValue)
                query = query.Where(al => createdOnFrom.Value <= al.CreatedOnUtc);
            if (createdOnTo.HasValue)
                query = query.Where(al => createdOnTo.Value >= al.CreatedOnUtc);

            var activityTypes = await GetAllActivityTypesCached();
            var activityTypeIds = activityTypes.ToList().Where(at => _activityKeywordsProvider.GetKnowledgebaseCategorySystemKeywords().Contains(at.SystemKeyword)).Select(x => x.Id);

            query = query.Where(al => activityTypeIds.Contains(al.ActivityLogTypeId));

            query = query.Where(al => al.EntityKeyId == categoryId);
            query = query.OrderByDescending(al => al.CreatedOnUtc);
            return await PagedList<ActivityLog>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets knowledgebase article activity log items
        /// </summary>
        /// <param name="createdOnFrom">Log item creation from; null to load all records</param>
        /// <param name="createdOnTo">Log item creation to; null to load all records</param>
        /// <param name="categoryId">Category identifier</param>        
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Activity log items</returns>
        public virtual async Task<IPagedList<ActivityLog>> GetKnowledgebaseArticleActivities(DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null, string categoryId = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _activityLogRepository.Table;
            if (createdOnFrom.HasValue)
                query = query.Where(al => createdOnFrom.Value <= al.CreatedOnUtc);
            if (createdOnTo.HasValue)
                query = query.Where(al => createdOnTo.Value >= al.CreatedOnUtc);

            var activityTypes = await GetAllActivityTypesCached();
            var activityTypeIds = activityTypes.ToList().Where(at => _activityKeywordsProvider.GetKnowledgebaseArticleSystemKeywords().Contains(at.SystemKeyword)).Select(x => x.Id);

            query = query.Where(al => activityTypeIds.Contains(al.ActivityLogTypeId));

            query = query.Where(al => al.EntityKeyId == categoryId);
            query = query.OrderByDescending(al => al.CreatedOnUtc);
            return await PagedList<ActivityLog>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets manufacturer activity log items
        /// </summary>
        /// <param name="createdOnFrom">Log item creation from; null to load all records</param>
        /// <param name="createdOnTo">Log item creation to; null to load all records</param>
        /// <param name="categoryId">Manufacturer identifier</param>        
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Activity log items</returns>
        public virtual async Task<IPagedList<ActivityLog>> GetManufacturerActivities(DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null, string manufacturerId = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _activityLogRepository.Table;
            if (createdOnFrom.HasValue)
                query = query.Where(al => createdOnFrom.Value <= al.CreatedOnUtc);
            if (createdOnTo.HasValue)
                query = query.Where(al => createdOnTo.Value >= al.CreatedOnUtc);

            var activityTypes = await GetAllActivityTypesCached();
            var activityTypeIds = activityTypes.ToList().Where(at => _activityKeywordsProvider.GetManufacturerSystemKeywords().Contains(at.SystemKeyword)).Select(x => x.Id);

            query = query.Where(al => activityTypeIds.Contains(al.ActivityLogTypeId));

            query = query.Where(al => al.EntityKeyId == manufacturerId);
            query = query.OrderByDescending(al => al.CreatedOnUtc);
            return await PagedList<ActivityLog>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets product activity log items
        /// </summary>
        /// <param name="createdOnFrom">Log item creation from; null to load all records</param>
        /// <param name="createdOnTo">Log item creation to; null to load all records</param>
        /// <param name="productId">Product identifier</param>        
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Activity log items</returns>
        public virtual async Task<IPagedList<ActivityLog>> GetProductActivities(DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null, string productId = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _activityLogRepository.Table;
            if (createdOnFrom.HasValue)
                query = query.Where(al => createdOnFrom.Value <= al.CreatedOnUtc);
            if (createdOnTo.HasValue)
                query = query.Where(al => createdOnTo.Value >= al.CreatedOnUtc);

            var activityTypes = await GetAllActivityTypesCached();
            var activityTypeIds = activityTypes.ToList().Where(at => _activityKeywordsProvider.GetProductSystemKeywords().Contains(at.SystemKeyword)).Select(x => x.Id);

            query = query.Where(al => activityTypeIds.Contains(al.ActivityLogTypeId));

            query = query.Where(al => al.EntityKeyId == productId);
            query = query.OrderByDescending(al => al.CreatedOnUtc);
            return await PagedList<ActivityLog>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets an activity log item
        /// </summary>
        /// <param name="activityLogId">Activity log identifier</param>
        /// <returns>Activity log item</returns>
        public virtual Task<ActivityLog> GetActivityById(string activityLogId)
        {
            return _activityLogRepository.GetByIdAsync(activityLogId);
        }

        /// <summary>
        /// Clears activity log
        /// </summary>
        public virtual async Task ClearAllActivities()
        {
            await _activityLogRepository.Collection.DeleteManyAsync(new MongoDB.Bson.BsonDocument());
        }
        #endregion

    }
}
