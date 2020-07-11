using Grand.Domain.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Grand.Domain.Customers.Tests
{
    [TestClass()]
    public class CustomerTests {
        [TestMethod()]
        public void CustomerTest() {

            Customer customer = new Customer();

            customer.CustomerRoles.Add(new CustomerRole {
                Active = true,
                Name = "name customer 001",
                SystemName = SystemCustomerRoleNames.Guests //"system name customer 001"
            });
            customer.CustomerRoles.Add(new CustomerRole {
                Active = false,
                Name = "name customer 011",
                SystemName = SystemCustomerRoleNames.Vendors //"system name customer 011"
            });

            //setting 2nd argument to false - it won't care if customer is active or inactive - it will gather both
            Assert.IsTrue(customer.IsInCustomerRole(SystemCustomerRoleNames.Guests, false));
            Assert.IsTrue(customer.IsInCustomerRole(SystemCustomerRoleNames.Vendors, false));

            //2nd argument is true by default
            Assert.IsTrue(customer.IsInCustomerRole(SystemCustomerRoleNames.Guests, true)); //is active, and will yield true
            Assert.IsFalse(customer.IsInCustomerRole(SystemCustomerRoleNames.Vendors, true)); //is inactive, and will yield false

            //checking unexisting customer - he surely isn't administrator
            Assert.IsFalse(customer.IsInCustomerRole(SystemCustomerRoleNames.Administrators));
        }

        [TestMethod()]
        public void Can_check_whether_customer_is_admin() {
            Customer customer = new Customer();
            customer.CustomerRoles.Add(new CustomerRole {
                Active = true, //for further tests, it should be always set to true
                SystemName = SystemCustomerRoleNames.ForumModerators
            });

            //2 ways to check it
            //1st way
            Assert.IsFalse(customer.IsInCustomerRole(SystemCustomerRoleNames.Administrators));
            //2nd way - easier
            Assert.IsFalse(customer.IsAdmin());
            customer.CustomerRoles.Add(new CustomerRole {
                Active = true,    
                SystemName = SystemCustomerRoleNames.Administrators //now he gains full power
            });

            //1st way
            Assert.IsTrue(customer.IsInCustomerRole(SystemCustomerRoleNames.Administrators));
            //2nd way - easier
            Assert.IsTrue(customer.IsAdmin());
        }

        [TestMethod()]
        public void New_customer_has_clear_password_type() {
            var customer = new Customer();
            Assert.AreEqual<PasswordFormat>(customer.PasswordFormat, PasswordFormat.Clear);
        }

        [TestMethod()]
        public void Can_remove_address_assigned_as_billing_address() {
            Customer customer = new Customer();
            Address address0101 = new Address { Id = "123" };
            customer.Addresses.Add(address0101);

            customer.BillingAddress = address0101; //I'm assigning 1st and only one

            Assert.IsTrue(customer.Addresses.Contains(address0101));
            Assert.AreEqual(address0101, customer.BillingAddress);

            //up to this line, we have the same address either in Addresses and BillingAddress
            customer.RemoveAddress(address0101);
            
            Assert.IsFalse(customer.Addresses.Contains(address0101)); //false - address removed from Addresses
            Assert.AreNotEqual(address0101, customer.BillingAddress); //false - address removed from BillingAddress
        }
    }
}