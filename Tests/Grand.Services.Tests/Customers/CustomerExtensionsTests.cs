using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Services.Customers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grand.Services.Tests.Customers
{
    [TestClass()]
    public class CustomerExtensionsTests
    {
        private Customer customer;
        private CustomerSettings _settings;

        [TestInitialize()]
        public void Init()
        {
            customer = new Customer() {
                GenericAttributes = new List<GenericAttribute>()
                {
                    new GenericAttribute()
                    {
                        StoreId="",
                        Key=SystemCustomerAttributeNames.FirstName,
                        Value="John"
                    },
                     new GenericAttribute()
                     {
                        StoreId="",
                        Key=SystemCustomerAttributeNames.LastName,
                        Value="Smith"
                    },
                }
            };
            _settings = new CustomerSettings();
        }

        [TestMethod()]
        public void GetFullName_ConcatenationFirstAndLastName()
        {
            var result = customer.GetFullName();
            Assert.IsTrue(result.Equals("John Smith"));
        }

        [TestMethod()]
        public void GetFullName_LastNameEmmpty_ReturnFirstNameAsFullName()
        {
            customer.GenericAttributes.ElementAtOrDefault(1).Value = "";
            var result = customer.GetFullName();
            Assert.IsTrue(result.Equals("John"));
        }

        [TestMethod()]
        public void GetFullName_FirstNameEmmpty_ReturnLastNameAsFullName()
        {
            customer.GenericAttributes.ElementAtOrDefault(0).Value = "";
            var result = customer.GetFullName();
            Assert.IsTrue(result.Equals("Smith"));
        }

  
        [TestMethod()]
        public void FormatUserName_ReturnGuestCustomer()
        {
            customer.CustomerRoles.Add(new CustomerRole() { SystemName = SystemCustomerRoleNames.Guests,Active=true });
            var result = customer.FormatUserName(CustomerNameFormat.ShowEmails);
            Assert.IsTrue(result.Equals("Customer.Guest"));
        }

        [TestMethod()]
        public void FormatUserName_ReturnEmail()
        {
            var email = "johny@gmail.com";
            customer.Email = email;
            var result = customer.FormatUserName(CustomerNameFormat.ShowEmails);
            Assert.IsTrue(result.Equals(email));
        }


        [TestMethod()]
        public void FormatUserName_ReturnUsername()
        {
            var username = "Jsmith";
            customer.Username = username;
            var result = customer.FormatUserName(CustomerNameFormat.ShowUsernames);
            Assert.IsTrue(result.Equals(username));
        }

        [TestMethod()]
        public void FormatUserName_ReturnFullName()
        {
            var expected = "John Smith";
            var result = customer.FormatUserName(CustomerNameFormat.ShowFullNames);
            Assert.IsTrue(result.Equals(expected));
        }
       

        [TestMethod()]
        public void ParseAppliedCouponCodes_ShouldReturnExpectedArray()
        {
            var key = "cuponKey";
            var cuponCodes = "123;456;789";
            var expected = new string[] { "123", "456", "789" };
            customer.GenericAttributes.Add(new GenericAttribute() { StoreId = "", Key = key, Value = cuponCodes });
            var result = customer.ParseAppliedCouponCodes(key);
            Assert.IsTrue(result.SequenceEqual(expected));
        }

        [TestMethod()]
        public void ApplyCouponCode_ShouldReturnExpectedString()
        {
            var key = "cuponKey";
            var cuponCodes = "123;456;789";
            var toAdd = "111";
            var expected = "123;456;789;111";
            customer.GenericAttributes.Add(new GenericAttribute() { StoreId = "", Key = key, Value = cuponCodes });
            var result = customer.ApplyCouponCode(key, toAdd);
            Assert.IsTrue(result.Equals(expected));
        }

        [TestMethod()]
        public void ApplyCouponCode_MultipleCodes_ShouldReturnExpectedString()
        {
            var key = "cuponKey";
            var cuponCodes = "123;456;789";
            var toAdd = new string[] { "111", "222", "333" };
            var expected = "123;456;789;111;222;333";
            customer.GenericAttributes.Add(new GenericAttribute() { StoreId = "", Key = key, Value = cuponCodes });
            var result = customer.ApplyCouponCode(key, toAdd);
            Assert.IsTrue(result.Equals(expected));
        }

        [TestMethod()]
        public void RemoveCouponCode_ShouldReturnExpectedString()
        {
            var key = "cuponKey";
            var cuponCodes = "123;456;789";
            var expected = "123;789";
            customer.GenericAttributes.Add(new GenericAttribute() { StoreId = "", Key = key, Value = cuponCodes });
            var result = customer.RemoveCouponCode(key, "456");
            Assert.IsTrue(result.Equals(expected));
        }

        [TestMethod()]
        public void IsPasswordRecoveryTokenValid_EmptyRecoveryToken_ReturnFalse()
        {
            Assert.IsFalse(customer.IsPasswordRecoveryTokenValid("token"));
        }

        [TestMethod()]
        public void IsPasswordRecoveryTokenValid_ReturnTrue()
        {
            var recoveryToken = "sample recovery token";
            customer.GenericAttributes.Add(new GenericAttribute() { StoreId = "", Key = SystemCustomerAttributeNames.PasswordRecoveryToken, Value =recoveryToken});
            Assert.IsTrue(customer.IsPasswordRecoveryTokenValid(recoveryToken));
        }

        [TestMethod()]
        public void IsPasswordRecoveryLinkExpired_ReturnTrue()
        {
            //Is Expired
            _settings.PasswordRecoveryLinkDaysValid = 1;
            var generatedDate = DateTime.UtcNow.AddDays(-1);
            customer.GenericAttributes.Add(new GenericAttribute() { StoreId = "", Key = SystemCustomerAttributeNames.PasswordRecoveryTokenDateGenerated, Value = generatedDate.ToString("MM/dd/yyyy HH:mm:ss") }) ;
            Assert.IsTrue(customer.IsPasswordRecoveryLinkExpired(_settings));
        }

        [TestMethod()]
        public void IsPasswordRecoveryLinkExpired_ReturnFalse()
        {
            //Not expired
            _settings.PasswordRecoveryLinkDaysValid = 2;
            var generatedDate = DateTime.UtcNow.AddDays(-1);
            customer.GenericAttributes.Add(new GenericAttribute() { StoreId = "", Key = SystemCustomerAttributeNames.PasswordRecoveryTokenDateGenerated, Value = generatedDate.ToString("MM/dd/yyyy HH:mm:ss") });
            Assert.IsFalse(customer.IsPasswordRecoveryLinkExpired(_settings));
        }
    }
}
