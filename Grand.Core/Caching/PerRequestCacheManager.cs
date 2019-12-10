using Grand.Core.Extensions;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Core.Caching
{
    /// <summary>
    /// Represents a manager for caching during an HTTP request (short term caching)
    /// </summary>
    public partial class PerRequestCacheManager : ICacheManager
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        /// <summary>
        /// Gets a key/value collection that can be used to share data within the scope of this request 
        /// </summary>
        public PerRequestCacheManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets a key/value collection that can be used to share data within the scope of this request 
        /// </summary>
        protected virtual IDictionary<object, object> GetItems()
        {
            return _httpContextAccessor.HttpContext?.Items;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Key of cached item</param>
        /// <returns>The cached value associated with the specified key</returns>
        public virtual Task<T> Get<T>(string key)
        {
            var items = GetItems();
            if (items == null)
                return Task.FromResult(default(T));

            return Task.FromResult((T)items[key]);
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Key of cached item</param>
        /// <returns>The cached value associated with the specified key</returns>
        public virtual (T, bool) TryGetValue<T>(string key)
        {
            var items = GetItems();
            if (items?[key] == null)
                return (default(T), false);

            return ((T)items[key], true);
        }

        /// <summary>
        /// Adds the specified key and object to the cache
        /// </summary>
        /// <param name="key">Key of cached item</param>
        /// <param name="data">Value for caching</param>
        /// <param name="cacheTime">Cache time in minutes</param>
        public virtual Task Set(string key, object data, int cacheTime)
        {
            var items = GetItems();
            if (items == null)
                return Task.CompletedTask;

            if (data != null)
                items[key] = data;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets a value indicating whether the value associated with the specified key is cached
        /// </summary>
        /// <param name="key">Key of cached item</param>
        /// <returns>True if item already is in cache; otherwise false</returns>
        public virtual bool IsSet(string key)
        {
            var items = GetItems();

            return items?[key] != null;
        }

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">Key of cached item</param>
        public virtual Task Remove(string key)
        {
            var items = GetItems();

            items?.Remove(key);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes items by key pattern
        /// </summary>
        /// <param name="pattern">String key pattern</param>
        public virtual async Task RemoveByPattern(string pattern)
        {
            var items = GetItems();
            if (items == null)
                return;

            await this.RemoveByPattern(pattern, items.Keys.Select(p => p.ToString()));
        }

        /// <summary>
        /// Clear all cache data
        /// </summary>
        public virtual Task Clear()
        {
            var items = GetItems();

            items?.Clear();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Dispose cache manager
        /// </summary>
        public virtual void Dispose()
        {
            //nothing special
        }

        #endregion
    }
}