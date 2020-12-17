using Grand.Core.Caching;
using Grand.Core.Caching.Constants;
using Grand.Domain.Customers;
using Grand.Domain.Data;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Customers
{
    public class SalesEmployeeService : ISalesEmployeeService
    {
        #region Fields

        private readonly IRepository<SalesEmployee> _salesEmployeeRepository;
        private readonly IMediator _mediator;
        private readonly ICacheManager _cacheManager;

        #endregion

        public SalesEmployeeService(
            IRepository<SalesEmployee> salesEmployeeRepository,
            IMediator mediator,
            ICacheManager cacheManager)
        {
            _salesEmployeeRepository = salesEmployeeRepository;
            _mediator = mediator;
            _cacheManager = cacheManager;
        }

        /// <summary>
        /// Gets a sales employee
        /// </summary>
        /// <param name="salesEmployeeId">The sales employee identifier</param>
        /// <returns>SalesEmployee</returns>
        public virtual Task<SalesEmployee> GetSalesEmployeeById(string salesEmployeeId)
        {
            string key = string.Format(CacheKey.SALESEMPLOYEE_BY_ID_KEY, salesEmployeeId);
            return _cacheManager.GetAsync(key, () => _salesEmployeeRepository.GetByIdAsync(salesEmployeeId));
        }

        /// <summary>
        /// Gets all sales employees
        /// </summary>
        /// <returns>Warehouses</returns>
        public virtual async Task<IList<SalesEmployee>> GetAll()
        {
            return await _cacheManager.GetAsync(CacheKey.SALESEMPLOYEE_ALL, () =>
            {
                var query = from se in _salesEmployeeRepository.Table
                            orderby se.DisplayOrder
                            select se;
                return query.ToListAsync();
            });
        }

        /// <summary>
        /// Inserts a sales employee
        /// </summary>
        /// <param name="salesEmployee">Sales Employee</param>
        public virtual async Task InsertSalesEmployee(SalesEmployee salesEmployee)
        {
            if (salesEmployee == null)
                throw new ArgumentNullException("salesEmployee");

            await _salesEmployeeRepository.InsertAsync(salesEmployee);

            //clear cache
            await _cacheManager.RemoveByPrefix(CacheKey.SALESEMPLOYEE_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(salesEmployee);
        }

        /// <summary>
        /// Updates the sales employee
        /// </summary>
        /// <param name="salesEmployee">Sales Employee</param>
        public virtual async Task UpdateSalesEmployee(SalesEmployee salesEmployee)
        {
            if (salesEmployee == null)
                throw new ArgumentNullException("salesEmployee");

            await _salesEmployeeRepository.UpdateAsync(salesEmployee);

            //clear cache
            await _cacheManager.RemoveByPrefix(CacheKey.SALESEMPLOYEE_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(salesEmployee);
        }

        /// <summary>
        /// Deletes a sales employee
        /// </summary>
        /// <param name="warehouse">The sales employee</param>
        public virtual async Task DeleteSalesEmployee(SalesEmployee salesEmployee)
        {
            if (salesEmployee == null)
                throw new ArgumentNullException("salesEmployee");

            await _salesEmployeeRepository.DeleteAsync(salesEmployee);

            //clear cache
            await _cacheManager.RemoveByPrefix(CacheKey.SALESEMPLOYEE_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(salesEmployee);
        }
    }
}
