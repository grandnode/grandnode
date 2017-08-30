using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Grand.Core.ComponentModel.Tests
{
    [TestClass()]
    public class GenericListTypeConverterTests {
        [TestInitialize()]
        public void TestInitialize() {
            TypeDescriptor.AddAttributes(typeof(List<int>), new TypeConverterAttribute(typeof(GenericListTypeConverter<int>)));
            TypeDescriptor.AddAttributes(typeof(List<string>), new TypeConverterAttribute(typeof(GenericListTypeConverter<string>)));
        }

        [TestMethod()]
        public void Can_get_int_list_type_converter() {
            TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(List<int>));
            Assert.AreEqual(typeConverter.GetType(), typeof(GenericListTypeConverter<int>));
        }

        [TestMethod()]
        public void Can_get_string_list_type_converter() {
            TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(List<string>));
            Assert.AreEqual(typeConverter.GetType(), typeof(GenericListTypeConverter<string>));
        }

        [TestMethod()]
        public void Can_get_int_list_from_string() {
            string items = "10,20,30,40,50";
            var converter = TypeDescriptor.GetConverter(typeof(List<int>));
            var result = converter.ConvertFrom(items) as IList<int>;     

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count());

            Assert.AreEqual(0, result.IndexOf(10));
            Assert.AreEqual(1, result.IndexOf(20));
            Assert.AreEqual(2, result.IndexOf(30));
            Assert.AreEqual(3, result.IndexOf(40));
            Assert.AreEqual(4, result.IndexOf(50));

            Assert.AreEqual(10, result[0]);
            Assert.AreEqual(20, result[1]);
            Assert.AreEqual(30, result[2]);
            Assert.AreEqual(40, result[3]);
            Assert.AreEqual(50, result[4]);
        }

        [TestMethod()]
        public void Can_get_string_list_from_string() {
            string items = "eins,zwei,drei,vier,funf,sechs";
            var converter = TypeDescriptor.GetConverter(typeof(List<string>));
            var result = converter.ConvertFrom(items) as List<string>;     

            Assert.IsNotNull(result);
            Assert.AreEqual(6, result.Count());

            Assert.AreEqual(0, result.IndexOf("eins"));
            Assert.AreEqual(1, result.IndexOf("zwei"));
            Assert.AreEqual(2, result.IndexOf("drei"));
            Assert.AreEqual(3, result.IndexOf("vier"));
            Assert.AreEqual(4, result.IndexOf("funf"));
            Assert.AreEqual(5, result.IndexOf("sechs"));

            Assert.AreEqual("eins", result[0]);
            Assert.AreEqual("zwei", result[1]);
            Assert.AreEqual("drei", result[2]);
            Assert.AreEqual("vier", result[3]);
            Assert.AreEqual("funf", result[4]);
            Assert.AreEqual("sechs", result[5]);
        }

        [TestMethod()]
        public void Can_convert_int_list_to_string() {
            List<int> listInt = new List<int> { 33, 34, 35, 36, 34 };
            var converter = TypeDescriptor.GetConverter(typeof(List<int>));
            string result = converter.ConvertTo(listInt, typeof(string)) as string;

            Assert.IsNotNull(result);
            Assert.AreEqual("33,34,35,36,34", result);
        }

        [TestMethod()]
        public void Can_convert_string_list_to_string() {
            List<string> listString = new List<string> { "3f3", "3s4", "3a5", "3q6", "3z4" };
            var converter = TypeDescriptor.GetConverter(typeof(List<string>));
            string result = converter.ConvertTo(listString, typeof(string)) as string;

            Assert.IsNotNull(result);
            Assert.AreEqual("3f3,3s4,3a5,3q6,3z4", result);
        }
    }
}