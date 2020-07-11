using Grand.Domain.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Category template service interface
    /// </summary>
    public partial interface ICategoryTemplateService
    {
        /// <summary>
        /// Delete category template
        /// </summary>
        /// <param name="categoryTemplate">Category template</param>
        Task DeleteCategoryTemplate(CategoryTemplate categoryTemplate);

        /// <summary>
        /// Gets all category templates
        /// </summary>
        /// <returns>Category templates</returns>
        Task<IList<CategoryTemplate>> GetAllCategoryTemplates();

        /// <summary>
        /// Gets a category template
        /// </summary>
        /// <param name="categoryTemplateId">Category template identifier</param>
        /// <returns>Category template</returns>
        Task<CategoryTemplate> GetCategoryTemplateById(string categoryTemplateId);

        /// <summary>
        /// Inserts category template
        /// </summary>
        /// <param name="categoryTemplate">Category template</param>
        Task InsertCategoryTemplate(CategoryTemplate categoryTemplate);

        /// <summary>
        /// Updates the category template
        /// </summary>
        /// <param name="categoryTemplate">Category template</param>
        Task UpdateCategoryTemplate(CategoryTemplate categoryTemplate);
    }
}
