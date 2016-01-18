using System;
using Nop.Core.Domain.Catalog;
using Nop.Tests;
using NUnit.Framework;

namespace Nop.Data.Tests.Catalog
{
    [TestFixture]
    public class ProductManufacturerPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_productManufacturer()
        {
            var productManufacturer = new ProductManufacturer
                                     {
                                         IsFeaturedProduct = true,
                                         DisplayOrder = 1,
                                         ProductId = 1,
                                         ManufacturerId = 1
                                     };

            var fromDb = SaveAndLoadEntity(productManufacturer);
            fromDb.ShouldNotBeNull();
            fromDb.IsFeaturedProduct.ShouldEqual(true);
            fromDb.DisplayOrder.ShouldEqual(1);

        }
    }
}
