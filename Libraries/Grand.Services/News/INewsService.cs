using System.Collections.Generic;
using Grand.Core;
using Grand.Core.Domain.News;

namespace Grand.Services.News
{
    /// <summary>
    /// News service interface
    /// </summary>
    public partial interface INewsService
    {
        /// <summary>
        /// Deletes a news
        /// </summary>
        /// <param name="newsItem">News item</param>
        void DeleteNews(NewsItem newsItem);

        /// <summary>
        /// Gets a news
        /// </summary>
        /// <param name="newsId">The news identifier</param>
        /// <returns>News</returns>
        NewsItem GetNewsById(string newsId);

        /// <summary>
        /// Gets all news
        /// </summary>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News items</returns>
        IPagedList<NewsItem> GetAllNews(string languageId = "", string storeId = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool ignorAcl = false, bool showHidden = false);

        /// <summary>
        /// Inserts a news item
        /// </summary>
        /// <param name="news">News item</param>
        void InsertNews(NewsItem news);

        /// <summary>
        /// Updates the news item
        /// </summary>
        /// <param name="news">News item</param>
        void UpdateNews(NewsItem news);

        /// <summary>
        /// Gets all comments
        /// </summary>
        /// <param name="customerId">Customer identifier; 0 to load all records</param>
        /// <returns>Comments</returns>
        IList<NewsComment> GetAllComments(string customerId);

        
        
    }
}
