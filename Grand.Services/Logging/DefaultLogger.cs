using Grand.Core;
using Grand.Domain;
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
    /// Default logger
    /// </summary>
    public partial class DefaultLogger : ILogger
    {
        #region Fields

        private readonly IRepository<Log> _logRepository;
        private readonly IWebHelper _webHelper;
        
        #endregion
        
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logRepository">Log repository</param>
        /// <param name="webHelper">Web helper</param>
        public DefaultLogger(IRepository<Log> logRepository, 
            IWebHelper webHelper)
        {
            _logRepository = logRepository;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether a log level is enabled
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>Result</returns>
        public virtual bool IsEnabled(LogLevel level)
        {
            switch(level)
            {
                case LogLevel.Debug:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Deletes a log item
        /// </summary>
        /// <param name="log">Log item</param>
        public virtual async Task DeleteLog(Log log)
        {
            if (log == null)
                throw new ArgumentNullException("log");

            await _logRepository.DeleteAsync(log);
        }

        /// <summary>
        /// Clears a log
        /// </summary>
        public virtual async Task ClearLog()
        {
            await _logRepository.Collection.DeleteManyAsync(new MongoDB.Bson.BsonDocument());
        }

        /// <summary>
        /// Gets all log items
        /// </summary>
        /// <param name="fromUtc">Log item creation from; null to load all records</param>
        /// <param name="toUtc">Log item creation to; null to load all records</param>
        /// <param name="message">Message</param>
        /// <param name="logLevel">Log level; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Log item items</returns>
        public virtual async Task<IPagedList<Log>> GetAllLogs(DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = "", LogLevel? logLevel = null, 
            int pageIndex = 0, int pageSize = int.MaxValue)
        {

            var builder = Builders<Log>.Filter;
            var filter = builder.Where(c => true);

            if (fromUtc.HasValue)
                filter = filter & builder.Where(l => fromUtc.Value <= l.CreatedOnUtc);
            if (toUtc.HasValue)
                filter = filter & builder.Where(l => toUtc.Value >= l.CreatedOnUtc);
            if (logLevel.HasValue)
            {
                var logLevelId = (int)logLevel.Value;
                filter = filter & builder.Where(l => logLevelId == l.LogLevelId);
            }
            if (!String.IsNullOrEmpty(message))
                filter = filter & builder.Where(l => l.ShortMessage.ToLower().Contains(message.ToLower()) || l.FullMessage.ToLower().Contains(message.ToLower()));

            var builderSort = Builders<Log>.Sort.Descending(x => x.CreatedOnUtc);
            var query = _logRepository.Collection;

            var logs = await PagedList<Log>.Create(query, filter, builderSort, pageIndex, pageSize);

            return logs;
        }

        /// <summary>
        /// Gets a log item
        /// </summary>
        /// <param name="logId">Log item identifier</param>
        /// <returns>Log item</returns>
        public virtual Task<Log> GetLogById(string logId)
        {
            return _logRepository.GetByIdAsync(logId);
        }

        /// <summary>
        /// Get log items by identifiers
        /// </summary>
        /// <param name="logIds">Log item identifiers</param>
        /// <returns>Log items</returns>
        public virtual async Task<IList<Log>> GetLogByIds(string[] logIds)
        {
            if (logIds == null || logIds.Length == 0)
                return new List<Log>();

            var query = from l in _logRepository.Table
                        where logIds.Contains(l.Id)
                        select l;

            var logItems = await query.ToListAsync();
            //sort by passed identifiers
            var sortedLogItems = new List<Log>();
            foreach (string id in logIds)
            {
                var log = logItems.Find(x => x.Id == id);
                if (log != null)
                    sortedLogItems.Add(log);
            }
            return sortedLogItems;
        }

        /// <summary>
        /// Inserts a log item
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="shortMessage">The short message</param>
        /// <param name="fullMessage">The full message</param>
        /// <param name="customer">The customer to associate log record with</param>
        /// <returns>A log item</returns>
        public virtual async Task<Log> InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer customer = null)
        {
            var log = new Log
            {
                LogLevelId = (int)logLevel,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                IpAddress = _webHelper.GetCurrentIpAddress(),
                CustomerId = customer!=null ? customer.Id : "",
                PageUrl = _webHelper.GetThisPageUrl(true),
                ReferrerUrl = _webHelper.GetUrlReferrer(),
                CreatedOnUtc = DateTime.UtcNow
            };

            await _logRepository.InsertAsync(log);

            return log;
        }

        #endregion
    }
}
