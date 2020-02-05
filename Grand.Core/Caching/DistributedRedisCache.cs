using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Core.Caching
{
    public partial class DistributedRedisCache : ICacheManager
    {
        private readonly IDatabase _distributedCache;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore };

        public DistributedRedisCache(string redisConnectionString)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            _distributedCache = _connectionMultiplexer.GetDatabase(0);
        }

        public virtual async Task<T> GetAsync<T>(string key)
        {
            //get serialized item from cache
            var serializedItem = await _distributedCache.StringGetAsync(key);
            if (string.IsNullOrEmpty(serializedItem))
                return default(T);

            //deserialize item
            var item = JsonConvert.DeserializeObject<T>(serializedItem);
            if (item == null)
                return default(T);

            return item;
        }

        public T Get<T>(string key)
        {
            //get serialized item from cache
            var serializedItem = _distributedCache.StringGet(key);
            if (string.IsNullOrEmpty(serializedItem))
                return default(T);

            //deserialize item
            var item = JsonConvert.DeserializeObject<T>(serializedItem);
            if (item == null)
                return default(T);

            return item;
        }

        public virtual async Task<(T, bool)> TryGetValueAsync<T>(string key)
        {
            var res = await _distributedCache.StringGetAsync(key);
            if (string.IsNullOrEmpty(res.ToString()))
            {
                return (default, res.HasValue);
            }
            else
            {
                return (JsonConvert.DeserializeObject<T>(res), true);
            }
        }

        public (T Result, bool FromCache) TryGetValue<T>(string key)
        {
            var res = _distributedCache.StringGet(key);
            if (string.IsNullOrEmpty(res.ToString()))
            {
                return (default, res.HasValue);
            }
            else
            {
                return (JsonConvert.DeserializeObject<T>(res), true);
            }
        }

        public virtual async Task RemoveAsync(string key)
        {
            await _distributedCache.KeyDeleteAsync(key, CommandFlags.PreferMaster);
        }

        public virtual async Task SetAsync(string key, object data, int cacheTime)
        {
            if (data == null)
                return;

            //serialize item
            var serializedItem = JsonConvert.SerializeObject(data, _jsonSettings);

            //and set it to cache
            await _distributedCache.StringSetAsync(key, serializedItem, TimeSpan.FromMinutes(cacheTime), When.Always, CommandFlags.FireAndForget);
        }

        public void Set(string key, object data, int cacheTime)
        {
            if (data == null)
                return;

            //serialize item
            var serializedItem = JsonConvert.SerializeObject(data, _jsonSettings);

            //and set it to cache
            _distributedCache.StringSet(key, serializedItem, TimeSpan.FromMinutes(cacheTime), When.Always, CommandFlags.FireAndForget);
        }

        public bool IsSet(string key)
        {
            return _distributedCache.KeyExists(key);
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            var keys = new List<RedisKey>();
            foreach (var endPoint in _connectionMultiplexer.GetEndPoints())
            {
                var server = _connectionMultiplexer.GetServer(endPoint);
                keys.AddRange(server.Keys(_distributedCache.Database, $"{prefix}*"));
            }
            await _distributedCache.KeyDeleteAsync(keys.Distinct().ToArray());
        }

        public Task RemoveByPrefix(string prefix)
        {
            return RemoveByPrefixAsync(prefix);
        }

        public virtual async Task Clear()
        {
            foreach (var endPoint in _connectionMultiplexer.GetEndPoints())
            {
                var server = _connectionMultiplexer.GetServer(endPoint);
                await server.FlushDatabaseAsync(0);
            }
        }

        public virtual void Dispose()
        {
            //nothing special
        }
    }
}