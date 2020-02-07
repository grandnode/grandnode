using Grand.Core.Caching;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace Grand.Core.Tests.Caching
{
    public class TestMemoryCacheManager : MemoryCacheManager
    {
        public override Task SetAsync(string key, object data, int cacheTime)
        {
            return Task.CompletedTask;
        }

        public TestMemoryCacheManager(IMemoryCache cache, IMediator mediator) : base(cache, mediator)
        {
        }
    }
}
