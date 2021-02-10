using Grand.Core.Events;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Core.Caching
{
    /// <summary>
    /// Represents a manager for memory caching
    /// </summary>
    public partial class MemoryCacheBase : ICacheBase
    {
        #region Fields

        private readonly IMemoryCache _cache;
        private readonly IMediator _mediator;

        private bool _disposed;
        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();

        protected static readonly ConcurrentDictionary<string, bool> _allCacheKeys;

        #endregion

        #region Ctor

        static MemoryCacheBase()
        {
            _allCacheKeys = new ConcurrentDictionary<string, bool>();
        }

        public MemoryCacheBase(IMemoryCache cache, IMediator mediator)
        {
            _cache = cache;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        public virtual async Task<T> GetAsync<T>(string key, Func<Task<T>> acquire)
        {
            return await _cache.GetOrCreateAsync(key, entry =>
            {
                AddKey(key);
                entry.SetOptions(GetMemoryCacheEntryOptions(CommonHelper.CacheTimeMinutes));
                return acquire();
            });
        }

        public T Get<T>(string key, Func<T> acquire)
        {
            return _cache.GetOrCreate(key, entry =>
            {
                AddKey(key);
                entry.SetOptions(GetMemoryCacheEntryOptions(CommonHelper.CacheTimeMinutes));
                return acquire();
            });
        }
        public virtual Task SetAsync(string key, object data, int cacheTime)
        {
            if (data != null)
            {
                _cache.Set(AddKey(key), data, GetMemoryCacheEntryOptions(cacheTime));
            }
            return Task.CompletedTask;
        }

        public virtual Task RemoveAsync(string key, bool publisher = true)
        {
            _cache.Remove(key);
            if (publisher)
                _mediator.Publish(new EntityCacheEvent(key, CacheEvent.RemoveKey));

            return Task.CompletedTask;
        }

        public virtual Task RemoveByPrefix(string prefix, bool publisher = true)
        {
            var keysToRemove = _allCacheKeys.Keys.Where(x => x.ToString().StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
            if (publisher)
                _mediator.Publish(new EntityCacheEvent(prefix, CacheEvent.RemovePrefix));

            return Task.CompletedTask;
        }

        public virtual Task Clear(bool publisher = true)
        {
            //clear key
            ClearKeys();

            //cancel
            _resetCacheToken.Cancel();
            //dispose
            _resetCacheToken.Dispose();

            _resetCacheToken = new CancellationTokenSource();

            return Task.CompletedTask;
        }

        ~MemoryCacheBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cache.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion

        #region Utilities

        protected MemoryCacheEntryOptions GetMemoryCacheEntryOptions(int cacheTime)
        {
            var options = new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTime) }
                .AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token))
                .RegisterPostEvictionCallback(PostEvictionCallback);

            return options;
        }


        protected string AddKey(string key)
        {
            _allCacheKeys.TryAdd(key, true);
            return key;
        }

        protected string RemoveKey(string key)
        {
            _allCacheKeys.TryRemove(key, out _);
            return key;
        }


        private void ClearKeys()
        {
            _allCacheKeys.Clear();
        }

        private void PostEvictionCallback(object key, object value, EvictionReason reason, object state)
        {
            if (reason == EvictionReason.Replaced)
                return;

            if (reason == EvictionReason.TokenExpired)
                return;

            RemoveKey(key.ToString());
        }

        #endregion

    }

}