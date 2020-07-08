using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Grand.Domain.Common.Tests
{
    [TestClass()]
    public class AddressTests {
        [TestMethod()]
        public void CloneTest() {
            Address object01 = new Address {
                FirstName = "Mariusz",
                LastName = "Pudzianowski",
                Email = "mariusz@pudzian.pl",
                Company = "Pudzian Transport",
                CountryId = "3",
                StateProvinceId = "4",
                City = "Biala Rawska",
                Address1 = "adres 01",
                Address2 = "adres 02",
                ZipPostalCode = "96 230",
                PhoneNumber = "123123123",
                FaxNumber = "fax",
                CreatedOnUtc = new DateTime(2016, 03, 16)
            };

            var clonedObject = object01.Clone() as Address;

            Assert.AreEqual(clonedObject.FirstName, "Mariusz");
            Assert.AreEqual(clonedObject.LastName, "Pudzianowski");
            Assert.AreEqual(clonedObject.Email, "mariusz@pudzian.pl");
            Assert.AreEqual(clonedObject.Company, "Pudzian Transport");

            Assert.AreEqual(clonedObject.CountryId, "3");

            Assert.AreEqual(clonedObject.StateProvinceId, "4");
            Assert.AreEqual(clonedObject.City, "Biala Rawska");
            Assert.AreEqual(clonedObject.Address1, "adres 01");
            Assert.AreEqual(clonedObject.Address2, "adres 02");
            Assert.AreEqual(clonedObject.ZipPostalCode, "96 230");
            Assert.AreEqual(clonedObject.PhoneNumber, "123123123");
            Assert.AreEqual(clonedObject.FaxNumber, "fax");

            Assert.AreEqual(clonedObject.CreatedOnUtc, new DateTime(2016, 03, 16));
        }
    }
}