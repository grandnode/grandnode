using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Services.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grand.Services.Tests.Common
{
    [TestClass()]
    public class AddressAttributeExtensionsTests
    {
        [TestMethod()]
        public void ShouldHaveValues_NullAddressAtribute_ReturnFalse()
        {
            AddressAttribute addressAttribute = null;
            Assert.IsFalse(addressAttribute.ShouldHaveValues());
        }

        [TestMethod()]
        public void ShouldHaveValues__ReturnFalse()
        {
            AddressAttribute addressAttribute = new AddressAttribute() { AttributeControlType = AttributeControlType.TextBox };
            AddressAttribute addressAttribute2 = new AddressAttribute() { AttributeControlType = AttributeControlType.MultilineTextbox };
            AddressAttribute addressAttribute3 = new AddressAttribute() { AttributeControlType = AttributeControlType.Datepicker};
            AddressAttribute addressAttribute4 = new AddressAttribute() { AttributeControlType = AttributeControlType.FileUpload };
            Assert.IsFalse(addressAttribute.ShouldHaveValues());
            Assert.IsFalse(addressAttribute2.ShouldHaveValues());
            Assert.IsFalse(addressAttribute3.ShouldHaveValues());
            Assert.IsFalse(addressAttribute4.ShouldHaveValues());
        }

            
        [TestMethod()]
        public void ShouldHaveValues__ReturnTrue()
        {
            AddressAttribute addressAttribute = new AddressAttribute() { AttributeControlType = AttributeControlType.DropdownList };
            AddressAttribute addressAttribute2 = new AddressAttribute() { AttributeControlType = AttributeControlType.ImageSquares };
            AddressAttribute addressAttribute3 = new AddressAttribute() { AttributeControlType = AttributeControlType.RadioList };
            AddressAttribute addressAttribute4 = new AddressAttribute() { AttributeControlType = AttributeControlType.ReadonlyCheckboxes };
            Assert.IsTrue(addressAttribute.ShouldHaveValues());
            Assert.IsTrue(addressAttribute2.ShouldHaveValues());
            Assert.IsTrue(addressAttribute3.ShouldHaveValues());
            Assert.IsTrue(addressAttribute4.ShouldHaveValues());
        }
    }
}
