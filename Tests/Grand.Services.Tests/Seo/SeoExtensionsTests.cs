using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Grand.Services.Seo.Tests
{
    [TestClass()]
    public class SeoExtensionsTests
    {
        [TestMethod()]
        public void SeoExtensionsTest()
        {
            var charConversions = "ä:a;ö:o;ü:u;т:t;е:e;с:s;т:t";

            //german letters with diacritics
            Assert.AreEqual("testaou", SeoExtensions.GetSeName("testäöü", true, false, charConversions));
            Assert.AreEqual("test", SeoExtensions.GetSeName("testäöü", false, false));
            //russian letters           
            Assert.AreEqual("testtest", SeoExtensions.GetSeName("testтест", true, false, charConversions));
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