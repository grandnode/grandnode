using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Category template service
    /// </summary>
    public partial class CategoryTemplateService : ICategoryTemplateService
    {
        #region Fields

        private readonly IRepository<CategoryTemplate> _categoryTemplateRepository;
        private readonly IMediator _mediator;

        #endregion
        
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="categoryTemplateRepository">Category template repository</param>
        /// <param name="mediator">Mediator</param>
        public CategoryTemplateService(IRepository<CategoryTemplate> categoryTemplateRepository, 
            IMediator mediator)
        {
            _categoryTemplateRepository = categoryTemplateRepository;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete category template
        /// </summary>
        /// <param name="categoryTemplate">Category template</param>
        public virtual async Task DeleteCategoryTemplate(CategoryTemplate categoryTemplate)
        {
            if (categoryTemplate == null)
                throw new ArgumentNullException("categoryTemplate");

            await _categoryTemplateRepository.DeleteAsync(categoryTemplate);

            //event notification
            await _mediator.EntityDeleted(categoryTemplate);
        }

        /// <summary>
        /// Gets all category templates
        /// </summary>
        /// <returns>Category templates</returns>
        public virtual async Task<IList<CategoryTemplate>> GetAllCategoryTemplates()
        {
            var query = await _categoryTemplateRepository.Table.ToListAsync();
            return query.OrderBy(x => x.DisplayOrder).ToList();
        }
 
        /// <summary>
        /// Gets a category template
        /// </summary>
        /// <param name="categoryTemplateId">Category template identifier</param>
        /// <returns>Category template</returns>
        public virtual Task<CategoryTemplate> GetCategoryTemplateById(string categoryTemplateId)
        {
            return _categoryTemplateRepository.GetByIdAsync(categoryTemplateId);
        }

        /// <summary>
        /// Inserts category template
        /// </summary>
        /// <param name="categoryTemplate">Category template</param>
        public virtual async Task InsertCategoryTemplate(CategoryTemplate categoryTemplate)
        {
            if (categoryTemplate == null)
                throw new ArgumentNullException("categoryTemplate");

            await _categoryTemplateRepository.InsertAsync(categoryTemplate);

            //event notification
            await _mediator.EntityInserted(categoryTemplate);
        }

        /// <summary>
        /// Updates the category template
        /// </summary>
        /// <param name="categoryTemplate">Category template</param>
        public virtual async Task UpdateCategoryTemplate(CategoryTemplate categoryTemplate)
        {
            if (categoryTemplate == null)
                throw new ArgumentNullException("categoryTemplate");

            await _categoryTemplateRepository.UpdateAsync(categoryTemplate);

            //event notification
            await _mediator.EntityUpdated(categoryTemplate);
        }
        
        #endregion
    }
}
