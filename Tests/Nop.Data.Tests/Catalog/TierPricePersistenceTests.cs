using System;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Tests;
using NUnit.Framework;

namespace Nop.Data.Tests.Catalog
{
    [TestFixture]
    public class TierPricePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_tierPrice()
        {
            var tierPrice = new TierPrice
            {
                StoreId = 7,
                Quantity = 1,
                Price = 2.1M,
                ProductId = GetTestProduct().Id,
           };

            var fromDb = SaveAndLoadEntity(tierPrice);
            fromDb.ShouldNotBeNull();
            fromDb.StoreId.ShouldEqual(7);
            fromDb.Quantity.ShouldEqual(1);
            fromDb.Price.ShouldEqual(2.1M);

        }

        [Test]
        public void Can_save_and_load_tierPriceWithCustomerRole()
        {
            var tierPrice = new TierPrice
            {
                Quantity = 1,
                Price = 2,
                ProductId = GetTestProduct().Id,
                CustomerRoleId = 1, 
            };

            var fromDb = SaveAndLoadEntity(tierPrice);
            fromDb.ShouldNotBeNull();

        }

        protected Product GetTestProduct()
        {
            return new Product
            {
                Name = "Product name 1",
                CreatedOnUtc = new DateTime(2010, 01, 03),
                UpdatedOnUtc = new DateTime(2010, 01, 04),
            };
        }
    }
}
