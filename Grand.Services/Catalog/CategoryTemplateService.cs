using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Services.Events;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Category template service
    /// </summary>
    public partial class CategoryTemplateService : ICategoryTemplateService
    {
        #region Fields

        private readonly IRepository<CategoryTemplate> _categoryTemplateRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion
        
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="categoryTemplateRepository">Category template repository</param>
        /// <param name="eventPublisher">Event published</param>
        public CategoryTemplateService(IRepository<CategoryTemplate> categoryTemplateRepository, 
            IEventPublisher eventPublisher)
        {
            this._categoryTemplateRepository = categoryTemplateRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete category template
        /// </summary>
        /// <param name="categoryTemplate">Category template</param>
        public virtual void DeleteCategoryTemplate(CategoryTemplate categoryTemplate)
        {
            if (categoryTemplate == null)
                throw new ArgumentNullException("categoryTemplate");

            _categoryTemplateRepository.Delete(categoryTemplate);

            //event notification
            _eventPublisher.EntityDeleted(categoryTemplate);
        }

        /// <summary>
        /// Gets all category templates
        /// </summary>
        /// <returns>Category templates</returns>
        public virtual IList<CategoryTemplate> GetAllCategoryTemplates()
        {
            return _categoryTemplateRepository.Collection.AsQueryable().OrderBy(x => x.DisplayOrder).ToList();
        }
 
        /// <summary>
        /// Gets a category template
        /// </summary>
        /// <param name="categoryTemplateId">Category template identifier</param>
        /// <returns>Category template</returns>
        public virtual CategoryTemplate GetCategoryTemplateById(string categoryTemplateId)
        {
            return _categoryTemplateRepository.GetById(categoryTemplateId);
        }

        /// <summary>
        /// Inserts category template
        /// </summary>
        /// <param name="categoryTemplate">Category template</param>
        public virtual void InsertCategoryTemplate(CategoryTemplate categoryTemplate)
        {
            if (categoryTemplate == null)
                throw new ArgumentNullException("categoryTemplate");

            _categoryTemplateRepository.Insert(categoryTemplate);

            //event notification
            _eventPublisher.EntityInserted(categoryTemplate);
        }

        /// <summary>
        /// Updates the category template
        /// </summary>
        /// <param name="categoryTemplate">Category template</param>
        public virtual void UpdateCategoryTemplate(CategoryTemplate categoryTemplate)
        {
            if (categoryTemplate == null)
                throw new ArgumentNullException("categoryTemplate");

            _categoryTemplateRepository.Update(categoryTemplate);

            //event notification
            _eventPublisher.EntityUpdated(categoryTemplate);
        }
        
        #endregion
    }
}
