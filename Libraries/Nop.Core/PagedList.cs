using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Core
{
    /// <summary>
    /// Paged list
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    [Serializable]
    public class PagedList<T> : List<T>, IPagedList<T> 
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            Init(source, pageIndex, pageSize);
        }
        public PagedList()
        {
        }
        public PagedList(IMongoQueryable<T> source, int pageIndex, int pageSize)
        {
            Init(source, pageIndex, pageSize);
        }

        public PagedList(IMongoQueryable<T> source, int pageIndex, int pageSize, bool product)
        {
            var products = source.Skip(pageIndex * pageSize).Take(pageSize+1).ToList();
            int count = products.Count();
            int total = count + (pageIndex * pageSize); 
            this.TotalCount = total;
            this.TotalPages = total / pageSize;

            if (total % pageSize > 0)
                TotalPages++;

            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.AddRange(products.Take(pageSize));
        }

        public PagedList(IAggregateFluent<T> source, int pageIndex, int pageSize)
        {
            var range = source.Skip(pageIndex * pageSize).Limit(pageSize+1).ToList();
            int total = range.Count > pageSize ? range.Count : pageSize;
            this.TotalCount = total;
            this.TotalPages = total / pageSize;

            if (total % pageSize > 0)
                TotalPages++;

            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.AddRange(range.Take(pageSize));
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        public PagedList(IList<T> source, int pageIndex, int pageSize)
        {
            Init(source, pageIndex, pageSize);
        }

        public PagedList(IMongoCollection<T> source, FilterDefinition<T> filterdefinition, SortDefinition<T> sortdefinition, int pageIndex, int pageSize)
        {
            var task = source.CountAsync(filterdefinition);
            task.Wait();
            TotalCount = (int)task.Result;
            TotalPages = TotalCount / pageSize;

            if (TotalCount % pageSize > 0)
                TotalPages++;

            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.AddRange(source.Find(filterdefinition).Sort(sortdefinition).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync().Result);
        }
        public PagedList(IMongoCollection<T> source, FilterDefinition<T> filterdefinition, int pageIndex, int pageSize)
        {
            var task = source.CountAsync(filterdefinition);
            task.Wait();
            TotalCount = (int)task.Result;
            TotalPages = TotalCount / pageSize;

            if (TotalCount % pageSize > 0)
                TotalPages++;

            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.AddRange(source.Find(filterdefinition).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync().Result);
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalCount">Total count</param>
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            Init(source, pageIndex, pageSize, totalCount);
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalCount">Total count</param>
        private void Init(IEnumerable<T> source, int pageIndex, int pageSize, int? totalCount = null)
        {
            TotalCount = totalCount ?? source.Count();
            TotalPages = TotalCount / pageSize;

            if (TotalCount % pageSize > 0)
                TotalPages++;

            PageSize = pageSize;
            PageIndex = pageIndex;
            AddRange(source.Skip(pageIndex * pageSize).Take(pageSize));
        }

        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }

        public bool HasPreviousPage
        {
            get { return (PageIndex > 0); }
        }
        public bool HasNextPage
        {
            get { return (PageIndex + 1 < TotalPages); }
        }
    }
}
