using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Services.Common;
using Grand.Services.Events;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Customers
{
    /// <summary>
    /// Customer service
    /// </summary>
    public partial class CustomerService : ICustomerService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        private const string CUSTOMERROLES_ALL_KEY = "Grand.customerrole.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : system name
        /// </remarks>
        private const string CUSTOMERROLES_BY_SYSTEMNAME_KEY = "Grand.customerrole.systemname-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CUSTOMERROLES_PATTERN_KEY = "Grand.customerrole.";
        private const string CUSTOMERROLESPRODUCTS_PATTERN_KEY = "Grand.product.cr";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CUSTOMER_PRODUCT_KEY = "Grand.product.personal-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer role Id?
        /// </remarks>
        private const string CUSTOMERROLESPRODUCTS_ROLE_KEY = "Grand.customerroleproducts.role-{0}";

        #endregion

        #region Fields

        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<CustomerRoleProduct> _customerRoleProductRepository;
        private readonly IRepository<CustomerProductPrice> _customerProductPriceRepository;
        private readonly IRepository<CustomerProduct> _customerProductRepository;
        private readonly IRepository<CustomerHistoryPassword> _customerHistoryPasswordProductRepository;
        private readonly IRepository<CustomerNote> _customerNoteRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<ForumPost> _forumPostRepository;
        private readonly IRepository<ForumTopic> _forumTopicRepository;
        private readonly IRepository<BlogComment> _blogCommentRepository;
        private readonly IRepository<ProductReview> _productReviewRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IDataProvider _dataProvider;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly CustomerSettings _customerSettings;
        private readonly CommonSettings _commonSettings;

        #endregion

        #region Ctor

        public CustomerService(ICacheManager cacheManager,
            IRepository<Customer> customerRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<CustomerProduct> customerProductRepository,
            IRepository<CustomerProductPrice> customerProductPriceRepository,
            IRepository<CustomerHistoryPassword> customerHistoryPasswordProductRepository,
            IRepository<CustomerRoleProduct> customerRoleProductRepository,
            IRepository<CustomerNote> customerNoteRepository,
            IRepository<Order> orderRepository,
            IRepository<ForumPost> forumPostRepository,
            IRepository<ForumTopic> forumTopicRepository,
            IRepository<BlogComment> blogCommentRepository,
            IRepository<ProductReview> productReviewRepository,
            IGenericAttributeService genericAttributeService,
            IDataProvider dataProvider,
            IEventPublisher eventPublisher, 
            CustomerSettings customerSettings,
            CommonSettings commonSettings)
        {
            this._cacheManager = cacheManager;
            this._customerRepository = customerRepository;
            this._customerRoleRepository = customerRoleRepository;
            this._customerProductRepository = customerProductRepository;
            this._customerProductPriceRepository = customerProductPriceRepository;
            this._customerHistoryPasswordProductRepository = customerHistoryPasswordProductRepository;
            this._customerRoleProductRepository = customerRoleProductRepository;
            this._customerNoteRepository = customerNoteRepository;
            this._orderRepository = orderRepository;
            this._forumPostRepository = forumPostRepository;
            this._forumTopicRepository = forumTopicRepository;
            this._blogCommentRepository = blogCommentRepository;
            this._productReviewRepository = productReviewRepository;
            this._genericAttributeService = genericAttributeService;
            this._dataProvider = dataProvider;
            this._eventPublisher = eventPublisher;
            this._customerSettings = customerSettings;
            this._commonSettings = commonSettings;
        }

        #endregion

        #region Methods

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
        public virtual IPagedList<Customer> GetAllCustomers(DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, string affiliateId = "", string vendorId = "",
            string[] customerRoleIds = null, string[] customerTagIds = null, string email = null, string username = null,
            string firstName = null, string lastName = null,
            string company = null, string phone = null, string zipPostalCode = null,
            bool loadOnlyWithShoppingCart = false, ShoppingCartType? sct = null,
            int pageIndex = 0, int pageSize = 2147483647)
        {
            var query = _customerRepository.Table;

            if (createdFromUtc.HasValue)
                query = query.Where(c => createdFromUtc.Value <= c.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(c => createdToUtc.Value >= c.CreatedOnUtc);
            if (!String.IsNullOrEmpty(affiliateId))
                query = query.Where(c => affiliateId == c.AffiliateId);
            if (!String.IsNullOrEmpty(vendorId))
                query = query.Where(c => vendorId == c.VendorId);
            query = query.Where(c => !c.Deleted);
            if (customerRoleIds != null && customerRoleIds.Length > 0)
                query = query.Where(c => c.CustomerRoles.Any(x => customerRoleIds.Contains(x.Id)));
            if (customerTagIds != null && customerTagIds.Length > 0)
            {
                foreach (var item in customerTagIds)
                {
                    query = query.Where(c => c.CustomerTags.Contains(item));
                }
            }
            if (!String.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email!=null && c.Email.Contains(email.ToLower()));
            if (!String.IsNullOrWhiteSpace(username))
                query = query.Where(c => c.Username!=null && c.Username.ToLower().Contains(username.ToLower()));
            
            if (!String.IsNullOrWhiteSpace(firstName))
            {
                query = query.Where(x => x.GenericAttributes.Any(y => y.Key == SystemCustomerAttributeNames.FirstName && y.Value!=null && y.Value.ToLower().Contains(firstName.ToLower())));
            }
            
            if (!String.IsNullOrWhiteSpace(lastName))
            {
                query = query.Where(x => x.GenericAttributes.Any(y => y.Key == SystemCustomerAttributeNames.LastName && y.Value != null && y.Value.ToLower().Contains(lastName.ToLower())));
            }

            //search by company
            if (!String.IsNullOrWhiteSpace(company))
            {
                query = query.Where(x => x.GenericAttributes.Any(y => y.Key == SystemCustomerAttributeNames.Company && y.Value != null && y.Value.ToLower().Contains(company.ToLower())));
            }
            //search by phone
            if (!String.IsNullOrWhiteSpace(phone))
            {
                query = query.Where(x => x.GenericAttributes.Any(y => y.Key == SystemCustomerAttributeNames.Phone && y.Value != null && y.Value.ToLower().Contains(phone.ToLower())));
            }
            //search by zip
            if (!String.IsNullOrWhiteSpace(zipPostalCode))
            {
                query = query.Where(x => x.GenericAttributes.Any(y => y.Key == SystemCustomerAttributeNames.ZipPostalCode && y.Value != null && y.Value.ToLower().Contains(zipPostalCode.ToLower())));
            }

            if (loadOnlyWithShoppingCart)
            {
                int? sctId = null;
                if (sct.HasValue)
                    sctId = (int)sct.Value;

                query = sct.HasValue ?
                    query.Where(c => c.ShoppingCartItems.Any(x => x.ShoppingCartTypeId == sctId.Value)) :
                    query.Where(c => c.ShoppingCartItems.Count() > 0);
            }
            
            query = query.OrderByDescending(c => c.CreatedOnUtc);

            var customers = new PagedList<Customer>(query, pageIndex, pageSize);
            return customers;
        }

        /// <summary>
        /// Gets all customers by customer format (including deleted ones)
        /// </summary>
        /// <param name="passwordFormat">Password format</param>
        /// <returns>Customers</returns>
        public virtual IList<Customer> GetAllCustomersByPasswordFormat(PasswordFormat passwordFormat)
        {
            var passwordFormatId = (int)passwordFormat;

            var query = _customerRepository.Table;
            query = query.Where(c => c.PasswordFormatId == passwordFormatId);
            query = query.OrderByDescending(c => c.CreatedOnUtc);
            var customers = query.ToList();
            return customers;
        }

        /// <summary>
        /// Gets online customers
        /// </summary>
        /// <param name="lastActivityFromUtc">Customer last activity date (from)</param>
        /// <param name="customerRoleIds">A list of customer role identifiers to filter by (at least one match); pass null or empty list in order to load all customers; </param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Customers</returns>
        public virtual IPagedList<Customer> GetOnlineCustomers(DateTime lastActivityFromUtc,
            string[] customerRoleIds, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _customerRepository.Table;
            query = query.Where(c => lastActivityFromUtc <= c.LastActivityDateUtc);
            query = query.Where(c => !c.Deleted);
            if (customerRoleIds != null && customerRoleIds.Length > 0)
                query = query.Where(c => c.CustomerRoles.Select(cr => cr.Id).Intersect(customerRoleIds).Any());
            
            query = query.OrderByDescending(c => c.LastActivityDateUtc);
            var customers = new PagedList<Customer>(query, pageIndex, pageSize);
            return customers;
        }


        public virtual int GetCountOnlineShoppingCart(DateTime lastActivityFromUtc)
        {
            var query = _customerRepository.Table;
            query = query.Where(c => lastActivityFromUtc <= c.LastUpdateCartDateUtc);
            return query.Count();
        }


        /// <summary>
        /// Delete a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void DeleteCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (customer.IsSystemAccount)
                throw new GrandException(string.Format("System customer account ({0}) could not be deleted", customer.SystemName));

            customer.Deleted = true;
            customer.Email = $"DELETED@{DateTime.UtcNow.Ticks}.COM";
            customer.Username = customer.Email;

            //delete address
            customer.Addresses.Clear();
            customer.BillingAddress = null;
            customer.ShippingAddress = null;
            //delete generic attr
            customer.GenericAttributes.Clear();
            //delete shopping cart
            customer.ShoppingCartItems.Clear();
            //delete customer roles
            customer.CustomerRoles.Clear();
            //clear customer tags
            customer.CustomerTags.Clear();
            //update customer
            _customerRepository.Update(customer);
        }

        /// <summary>
        /// Gets a customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>A customer</returns>
        public virtual Customer GetCustomerById(string customerId)
        {            
            return _customerRepository.GetById(customerId);
        }

        /// <summary>
        /// Get customers by identifiers
        /// </summary>
        /// <param name="customerIds">Customer identifiers</param>
        /// <returns>Customers</returns>
        public virtual IList<Customer> GetCustomersByIds(string[] customerIds)
        {
            if (customerIds == null || customerIds.Length == 0)
                return new List<Customer>();

            var query = from c in _customerRepository.Table
                        where customerIds.Contains(c.Id)
                        select c;
            var customers = query.ToList();
            //sort by passed identifiers
            var sortedCustomers = new List<Customer>();
            foreach (string id in customerIds)
            {
                var customer = customers.Find(x => x.Id == id);
                if (customer != null)
                    sortedCustomers.Add(customer);
            }
            return sortedCustomers;
        }

        /// <summary>
        /// Gets a customer by GUID
        /// </summary>
        /// <param name="customerGuid">Customer GUID</param>
        /// <returns>A customer</returns>
        public virtual Customer GetCustomerByGuid(Guid customerGuid)
        {
            if (customerGuid == Guid.Empty)
                return null;

            var filter = Builders<Customer>.Filter.Eq(x => x.CustomerGuid, customerGuid);
            return _customerRepository.Collection.Find(filter).FirstOrDefault();

        }

        /// <summary>
        /// Get customer by email
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>Customer</returns>
        public virtual Customer GetCustomerByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;
            var filter = Builders<Customer>.Filter.Eq(x => x.Email, email.ToLower());
            return _customerRepository.Collection.Find(filter).FirstOrDefault();
        }

        /// <summary>
        /// Get customer by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Customer</returns>
        public virtual Customer GetCustomerBySystemName(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return null;

            var filter = Builders<Customer>.Filter.Eq(x => x.SystemName, systemName);
            return _customerRepository.Collection.Find(filter).FirstOrDefault();
        }

        /// <summary>
        /// Get customer by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>Customer</returns>
        public virtual Customer GetCustomerByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var filter = Builders<Customer>.Filter.Eq(x => x.Username, username.ToLower());
            return _customerRepository.Collection.Find(filter).FirstOrDefault();

        }
        
        /// <summary>
        /// Insert a guest customer
        /// </summary>
        /// <returns>Customer</returns>
        public virtual Customer InsertGuestCustomer(string urlreferrer = "")
        {
            var customer = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                UrlReferrer = urlreferrer
            };

            //add to 'Guests' role
            var guestRole = GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests);
            if (guestRole == null)
                throw new GrandException("'Guests' role could not be loaded");
            customer.CustomerRoles.Add(guestRole);

            _customerRepository.Insert(customer);

            return customer;
        }
        
        /// <summary>
        /// Insert a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void InsertCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (!string.IsNullOrEmpty(customer.Email))
                customer.Email = customer.Email.ToLower();

            if (!string.IsNullOrEmpty(customer.Username))
                customer.Username = customer.Username.ToLower();

            _customerRepository.Insert(customer);

            //event notification
            _eventPublisher.EntityInserted(customer);
        }

        /// <summary>
        /// Insert a customer history password
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void InsertCustomerPassword(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var chp = new CustomerHistoryPassword();
            chp.Password = customer.Password;
            chp.PasswordFormatId = customer.PasswordFormatId;
            chp.PasswordSalt = customer.PasswordSalt;
            chp.CustomerId = customer.Id;
            chp.CreatedOnUtc = DateTime.UtcNow;

            _customerHistoryPasswordProductRepository.Insert(chp);

            //event notification
            _eventPublisher.EntityInserted(chp);
        }

        /// <summary>
        /// Gets customer passwords
        /// </summary>
        /// <param name="customerId">Customer identifier; pass null to load all records</param>
        /// <param name="passwordsToReturn">Number of returning passwords; pass null to load all records</param>
        /// <returns>List of customer passwords</returns>
        public virtual IList<CustomerHistoryPassword> GetPasswords(string customerId, int passwordsToReturn)
        {
            var filter = Builders<CustomerHistoryPassword>.Filter.Eq(x => x.CustomerId, customerId);
            return _customerHistoryPasswordProductRepository.Collection.Find(filter)
                    .SortByDescending(password => password.CreatedOnUtc)
                    .Limit(passwordsToReturn)
                    .ToList();
        }
        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void UpdateCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customer.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.Email, string.IsNullOrEmpty(customer.Email) ? "" : customer.Email.ToLower())
                .Set(x => x.PasswordFormatId, customer.PasswordFormatId)
                .Set(x => x.PasswordSalt, customer.PasswordSalt)
                .Set(x => x.Active, customer.Active)
                .Set(x => x.Password, customer.Password)
                .Set(x => x.PasswordChangeDateUtc, customer.PasswordChangeDateUtc)
                .Set(x => x.Username, string.IsNullOrEmpty(customer.Username) ? "" : customer.Username.ToLower())
                .Set(x => x.Deleted, customer.Deleted);

            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;

            //event notification
            _eventPublisher.EntityUpdated(customer);
        }
        /// <summary>
        /// Updates the customer - last activity date
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void UpdateCustomerLastActivityDate(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");
            
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customer.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.LastActivityDateUtc, customer.LastActivityDateUtc);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;

        }
        /// <summary>
        /// Updates the customer - last activity date
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void UpdateCustomerLastLoginDate(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customer.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.LastLoginDateUtc, customer.LastLoginDateUtc)
                .Set(x => x.FailedLoginAttempts, customer.FailedLoginAttempts)
                .Set(x => x.CannotLoginUntilDateUtc, customer.CannotLoginUntilDateUtc);

            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;

        }
        /// Updates the customer - last activity date
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void UpdateCustomerVendor(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customer.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.VendorId, customer.VendorId);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;

            //event notification
            _eventPublisher.EntityUpdated(customer);
        }
        /// <summary>
        /// Updates the customer - last activity date
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void UpdateCustomerLastIpAddress(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customer.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.LastIpAddress, customer.LastIpAddress);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;

        }
        /// <summary>
        /// Updates the customer - password
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void UpdateCustomerPassword(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customer.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.Password, customer.Password);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;
        }
        public virtual void UpdateCustomerinAdminPanel(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customer.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.Active, customer.Active)
                .Set(x => x.AdminComment, customer.AdminComment)
                .Set(x => x.AffiliateId, customer.AffiliateId)
                .Set(x => x.IsSystemAccount, customer.IsSystemAccount)
                .Set(x => x.Active, customer.Active)
                .Set(x => x.Email, string.IsNullOrEmpty(customer.Email) ? "" : customer.Email.ToLower())
                .Set(x => x.IsTaxExempt, customer.IsTaxExempt)
                .Set(x => x.Password, customer.Password)
                .Set(x => x.SystemName, customer.SystemName)
                .Set(x => x.Username, string.IsNullOrEmpty(customer.Username) ? "" : customer.Username.ToLower())
                .Set(x => x.CustomerRoles, customer.CustomerRoles)
                .Set(x => x.Addresses, customer.Addresses)
                .Set(x => x.FreeShipping, customer.FreeShipping)
                .Set(x => x.VendorId, customer.VendorId);

            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;
            //event notification
            _eventPublisher.EntityUpdated(customer);

        }
        public virtual void UpdateFreeShipping(string customerId, bool freeShipping)
        {
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customerId);
            var update = Builders<Customer>.Update
                .Set(x => x.FreeShipping, freeShipping);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;
        }
        public virtual void UpdateAffiliate(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customer.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.AffiliateId, customer.AffiliateId);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;

        }
        public virtual void UpdateActive(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customer.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.Active, customer.Active);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;
        }

        public virtual void UpdateContributions(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customer.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.HasContributions, true);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;
        }

        public virtual void UpdateCustomerLastPurchaseDate(string customerId, DateTime date)
        {
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customerId);
            var update = Builders<Customer>.Update
                .Set(x => x.LastPurchaseDateUtc, date);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;
        }
        public virtual void UpdateCustomerLastUpdateCartDate(string customerId, DateTime? date)
        {
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customerId);
            var update = Builders<Customer>.Update
                .Set(x => x.LastUpdateCartDateUtc, date);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;

        }
        public virtual void UpdateCustomerLastUpdateWishList(string customerId, DateTime date)
        {
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customerId);
            var update = Builders<Customer>.Update
                .Set(x => x.LastUpdateWishListDateUtc, date);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;

        }
       
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
        public virtual void ResetCheckoutData(Customer customer, string storeId,
            bool clearCouponCodes = false, bool clearCheckoutAttributes = false,
            bool clearRewardPoints = true, bool clearShippingMethod = true,
            bool clearPaymentMethod = true)
        {
            if (customer == null)
                throw new ArgumentNullException();
            
            //clear entered coupon codes
            if (clearCouponCodes)
            {
                _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.DiscountCouponCode, null);
                _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.GiftCardCouponCodes, null);
            }

            //clear checkout attributes
            if (clearCheckoutAttributes)
            {
                _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.CheckoutAttributes, null, storeId);
            }

            //clear reward points flag
            if (clearRewardPoints)
            {
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, false, storeId);
            }

            //clear selected shipping method
            if (clearShippingMethod)
            {
                _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.SelectedShippingOption, null, storeId);
                _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.OfferedShippingOptions, null, storeId);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.SelectedPickupPoint, "", storeId);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ShippingOptionAttributeDescription, "", storeId);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ShippingOptionAttributeXml, "", storeId);
            }

            //clear selected payment method
            if (clearPaymentMethod)
            {
                _genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, null, storeId);
            }
            
        }

        public virtual void UpdateCustomerReminderHistory(string customerId, string orderId)
        {
            var builder = Builders<CustomerReminderHistory>.Filter;
            var filter = builder.Eq(x => x.CustomerId, customerId);
            var customerReminderRepository = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IRepository<CustomerReminderHistory>>();

            //update started reminders
            filter = filter & builder.Eq(x => x.Status, (int)CustomerReminderHistoryStatusEnum.Started);
            var update = Builders<CustomerReminderHistory>.Update
                .Set(x => x.EndDate, DateTime.UtcNow)
                .Set(x => x.Status, (int)CustomerReminderHistoryStatusEnum.CompletedOrdered)
                .Set(x => x.OrderId, orderId);
            customerReminderRepository.Collection.UpdateManyAsync(filter, update);

            //update Ended reminders
            filter = builder.Eq(x => x.CustomerId, customerId);
            filter = filter & builder.Eq(x => x.Status, (int)CustomerReminderHistoryStatusEnum.CompletedReminder);
            filter = filter & builder.Gt(x => x.EndDate, DateTime.UtcNow.AddHours(-36));

            update = Builders<CustomerReminderHistory>.Update
                .Set(x => x.Status, (int)CustomerReminderHistoryStatusEnum.CompletedOrdered)
                .Set(x => x.OrderId, orderId);

            customerReminderRepository.Collection.UpdateManyAsync(filter, update);

        }


        /// <summary>
        /// Delete guest customer records
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="onlyWithoutShoppingCart">A value indicating whether to delete customers only without shopping cart</param>
        /// <returns>Number of deleted customers</returns>
        public virtual int DeleteGuestCustomers(DateTime? createdFromUtc, DateTime? createdToUtc, bool onlyWithoutShoppingCart)
        {

            var guestRole = GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests);
            if (guestRole == null)
                throw new GrandException("'Guests' role could not be loaded");

            var builder = Builders<Customer>.Filter;
            var filter = builder.ElemMatch(x => x.CustomerRoles, role => role.Id == guestRole.Id);

            if (createdFromUtc.HasValue)
                filter = filter & builder.Gte(x => x.LastActivityDateUtc, createdFromUtc.Value);
            if (createdToUtc.HasValue)
                filter = filter & builder.Lte(x => x.LastActivityDateUtc, createdToUtc.Value);
            if (onlyWithoutShoppingCart)
                filter = filter & builder.Size(x => x.ShoppingCartItems, 0);

            filter = filter & builder.Eq(x => x.HasContributions, false);

            var customers = _customerRepository.Collection.DeleteMany(filter);

            return (int)customers.DeletedCount;

        }

        #endregion
        
        #region Customer roles

        /// <summary>
        /// Delete a customer role
        /// </summary>
        /// <param name="customerRole">Customer role</param>
        public virtual void DeleteCustomerRole(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException("customerRole");

            if (customerRole.IsSystemRole)
                throw new GrandException("System role could not be deleted");

            _customerRoleRepository.Delete(customerRole);

            var builder = Builders<Customer>.Update;
            var updatefilter = builder.PullFilter(x => x.CustomerRoles, y => y.Id == customerRole.Id);
            var result = _customerRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;

            _cacheManager.RemoveByPattern(CUSTOMERROLES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(customerRole);
        }

        /// <summary>
        /// Gets a customer role
        /// </summary>
        /// <param name="customerRoleId">Customer role identifier</param>
        /// <returns>Customer role</returns>
        public virtual CustomerRole GetCustomerRoleById(string customerRoleId)
        {
            return _customerRoleRepository.GetById(customerRoleId);
        }

        /// <summary>
        /// Gets a customer role
        /// </summary>
        /// <param name="systemName">Customer role system name</param>
        /// <returns>Customer role</returns>
        public virtual CustomerRole GetCustomerRoleBySystemName(string systemName)
        {
            if (String.IsNullOrWhiteSpace(systemName))
                return null;

            string key = string.Format(CUSTOMERROLES_BY_SYSTEMNAME_KEY, systemName);
            return _cacheManager.Get(key, () =>
            {
                var filter = Builders<CustomerRole>.Filter.Eq(x => x.SystemName, systemName);
                return _customerRoleRepository.Collection.Find(filter).FirstOrDefault();
            });
        }

        /// <summary>
        /// Gets all customer roles
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Customer roles</returns>
        public virtual IList<CustomerRole> GetAllCustomerRoles(bool showHidden = false)
        {
            string key = string.Format(CUSTOMERROLES_ALL_KEY, showHidden);
            return _cacheManager.Get(key, () =>
            {
                var query = from cr in _customerRoleRepository.Table
                            where (showHidden || cr.Active)
                            orderby cr.Name
                            select cr;
                var customerRoles = query.ToList();
                return customerRoles;
            });
        }
        
        /// <summary>
        /// Inserts a customer role
        /// </summary>
        /// <param name="customerRole">Customer role</param>
        public virtual void InsertCustomerRole(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException("customerRole");

            _customerRoleRepository.Insert(customerRole);

            _cacheManager.RemoveByPattern(CUSTOMERROLES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(customerRole);
        }

        /// <summary>
        /// Updates the customer role
        /// </summary>
        /// <param name="customerRole">Customer role</param>
        public virtual void UpdateCustomerRole(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException("customerRole");

            _customerRoleRepository.Update(customerRole);

            var builder = Builders<Customer>.Filter;
            var filter = builder.ElemMatch(x => x.CustomerRoles, y => y.Id == customerRole.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.CustomerRoles.ElementAt(-1), customerRole);

            var result = _customerRepository.Collection.UpdateManyAsync(filter, update).Result;

            _cacheManager.RemoveByPattern(CUSTOMERROLES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(customerRole);
        }

        #endregion

        #region Customer role in customer

        public virtual void DeleteCustomerRoleInCustomer(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException("pwi");

            var updatebuilder = Builders<Customer>.Update;
            var update = updatebuilder.Pull(p => p.CustomerRoles, customerRole);
            _customerRepository.Collection.UpdateOneAsync(new BsonDocument("_id", customerRole.CustomerId), update);

        }

        public virtual void InsertCustomerRoleInCustomer(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException("productWarehouse");

            var updatebuilder = Builders<Customer>.Update;
            var update = updatebuilder.AddToSet(p => p.CustomerRoles, customerRole);
            _customerRepository.Collection.UpdateOneAsync(new BsonDocument("_id", customerRole.CustomerId), update);

        }

        #endregion

        #region Customer Role Products

        /// <summary>
        /// Delete a customer role product
        /// </summary>
        /// <param name="customerRoleProduct">Customer role product</param>
        public virtual void DeleteCustomerRoleProduct(CustomerRoleProduct customerRoleProduct)
        {
            if (customerRoleProduct == null)
                throw new ArgumentNullException("customerRole");

            _customerRoleProductRepository.Delete(customerRoleProduct);
            
            //clear cache
            _cacheManager.RemoveByPattern(string.Format(CUSTOMERROLESPRODUCTS_ROLE_KEY, customerRoleProduct.CustomerRoleId));
            _cacheManager.RemoveByPattern(CUSTOMERROLESPRODUCTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(customerRoleProduct);
        }


        /// <summary>
        /// Inserts a customer role product
        /// </summary>
        /// <param name="customerRoleProduct">Customer role product</param>
        public virtual void InsertCustomerRoleProduct(CustomerRoleProduct customerRoleProduct)
        {
            if (customerRoleProduct == null)
                throw new ArgumentNullException("customerRoleProduct");

            _customerRoleProductRepository.Insert(customerRoleProduct);

            //clear cache
            _cacheManager.RemoveByPattern(string.Format(CUSTOMERROLESPRODUCTS_ROLE_KEY, customerRoleProduct.CustomerRoleId));
            _cacheManager.RemoveByPattern(CUSTOMERROLESPRODUCTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(customerRoleProduct);
        }

        /// <summary>
        /// Updates the customer role product
        /// </summary>
        /// <param name="customerRoleProduct">Customer role product</param>
        public virtual void UpdateCustomerRoleProduct(CustomerRoleProduct customerRoleProduct)
        {
            if (customerRoleProduct == null)
                throw new ArgumentNullException("customerRoleProduct");

            var builder = Builders<CustomerRoleProduct>.Filter;
            var filter = builder.Eq(x => x.Id, customerRoleProduct.Id);
            var update = Builders<CustomerRoleProduct>.Update
                .Set(x => x.DisplayOrder, customerRoleProduct.DisplayOrder);
            var result = _customerRoleProductRepository.Collection.UpdateOneAsync(filter, update).Result;

            //clear cache
            _cacheManager.RemoveByPattern(string.Format(CUSTOMERROLESPRODUCTS_ROLE_KEY, customerRoleProduct.CustomerRoleId));
            _cacheManager.RemoveByPattern(CUSTOMERROLESPRODUCTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(customerRoleProduct);
        }


        /// <summary>
        /// Gets customer roles products for customer role
        /// </summary>
        /// <param name="customerRoleId">Customer role id</param>
        /// <returns>Customer role products</returns>
        public virtual IList<CustomerRoleProduct> GetCustomerRoleProducts(string customerRoleId)
        {
            string key = string.Format(CUSTOMERROLESPRODUCTS_ROLE_KEY, customerRoleId);
            return _cacheManager.Get(key, () =>
            {
                var filter = Builders<CustomerRoleProduct>.Filter.Eq(x => x.CustomerRoleId, customerRoleId);
                return _customerRoleProductRepository.Collection.Find(filter).SortBy(x => x.DisplayOrder).ToList();
            });
        }

        /// <summary>
        /// Gets customer roles products for customer role
        /// </summary>
        /// <param name="customerRoleId">Customer role id</param>
        /// <param name="productId">Product id</param>
        /// <returns>Customer role product</returns>
        public virtual CustomerRoleProduct GetCustomerRoleProduct(string customerRoleId, string productId)
        {
            var filters = Builders<CustomerRoleProduct>.Filter;
            var filter = filters.Eq(x => x.CustomerRoleId, customerRoleId);
            filter = filter & filters.Eq(x => x.ProductId, productId);

            return _customerRoleProductRepository.Collection.Find(filter).SortBy(x => x.DisplayOrder).FirstOrDefault();
        }

        /// <summary>
        /// Gets customer roles product
        /// </summary>
        /// <param name="Id">id</param>
        /// <returns>Customer role product</returns>
        public virtual CustomerRoleProduct GetCustomerRoleProductById(string id)
        {
            var query = from cr in _customerRoleProductRepository.Table
                        where cr.Id == id
                        orderby cr.DisplayOrder
                        select cr;
            var customerRoles = query.ToList();
            return query.FirstOrDefault();
        }


        #endregion

        #region Customer Address

        public virtual void DeleteAddress(Address address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            var updatebuilder = Builders<Customer>.Update;
            var update = updatebuilder.Pull(p => p.Addresses, address);
            _customerRepository.Collection.UpdateOneAsync(new BsonDocument("_id", address.CustomerId), update);

        }

        public virtual void InsertAddress(Address address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            if (address.StateProvinceId == "0")
                address.StateProvinceId = "";

            var updatebuilder = Builders<Customer>.Update;
            var update = updatebuilder.AddToSet(p => p.Addresses, address);
            _customerRepository.Collection.UpdateOneAsync(new BsonDocument("_id", address.CustomerId), update);

            //event notification
            _eventPublisher.EntityInserted(address);
        }

        public virtual void UpdateAddress(Address address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, address.CustomerId);
            filter = filter & builder.ElemMatch(x => x.Addresses, y => y.Id == address.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.Addresses.ElementAt(-1).Address1, address.Address1)
                .Set(x => x.Addresses.ElementAt(-1).Address2, address.Address2)
                .Set(x => x.Addresses.ElementAt(-1).City, address.City)
                .Set(x => x.Addresses.ElementAt(-1).Company, address.Company)
                .Set(x => x.Addresses.ElementAt(-1).VatNumber, address.VatNumber)
                .Set(x => x.Addresses.ElementAt(-1).CountryId, address.CountryId)
                .Set(x => x.Addresses.ElementAt(-1).CustomAttributes, address.CustomAttributes)
                .Set(x => x.Addresses.ElementAt(-1).Email, address.Email)
                .Set(x => x.Addresses.ElementAt(-1).FaxNumber, address.FaxNumber)
                .Set(x => x.Addresses.ElementAt(-1).FirstName, address.FirstName)
                .Set(x => x.Addresses.ElementAt(-1).LastName, address.LastName)
                .Set(x => x.Addresses.ElementAt(-1).PhoneNumber, address.PhoneNumber)
                .Set(x => x.Addresses.ElementAt(-1).StateProvinceId, address.StateProvinceId)
                .Set(x => x.Addresses.ElementAt(-1).ZipPostalCode, address.ZipPostalCode);

            var result = _customerRepository.Collection.UpdateManyAsync(filter, update).Result;
            //event notification
            _eventPublisher.EntityUpdated(address);
        }


        public virtual void UpdateBillingAddress(Address address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, address.CustomerId);
            var update = Builders<Customer>.Update
                .Set(x => x.BillingAddress, address);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;

        }
        public virtual void UpdateShippingAddress(Address address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, address.CustomerId);
            var update = Builders<Customer>.Update
                .Set(x => x.ShippingAddress, address);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;

        }

        public virtual void RemoveShippingAddress(string customerId)
        {
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customerId);
            var update = Builders<Customer>.Update
                .Set(x => x.ShippingAddress, null);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;

        }

        #endregion

        #region Customer Shopping Cart Item

        public virtual void DeleteShoppingCartItem(string customerId, ShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");

            var updatebuilder = Builders<Customer>.Update;
            var update = updatebuilder.Pull(p => p.ShoppingCartItems, shoppingCartItem);
            _customerRepository.Collection.UpdateOneAsync(new BsonDocument("_id", customerId), update);

            //event notification
            _eventPublisher.EntityDeleted(shoppingCartItem);

            if (shoppingCartItem.ShoppingCartType == ShoppingCartType.ShoppingCart)
                UpdateCustomerLastUpdateCartDate(customerId, DateTime.UtcNow);
            else
                UpdateCustomerLastUpdateWishList(customerId, DateTime.UtcNow);

        }

        public virtual void ClearShoppingCartItem(string customerId, string storeId, ShoppingCartType shoppingCartType)
        {

            var updatebuilder = Builders<Customer>.Update;
            var update = updatebuilder.PullFilter(p => p.ShoppingCartItems, p=>p.StoreId == storeId && p.ShoppingCartTypeId == (int)shoppingCartType);
            _customerRepository.Collection.UpdateOneAsync(new BsonDocument("_id", customerId), update);

            if (shoppingCartType == ShoppingCartType.ShoppingCart || shoppingCartType == ShoppingCartType.Auctions)
                UpdateCustomerLastUpdateCartDate(customerId, DateTime.UtcNow);
            else
                UpdateCustomerLastUpdateWishList(customerId, DateTime.UtcNow);

        }


        public virtual void InsertShoppingCartItem(string customerId, ShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");

            var updatebuilder = Builders<Customer>.Update;
            var update = updatebuilder.AddToSet(p => p.ShoppingCartItems, shoppingCartItem);
            _customerRepository.Collection.UpdateOneAsync(new BsonDocument("_id", customerId), update);

            //event notification
            _eventPublisher.EntityInserted(shoppingCartItem);

            if (shoppingCartItem.ShoppingCartType == ShoppingCartType.ShoppingCart)
                UpdateCustomerLastUpdateCartDate(customerId, DateTime.UtcNow);
            else
                UpdateCustomerLastUpdateWishList(customerId, DateTime.UtcNow);
        }

        public virtual void UpdateShoppingCartItem(string customerId, ShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");

            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customerId);
            filter = filter & builder.ElemMatch(x => x.ShoppingCartItems, y => y.Id == shoppingCartItem.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.ShoppingCartItems.ElementAt(-1).Quantity, shoppingCartItem.Quantity)
                .Set(x => x.ShoppingCartItems.ElementAt(-1).AdditionalShippingChargeProduct, shoppingCartItem.AdditionalShippingChargeProduct)
                .Set(x => x.ShoppingCartItems.ElementAt(-1).IsFreeShipping, shoppingCartItem.IsFreeShipping)
                .Set(x => x.ShoppingCartItems.ElementAt(-1).IsGiftCard, shoppingCartItem.IsGiftCard)
                .Set(x => x.ShoppingCartItems.ElementAt(-1).IsRecurring, shoppingCartItem.IsRecurring)
                .Set(x => x.ShoppingCartItems.ElementAt(-1).IsShipEnabled, shoppingCartItem.IsShipEnabled)
                .Set(x => x.ShoppingCartItems.ElementAt(-1).IsTaxExempt, shoppingCartItem.IsTaxExempt)
                .Set(x => x.ShoppingCartItems.ElementAt(-1).RentalStartDateUtc, shoppingCartItem.RentalStartDateUtc)
                .Set(x => x.ShoppingCartItems.ElementAt(-1).RentalEndDateUtc, shoppingCartItem.RentalEndDateUtc)
                .Set(x => x.ShoppingCartItems.ElementAt(-1).AttributesXml, shoppingCartItem.AttributesXml)
                .Set(x => x.ShoppingCartItems.ElementAt(-1).CustomerEnteredPrice, shoppingCartItem.CustomerEnteredPrice)
                .Set(x => x.ShoppingCartItems.ElementAt(-1).UpdatedOnUtc, shoppingCartItem.UpdatedOnUtc)
                .Set(x => x.ShoppingCartItems.ElementAt(-1).ShoppingCartTypeId, shoppingCartItem.ShoppingCartTypeId);

            var result = _customerRepository.Collection.UpdateManyAsync(filter, update).Result;
            //event notification
            _eventPublisher.EntityUpdated(shoppingCartItem);

            if (shoppingCartItem.ShoppingCartType == ShoppingCartType.ShoppingCart)
                UpdateCustomerLastUpdateCartDate(customerId, DateTime.UtcNow);
            else
                UpdateCustomerLastUpdateWishList(customerId, DateTime.UtcNow);

        }

        #endregion

        #region Customer Product Price

        /// <summary>
        /// Gets a customer product price
        /// </summary>
        /// <param name="Id">Identifier</param>
        /// <returns>Customer product price</returns>
        public virtual CustomerProductPrice GetCustomerProductPriceById(string id)
        {
            if (id == null)
                throw new ArgumentNullException("Id");

            return _customerProductPriceRepository.GetById(id);
        }

        /// <summary>
        /// Gets a price
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// <param name="productId">Product Identifier</param>
        /// <returns>Customer product price</returns>
        public virtual decimal? GetPriceByCustomerProduct(string customerId, string productId)
        {
            var builder = Builders<CustomerProductPrice>.Filter;
            var filter = builder.Eq(x => x.CustomerId, customerId);
            filter = filter & builder.Eq(x => x.ProductId, productId);
            var productprice = _customerProductPriceRepository.Collection.Find(filter).FirstOrDefault();
            if (productprice == null)
                return null;
            else
                return productprice.Price;
        }

        /// <summary>
        /// Inserts a customer product price
        /// </summary>
        /// <param name="customerProductPrice">Customer product price</param>
        public virtual void InsertCustomerProductPrice(CustomerProductPrice customerProductPrice)
        {
            if (customerProductPrice == null)
                throw new ArgumentNullException("customerProductPrice");

            _customerProductPriceRepository.Insert(customerProductPrice);

            //event notification
            _eventPublisher.EntityInserted(customerProductPrice);
        }

        /// <summary>
        /// Updates the customer product price
        /// </summary>
        /// <param name="customerProductPrice">Customer product price</param>
        public virtual void UpdateCustomerProductPrice(CustomerProductPrice customerProductPrice)
        {
            if (customerProductPrice == null)
                throw new ArgumentNullException("customerProductPrice");

            _customerProductPriceRepository.Update(customerProductPrice);

            //event notification
            _eventPublisher.EntityUpdated(customerProductPrice);
        }

        /// <summary>
        /// Delete a customer product price
        /// </summary>
        /// <param name="customerProductPrice">Customer product price</param>
        public virtual void DeleteCustomerProductPrice(CustomerProductPrice customerProductPrice)
        {
            if (customerProductPrice == null)
                throw new ArgumentNullException("customerProductPrice");

            _customerProductPriceRepository.Delete(customerProductPrice);

            //event notification
            _eventPublisher.EntityDeleted(customerProductPrice);
        }

        public virtual IPagedList<CustomerProductPrice> GetProductsPriceByCustomer(string customerId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from pp in _customerProductPriceRepository.Table
                        where pp.CustomerId == customerId
                        select pp;
            return new PagedList<CustomerProductPrice>(query, pageIndex, pageSize);
        }

        #endregion

        #region Personalize products

        /// <summary>
        /// Gets a customer product 
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Customer product</returns>
        public virtual CustomerProduct GetCustomerProduct(string id)
        {
            var query = from pp in _customerProductRepository.Table
                        where pp.Id == id
                        select pp;

            return query.FirstOrDefault();
        }

        /// <summary>
        /// Gets a customer product 
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// <param name="productId">Product Identifier</param>
        /// <returns>Customer product</returns>
        public virtual CustomerProduct GetCustomerProduct(string customerId, string productId)
        {
            var query = from pp in _customerProductRepository.Table
                        where pp.CustomerId == customerId && pp.ProductId == productId
                        select pp;

            return query.FirstOrDefault();
        }

        /// <summary>
        /// Insert a customer product 
        /// </summary>
        /// <param name="customerProduct">Customer product</param>
        public virtual void InsertCustomerProduct(CustomerProduct customerProduct)
        {
            if (customerProduct == null)
                throw new ArgumentNullException("customerProduct");

            _customerProductRepository.Insert(customerProduct);

            //clear cache
            _cacheManager.RemoveByPattern(string.Format(CUSTOMER_PRODUCT_KEY, customerProduct.CustomerId));

            //event notification
            _eventPublisher.EntityInserted(customerProduct);
        }

        /// <summary>
        /// Updates the customer product
        /// </summary>
        /// <param name="customerProduct">Customer product </param>
        public virtual void UpdateCustomerProduct(CustomerProduct customerProduct)
        {
            if (customerProduct == null)
                throw new ArgumentNullException("customerProduct");

            _customerProductRepository.Update(customerProduct);

            //clear cache
            _cacheManager.RemoveByPattern(string.Format(CUSTOMER_PRODUCT_KEY, customerProduct.CustomerId));

            //event notification
            _eventPublisher.EntityUpdated(customerProduct);
        }

        /// <summary>
        /// Delete a customer product 
        /// </summary>
        /// <param name="customerProduct">Customer product</param>
        public virtual void DeleteCustomerProduct(CustomerProduct customerProduct)
        {
            if (customerProduct == null)
                throw new ArgumentNullException("customerProduct");

            _customerProductRepository.Delete(customerProduct);

            //clear cache
            _cacheManager.RemoveByPattern(string.Format(CUSTOMER_PRODUCT_KEY, customerProduct.CustomerId));

            //event notification
            _eventPublisher.EntityDeleted(customerProduct);
        }

        public virtual IPagedList<CustomerProduct> GetProductsByCustomer(string customerId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from pp in _customerProductRepository.Table
                        where pp.CustomerId == customerId
                        orderby pp.DisplayOrder
                        select pp;
            return new PagedList<CustomerProduct>(query, pageIndex, pageSize);
        }

        #endregion

        #region Customer note

        // <summary>
        /// Get note for customer
        /// </summary>
        /// <param name="id">Note identifier</param>
        /// <returns>CustomerNote</returns>
        public virtual CustomerNote GetCustomerNote(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            return _customerNoteRepository.Table.Where(x => x.Id == id).FirstOrDefault();
        }


        /// <summary>
        /// Deletes an customer note
        /// </summary>
        /// <param name="customerNote">The customer note</param>
        public virtual void DeleteCustomerNote(CustomerNote customerNote)
        {
            if (customerNote == null)
                throw new ArgumentNullException("customerNote");

            _customerNoteRepository.Delete(customerNote);

            //event notification
            _eventPublisher.EntityDeleted(customerNote);
        }

        /// <summary>
        /// Insert an customer note
        /// </summary>
        /// <param name="customerNote">The customer note</param>
        public virtual void InsertCustomerNote(CustomerNote customerNote)
        {
            if (customerNote == null)
                throw new ArgumentNullException("customerNote");

            _customerNoteRepository.Insert(customerNote);

            //event notification
            _eventPublisher.EntityInserted(customerNote);
        }

        /// <summary>
        /// Get notes for customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="displaytocustomer">Display to customer</param>
        /// <returns>OrderNote</returns>
        public virtual IList<CustomerNote> GetCustomerNotes(string customerId, bool? displaytocustomer = null)
        {
            var query = from customerNote in _customerNoteRepository.Table
                        where customerNote.CustomerId == customerId                        
                        select customerNote;

            if (displaytocustomer.HasValue)
                query = query.Where(x => x.DisplayToCustomer == displaytocustomer.Value);

            query = query.OrderByDescending(x => x.CreatedOnUtc);

            return query.ToList();
        }

        #endregion

        #endregion
    }
}