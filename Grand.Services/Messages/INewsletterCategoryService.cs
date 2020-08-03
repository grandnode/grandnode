using Grand.Domain.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    public partial interface INewsletterCategoryService
    {
        /// <summary>
        /// Inserts a newsletter category
        /// </summary>
        /// <param name="NewsletterCategory">NewsletterCategory</param>        
        Task InsertNewsletterCategory(NewsletterCategory newslettercategory);

        /// <summary>
        /// Updates a newsletter category
        /// </summary>
        /// <param name="NewsletterCategory">NewsletterCategory</param>
        Task UpdateNewsletterCategory(NewsletterCategory newslettercategory);

        /// <summary>
        /// Deleted a newsletter category
        /// </summary>
        /// <param name="NewsletterCategory">NewsletterCategory</param>
        Task DeleteNewsletterCategory(NewsletterCategory newslettercategory);

        /// <summary>
        /// Gets a newsletter by category
        /// </summary>
        /// <param name="Id">newsletter category</param>
        /// <returns>Banner</returns>
        Task<NewsletterCategory> GetNewsletterCategoryById(string Id);

        /// <summary>
        /// Gets all newsletter categories
        /// </summary>
        /// <returns>NewsletterCategories</returns>
        Task<IList<NewsletterCategory>> GetAllNewsletterCategory();

        /// <summary>
        /// Gets all newsletter categories by store
        /// </summary>
        /// <returns>NewsletterCategories</returns>
        Task<IList<NewsletterCategory>> GetNewsletterCategoriesByStore(string storeId);

    }
}
