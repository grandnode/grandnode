using Grand.Core.Caching.Message;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Core.Caching.Redis
{
    public class RedisMessageCacheManager : MemoryCacheManager, ICacheManager
    {
        private readonly IMemoryCache _cache;
        private readonly IMessageBus _messageBus;

        public RedisMessageCacheManager(IMemoryCache cache, IMediator mediator, IMessageBus messageBus)
            : base(cache, mediator)
        {
            _cache = cache;
            _messageBus = messageBus;
        }

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">Key of cached item</param>
        public override Task RemoveAsync(string key, bool publisher = true)
        {
            _cache.Remove(RemoveKey(key));

            if (publisher)
                _messageBus.PublishAsync(new MessageEvent() { Key = key, MessageType = (int)MessageEventType.RemoveKey });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes items by key prefix
        /// </summary>
        /// <param name="prefix">String prefix</param>
        /// <param name="publisher">publisher</param>
        public override Task RemoveByPrefix(string prefix, bool publisher = true)
        {
            var keysToRemove = _allKeys.Keys.Where(x => x.ToString().StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.Remove(RemoveKey(key));
            }
            if (publisher)
                _messageBus.PublishAsync(new MessageEvent() { Key = prefix, MessageType = (int)MessageEventType.RemoveByPrefix });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes items by key prefix
        /// </summary>
        /// <param name="prefix">String prefix</param>
        /// <param name="publisher">publisher</param>
        public override Task RemoveByPrefixAsync(string prefix, bool publisher = true)
        {
            return RemoveByPrefix(prefix, publisher);
        }
        ///<summary>
        /// Clear cache
        ///</summary>
        /// <param name="publisher">publisher</param>
        public override Task Clear(bool publisher = true)
        {
            base.Clear();
            if (publisher)
                _messageBus.PublishAsync(new MessageEvent() { Key = "", MessageType = (int)MessageEventType.ClearCache });
            return Task.CompletedTask;
        }
    }
}
