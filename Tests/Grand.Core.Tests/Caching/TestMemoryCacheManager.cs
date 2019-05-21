using Grand.Core.Caching;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace Grand.Core.Tests.Caching
{
    public class TestMemoryCacheManager : MemoryCacheManager
    {
        public override Task Set(string key, object data, int cacheTime)
        {
            return Task.CompletedTask;
        }

        public TestMemoryCacheManager(IMemoryCache cache) : base(cache)
        {
        }
    }
}
