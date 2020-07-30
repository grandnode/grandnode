using Grand.Domain;
using Grand.Domain.Customers;
using Grand.Domain.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Logging
{
    /// <summary>
    /// Null logger
    /// </summary>
    public partial class NullLogger : ILogger
    {
        /// <summary>
        /// Determines whether a log level is enabled
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>Result</returns>
        public virtual bool IsEnabled(LogLevel level)
        {
            return false;
        }

        /// <summary>
        /// Deletes a log item
        /// </summary>
        /// <param name="log">Log item</param>
        public virtual async Task DeleteLog(Log log)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Clears a log
        /// </summary>
        public virtual async Task ClearLog()
        {
            await Task.CompletedTask;
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
            return await Task.FromResult(new PagedList<Log>(new List<Log>(), pageIndex, pageSize));
        }

        /// <summary>
        /// Gets a log item
        /// </summary>
        /// <param name="logId">Log item identifier</param>
        /// <returns>Log item</returns>
        public virtual Task<Log> GetLogById(string logId)
        {
            return Task.FromResult<Log>(null);
        }

        /// <summary>
        /// Get log items by identifiers
        /// </summary>
        /// <param name="logIds">Log item identifiers</param>
        /// <returns>Log items</returns>
        public virtual async Task<IList<Log>> GetLogByIds(string[] logIds)
        {
            return await Task.FromResult(new List<Log>());
        }

        /// <summary>
        /// Inserts a log item
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="shortMessage">The short message</param>
        /// <param name="fullMessage">The full message</param>
        /// <param name="customer">The customer to associate log record with</param>
        /// <returns>A log item</returns>
        public virtual Task<Log> InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer customer = null)
        {
            return Task.FromResult<Log>(null);
        }
    }
}
