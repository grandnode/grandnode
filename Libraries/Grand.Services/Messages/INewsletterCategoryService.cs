using Grand.Core.Domain.Messages;
using System.Collections.Generic;

namespace Grand.Services.Messages
{
    public partial interface INewsletterCategoryService
    {
        /// <summary>
        /// Inserts a newsletter category
        /// </summary>
        /// <param name="NewsletterCategory">NewsletterCategory</param>        
        void InsertNewsletterCategory(NewsletterCategory newslettercategory);

        /// <summary>
        /// Updates a newsletter category
        /// </summary>
        /// <param name="NewsletterCategory">NewsletterCategory</param>
        void UpdateNewsletterCategory(NewsletterCategory newslettercategory);

        /// <summary>
        /// Deleted a newsletter category
        /// </summary>
        /// <param name="NewsletterCategory">NewsletterCategory</param>
        void DeleteNewsletterCategory(NewsletterCategory newslettercategory);

        /// <summary>
        /// Gets a newsletter by category
        /// </summary>
        /// <param name="Id">newsletter category</param>
        /// <returns>Banner</returns>
        NewsletterCategory GetNewsletterCategoryById(string Id);

        /// <summary>
        /// Gets all newsletter categories
        /// </summary>
        /// <returns>NewsletterCategories</returns>
        IList<NewsletterCategory> GetAllNewsletterCategory();

        /// <summary>
        /// Gets all newsletter categories by store
        /// </summary>
        /// <returns>NewsletterCategories</returns>
        IList<NewsletterCategory> GetNewsletterCategoriesByStore(string storeId);

    }
}
