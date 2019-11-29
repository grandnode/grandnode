using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Grand.Core.Caching
{
    public partial class DistributedRedisCache : ICacheManager
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IDistributedRedisCacheExtended _distributedRedisCacheExtended;

        public DistributedRedisCache(IDistributedCache distributedCache, IDistributedRedisCacheExtended distributedRedisCacheExtended)
        {
            _distributedCache = distributedCache;
            _distributedRedisCacheExtended = distributedRedisCacheExtended;
        }

        /// <summary>
        /// Create entry options to item of redis cache 
        /// </summary>
        /// <param name="cacheTime">Cache time in minutes</param>
        /// <returns></returns>
        protected DistributedCacheEntryOptions GetDistributedCacheEntryOptions(int cacheTime)
        {
            return new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(cacheTime));
        }

      
        public virtual async Task<T> Get<T>(string key)
        {
            //get serialized item from cache
            var serializedItem = await _distributedCache.GetStringAsync(key);
            if (string.IsNullOrEmpty(serializedItem))
                return default(T);

            //deserialize item
            var item = JsonConvert.DeserializeObject<T>(serializedItem);
            if (item == null)
                return default(T);

            return item;
        }

        public virtual (T, bool) TryGetValue<T>(string key)
        {
            byte[] result;
            try
            {
                result = _distributedCache.Get(key);
                if (result == null)
                    return (default(T), false);
            }
            catch
            {
                return (default(T), false);
            }

            //get serialized item from cache
            var serializedItem = System.Text.Encoding.Default.GetString(result);
            if (string.IsNullOrEmpty(serializedItem))
                return (default(T), true);

            //deserialize item
            var item = JsonConvert.DeserializeObject<T>(serializedItem);
            if (item == null)
                return (default(T), true);

            return (item, true);
        }

        public virtual async Task Remove(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }

        public virtual async Task RemoveByPattern(string pattern)
        {
            await _distributedRedisCacheExtended.RemoveByPatternAsync(pattern);
        }

        public virtual async Task Set(string key, object data, int cacheTime)
        {
            if (data == null)
                return;

            //serialize item
            var serializedItem = JsonConvert.SerializeObject(data);

            //and set it to cache
            await _distributedCache.SetStringAsync(key, serializedItem, GetDistributedCacheEntryOptions(cacheTime));
        }

        public bool IsSet(string key)
        {
            return _distributedCache.Get(key)?.Length > 0;
        }

        public virtual async Task Clear()
        {
            await _distributedRedisCacheExtended.ClearAsync();
        }


        public virtual void Dispose()
        {
            //nothing special
        }

        
    }
}
