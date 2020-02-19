using System;
using System.Threading.Tasks;

namespace Grand.Core.Caching
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class CacheExtensions
    {        
        /// <summary>
        /// Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="key">Cache key</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>Cached item</returns>
        public static Task<T> GetAsync<T>(this ICacheManager cacheManager, string key, Func<Task<T>> acquire)
        {
            return GetAsync(cacheManager, key, CommonHelper.CacheTimeMinutes, acquire);
        }

        /// <summary>
        /// Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="key">Cache key</param>
        /// <param name="cacheTime">Cache time in minutes (0 - do not cache)</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>Cached item</returns>
        public static async Task<T> GetAsync<T>(this ICacheManager cacheManager, string key, int cacheTime, Func<Task<T>> acquire)
        {
            var value = await cacheManager.TryGetValueAsync<T>(key);
            if (value.FromCache == true)
                return value.Result;

            var result = await acquire();
            if (cacheTime > 0)
                await cacheManager.SetAsync(key, result, cacheTime);
            return result;
        }

        /// <summary>
        /// Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="key">Cache key</param>
        /// <param name="cacheTime">Cache time in minutes (0 - do not cache)</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>Cached item</returns>
        public static T Get<T>(this ICacheManager cacheManager, string key, int cacheTime, Func<T> acquire)
        {
            var value = cacheManager.TryGetValue<T>(key);
            if (value.FromCache == true)
                return value.Result;

            var result = acquire();
            if (cacheTime > 0)
                cacheManager.Set(key, result, cacheTime);

            return result;
        }

        /// <summary>
        /// Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="key">Cache key</param>
        /// <param name="cacheTime">Cache time in minutes (0 - do not cache)</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>Cached item</returns>
        public static T Get<T>(this ICacheManager cacheManager, string key, Func<T> acquire)
        {
            return Get(cacheManager, key, CommonHelper.CacheTimeMinutes, acquire);
        }
    }
}
