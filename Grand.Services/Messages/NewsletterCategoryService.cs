using Grand.Core.Data;
using Grand.Core.Domain.Messages;
using Grand.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Messages
{
    public partial class NewsletterCategoryService: INewsletterCategoryService
    {
        #region Fields

        private readonly IRepository<NewsletterCategory> _newsletterCategoryRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public NewsletterCategoryService(IRepository<NewsletterCategory> newsletterCategoryRepository, IEventPublisher eventPublisher)
        {
            this._newsletterCategoryRepository = newsletterCategoryRepository;
            this._eventPublisher = eventPublisher;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Inserts a newsletter category
        /// </summary>
        /// <param name="NewsletterCategory">NewsletterCategory</param>        
        public void InsertNewsletterCategory(NewsletterCategory newslettercategory)
        {
            if (newslettercategory == null)
                throw new ArgumentNullException("newslettercategory");

            _newsletterCategoryRepository.Insert(newslettercategory);

            //event notification
            _eventPublisher.EntityInserted(newslettercategory);
        }

        /// <summary>
        /// Updates a newsletter category
        /// </summary>
        /// <param name="NewsletterCategory">NewsletterCategory</param>
        public void UpdateNewsletterCategory(NewsletterCategory newslettercategory)
        {
            if (newslettercategory == null)
                throw new ArgumentNullException("newslettercategory");

            _newsletterCategoryRepository.Update(newslettercategory);

            //event notification
            _eventPublisher.EntityUpdated(newslettercategory);

        }

        /// <summary>
        /// Deleted a newsletter category
        /// </summary>
        /// <param name="NewsletterCategory">NewsletterCategory</param>
        public void DeleteNewsletterCategory(NewsletterCategory newslettercategory)
        {
            if (newslettercategory == null)
                throw new ArgumentNullException("newslettercategory");

            _newsletterCategoryRepository.Delete(newslettercategory);

            //event notification
            _eventPublisher.EntityDeleted(newslettercategory);

        }

        /// <summary>
        /// Gets a newsletter by category
        /// </summary>
        /// <param name="Id">newsletter category</param>
        /// <returns>Banner</returns>
        public NewsletterCategory GetNewsletterCategoryById(string id)
        {

            if (id == null)
                throw new ArgumentNullException("id");

            return _newsletterCategoryRepository.Table.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Gets all newsletter categories
        /// </summary>
        /// <returns>NewsletterCategories</returns>
        public IList<NewsletterCategory> GetAllNewsletterCategory()
        {
            return _newsletterCategoryRepository.Table.ToList();
        }

        /// <summary>
        /// Gets all newsletter categories by store
        /// </summary>
        /// <returns>NewsletterCategories</returns>
        public IList<NewsletterCategory> GetNewsletterCategoriesByStore(string storeId)
        {
            var query = from p in _newsletterCategoryRepository.Table
                        where !p.LimitedToStores || p.Stores.Contains(storeId)
                        select p;
            return query.ToList();
        }

        #endregion

    }
}
