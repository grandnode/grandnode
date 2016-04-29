using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Caching;

namespace Nop.Core.Tests {
    public struct tempStruct {
        public string str;
        public int integral;
    }

    [TestClass()]
    public class ExtensionsTests {
        [TestMethod()]
        public void IsNullOrDefaultTest() {
            //tests if struct is null or default (==0
            //null - for reference type (object)
            //default (0) - for value type

            //is true - tempStruct is null/ default
            tempStruct struktura = default(tempStruct);
            Assert.IsTrue(Extensions.IsNullOrDefault<tempStruct>(struktura));

            //is false - tempStruct isn't null
            struktura.integral = 123;
            struktura.str = "asd";
            Assert.IsFalse(Extensions.IsNullOrDefault<tempStruct>(struktura));
        }

        [TestMethod()]
        public void RemoveByPatternTest() {
            /*
            In this test I provide:
                1. pattern that consists of "key100\d" which matchs to: key1009, but not to key1010
                2. ICacheManager object with several keys 
                3. List<string> with several keys
            */

            ICacheManager icacheManager = new MemoryCacheManager();
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

            icacheManager.RemoveByPattern(pattern, keys);

            Assert.IsNotNull(icacheManager.Get<int>("key1202"));
            Assert.IsNotNull(icacheManager.Get<int>("key1204"));
        }
    }
}