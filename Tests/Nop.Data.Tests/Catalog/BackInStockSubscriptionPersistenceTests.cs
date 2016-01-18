using System;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Tests;
using NUnit.Framework;

namespace Nop.Data.Tests.Catalog
{
    [TestFixture]
    public class BackInStockSubscriptionPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_backInStockSubscription()
        {
            var backInStockSubscription = new BackInStockSubscription
                                     {
                                         ProductId = GetTestProduct().Id,
                                         CustomerId = 1,
                                         CreatedOnUtc = new DateTime(2010, 01, 02)
                                     };

            var fromDb = SaveAndLoadEntity(backInStockSubscription);
            fromDb.ShouldNotBeNull();

            fromDb.CreatedOnUtc.ShouldEqual(new DateTime(2010, 01, 02));
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
