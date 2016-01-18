using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Tests;
using NUnit.Framework;

namespace Nop.Data.Tests.Customers
{
    [TestFixture]
    public class CheckoutAttributeValuePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_customerAttributeValue()
        {
            var cav = new CustomerAttributeValue
                    {
                        Name = "Name 2",
                        IsPreSelected = true,
                        DisplayOrder = 1,
                        CustomerAttributeId = 1
                    };

            var fromDb = SaveAndLoadEntity(cav);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("Name 2");
            fromDb.IsPreSelected.ShouldEqual(true);
            fromDb.DisplayOrder.ShouldEqual(1);

        }
    }
}