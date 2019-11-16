using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            MemoryCacheManager memoryCacheManager = new MemoryCacheManager(new MemoryCache(new MemoryCacheOptions { }));

            await memoryCacheManager.Set(key, data, cacheTime);
            Assert.AreEqual(await memoryCacheManager.Get<byte>(key), data);
        }

        [TestMethod()]
        public async Task IsSetTest()
        {
            MemoryCacheManager memoryCacheManager = new MemoryCacheManager(new MemoryCache(new MemoryCacheOptions { }));
            await memoryCacheManager.Set("exampleKey05", 0, int.MaxValue);

            Assert.IsTrue(memoryCacheManager.IsSet("exampleKey05"));
            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey08"));
        }

        [TestMethod()]
        public async Task Removing_one_item_of_Cache()
        {
            MemoryCacheManager memoryCacheManager = new MemoryCacheManager(new MemoryCache(new MemoryCacheOptions { }));
            await memoryCacheManager.Set("exampleKey15", 5, int.MaxValue);

            Assert.IsTrue(memoryCacheManager.IsSet("exampleKey15"));
            await memoryCacheManager.Remove("exampleKey15");
            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey15"));
        }

        [TestMethod()]
        public async Task Clearing_whole_Cache()
        {
            MemoryCacheManager memoryCacheManager = new MemoryCacheManager(new MemoryCache(new MemoryCacheOptions { }));
            await memoryCacheManager.Set("exampleKey25", 5, int.MaxValue);
            await memoryCacheManager.Set("exampleKey35", 5, int.MaxValue);

            Assert.IsTrue(memoryCacheManager.IsSet("exampleKey25"));
            Assert.IsTrue(memoryCacheManager.IsSet("exampleKey35"));

            await memoryCacheManager.Clear();

            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey25"));
            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey35"));
        }

        [TestMethod()]
        public async Task RemoveByPatternTest()
        {
            MemoryCacheManager memoryCacheManager = new MemoryCacheManager(new MemoryCache(new MemoryCacheOptions { }));
            await memoryCacheManager.Set("exampleKey025", 5, int.MaxValue);
            await memoryCacheManager.Set("exampleKey026", 5, int.MaxValue);
            await memoryCacheManager.Set("exampleKey027", 5, int.MaxValue);

            await memoryCacheManager.Set("exampleKey127", 5, int.MaxValue);

            string pattern = @"exampleKey0\d\d";
            await memoryCacheManager.RemoveByPattern(pattern);

            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey025"));
            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey026"));
            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey027"));
            Assert.IsTrue(memoryCacheManager.IsSet("exampleKey127"));
        }
    }
}