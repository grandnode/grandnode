using System;
using Nop.Core.Domain.Catalog;
using Nop.Tests;
using NUnit.Framework;

namespace Nop.Data.Tests.Catalog
{
    [TestFixture]
    public class ProductSpecificationAttributePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_productSpecificationAttribute()
        {
            var productSpecificationAttribute = new ProductSpecificationAttribute
            {
                AttributeType = SpecificationAttributeType.Hyperlink,
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                ProductId = 1,
                SpecificationAttributeOptionId = 1,
            };

            var fromDb = SaveAndLoadEntity(productSpecificationAttribute);
            fromDb.ShouldNotBeNull();
            fromDb.AttributeType.ShouldEqual(SpecificationAttributeType.Hyperlink);
            fromDb.AllowFiltering.ShouldEqual(true);
            fromDb.ShowOnProductPage.ShouldEqual(true);
            fromDb.DisplayOrder.ShouldEqual(1);

        }
    }
}
