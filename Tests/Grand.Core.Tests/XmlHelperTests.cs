using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Grand.Core.Tests
{
    [TestClass()]
    public class XmlHelperTests {
       
        [TestMethod()]
        public void XmlEncodeAsIsTest() {
            Assert.IsNull(XmlHelper.XmlEncode(null));

            string actual = "anything";
            Assert.AreEqual(actual, XmlHelper.XmlEncodeAsIs(actual));
        }
       

        [TestMethod()]
        public void SerializeDateTimeTest() {
            //method sends a DateTime instance, and makes XML Marker <dateTime></dateTime>
            //Environment.NewLine is required, because if I type there normal \n it won't recognize it as the same
            string expected = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine + "<dateTime>2000-01-02T03:04:05</dateTime>";
            DateTime actual = new DateTime(2000, 01, 02, 03, 04, 05);

            Assert.AreEqual(expected, XmlHelper.SerializeDateTime(actual));
        }

        [TestMethod()]
        public void DeserializeDateTimeTest() {
            //arrange
            DateTime expected = new DateTime(2000, 01, 02, 03, 04, 05);

            //act
            //let's first serialize DateTime and then deserialize it to check if there are no glitches between
            DateTime actual = XmlHelper.DeserializeDateTime((string)XmlHelper.SerializeDateTime(expected));

            //assert
            Assert.AreEqual(expected, actual);
        }
    }
}
