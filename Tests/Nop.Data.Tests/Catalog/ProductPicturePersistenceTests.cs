using System;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Tests;
using NUnit.Framework;

namespace Nop.Data.Tests.Catalog
{
    [TestFixture]
    public class ProductPicturePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_productPicture()
        {
            var productPicture = new ProductPicture
                                     {
                                         DisplayOrder = 1,
                                         ProductId = 1,
                                         PictureId = 1
                                     };

            var fromDb = SaveAndLoadEntity(productPicture);
            fromDb.ShouldNotBeNull();
            fromDb.DisplayOrder.ShouldEqual(1);

        }
    }
}
