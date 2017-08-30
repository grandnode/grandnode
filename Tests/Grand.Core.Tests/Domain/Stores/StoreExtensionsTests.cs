using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Grand.Core.Domain.Stores.Tests
{
    [TestClass()]
    public class StoreExtensionsTests {
        [TestMethod()]
        public void ParseHostValuesTest() {
            Store store = null;
            try {
                store.ParseHostValues();
            }
            catch(Exception ex) {
                Assert.AreEqual(typeof(ArgumentNullException), ex.GetType());
            }

            //split by comma
            //parser removes spaces at beginning and at end, but lefts the ones inside 
            string input = "serio usly,i,have,no,idea,what should, i type, here, 12321,@@,# ,@12";
            string[] expected = { "serio usly", "i", "have", "no", "idea", "what should", "i type", "here", "12321", "@@", "#", "@12" };

            store = new Store();
            store.Hosts = input;
            var result = store.ParseHostValues();

            for(int a = 0; a < expected.Length; ++a) {
                Assert.AreEqual(expected[a], result[a]);
            }
        }

        [TestMethod()]
        public void ContainsHostValueTest() {
            Store store = new Store { Hosts = "exampleWebSite.com, www.ExampleWebSite.pl" };

            Assert.IsTrue(store.ContainsHostValue("EXAMPLEWEBSITE.COM"));
            Assert.IsTrue(store.ContainsHostValue("WWW.eXAMPLEwEBsITE.PL"));
            Assert.IsTrue(store.ContainsHostValue("examplewebsite.com"));

            Assert.IsFalse(store.ContainsHostValue(null));
            Assert.IsFalse(store.ContainsHostValue("ExampleWebSities.com"));
        }
    }
}