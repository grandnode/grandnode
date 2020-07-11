using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Grand.Domain.Catalog.Tests
{
    [TestClass()]
    public class ProductExtensionsTests
    {
        [TestMethod()]
        public void ParseRequiredProductIdsTest()
        {
          
            Product product = null;
            try
            {
                product.ParseRequiredProductIds();
            }
            catch (Exception ex)
            {
                Assert.AreEqual(typeof(ArgumentNullException), ex.GetType());
            }

            product = new Product { RequiredProductIds = "" };
            Assert.AreEqual(new int[0].Length, product.ParseRequiredProductIds().Length);

            product = new Product { RequiredProductIds = ",1,23,4,5,6,ww,12,xczx,1231" };

            string[] expected =
                { "1", "23", "4", "5", "6", "ww", "12", "xczx", "1231" };   //9 elements
            string[] actual =
                product.ParseRequiredProductIds();                          //9 elements 

            Assert.AreEqual(9, actual.Length);

            for (int a = 0; a < expected.Length; ++a)
            {
                Assert.AreEqual(expected[a], actual[a]);
            }
        }

        [TestMethod()]
        public void Should_be_available_when_startdate_is_not_set()
        {
            //startdate
            Product product = new Product { AvailableStartDateTimeUtc = null };
            Assert.IsTrue(product.IsAvailable(DateTime.UtcNow));
        }

        [TestMethod()]
        public void Should_be_available_when_startdate_is_smaller_than_somedate()
        {
            Product product = new Product { AvailableStartDateTimeUtc = new DateTime(2016, 03, 15) };   //older
            Assert.IsTrue(product.IsAvailable(new DateTime(2016, 03, 17)));                             //newer
        }

        [TestMethod()]
        public void Should_not_be_available_when_startdate_is_greater_than_somedate()
        {
            Product product = new Product { AvailableStartDateTimeUtc = new DateTime(2016, 03, 30) };   //newer
            Assert.IsFalse(product.IsAvailable(new DateTime(2016, 03, 10)));                            //older
        }

        [TestMethod()]
        public void Should_be_available_when_enddate_is_not_set()
        {
            //enddate
            Product product = new Product { AvailableEndDateTimeUtc = null };
            Assert.IsTrue(product.IsAvailable(DateTime.UtcNow));
        }

        [TestMethod()]
        public void Should_be_available_when_enddate_is_greater_than_somedate()
        {
            Product product = new Product { AvailableEndDateTimeUtc = new DateTime(2016, 03, 15) }; //older
            Assert.IsTrue(product.IsAvailable(new DateTime(2016, 03, 11))); //newer
        }

        [TestMethod()]
        public void Should_not_be_available_when_enddate_is_smaller_than_somedate()
        {
            Product product = new Product { AvailableEndDateTimeUtc = new DateTime(2016, 03, 10) }; //later
            Assert.IsFalse(product.IsAvailable(new DateTime(2016, 04, 10))); //earlier
        }


    }
}