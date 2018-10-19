using Grand.Core;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using System;
using System.Collections.Generic;

namespace Grand.Services.Customers
{
    /// <summary>
    /// Customer service interface
    /// </summary>
    public partial interface ICustomerService
    {
        #region Customers

        /// <summary>
        /// Gets all customers
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="customerRoleIds">A list of customer role identifiers to filter by (at least one match); pass null or empty list in order to load all customers; </param>
        /// <param name="email">Email; null to load all customers</param>
        /// <param name="username">Username; null to load all customers</param>
        /// <param name="firstName">First name; null to load all customers</param>
        /// <param name="lastName">Last name; null to load all customers</param>
        /// <param name="dayOfBirth">Day of birth; 0 to load all customers</param>
        /// <param name="monthOfBirth">Month of birth; 0 to load all customers</param>
        /// <param name="company">Company; null to load all customers</param>
        /// <param name="phone">Phone; null to load all customers</param>
        /// <param name="zipPostalCode">Phone; null to load all customers</param>
        /// <param name="loadOnlyWithShoppingCart">Value indicating whether to load customers only with shopping cart</param>
        /// <param name="sct">Value indicating what shopping cart type to filter; userd when 'loadOnlyWithShoppingCart' param is 'true'</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Customers</returns>
        IPagedList<Customer> GetAllCustomers(DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, string affiliateId = "", string vendorId = "",
            string[] customerRoleIds = null, string[] customerTagIds = null, string email = null, string username = null,
            string firstName = null, string lastName = null,
            string company = null, string phone = null, string zipPostalCode = null,
            bool loadOnlyWithShoppingCart = false, ShoppingCartType? sct = null,
            int pageIndex = 0, int pageSize = int.MaxValue); //Int32.MaxValue
        
        /// <summary>
        /// Gets all customers by customer format (including deleted ones)
        /// </summary>
        /// <param name="passwordFormat">Password format</param>
        /// <returns>Customers</returns>
        IList<Customer> GetAllCustomersByPasswordFormat(PasswordFormat passwordFormat);

        /// <summary>
        /// Gets online customers
        /// </summary>
        /// <param name="lastActivityFromUtc">Customer last activity date (from)</param>
        /// <param name="customerRoleIds">A list of customer role identifiers to filter by (at least one match); pass null or empty list in order to load all customers; </param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Customers</returns>
        IPagedList<Customer> GetOnlineCustomers(DateTime lastActivityFromUtc,
            string[] customerRoleIds, int pageIndex = 0, int pageSize = int.MaxValue);

        int GetCountOnlineShoppingCart(DateTime lastActivityFromUtc);

        /// <summary>
        /// Delete a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void DeleteCustomer(Customer customer);

        /// <summary>
        /// Gets a customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>A customer</returns>
        Customer GetCustomerById(string customerId);

        /// <summary>
        /// Get customers by identifiers
        /// </summary>
        /// <param name="customerIds">Customer identifiers</param>
        /// <returns>Customers</returns>
        IList<Customer> GetCustomersByIds(string[] customerIds);

        /// <summary>
        /// Gets a customer by GUID
        /// </summary>
        /// <param name="customerGuid">Customer GUID</param>
        /// <returns>A customer</returns>
        Customer GetCustomerByGuid(Guid customerGuid);

        /// <summary>
        /// Get customer by email
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>Customer</returns>
        Customer GetCustomerByEmail(string email);
        
        /// <summary>
        /// Get customer by system role
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Customer</returns>
        Customer GetCustomerBySystemName(string systemName);

        /// <summary>
        /// Get customer by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>Customer</returns>
        Customer GetCustomerByUsername(string username);

        /// <summary>
        /// Insert a guest customer
        /// </summary>
        /// <returns>Customer</returns>
        Customer InsertGuestCustomer(string urlreferrer = "");

        /// <summary>
        /// Insert a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void InsertCustomer(Customer customer);

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateCustomer(Customer customer);

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateCustomerLastActivityDate(Customer customer);

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateCustomerVendor(Customer customer);

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateCustomerPassword(Customer customer);


        /// <summary>
        /// Update free shipping
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="freeShipping"></param>
        void UpdateFreeShipping(string customerId, bool freeShipping);

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateAffiliate(Customer customer);

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateActive(Customer customer);

