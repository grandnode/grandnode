using System;
using Nop.Core.Domain.Catalog;
using Nop.Tests;
using NUnit.Framework;

namespace Nop.Data.Tests.Catalog
{
    [TestFixture]
    public class ProductCategoryPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_productCategory()
        {
            var productCategory = new ProductCategory
                                     {
                                         IsFeaturedProduct = true,
                                         DisplayOrder = 1,
                                         ProductId = 1,
                                         CategoryId = 1
                                     };

            var fromDb = SaveAndLoadEntity(productCategory);
            fromDb.ShouldNotBeNull();
            fromDb.IsFeaturedProduct.ShouldEqual(true);
            fromDb.DisplayOrder.ShouldEqual(1);


        }
    }
}
