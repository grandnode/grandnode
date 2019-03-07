using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;

namespace Grand.Core.Caching
{
    public partial class DistributedRedisCache : ICacheManager
    {
        private readonly IDistributedCache _distributedCache;

        public DistributedRedisCache(IDistributedCache distributedCache)
        {
            this._distributedCache = distributedCache;
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

      
        public virtual T Get<T>(string key)
        {

            //get serialized item from cache
            var serializedItem = _distributedCache.GetString(key);
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

        public virtual void Remove(string key)
        {
            _distributedCache.Remove(key);
        }

        public virtual void RemoveByPattern(string pattern)
        {
            Infrastructure.EngineContext.Current.Resolve<IDistributedRedisCacheExtended>().RemoveByPatternAsync(pattern);
        }

        public virtual void Set(string key, object data, int cacheTime)
        {
            if (data == null)
                return;

            //serialize item
            var serializedItem = JsonConvert.SerializeObject(data);

            //and set it to cache
            _distributedCache.SetString(key, serializedItem, GetDistributedCacheEntryOptions(cacheTime));
        }

        public bool IsSet(string key)
        {
            return _distributedCache.Get(key)?.Length > 0;
        }

        public virtual void Clear()
        {
            Infrastructure.EngineContext.Current.Resolve<IDistributedRedisCacheExtended>().ClearAsync();
        }


        public virtual void Dispose()
        {
            //nothing special
        }

        
    }
}
