using Grand.Core.Caching;
using Grand.Domain;
using Grand.Domain.Data;
using Grand.Plugin.Tax.CountryStateZip.Domain;
using Grand.Services.Events;
using MediatR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.Tax.CountryStateZip.Services
{
    /// <summary>
    /// Tax rate service
    /// </summary>
    public partial class TaxRateService : ITaxRateService
    {
        #region Constants
        private const string TAXRATE_ALL_KEY = "Grand.taxrate.all-{0}-{1}";
        private const string TAXRATE_PATTERN_KEY = "Grand.taxrate.";
        #endregion

        #region Fields

        private readonly IMediator _mediator;
        private readonly IRepository<TaxRate> _taxRateRepository;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="mediator">Mediator</param>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="taxRateRepository">Tax rate repository</param>
        public TaxRateService(IMediator mediator,
            ICacheManager cacheManager,
            IRepository<TaxRate> taxRateRepository)
        {
            _mediator = mediator;
            _cacheManager = cacheManager;
            _taxRateRepository = taxRateRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        public virtual async Task DeleteTaxRate(TaxRate taxRate)
        {
            if (taxRate == null)
                throw new ArgumentNullException("taxRate");

            await _taxRateRepository.DeleteAsync(taxRate);

            await _cacheManager.RemoveByPrefix(TAXRATE_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(taxRate);
        }

        /// <summary>
        /// Gets all tax rates
        /// </summary>
        /// <returns>Tax rates</returns>
        public virtual async Task<IPagedList<TaxRate>> GetAllTaxRates(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            string key = string.Format(TAXRATE_ALL_KEY, pageIndex, pageSize);
            return await _cacheManager.GetAsync(key, async () =>
            {
                var query = from tr in _taxRateRepository.Table
                            orderby tr.StoreId, tr.CountryId, tr.StateProvinceId, tr.Zip, tr.TaxCategoryId
                            select tr;
                return await Task.FromResult(new PagedList<TaxRate>(query, pageIndex, pageSize));
            });
        }

        /// <summary>
        /// Gets a tax rate
        /// </summary>
        /// <param name="taxRateId">Tax rate identifier</param>
        /// <returns>Tax rate</returns>
        public virtual Task<TaxRate> GetTaxRateById(string taxRateId)
        {
            return _taxRateRepository.GetByIdAsync(taxRateId);
        }

        /// <summary>
        /// Inserts a tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        public virtual async Task InsertTaxRate(TaxRate taxRate)
        {
            if (taxRate == null)
                throw new ArgumentNullException("taxRate");

            await _taxRateRepository.InsertAsync(taxRate);

            await _cacheManager.RemoveByPrefix(TAXRATE_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(taxRate);
        }

        /// <summary>
        /// Updates the tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        public virtual async Task UpdateTaxRate(TaxRate taxRate)
        {
            if (taxRate == null)
                throw new ArgumentNullException("taxRate");

            await _taxRateRepository.UpdateAsync(taxRate);

            await _cacheManager.RemoveByPrefix(TAXRATE_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(taxRate);
        }
        #endregion
    }
}
