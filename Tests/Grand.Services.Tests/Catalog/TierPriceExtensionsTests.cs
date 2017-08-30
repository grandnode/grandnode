using Grand.Core.Domain.Catalog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Catalog.Tests
{
    [TestClass()]
    public class TierPriceExtensionsTests {
        [TestMethod()]
        public void Can_remove_duplicatedQuantities() {
            
            var tierPrices = new List<TierPrice>();
            tierPrices.Add(new TierPrice
            {
                //will be removed
                Id = "1",
                Price = 150,
                Quantity = 1
            });
            tierPrices.Add(new TierPrice
            {
                //will stay
                Id = "2",
                Price = 100,
                Quantity = 1
            });
            tierPrices.Add(new TierPrice
            {
                //will stay
                Id = "3",
                Price = 200,
                Quantity = 3
            });
            tierPrices.Add(new TierPrice
            {
                //will stay
                Id = "4",
                Price = 250,
                Quantity = 4
            });
            tierPrices.Add(new TierPrice
            {
                //will be removed
                Id = "5",
                Price = 300,
                Quantity = 4
            });
            tierPrices.Add(new TierPrice
            {
                //will stay
                Id = "6",
                Price = 350,
                Quantity = 5
            });

            var tierPriceCollection = tierPrices.RemoveDuplicatedQuantities();
            

            Assert.IsNull(tierPriceCollection.FirstOrDefault(v =>    v.Id == "1"));
            Assert.IsNotNull(tierPriceCollection.FirstOrDefault(v =>    v.Id == "2"));      //doubled 15 - saved
            Assert.IsNotNull(tierPriceCollection.FirstOrDefault(v =>       v.Id == "3"));      //! doubled 15 - removed
            Assert.IsNotNull(tierPriceCollection.FirstOrDefault(v =>    v.Id == "4"));      //doubled 23 - saved
            Assert.IsNull(tierPriceCollection.FirstOrDefault(v =>    v.Id == "5"));
            Assert.IsNotNull(tierPriceCollection.FirstOrDefault(v =>       v.Id == "6"));      //! doubled 23 - removed
            
        }
    }
}