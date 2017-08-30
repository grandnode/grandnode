using Grand.Core.Domain.Orders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Orders.Tests
{
    [TestClass()]
    public class CheckoutAttributeExtensionsTests {
        [TestMethod()]
        public void Can_remove_shippable_attributes() {
            List<CheckoutAttribute> attributes = new List<CheckoutAttribute>();
            attributes.Add(new CheckoutAttribute { Id = "1", Name = "Attribute001", ShippableProductRequired = false });
            attributes.Add(new CheckoutAttribute { Id = "2", Name = "Attribute002", ShippableProductRequired = true });
            attributes.Add(new CheckoutAttribute { Id = "3", Name = "Attribute003", ShippableProductRequired = false });
            attributes.Add(new CheckoutAttribute { Id = "4", Name = "Attribute004", ShippableProductRequired = true });

            //removes these with "ShippableProductRequired = true"
            IList<CheckoutAttribute> afterRemoval = attributes.RemoveShippableAttributes();
            Assert.AreEqual(2, afterRemoval.Count());
            Assert.AreEqual("Attribute001", afterRemoval[0].Name);
            Assert.AreEqual("3", afterRemoval[1].Id);
        }
    }
}