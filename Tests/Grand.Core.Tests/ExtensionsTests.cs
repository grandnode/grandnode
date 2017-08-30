using Grand.Core.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
        public void RemoveByPatternTest()
        {
            /*
            In this test I provide:
                1. pattern that consists of "key100\d" which matchs to: key1009, but not to key1010
                2. ICacheManager object with several keys 
                3. List<string> with several keys
            */

            ICacheManager icacheManager = new MemoryCacheManager(new MemoryCache(new MemoryCacheOptions { }));
            icacheManager.Set("key1001", 33, int.MaxValue);
            icacheManager.Set("key1202", 1244, int.MaxValue);
            icacheManager.Set("key1003", 512, int.MaxValue);
            icacheManager.Set("key1204", 55, int.MaxValue);
            icacheManager.Set("key1005", 32, int.MaxValue);

            string pattern = @"key100\d"; //"key100" and one digit

            List<string> keys = new List<string>();
            keys.Add("key1001");
            keys.Add("key1202");
            keys.Add("key1003");
            keys.Add("key1204");
            keys.Add("key1005");

            icacheManager.RemoveByPattern(pattern);

            Assert.IsNotNull(icacheManager.Get<int>("key1202"));
            Assert.IsNotNull(icacheManager.Get<int>("key1204"));
        }
    }
}