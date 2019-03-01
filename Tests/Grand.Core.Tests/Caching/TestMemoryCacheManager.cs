using Grand.Core.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace Grand.Core.Tests.Caching
{
    public class TestMemoryCacheManager : MemoryCacheManager
    {
        public override void Set(string key, object data, int cacheTime)
        {

        }

        public TestMemoryCacheManager(IMemoryCache cache) : base(cache)
        {
        }
    }
}
