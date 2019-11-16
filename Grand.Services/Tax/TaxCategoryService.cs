using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Tax;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Tax
{
    /// <summary>
    /// Tax category service
    /// </summary>
    public partial class TaxCategoryService : ITaxCategoryService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string TAXCATEGORIES_ALL_KEY = "Grand.taxcategory.all";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : tax category ID
        /// </remarks>
        private const string TAXCATEGORIES_BY_ID_KEY = "Grand.taxcategory.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string TAXCATEGORIES_PATTERN_KEY = "Grand.taxcategory.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTS_PATTERN_KEY = "Grand.product.";


        #endregion

        #region Fields

        private readonly IRepository<TaxCategory> _taxCategoryRepository;
        private readonly IMediator _mediator;
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<Product> _productRepository;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="taxCategoryRepository">Tax category repository</param>
        /// <param name="mediator">Mediator</param>
        public TaxCategoryService(ICacheManager cacheManager,
            IRepository<TaxCategory> taxCategoryRepository,
            IMediator mediator, IRepository<Product> productRepository)
        {
            _cacheManager = cacheManager;
            _taxCategoryRepository = taxCategoryRepository;
            _mediator = mediator;
            _productRepository = productRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a tax category
        /// </summary>
        /// <param name="taxCategory">Tax category</param>
        public virtual async Task DeleteTaxCategory(TaxCategory taxCategory)
        {
            if (taxCategory == null)
                throw new ArgumentNullException("taxCategory");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.TaxCategoryId, taxCategory.Id);
            var update = Builders<Product>.Update
                .Set(x => x.TaxCategoryId, "");
            await _productRepository.Collection.UpdateManyAsync(filter, update);

            await _taxCategoryRepository.DeleteAsync(taxCategory);

            await _cacheManager.RemoveByPattern(TAXCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(taxCategory);
        }

        /// <summary>
        /// Gets all tax categories
        /// </summary>
        /// <returns>Tax categories</returns>
        public virtual async Task<IList<TaxCategory>> GetAllTaxCategories()
        {
            string key = string.Format(TAXCATEGORIES_ALL_KEY);
            return await _cacheManager.GetAsync(key, () =>
            {
                var query = from tc in _taxCategoryRepository.Table
                            orderby tc.DisplayOrder
                            select tc;
                return query.ToListAsync();
            });
        }

        /// <summary>
        /// Gets a tax category
        /// </summary>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <returns>Tax category</returns>
        public virtual Task<TaxCategory> GetTaxCategoryById(string taxCategoryId)
        {
            string key = string.Format(TAXCATEGORIES_BY_ID_KEY, taxCategoryId);
            return _cacheManager.GetAsync(key, () => _taxCategoryRepository.GetByIdAsync(taxCategoryId));
        }

        /// <summary>
        /// Inserts a tax category
        /// </summary>
        /// <param name="taxCategory">Tax category</param>
        public virtual async Task InsertTaxCategory(TaxCategory taxCategory)
        {
            if (taxCategory == null)
                throw new ArgumentNullException("taxCategory");

            await _taxCategoryRepository.InsertAsync(taxCategory);

            await _cacheManager.RemoveByPattern(TAXCATEGORIES_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(taxCategory);
        }

        /// <summary>
        /// Updates the tax category
        /// </summary>
        /// <param name="taxCategory">Tax category</param>
        public virtual async Task UpdateTaxCategory(TaxCategory taxCategory)
        {
            if (taxCategory == null)
                throw new ArgumentNullException("taxCategory");

            await _taxCategoryRepository.UpdateAsync(taxCategory);

            await _cacheManager.RemoveByPattern(TAXCATEGORIES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(taxCategory);
        }
        #endregion
    }
}
