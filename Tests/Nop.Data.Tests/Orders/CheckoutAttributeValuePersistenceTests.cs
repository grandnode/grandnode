using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Tests;
using NUnit.Framework;

namespace Nop.Data.Tests.Orders
{
    [TestFixture]
    public class CheckoutAttributeValuePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_checkoutAttributeValue()
        {
            var cav = new CheckoutAttributeValue
                    {
                        Name = "Name 2",
                        ColorSquaresRgb = "#112233",
                        PriceAdjustment = 1.1M,
                        WeightAdjustment = 2.1M,
                        IsPreSelected = true,
                        DisplayOrder = 3,
                        CheckoutAttributeId = 1
                    };

            var fromDb = SaveAndLoadEntity(cav);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("Name 2");
            fromDb.ColorSquaresRgb.ShouldEqual("#112233");
            fromDb.PriceAdjustment.ShouldEqual(1.1M);
            fromDb.WeightAdjustment.ShouldEqual(2.1M);
            fromDb.IsPreSelected.ShouldEqual(true);
            fromDb.DisplayOrder.ShouldEqual(3);

        }
    }
}