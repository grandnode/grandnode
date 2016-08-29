﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Grand.Core.Caching.Tests {
    [TestClass()]
    public class MemoryCacheManagerTests {
        [TestMethod()]
        public void set_and_get_example_data_by_passing_specified_key() {
            string key = "exampleKey01";
            byte data = 255;
            int cacheTime = int.MaxValue;
            MemoryCacheManager memoryCacheManager = new MemoryCacheManager();

            memoryCacheManager.Set(key, data, cacheTime);
            Assert.AreEqual(memoryCacheManager.Get<byte>(key), data);
        }

        [TestMethod()]
        public void IsSetTest() {
            MemoryCacheManager memoryCacheManager = new MemoryCacheManager();
            memoryCacheManager.Set("exampleKey05", 0, int.MaxValue);

            Assert.IsTrue(memoryCacheManager.IsSet("exampleKey05"));
            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey08"));
        }

        [TestMethod()]
        public void removing_one_item_of_Cache() {
            MemoryCacheManager memoryCacheManager = new MemoryCacheManager();
            memoryCacheManager.Set("exampleKey15", 5, int.MaxValue);

            Assert.IsTrue(memoryCacheManager.IsSet("exampleKey15"));
            memoryCacheManager.Remove("exampleKey15");
            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey15"));
        }

        [TestMethod()]
        public void clearing_whole_Cache() {
            MemoryCacheManager memoryCacheManager = new MemoryCacheManager();
            memoryCacheManager.Set("exampleKey25", 5, int.MaxValue);
            memoryCacheManager.Set("exampleKey35", 5, int.MaxValue);

            Assert.IsTrue(memoryCacheManager.IsSet("exampleKey25"));
            Assert.IsTrue(memoryCacheManager.IsSet("exampleKey35"));

            memoryCacheManager.Clear();

            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey25"));
            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey35"));
        }

        [TestMethod()]
        public void RemoveByPatternTest() {
            MemoryCacheManager memoryCacheManager = new MemoryCacheManager();
            memoryCacheManager.Set("exampleKey025", 5, int.MaxValue);
            memoryCacheManager.Set("exampleKey026", 5, int.MaxValue);
            memoryCacheManager.Set("exampleKey027", 5, int.MaxValue);

            memoryCacheManager.Set("exampleKey127", 5, int.MaxValue);

            string pattern = @"exampleKey0\d\d";
            memoryCacheManager.RemoveByPattern(pattern);

            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey025"));
            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey026"));
            Assert.IsFalse(memoryCacheManager.IsSet("exampleKey027"));
            Assert.IsTrue(memoryCacheManager.IsSet("exampleKey127"));
        }
    }
}