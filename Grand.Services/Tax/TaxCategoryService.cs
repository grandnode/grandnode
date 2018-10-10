using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Tax;
using Grand.Services.Events;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<Product> _productRepository;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="taxCategoryRepository">Tax category repository</param>
        /// <param name="eventPublisher">Event published</param>
        public TaxCategoryService(ICacheManager cacheManager,
            IRepository<TaxCategory> taxCategoryRepository,
            IEventPublisher eventPublisher, IRepository<Product> productRepository)
        {
            _cacheManager = cacheManager;
            _taxCategoryRepository = taxCategoryRepository;
            _eventPublisher = eventPublisher;
            _productRepository = productRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a tax category
        /// </summary>
        /// <param name="taxCategory">Tax category</param>
        public virtual void DeleteTaxCategory(TaxCategory taxCategory)
        {
            if (taxCategory == null)
                throw new ArgumentNullException("taxCategory");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.TaxCategoryId, taxCategory.Id);
            var update = Builders<Product>.Update
                .Set(x => x.TaxCategoryId, "");
            var result = _productRepository.Collection.UpdateManyAsync(filter, update).Result;

            _taxCategoryRepository.Delete(taxCategory);

            _cacheManager.RemoveByPattern(TAXCATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(taxCategory);
        }

        /// <summary>
        /// Gets all tax categories
        /// </summary>
        /// <returns>Tax categories</returns>
        public virtual IList<TaxCategory> GetAllTaxCategories()
        {
            string key = string.Format(TAXCATEGORIES_ALL_KEY);
            return _cacheManager.Get(key, () =>
            {
                var query = from tc in _taxCategoryRepository.Table
                            orderby tc.DisplayOrder
                            select tc;
                var taxCategories = query.ToList();
                return taxCategories;
            });
        }

        /// <summary>
        /// Gets a tax category
        /// </summary>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <returns>Tax category</returns>
        public virtual TaxCategory GetTaxCategoryById(string taxCategoryId)
        {
            string key = string.Format(TAXCATEGORIES_BY_ID_KEY, taxCategoryId);
            return _cacheManager.Get(key, () => _taxCategoryRepository.GetById(taxCategoryId));
        }

        /// <summary>
        /// Inserts a tax category
        /// </summary>
        /// <param name="taxCategory">Tax category</param>
        public virtual void InsertTaxCategory(TaxCategory taxCategory)
        {
            if (taxCategory == null)
                throw new ArgumentNullException("taxCategory");

            _taxCategoryRepository.Insert(taxCategory);

            _cacheManager.RemoveByPattern(TAXCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(taxCategory);
        }

        /// <summary>
        /// Updates the tax category
        /// </summary>
        /// <param name="taxCategory">Tax category</param>
        public virtual void UpdateTaxCategory(TaxCategory taxCategory)
        {
            if (taxCategory == null)
                throw new ArgumentNullException("taxCategory");

            _taxCategoryRepository.Update(taxCategory);

            _cacheManager.RemoveByPattern(TAXCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(taxCategory);
        }
        #endregion
    }
}
