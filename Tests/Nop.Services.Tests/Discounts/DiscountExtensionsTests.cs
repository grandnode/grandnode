using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nop.Services.Discounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Discounts;

namespace Nop.Services.Discounts.Tests {
    [TestClass()]
    public class DiscountExtensionsTests {
        [TestMethod()]
        public void Can_calculate_discount_amount_percentage() {
            /*
            GetDiscountAmount() returns the money amount that
            will be substracted from money that customer has to pay
            */
            var discount = new Discount { UsePercentage = true };

            discount.DiscountPercentage = 49;
            Assert.AreEqual(49M, discount.GetDiscountAmount(100));

            discount.DiscountPercentage = 15;
            Assert.AreEqual(75M, discount.GetDiscountAmount(500));

            discount.DiscountPercentage = 23.12M;
            Assert.AreEqual(1415.638M, discount.GetDiscountAmount(6123));
        }

        [TestMethod()]
        public void Can_calculate_discount_amount_fixed() {
            /*
            GetDiscountAmount() 
            DiscountAmount
            returns THE EXACT amount that is given in Ctor
            */
            var discount = new Discount { UsePercentage = false, DiscountAmount = 10 };

            //I'm passing strange values, and it keeps returnig 10
            Assert.AreEqual(10, discount.GetDiscountAmount(0));
            Assert.AreEqual(10, discount.GetDiscountAmount(-100));
            Assert.AreEqual(10, discount.GetDiscountAmount(9999));
            Assert.AreEqual(10, discount.GetDiscountAmount(123124));

            discount.DiscountAmount = 205;
            Assert.AreEqual(205, discount.GetDiscountAmount(0));
            Assert.AreEqual(205, discount.GetDiscountAmount(-100));
            Assert.AreEqual(205, discount.GetDiscountAmount(9999));
            Assert.AreEqual(205, discount.GetDiscountAmount(123124));
        }

        [TestMethod()]
        public void Maximum_discount_amount_is_used() {
            /*
            GetDiscountAmount() 
            MaximumDiscountAmount
            returns only top-allowed amount, even if DiscountPercentage is set to 100%
            */
            var discount = new Discount {
                UsePercentage = true,
                DiscountPercentage = 100, //notice I set 100% discount, so it is 4free
                MaximumDiscountAmount = 25.5M
            };

            //but only things that are priced <25.5M are 4free
            Assert.AreEqual(14, discount.GetDiscountAmount(14));
            Assert.AreEqual(25.5M, discount.GetDiscountAmount(25.5M));
            //still everything more expensive than 25.5M needs to be paid for
            Assert.AreEqual(25.5M, discount.GetDiscountAmount(100));
            Assert.AreEqual(25.5M, discount.GetDiscountAmount(9999));
            Assert.AreEqual(25.5M, discount.GetDiscountAmount(123124));
        }
    }
}