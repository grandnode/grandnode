using Grand.Domain.Common;
using System;
using System.Linq;

namespace Grand.Domain.Customers
{
    public static class CustomerExtensions
    {
        #region Customer role

        /// <summary>
        /// Gets a value indicating whether customer is in a certain customer role
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="customerRoleSystemName">Customer role system name</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        /// <param name="isSystemRoles">A value indicating whether we should look only in system roles</param>
        /// <returns>Result</returns>
        public static bool IsInCustomerRole(this Customer customer,
            string customerRoleSystemName, bool onlyActiveCustomerRoles = true, bool? isSystemRoles = null)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (String.IsNullOrEmpty(customerRoleSystemName))
                throw new ArgumentNullException("customerRoleSystemName");

            var result = customer.CustomerRoles
                .FirstOrDefault(cr => (!onlyActiveCustomerRoles || cr.Active) 
                && (cr.SystemName == customerRoleSystemName) 
                && (!isSystemRoles.HasValue || cr.IsSystemRole == isSystemRoles)) != null;
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customer a search engine
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public static bool IsSearchEngineAccount(this Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (!customer.IsSystemAccount || string.IsNullOrEmpty(customer.SystemName))
                return false;

            var result = customer.SystemName.Equals(SystemCustomerNames.SearchEngine, StringComparison.OrdinalIgnoreCase);
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether the customer is a built-in record for background tasks
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public static bool IsBackgroundTaskAccount(this Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (!customer.IsSystemAccount || String.IsNullOrEmpty(customer.SystemName))
                return false;

            var result = customer.SystemName.Equals(SystemCustomerNames.BackgroundTask, StringComparison.OrdinalIgnoreCase);
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customer is administrator
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public static bool IsAdmin(this Customer customer)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.Administrators, true, true);
        }

        /// <summary>
        /// Gets a value indicating whether customer is staff
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public static bool IsStaff(this Customer customer)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.Staff, true, true);
        }

        /// <summary>
        /// Gets a value indicating whether customer is sales manager
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public static bool IsSalesManager(this Customer customer)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.SalesManager, true, true);
        }

        /// <summary>
        /// Gets a value indicating whether customer is a forum moderator
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public static bool IsForumModerator(this Customer customer)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.ForumModerators);
        }

        /// <summary>
        /// Gets a value indicating whether customer is registered
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public static bool IsRegistered(this Customer customer)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.Registered, true, true);
        }

        /// <summary>
        /// Gets a value indicating whether customer is guest
        /// </summary>
        /// <param name="customer">Customer</param>        
        /// <returns>Result</returns>
        public static bool IsGuest(this Customer customer)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.Guests, true, true);
        }

        /// <summary>
        /// Gets a value indicating whether customer is vendor
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public static bool IsVendor(this Customer customer)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.Vendors, true, true);
        }

        /// <summary>
        /// Gets a value indicating whether customer is owner subaccount
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public static bool IsOwner(this Customer customer)
        {
            return string.IsNullOrEmpty(customer.OwnerId);
        }

        #endregion

        #region Addresses

        public static void RemoveAddress(this Customer customer, Address address)
        {
            if (customer.Addresses.Contains(address))
            {
                if (customer.BillingAddress == address) customer.BillingAddress = null;
                if (customer.ShippingAddress == address) customer.ShippingAddress = null;

                customer.Addresses.Remove(address);
            }
        }

        #endregion

        
    }
}
