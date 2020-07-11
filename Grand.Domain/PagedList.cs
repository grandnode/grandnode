using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Domain
{
    /// <summary>
    /// Paged list
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    [Serializable]
    public class PagedList<T> : List<T>, IPagedList<T> 
    {
        private async Task InitializeAsync(IMongoQueryable<T> source, int pageIndex, int pageSize, int? totalCount = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (pageSize <= 0)
                throw new ArgumentException("pageSize must be greater than zero");

            TotalCount = totalCount ?? await source.CountAsync();
            source = totalCount == null ? source.Skip(pageIndex * pageSize).Take(pageSize) : source;
            AddRange(source);
            
            if (pageSize > 0)
            {
                TotalPages = TotalCount / pageSize;
                if (TotalCount % pageSize > 0)
                    TotalPages++;
            }

            PageSize = pageSize;
            PageIndex = pageIndex;
        }

        private async Task InitializeAsync(IMongoCollection<T> source, FilterDefinition<T> filterdefinition, SortDefinition<T> sortdefinition, int pageIndex, int pageSize)
        {
            TotalCount = (int) await source.CountDocumentsAsync(filterdefinition);
            AddRange(await source.Find(filterdefinition).Sort(sortdefinition).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync());
            if (pageSize > 0)
            {
                TotalPages = TotalCount / pageSize;
                if (TotalCount % pageSize > 0)
                    TotalPages++;
            }
            PageSize = pageSize;
            PageIndex = pageIndex;
        }

        private async Task InitializeAsync(IAggregateFluent<T> source, int pageIndex, int pageSize)
        {
            var range = source.Skip(pageIndex * pageSize).Limit(pageSize + 1).ToList();
            int total = range.Count > pageSize ? range.Count : pageSize;
            TotalCount = (await source.ToListAsync()).Count;
            if (pageSize > 0)
                TotalPages = total / pageSize;

            if (total % pageSize > 0)
                TotalPages++;

            PageSize = pageSize;
            PageIndex = pageIndex;
            AddRange(range.Take(pageSize));
        }

        private void Initialize(IEnumerable<T> source, int pageIndex, int pageSize, int? totalCount = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (pageSize <= 0)
                throw new ArgumentException("pageSize must be greater than zero");

            TotalCount = totalCount ?? source.Count();

            if (pageSize > 0)
            {
                TotalPages = TotalCount / pageSize;
                if (TotalCount % pageSize > 0)
                    TotalPages++;
            }

            PageSize = pageSize;
            PageIndex = pageIndex;
            source = totalCount == null ? source.Skip(pageIndex * pageSize).Take(pageSize) : source;
            AddRange(source);
        }

        public static async Task<PagedList<T>> Create(IMongoCollection<T> source, FilterDefinition<T> filterdefinition, SortDefinition<T> sortdefinition, int pageIndex, int pageSize)
        {
            var pagelist = new PagedList<T>();
            await pagelist.InitializeAsync(source, filterdefinition, sortdefinition, pageIndex, pageSize);
            return pagelist;
        }

        public static async Task<PagedList<T>> Create(IMongoQueryable<T> source, int pageIndex, int pageSize)
        {
            var pagelist = new PagedList<T>();
            await pagelist.InitializeAsync(source, pageIndex, pageSize);
            return pagelist;
        }
        public static async Task<PagedList<T>> Create(IAggregateFluent<T> source, int pageIndex, int pageSize)
        {
            var pagelist = new PagedList<T>();
            await pagelist.InitializeAsync(source, pageIndex, pageSize);
            return pagelist;
        }

        public PagedList()
        {
        }
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            Initialize(source, pageIndex, pageSize);
        }
        
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            Initialize(source, pageIndex, pageSize, totalCount);
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
