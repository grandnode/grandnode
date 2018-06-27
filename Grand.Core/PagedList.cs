using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Core
{
    /// <summary>
    /// Paged list
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    [Serializable]
    public class PagedList<T> : List<T>, IPagedList<T> 
    {
       
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
        public PagedList(IAggregateFluent<T> source, int pageIndex, int pageSize)
        {
            var range = source.Skip(pageIndex * pageSize).Limit(pageSize+1).ToList();
            int total = range.Count > pageSize ? range.Count : pageSize;
            this.TotalCount = source.ToListAsync().Result.Count;
            if(pageSize > 0)
                this.TotalPages = total / pageSize;

            if (total % pageSize > 0)
                TotalPages++;

            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.AddRange(range.Take(pageSize));
        }

        public PagedList(IMongoCollection<T> source, FilterDefinition<T> filterdefinition, SortDefinition<T> sortdefinition, int pageIndex, int pageSize)
        {
            TotalCount = (int)source.CountDocuments(filterdefinition);
            AddRange(source.Find(filterdefinition).Sort(sortdefinition).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync().Result);
            if (pageSize > 0)
            {
                TotalPages = TotalCount / pageSize;
                if (TotalCount % pageSize > 0)
                    TotalPages++;
            }
            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
        }

        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            Init(source, pageIndex, pageSize, totalCount);
        }       

        private void Init(IMongoQueryable<T> source, int pageIndex, int pageSize, int? totalCount = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (pageSize <= 0)
                throw new ArgumentException("pageSize must be greater than zero");

            var taskCount = source.CountAsync();
            source = totalCount == null ? source.Skip(pageIndex * pageSize).Take(pageSize) : source;
            AddRange(source);
            taskCount.Wait();
            TotalCount = totalCount ?? (int)taskCount.Result;
            if (pageSize > 0)
            {
                TotalPages = TotalCount / pageSize;
                if (TotalCount % pageSize > 0)
                    TotalPages++;
            }

            PageSize = pageSize;
            PageIndex = pageIndex;
        }
        private void Init(IEnumerable<T> source, int pageIndex, int pageSize, int? totalCount = null)
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
