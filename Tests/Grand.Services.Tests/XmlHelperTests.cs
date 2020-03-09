using Grand.Services.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Grand.Services.Tests
{
    [TestClass()]
    public class XmlHelperTests
    {

        [TestMethod()]
        public void XmlEncodeAsIsTest()
        {
            Assert.IsNull(XmlHelper.XmlEncode(null));

            string actual = "anything";
            Assert.AreEqual(actual, XmlHelper.XmlEncodeAsIs(actual));
        }
    }
}
