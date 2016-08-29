using System;
using System.Linq;
using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Common;
using Grand.Services.Events;

namespace Grand.Services.Common
{
    /// <summary>
    /// Search term service
    /// </summary>
    public partial class SearchTermService : ISearchTermService
    {
        #region Fields

        private readonly IRepository<SearchTerm> _searchTermRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public SearchTermService(IRepository<SearchTerm> searchTermRepository,
            IEventPublisher eventPublisher)
        {
            this._searchTermRepository = searchTermRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a search term record
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        public virtual void DeleteSearchTerm(SearchTerm searchTerm)
        {
            if (searchTerm == null)
                throw new ArgumentNullException("searchTerm");

            _searchTermRepository.Delete(searchTerm);

            //event notification
            _eventPublisher.EntityDeleted(searchTerm);
        }

        /// <summary>
        /// Gets a search term record by identifier
        /// </summary>
        /// <param name="searchTermId">Search term identifier</param>
        /// <returns>Search term</returns>
        public virtual SearchTerm GetSearchTermById(string searchTermId)
        {
            return _searchTermRepository.GetById(searchTermId);
        }

        /// <summary>
        /// Gets a search term record by keyword
        /// </summary>
        /// <param name="keyword">Search term keyword</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Search term</returns>
        public virtual SearchTerm GetSearchTermByKeyword(string keyword, string storeId)
        {
            if (String.IsNullOrEmpty(keyword))
                return null;

            var query = from st in _searchTermRepository.Table
                        where st.Keyword == keyword && st.StoreId == storeId
                        orderby st.Id
                        select st;
            var searchTerm = query.FirstOrDefault();
            return searchTerm;
        }

        /// <summary>
        /// Gets a search term statistics
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>A list search term report lines</returns>
        public virtual IPagedList<SearchTermReportLine> GetStats(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = (from st in _searchTermRepository.Table
                        group st by st.Keyword into groupedResult
                        select new
                        {
                            Keyword = groupedResult.Key,
                            Count = groupedResult.Sum(o => o.Count)
                        })
                        .OrderByDescending(m => m.Count)
                        .Select(r => new SearchTermReportLine
                        {
                            Keyword = r.Keyword,
                            Count = r.Count
                        });

            var result = new PagedList<SearchTermReportLine>(query, pageIndex, pageSize);
            return result;
        }

        /// <summary>
        /// Inserts a search term record
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        public virtual void InsertSearchTerm(SearchTerm searchTerm)
        {
            if (searchTerm == null)
                throw new ArgumentNullException("searchTerm");

            _searchTermRepository.Insert(searchTerm);

            //event notification
            _eventPublisher.EntityInserted(searchTerm);
        }

        /// <summary>
        /// Updates the search term record
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        public virtual void UpdateSearchTerm(SearchTerm searchTerm)
        {
            if (searchTerm == null)
                throw new ArgumentNullException("searchTerm");

            _searchTermRepository.Update(searchTerm);

            //event notification
            _eventPublisher.EntityUpdated(searchTerm);
        }
        
        #endregion
    }
}