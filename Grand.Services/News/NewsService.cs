using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.News;
using Grand.Services.Events;
using Grand.Services.Customers;
using MongoDB.Driver.Linq;

namespace Grand.Services.News
{
    /// <summary>
    /// News service
    /// </summary>
    public partial class NewsService : INewsService
    {
        #region Fields

        private readonly IRepository<NewsItem> _newsItemRepository;
        //private readonly IRepository<NewsComment> _newsCommentRepository;
        private readonly CatalogSettings _catalogSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public NewsService(IRepository<NewsItem> newsItemRepository, 
            //IRepository<NewsComment> newsCommentRepository,
            CatalogSettings catalogSettings,
            IEventPublisher eventPublisher,
            IWorkContext workContext)
        {
            this._newsItemRepository = newsItemRepository;
            //this._newsCommentRepository = newsCommentRepository;
            this._catalogSettings = catalogSettings;
            this._eventPublisher = eventPublisher;
            this._workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a news
        /// </summary>
        /// <param name="newsItem">News item</param>
        public virtual void DeleteNews(NewsItem newsItem)
        {
            if (newsItem == null)
                throw new ArgumentNullException("newsItem");

            _newsItemRepository.Delete(newsItem);
            
            //event notification
            _eventPublisher.EntityDeleted(newsItem);
        }

        /// <summary>
        /// Gets a news
        /// </summary>
        /// <param name="newsId">The news identifier</param>
        /// <returns>News</returns>
        public virtual NewsItem GetNewsById(string newsId)
        {
            return _newsItemRepository.GetById(newsId);
        }

        /// <summary>
        /// Gets all news
        /// </summary>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News items</returns>
        public virtual IPagedList<NewsItem> GetAllNews(string storeId = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool ignorAcl = false, bool showHidden = false)
        {
            var query = _newsItemRepository.Table;

            if (!showHidden)
            {
                var utcNow = DateTime.UtcNow;
                query = query.Where(n => n.Published);
                query = query.Where(n => !n.StartDateUtc.HasValue || n.StartDateUtc <= utcNow);
                query = query.Where(n => !n.EndDateUtc.HasValue || n.EndDateUtc >= utcNow);
            }

            if ((!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations) ||
                    (!ignorAcl && !_catalogSettings.IgnoreAcl))
            {
                if (!ignorAcl && !_catalogSettings.IgnoreAcl)
                {
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                    query = from p in query
                            where !p.SubjectToAcl || allowedCustomerRolesIds.Any(x => p.CustomerRoles.Contains(x))
                            select p;
                }
                //Store mapping
                if (!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)
                {
                    query = from p in query
                            where !p.LimitedToStores || p.Stores.Contains(storeId)
                            select p;
                }
            }
            query = query.OrderByDescending(n => n.CreatedOnUtc);

            var news = new PagedList<NewsItem>(query, pageIndex, pageSize);
            return news;
        }

        /// <summary>
        /// Inserts a news item
        /// </summary>
        /// <param name="news">News item</param>
        public virtual void InsertNews(NewsItem news)
        {
            if (news == null)
                throw new ArgumentNullException("news");

            _newsItemRepository.Insert(news);

            //event notification
            _eventPublisher.EntityInserted(news);
        }

        /// <summary>
        /// Updates the news item
        /// </summary>
        /// <param name="news">News item</param>
        public virtual void UpdateNews(NewsItem news)
        {
            if (news == null)
                throw new ArgumentNullException("news");

            _newsItemRepository.Update(news);
            
            //event notification
            _eventPublisher.EntityUpdated(news);
        }
        
        /// <summary>
        /// Gets all comments
        /// </summary>
        /// <param name="customerId">Customer identifier; 0 to load all records</param>
        /// <returns>Comments</returns>
        public virtual IList<NewsComment> GetAllComments(string customerId)
        {
            var query = from n in _newsItemRepository.Table
                        from c in n.NewsComments
                        select c;

            var query2 = from c in query
                         orderby c.CreatedOnUtc
                         where (customerId == "" || c.CustomerId == customerId)
                         select c;

            var content = query2.ToList();
            return content;
        }

       
        #endregion
    }
}
