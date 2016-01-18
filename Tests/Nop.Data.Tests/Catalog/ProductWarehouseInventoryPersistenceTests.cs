using System;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Shipping;
using Nop.Tests;
using NUnit.Framework;

namespace Nop.Data.Tests.Catalog
{
    [TestFixture]
    public class ProductWarehouseInventoryPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_productWarehouseInventory()
        {
            var pwi = new ProductWarehouseInventory
            {
                ProductId = 1,
                WarehouseId = 1,
                StockQuantity = 3,
                ReservedQuantity = 4,
            };

            var fromDb = SaveAndLoadEntity(pwi);
            fromDb.ShouldNotBeNull();
            fromDb.StockQuantity.ShouldEqual(3);
            fromDb.ReservedQuantity.ShouldEqual(4);
        }
    }
}
