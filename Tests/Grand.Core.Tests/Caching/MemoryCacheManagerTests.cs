using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Grand.Core.Caching.Tests
{
    [TestClass()]
    public class MemoryCacheManagerTests
    {
        [TestMethod()]
        public async Task Set_and_get_example_data_by_passing_specified_key()
        {
            string key = "exampleKey01";
            byte data = 255;
            int cacheTime = int.MaxValue;
            var eventPublisher = new Mock<IMediator>();

            MemoryCacheManager memoryCacheManager = new MemoryCacheManager(new MemoryCache(new MemoryCacheOptions { }), eventPublisher.Object);

            await memoryCacheManager.SetAsync(key, data, cacheTime);
            Assert.AreEqual(await memoryCacheManager.GetAsync<byte>(key, async () => { return await Task.FromResult(new byte()); }),data);
        }

       
    }
}