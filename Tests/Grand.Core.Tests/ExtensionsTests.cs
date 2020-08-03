using Grand.Core.Caching;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Core.Tests
{
    public struct tempStruct
    {
        public string str;
        public int integral;
    }

    [TestClass()]
    public class ExtensionsTests
    {
        [TestMethod()]
        public async Task RemoveByPatternTest()
        {
            /*
            In this test I provide:
                1. pattern that consists of "key100\d" which matchs to: key1009, but not to key1010
                2. ICacheManager object with several keys 
                3. List<string> with several keys
            */
            var eventPublisher = new Mock<IMediator>();

            ICacheManager icacheManager = new MemoryCacheManager(new MemoryCache(new MemoryCacheOptions { }), eventPublisher.Object);
            await icacheManager.SetAsync("key1001", 33, int.MaxValue);
            await icacheManager.SetAsync("key1202", 1244, int.MaxValue);
            await icacheManager.SetAsync("key1003", 512, int.MaxValue);
            await icacheManager.SetAsync("key1204", 55, int.MaxValue);
            await icacheManager.SetAsync("key1005", 32, int.MaxValue);

            string pattern = @"key100\d"; //"key100" and one digit

            List<string> keys = new List<string>();
            keys.Add("key1001");
            keys.Add("key1202");
            keys.Add("key1003");
            keys.Add("key1204");
            keys.Add("key1005");

            await icacheManager.RemoveByPrefix(pattern);

            Assert.IsNotNull(icacheManager.GetAsync<int>("key1202", async () => { return await Task.FromResult(0); }));
            Assert.IsNotNull(icacheManager.GetAsync<int>("key1204", async () => { return await Task.FromResult(0); }));
        }
    }
}