using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grand.Services.Seo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Services.Seo.Tests {
    [TestClass()]
    public class SeoExtensionsTests {
        [TestMethod()]
        public void SeoExtensionsTest() {
           
            //german letters with diacritics
            Assert.AreEqual("testaou", SeoExtensions.GetSeName("testäöü",true,false));
            Assert.AreEqual("test", SeoExtensions.GetSeName("testäöü",false,false));
            //russian letters           
            Assert.AreEqual("testtest", SeoExtensions.GetSeName("testтест", true, false));
            Assert.AreEqual("test", SeoExtensions.GetSeName("testтест", false, false));

            Assert.AreEqual("test", SeoExtensions.GetSeName("testтест", false, false));
            Assert.AreEqual("testтест", SeoExtensions.GetSeName("testтест", false, true));

            //other
            Assert.AreEqual("abcdefghijklmnopqrstuvwxyz1234567890", SeoExtensions.GetSeName("abcdefghijklmnopqrstuvwxyz1234567890", false, false));

            Assert.AreEqual("test-test", SeoExtensions.GetSeName("test test", false, false));
            Assert.AreEqual("test-test", SeoExtensions.GetSeName("test     test", false, false));
       
        }
    }
}