        /// <summary>
        /// Update the customer
        /// </summary>
        /// <param name="customer"></param>
        void UpdateContributions(Customer customer);

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateCustomerLastLoginDate(Customer customer);

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateCustomerLastIpAddress(Customer customer);

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateCustomerLastPurchaseDate(string customerId, DateTime date);
        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateCustomerLastUpdateCartDate(string customerId, DateTime? date);
        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateCustomerLastUpdateWishList(string customerId, DateTime date);
        /// <summary>
        /// Updates the customer in admin panel
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateCustomerinAdminPanel(Customer customer);

        /// <summary>
        /// Reset data required for checkout
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="clearCouponCodes">A value indicating whether to clear coupon code</param>
        /// <param name="clearCheckoutAttributes">A value indicating whether to clear selected checkout attributes</param>
        /// <param name="clearRewardPoints">A value indicating whether to clear "Use reward points" flag</param>
        /// <param name="clearShippingMethod">A value indicating whether to clear selected shipping method</param>
        /// <param name="clearPaymentMethod">A value indicating whether to clear selected payment method</param>
        void ResetCheckoutData(Customer customer, string storeId,
            bool clearCouponCodes = false, bool clearCheckoutAttributes = false,
            bool clearRewardPoints = true, bool clearShippingMethod = true,
            bool clearPaymentMethod = true);

        /// <summary>
        /// Update Customer Reminder History
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="orderId"></param>
        void UpdateCustomerReminderHistory(string customerId, string orderId);


        /// <summary>
        /// Delete guest customer records
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="onlyWithoutShoppingCart">A value indicating whether to delete customers only without shopping cart</param>
        /// <returns>Number of deleted customers</returns>
        int DeleteGuestCustomers(DateTime? createdFromUtc, DateTime? createdToUtc, bool onlyWithoutShoppingCart);

        #endregion

        #region Password history

        /// <summary>
        /// Gets customer passwords
        /// </summary>
        /// <param name="customerId">Customer identifier; pass null to load all records</param>
        /// <param name="passwordsToReturn">Number of returning passwords; pass null to load all records</param>
        /// <returns>List of customer passwords</returns>
        IList<CustomerHistoryPassword> GetPasswords(string customerId, int passwordsToReturn);

        /// <summary>
        /// Insert a customer history password
        /// </summary>
        /// <param name="customer">Customer</param>
        void InsertCustomerPassword(Customer customer);


        #endregion

        #region Customer roles

        /// <summary>
        /// Inserts a customer role
        /// </summary>
        /// <param name="customerRole">Customer role</param>
        void InsertCustomerRole(CustomerRole customerRole);

        /// <summary>
        /// Updates the customer role
        /// </summary>
        /// <param name="customerRole">Customer role</param>
        void UpdateCustomerRole(CustomerRole customerRole);

        /// <summary>
        /// Delete a customer role
        /// </summary>
        /// <param name="customerRole">Customer role</param>
        void DeleteCustomerRole(CustomerRole customerRole);

        /// <summary>
        /// Gets a customer role
        /// </summary>
        /// <param name="customerRoleId">Customer role identifier</param>
        /// <returns>Customer role</returns>
        CustomerRole GetCustomerRoleById(string customerRoleId);

        /// <summary>
        /// Gets a customer role
        /// </summary>
        /// <param name="systemName">Customer role system name</param>
        /// <returns>Customer role</returns>
        CustomerRole GetCustomerRoleBySystemName(string systemName);

        /// <summary>
        /// Gets all customer roles
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Customer roles</returns>
        IList<CustomerRole> GetAllCustomerRoles(bool showHidden = false);

        /// <summary>
        /// Gets customer roles products for customer role
        /// </summary>
        /// <param name="customerRoleId">Customer role id</param>
        /// <returns>Customer role products</returns>
        IList<CustomerRoleProduct> GetCustomerRoleProducts(string customerRoleId);

        /// <summary>
        /// Gets customer roles products for customer role
        /// </summary>
        /// <param name="customerRoleId">Customer role id</param>
        /// <param name="productId">Product id</param>
        /// <returns>Customer role product</returns>
        CustomerRoleProduct GetCustomerRoleProduct(string customerRoleId, string productId);

        /// <summary>
        /// Gets customer roles product
        /// </summary>
        /// <param name="Id">id</param>
        /// <returns>Customer role product</returns>
        CustomerRoleProduct GetCustomerRoleProductById(string id);


        /// <summary>
        /// Inserts a customer role product
        /// </summary>
        /// <param name="customerRoleProduct">Customer role product</param>
        void InsertCustomerRoleProduct(CustomerRoleProduct customerRoleProduct);

        /// <summary>
        /// Updates the customer role product
        /// </summary>
        /// <param name="customerRoleProduct">Customer role product</param>
        void UpdateCustomerRoleProduct(CustomerRoleProduct customerRoleProduct);

