using Grand.Core.TypeConverters.Converter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;

namespace Grand.Core.Tests.ComponentModel
{
    [TestClass()]
    public class BoolTypeConverterTests
    {
        [TestInitialize()]
        public void TestInitialize()
        {
            TypeDescriptor.AddAttributes(typeof(bool), new TypeConverterAttribute(typeof(BoolTypeConverter)));
        }

        [TestMethod()]
        public void Should_Return_Expected_Value()
        {
            var converter = TypeDescriptor.GetConverter(typeof(bool));
            Assert.AreEqual(false, converter.ConvertFrom("false"));
            Assert.AreEqual(false, converter.ConvertFrom("False"));
            Assert.AreEqual(false, converter.ConvertFrom("FaLSe"));
            Assert.AreEqual(true, converter.ConvertFrom("True"));
            Assert.AreEqual(true, converter.ConvertFrom("true"));
            Assert.AreEqual(true, converter.ConvertFrom("tRuE"));
        }

        [TestMethod()]
        public void Should_Return_False_InstedOf_Exception()
        {
            var converter = TypeDescriptor.GetConverter(typeof(bool));
            Assert.AreEqual(false, converter.ConvertFrom("1"));
            Assert.AreEqual(false, converter.ConvertFrom("0"));
            Assert.AreEqual(false, converter.ConvertFrom("323"));
            Assert.AreEqual(false, converter.ConvertFrom("fdsfsd"));
            Assert.AreEqual(false, converter.ConvertFrom("///!!!"));
        }
    }
}
