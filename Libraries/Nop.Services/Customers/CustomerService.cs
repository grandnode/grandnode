using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Events;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;

namespace Nop.Services.Customers
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
        private const string CUSTOMERROLES_ALL_KEY = "Nop.customerrole.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : system name
        /// </remarks>
        private const string CUSTOMERROLES_BY_SYSTEMNAME_KEY = "Nop.customerrole.systemname-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CUSTOMERROLES_PATTERN_KEY = "Nop.customerrole.";
        private const string CUSTOMERROLESPRODUCTS_PATTERN_KEY = "Nop.product.cr";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer role Id?
        /// </remarks>
        private const string CUSTOMERROLESPRODUCTS_ROLE_KEY = "Nop.customerroleproducts.role-{0}";

        #endregion

        #region Fields

        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<CustomerRoleProduct> _customerRoleProductRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<ForumPost> _forumPostRepository;
        private readonly IRepository<ForumTopic> _forumTopicRepository;
        private readonly IRepository<BlogComment> _blogCommentRepository;
        private readonly IRepository<NewsComment> _newsCommentRepository;
        private readonly IRepository<PollVotingRecord> _pollVotingRecordRepository;
        private readonly IRepository<ProductReview> _productReviewRepository;
        private readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
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
            IRepository<CustomerRoleProduct> customerRoleProductRepository,
            IRepository<Order> orderRepository,
            IRepository<ForumPost> forumPostRepository,
            IRepository<ForumTopic> forumTopicRepository,
            IRepository<BlogComment> blogCommentRepository,
            IRepository<NewsComment> newsCommentRepository,
            IRepository<PollVotingRecord> pollVotingRecordRepository,
            IRepository<ProductReview> productReviewRepository,
            IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
            IGenericAttributeService genericAttributeService,
            IDataProvider dataProvider,
            IEventPublisher eventPublisher, 
            CustomerSettings customerSettings,
            CommonSettings commonSettings)
        {
            this._cacheManager = cacheManager;
            this._customerRepository = customerRepository;
            this._customerRoleRepository = customerRoleRepository;
            this._customerRoleProductRepository = customerRoleProductRepository;
            this._orderRepository = orderRepository;
            this._forumPostRepository = forumPostRepository;
            this._forumTopicRepository = forumTopicRepository;
            this._blogCommentRepository = blogCommentRepository;
            this._newsCommentRepository = newsCommentRepository;
            this._pollVotingRecordRepository = pollVotingRecordRepository;
            this._productReviewRepository = productReviewRepository;
            this._productReviewHelpfulnessRepository = productReviewHelpfulnessRepository;
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
            DateTime? createdToUtc = null, int affiliateId = 0, int vendorId = 0,
            int[] customerRoleIds = null, int[] customerTagIds = null, string email = null, string username = null,
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
            if (affiliateId > 0)
                query = query.Where(c => affiliateId == c.AffiliateId);
            if (vendorId > 0)
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
                query = query.Where(c => c.Email!=null && c.Email.ToLower().Contains(email.ToLower()));
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
                    query.Where(c => c.ShoppingCartItems.Any(x => x.ShoppingCartTypeId == sctId)) :
                    query.Where(c => c.ShoppingCartItems.Any());
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
            int[] customerRoleIds, int pageIndex = 0, int pageSize = int.MaxValue)
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

        /// <summary>
        /// Delete a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void DeleteCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (customer.IsSystemAccount)
                throw new NopException(string.Format("System customer account ({0}) could not be deleted", customer.SystemName));

            customer.Deleted = true;

            if (_customerSettings.SuffixDeletedCustomers)
            {
                if (!String.IsNullOrEmpty(customer.Email))
                    customer.Email += "-DELETED";
                if (!String.IsNullOrEmpty(customer.Username))
                    customer.Username += "-DELETED";
            }

            UpdateCustomer(customer);
        }

        /// <summary>
        /// Gets a customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>A customer</returns>
        public virtual Customer GetCustomerById(int customerId)
        {
            if (customerId == 0)
                return null;
            
            return _customerRepository.GetById(customerId);
        }

        /// <summary>
        /// Get customers by identifiers
        /// </summary>
        /// <param name="customerIds">Customer identifiers</param>
        /// <returns>Customers</returns>
        public virtual IList<Customer> GetCustomersByIds(int[] customerIds)
        {
            if (customerIds == null || customerIds.Length == 0)
                return new List<Customer>();

            var query = from c in _customerRepository.Table
                        where customerIds.Contains(c.Id)
                        select c;
            var customers = query.ToList();
            //sort by passed identifiers
            var sortedCustomers = new List<Customer>();
            foreach (int id in customerIds)
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

            var query = from c in _customerRepository.Table
                        where c.CustomerGuid == customerGuid
                        orderby c.Id
                        select c;
            var customer = query.FirstOrDefault();
            return customer;
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
            var query = from c in _customerRepository.Table
                        orderby c.Id
                        where c.Email != null && c.Email.ToLower() == email.ToLower()
                        select c;
            var customer = query.FirstOrDefault();
            return customer;
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

            var query = from c in _customerRepository.Table
                        orderby c.Id
                        where c.SystemName == systemName
                        select c;
            var customer = query.FirstOrDefault();
            return customer;
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

            var query = from c in _customerRepository.Table
                        orderby c.Id
                        where c.Username != null && c.Username.ToLower() == username.ToLower()
                        select c;
            var customer = query.FirstOrDefault();
            return customer;
        }
        
        /// <summary>
        /// Insert a guest customer
        /// </summary>
        /// <returns>Customer</returns>
        public virtual Customer InsertGuestCustomer()
        {
            var customer = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };

            //add to 'Guests' role
            var guestRole = GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests);
            if (guestRole == null)
                throw new NopException("'Guests' role could not be loaded");
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

            _customerRepository.Insert(customer);

            //event notification
            _eventPublisher.EntityInserted(customer);
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
                .Set(x => x.Email, customer.Email)
                .Set(x => x.PasswordFormatId, customer.PasswordFormatId)
                .Set(x => x.PasswordSalt, customer.PasswordSalt)
                .Set(x => x.Active, customer.Active)
                .Set(x => x.Password, customer.Password)
                .Set(x => x.Username, customer.Username)
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
                .Set(x => x.LastLoginDateUtc, customer.LastLoginDateUtc);
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
                .Set(x => x.Email, customer.Email)
                .Set(x => x.IsSystemAccount, customer.IsSystemAccount)
                .Set(x => x.Active, customer.Active)
                .Set(x => x.Email, customer.Email)
                .Set(x => x.IsTaxExempt, customer.IsTaxExempt)
                .Set(x => x.Password, customer.Password)
                .Set(x => x.SystemName, customer.SystemName)
                .Set(x => x.Username, customer.Username)
                .Set(x => x.CustomerRoles, customer.CustomerRoles)
                .Set(x => x.Addresses, customer.Addresses)
                .Set(x => x.FreeShipping, customer.FreeShipping)
                .Set(x => x.VendorId, customer.VendorId);

            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;
            //event notification
            _eventPublisher.EntityUpdated(customer);

        }

        public virtual void UpdateFreeShipping(int customerId, bool freeShipping)
        {
            if (customerId == 0)
                throw new ArgumentNullException("customer");
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
        public virtual void UpdateNewsItem(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customer.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.IsNewsItem, true);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;
        }
        public virtual void UpdateHasShoppingCartItems(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customer.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.HasShoppingCartItems, customer.HasShoppingCartItems);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;
        }

        public virtual void UpdateHasForumTopic(int customerId)
        {
            if (customerId == 0)
                throw new ArgumentNullException("customer");
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customerId);
            var update = Builders<Customer>.Update
                .Set(x => x.IsHasForumTopic, true);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;
        }
        public virtual void UpdateHasForumPost(int customerId)
        {
            if (customerId == 0)
                throw new ArgumentNullException("customer");
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customerId);
            var update = Builders<Customer>.Update
                .Set(x => x.IsHasForumPost, true);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;
        }
        public virtual void UpdateHasOrders(int customerId)
        {
            if (customerId == 0)
                throw new ArgumentNullException("customer");
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customerId);
            var update = Builders<Customer>.Update
                .Set(x => x.IsHasOrders, true);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;
        }
        public virtual void UpdateHasBlogComments(int customerId)
        {
            if (customerId == 0)
                throw new ArgumentNullException("customer");
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customerId);
            var update = Builders<Customer>.Update
                .Set(x => x.IsHasBlogComments, true);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;
        }
        public virtual void UpdateHasProductReview(int customerId)
        {
            if (customerId == 0)
                throw new ArgumentNullException("customer");
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customerId);
            var update = Builders<Customer>.Update
                .Set(x => x.IsHasProductReview, true);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;
        }
        public virtual void UpdateHasProductReviewH(int customerId)
        {
            if (customerId == 0)
                throw new ArgumentNullException("customer");
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customerId);
            var update = Builders<Customer>.Update
                .Set(x => x.IsHasProductReviewH, true);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;
        }
        public virtual void UpdateHasPoolVoting(int customerId)
        {
            if (customerId == 0)
                throw new ArgumentNullException("customer");
            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customerId);
            var update = Builders<Customer>.Update
                .Set(x => x.IsHasPoolVoting, true);
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
        public virtual void ResetCheckoutData(Customer customer, int storeId,
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
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.SelectedPickUpInStore, false, storeId);
            }

            //clear selected payment method
            if (clearPaymentMethod)
            {
                _genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, null, storeId);
            }
            
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

                #region No stored procedure

                var guestRole = GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests);
                if (guestRole == null)
                    throw new NopException("'Guests' role could not be loaded");

                var query = _customerRepository.Table;

                if (createdFromUtc.HasValue)
                    query = query.Where(c => createdFromUtc.Value <= c.CreatedOnUtc);
                if (createdToUtc.HasValue)
                    query = query.Where(c => createdToUtc.Value >= c.CreatedOnUtc);
                query = query.Where(c => c.CustomerRoles.Any(cr => cr.Id == guestRole.Id));
                if (onlyWithoutShoppingCart)
                    query = query.Where(c => !c.ShoppingCartItems.Any());

                //no orders     
                query = query.Where(c => !c.IsHasOrders);
                //no blog comments
                query = query.Where(c => !c.IsHasBlogComments);

                //no news comments
                query = query.Where(c => !c.IsNewsItem);

                //no product reviews
                query = query.Where(c => !c.IsHasProductReview);

                //no product reviews helpfulness
                query = query.Where(c => !c.IsHasProductReviewH);

                //no poll voting
                query = query.Where(c => !c.IsHasPoolVoting);

                //no forum posts 
                query = query.Where(c => !c.IsHasForumPost);

                //no forum topics
                query = query.Where(c => !c.IsHasForumTopic);

                //don't delete system accounts
                query = query.Where(c => !c.IsSystemAccount);

                query = query.OrderBy(c => c.Id);
                var customers = query.ToList();

                int totalRecordsDeleted = 0;
                foreach (var c in customers)
                {
                    try
                    {
                        //delete from database
                        _customerRepository.Delete(c);
                        totalRecordsDeleted++;
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine(exc);
                    }
            }
            return totalRecordsDeleted;

                #endregion
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
                throw new NopException("System role could not be deleted");

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
        public virtual CustomerRole GetCustomerRoleById(int customerRoleId)
        {
            if (customerRoleId == 0)
                return null;

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
                var query = from cr in _customerRoleRepository.Table
                            orderby cr.Id
                            where cr.SystemName == systemName
                            select cr;
                var customerRole = query.FirstOrDefault();
                return customerRole;
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
                            orderby cr.Name
                            where (showHidden || cr.Active)
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


        public virtual void DeleteCustomerRoleInCustomer(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException("pwi");

            var updatebuilder = Builders<Customer>.Update;
            var update = updatebuilder.Pull(p => p.CustomerRoles, customerRole);
            _customerRepository.Collection.UpdateOneAsync(new BsonDocument("Id", customerRole.CustomerId), update);

        }

        public virtual void InsertCustomerRoleInCustomer(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException("productWarehouse");

            var updatebuilder = Builders<Customer>.Update;
            var update = updatebuilder.AddToSet(p => p.CustomerRoles, customerRole);
            _customerRepository.Collection.UpdateOneAsync(new BsonDocument("Id", customerRole.CustomerId), update);

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
        public virtual IList<CustomerRoleProduct> GetCustomerRoleProducts(int customerRoleId)
        {
            string key = string.Format(CUSTOMERROLESPRODUCTS_ROLE_KEY, customerRoleId);
            return _cacheManager.Get(key, () =>
            {
                var query = from cr in _customerRoleProductRepository.Table
                            orderby cr.DisplayOrder
                            where (cr.CustomerRoleId == customerRoleId)
                            select cr;
                var customerRoles = query.ToList();
                return customerRoles;
            });
        }

        /// <summary>
        /// Gets customer roles products for customer role
        /// </summary>
        /// <param name="customerRoleId">Customer role id</param>
        /// <param name="productId">Product id</param>
        /// <returns>Customer role product</returns>
        public virtual CustomerRoleProduct GetCustomerRoleProduct(int customerRoleId, int productId)
        {
            var query = from cr in _customerRoleProductRepository.Table
                        orderby cr.DisplayOrder
                        where cr.CustomerRoleId == customerRoleId && cr.ProductId == productId
                        select cr;
            var customerRoles = query.ToList();
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Gets customer roles product
        /// </summary>
        /// <param name="Id">id</param>
        /// <returns>Customer role product</returns>
        public virtual CustomerRoleProduct GetCustomerRoleProductById(int id)
        {
            var query = from cr in _customerRoleProductRepository.Table
                        orderby cr.DisplayOrder
                        where cr.Id == id
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
            _customerRepository.Collection.UpdateOneAsync(new BsonDocument("Id", address.CustomerId), update);

        }

        public virtual void InsertAddress(Address address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            var updatebuilder = Builders<Customer>.Update;
            var update = updatebuilder.AddToSet(p => p.Addresses, address);
            _customerRepository.Collection.UpdateOneAsync(new BsonDocument("Id", address.CustomerId), update);

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

        public virtual void RemoveShippingAddress(int customerId)
        {
            if (customerId == 0)
                throw new ArgumentNullException("customerId");

            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customerId);
            var update = Builders<Customer>.Update
                .Set(x => x.ShippingAddress, null);
            var result = _customerRepository.Collection.UpdateOneAsync(filter, update).Result;

        }

        #endregion

        #region Customer Shopping Cart Item

        public virtual void DeleteShoppingCartItem(ShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");

            var updatebuilder = Builders<Customer>.Update;
            var update = updatebuilder.Pull(p => p.ShoppingCartItems, shoppingCartItem);
            _customerRepository.Collection.UpdateOneAsync(new BsonDocument("Id", shoppingCartItem.CustomerId), update);
            _eventPublisher.EntityDeleted(shoppingCartItem);
        }

        public virtual void InsertShoppingCartItem(ShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");

            var updatebuilder = Builders<Customer>.Update;
            var update = updatebuilder.AddToSet(p => p.ShoppingCartItems, shoppingCartItem);
            _customerRepository.Collection.UpdateOneAsync(new BsonDocument("Id", shoppingCartItem.CustomerId), update);

            //event notification
            _eventPublisher.EntityInserted(shoppingCartItem);
        }

        public virtual void UpdateShoppingCartItem(ShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");

            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, shoppingCartItem.CustomerId);
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
        }

        #endregion

        #endregion
    }
}