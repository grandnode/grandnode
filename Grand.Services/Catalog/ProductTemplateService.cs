using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using MediatR;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Product template service
    /// </summary>
    public partial class ProductTemplateService : IProductTemplateService
    {
        #region Fields

        private readonly IRepository<ProductTemplate> _productTemplateRepository;
        private readonly IMediator _mediator;

        #endregion
        
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="productTemplateRepository">Product template repository</param>
        /// <param name="mediator">Mediator</param>
        public ProductTemplateService(IRepository<ProductTemplate> productTemplateRepository,
            IMediator mediator)
        {
            _productTemplateRepository = productTemplateRepository;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete product template
        /// </summary>
        /// <param name="productTemplate">Product template</param>
        public virtual async Task DeleteProductTemplate(ProductTemplate productTemplate)
        {
            if (productTemplate == null)
                throw new ArgumentNullException("productTemplate");

            await _productTemplateRepository.DeleteAsync(productTemplate);

            //event notification
            await _mediator.EntityDeleted(productTemplate);
        }

        /// <summary>
        /// Gets all product templates
        /// </summary>
        /// <returns>Product templates</returns>
        public virtual async Task<IList<ProductTemplate>> GetAllProductTemplates()
        {
            var query = from pt in _productTemplateRepository.Table
                        orderby pt.DisplayOrder
                        select pt;

            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets a product template
        /// </summary>
        /// <param name="productTemplateId">Product template identifier</param>
        /// <returns>Product template</returns>
        public virtual Task<ProductTemplate> GetProductTemplateById(string productTemplateId)
        {
            return _productTemplateRepository.GetByIdAsync(productTemplateId);
        }

        /// <summary>
        /// Inserts product template
        /// </summary>
        /// <param name="productTemplate">Product template</param>
        public virtual async Task InsertProductTemplate(ProductTemplate productTemplate)
        {
            if (productTemplate == null)
                throw new ArgumentNullException("productTemplate");

            await _productTemplateRepository.InsertAsync(productTemplate);

            //event notification
            await _mediator.EntityInserted(productTemplate);
        }

        /// <summary>
        /// Updates the product template
        /// </summary>
        /// <param name="productTemplate">Product template</param>
        public virtual async Task UpdateProductTemplate(ProductTemplate productTemplate)
        {
            if (productTemplate == null)
                throw new ArgumentNullException("productTemplate");

            await _productTemplateRepository.UpdateAsync(productTemplate);

            //event notification
            await _mediator.EntityUpdated(productTemplate);
        }
        
        #endregion
    }
}
