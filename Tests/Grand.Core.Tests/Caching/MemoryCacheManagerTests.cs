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

        [TestMethod()]
        public async Task IsSetTest()
        {
            var eventPublisher = new Mock<IMediator>();

            MemoryCacheManager memoryCacheManager = new MemoryCacheManager(new MemoryCache(new MemoryCacheOptions { }), eventPublisher.Object);
            await memoryCacheManager.SetAsync("exampleKey05", 0, int.MaxValue);

            Assert.IsTrue(memoryCacheManager.IsSet("exampleKey05"));
            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey08"));
        }

        [TestMethod()]
        public async Task Removing_one_item_of_Cache()
        {
            var eventPublisher = new Mock<IMediator>();

            MemoryCacheManager memoryCacheManager = new MemoryCacheManager(new MemoryCache(new MemoryCacheOptions { }), eventPublisher.Object);
            await memoryCacheManager.SetAsync("exampleKey15", 5, int.MaxValue);

            Assert.IsTrue(memoryCacheManager.IsSet("exampleKey15"));
            await memoryCacheManager.RemoveAsync("exampleKey15");
            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey15"));
        }

        [TestMethod()]
        public async Task Clearing_whole_Cache()
        {
            var eventPublisher = new Mock<IMediator>();

            MemoryCacheManager memoryCacheManager = new MemoryCacheManager(new MemoryCache(new MemoryCacheOptions { }), eventPublisher.Object);
            await memoryCacheManager.SetAsync("exampleKey25", 5, int.MaxValue);
            await memoryCacheManager.SetAsync("exampleKey35", 5, int.MaxValue);

            Assert.IsTrue(memoryCacheManager.IsSet("exampleKey25"));
            Assert.IsTrue(memoryCacheManager.IsSet("exampleKey35"));

            await memoryCacheManager.Clear();

            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey25"));
            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey35"));
        }

        [TestMethod()]
        public async Task RemoveByPrefixTest()
        {
            var eventPublisher = new Mock<IMediator>();

            MemoryCacheManager memoryCacheManager = new MemoryCacheManager(new MemoryCache(new MemoryCacheOptions { }), eventPublisher.Object);
            await memoryCacheManager.SetAsync("exampleKey025", 5, int.MaxValue);
            await memoryCacheManager.SetAsync("exampleKey026", 5, int.MaxValue);
            await memoryCacheManager.SetAsync("exampleKey027", 5, int.MaxValue);

            await memoryCacheManager.SetAsync("exampleKey127", 5, int.MaxValue);

            string pattern = @"exampleKey0";
            await memoryCacheManager.RemoveByPrefix(pattern);

            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey025"));
            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey026"));
            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey027"));
            Assert.IsTrue(memoryCacheManager.IsSet("exampleKey127"));
        }
    }
}