        /// <summary>
        /// Delete a customer role product
        /// </summary>
        /// <param name="customerRoleProduct">Customer role product</param>
        void DeleteCustomerRoleProduct(CustomerRoleProduct customerRoleProduct);


        #endregion

        #region Customer Role in Customer

        void InsertCustomerRoleInCustomer(CustomerRole customerRole);

        void DeleteCustomerRoleInCustomer(CustomerRole customerRole);
        
        #endregion

        #region Customer address

        void DeleteAddress(Address address);
        void InsertAddress(Address address);
        void UpdateAddress(Address address);
        void UpdateBillingAddress(Address address);
        void UpdateShippingAddress(Address address);
        void RemoveShippingAddress(string customerId);

        #endregion

        #region Shopping cart 

        void ClearShoppingCartItem(string customerId, string storeId, ShoppingCartType shoppingCartType);
        void DeleteShoppingCartItem(string customerId, ShoppingCartItem shoppingCartItem);
        void InsertShoppingCartItem(string customerId, ShoppingCartItem shoppingCartItem);
        void UpdateShoppingCartItem(string customerId, ShoppingCartItem shoppingCartItem);
        #endregion

        #region Customer Product Price

        /// <summary>
        /// Gets a customer product price
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Customer product price</returns>
        CustomerProductPrice GetCustomerProductPriceById(string id);

        /// <summary>
        /// Gets a price
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// <param name="productId">Product Identifier</param>
        /// <returns>Customer product price</returns>
        decimal? GetPriceByCustomerProduct(string customerId, string productId);

        /// <summary>
        /// Gets a customer product 
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Customer product</returns>
        CustomerProduct GetCustomerProduct(string id);

        /// <summary>
        /// Gets a customer product 
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// <param name="productId">Product Identifier</param>
        /// <returns>Customer product</returns>
        CustomerProduct GetCustomerProduct(string customerId, string productId);

        /// <summary>
        /// Inserts a customer product price
        /// </summary>
        /// <param name="customerProductPrice">Customer product price</param>
        void InsertCustomerProductPrice(CustomerProductPrice customerProductPrice);

        /// <summary>
        /// Updates the customer product price
        /// </summary>
        /// <param name="customerProductPrice">Customer product price</param>
        void UpdateCustomerProductPrice(CustomerProductPrice customerProductPrice);

        /// <summary>
        /// Delete a customer product price
        /// </summary>
        /// <param name="customerProductPrice">Customer product price</param>
        void DeleteCustomerProductPrice(CustomerProductPrice customerProductPrice);



        /// <summary>
        /// Gets products price for customer
        /// </summary>
        /// <param name="customerId">Customer id</param>
        /// <returns>Customer products price</returns>
        IPagedList<CustomerProductPrice> GetProductsPriceByCustomer(string customerId, int pageIndex = 0, int pageSize = int.MaxValue);

        #endregion

        #region Customer product

        /// <summary>
        /// Inserts a customer product 
        /// </summary>
        /// <param name="customerProduct">Customer product</param>
        void InsertCustomerProduct(CustomerProduct customerProduct);

        /// <summary>
        /// Updates the customer product
        /// </summary>
        /// <param name="customerProduct">Customer product </param>
        void UpdateCustomerProduct(CustomerProduct customerProduct);

        /// <summary>
        /// Delete a customer product 
        /// </summary>
        /// <param name="customerProduct">Customer product </param>
        void DeleteCustomerProduct(CustomerProduct customerProduct);

        IPagedList<CustomerProduct> GetProductsByCustomer(string customerId, int pageIndex = 0, int pageSize = int.MaxValue);

        #endregion

        #region Customer note

        // <summary>
        /// Get note for customer
        /// </summary>
        /// <param name="id">Note identifier</param>
        /// <returns>CustomerNote</returns>
        CustomerNote GetCustomerNote(string id);

        /// <summary>
        /// Deletes an customer note
        /// </summary>
        /// <param name="customerNote">The customer note</param>
        void DeleteCustomerNote(CustomerNote customerNote);

        /// <summary>
        /// Insert an customer note
        /// </summary>
        /// <param name="customerNote">The customer note</param>
        void InsertCustomerNote(CustomerNote customerNote);

        /// <summary>
        /// Get notes for customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="displaytocustomer">Display to customer</param>
        /// <returns>OrderNote</returns>
        IList<CustomerNote> GetCustomerNotes(string customerId, bool? displaytocustomer = null);

        #endregion
    }
}