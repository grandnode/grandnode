using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog.Tests {
    [TestClass()]
    public class TierPriceExtensionsTests {
        [TestMethod()]
        public void Can_remove_duplicatedQuantities() {
            List<TierPrice> tierPriceCollection = new List<TierPrice>();

            //it works by removing instances with the same Quantity. 
            //Even if different objects, but the same Quantity - the latter will be removed
            tierPriceCollection.Add(new TierPrice { Id = "1", Price = 100, Quantity = 10 });
            tierPriceCollection.Add(new TierPrice { Id = "2", Price = 200, Quantity = 15 });
            tierPriceCollection.Add(new TierPrice { Id = "3", Price = 300, Quantity = 15 });
            tierPriceCollection.Add(new TierPrice { Id = "4", Price = 400, Quantity = 23 });
            tierPriceCollection.Add(new TierPrice { Id = "5", Price = 500, Quantity = 53 });
            tierPriceCollection.Add(new TierPrice { Id = "6", Price = 600, Quantity = 23 });

            //when meet doubled, the 1st occurance will be preserved, while second will be removed
    tierPriceCollection.RemoveDuplicatedQuantities();

            //as you see below, the former is saved, and the latter will be removed
            //objects with non-duplicated Quantity will be left intact
            Assert.IsNotNull(tierPriceCollection.FirstOrDefault(v =>    v.Id == "1"));
            Assert.IsNotNull(tierPriceCollection.FirstOrDefault(v =>    v.Id == "2"));      //doubled 15 - saved
            Assert.IsNull(tierPriceCollection.FirstOrDefault(v =>       v.Id == "3"));      //! doubled 15 - removed
            Assert.IsNotNull(tierPriceCollection.FirstOrDefault(v =>    v.Id == "4"));      //doubled 23 - saved
            Assert.IsNotNull(tierPriceCollection.FirstOrDefault(v =>    v.Id == "5"));
            Assert.IsNull(tierPriceCollection.FirstOrDefault(v =>       v.Id == "6"));      //! doubled 23 - removed
        }
    }
}