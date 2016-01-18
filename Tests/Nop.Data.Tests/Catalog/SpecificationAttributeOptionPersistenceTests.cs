using Nop.Core.Domain.Catalog;
using Nop.Tests;
using NUnit.Framework;

namespace Nop.Data.Tests.Catalog
{
    [TestFixture]
    public class SpecificationAttributeOptionPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_specificationAttributeOption()
        {
            var specificationAttributeOption = new SpecificationAttributeOption
            {
                Name = "SpecificationAttributeOption name 1",
                DisplayOrder = 1,
                SpecificationAttributeId = 1,
            };

            var fromDb = SaveAndLoadEntity(specificationAttributeOption);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("SpecificationAttributeOption name 1");
            fromDb.DisplayOrder.ShouldEqual(1);

        }
    }
}
