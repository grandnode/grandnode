using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Discounts;
using Grand.Services.Discounts.Cache;
using Grand.Core.Domain.Orders;
using Grand.Core.Plugins;
using Grand.Services.Common;
using Grand.Services.Events;
using Grand.Services.Orders;
using MongoDB.Driver;
using Grand.Core.Domain.Catalog;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using Grand.Services.Localization;
using Grand.Core.Infrastructure;
using Grand.Services.Vendors;
using Grand.Services.Stores;
using Grand.Core.Domain.Vendors;
using Grand.Core.Domain.Stores;
using Grand.Services.Customers;

namespace Grand.Services.Discounts
{
    /// <summary>
    /// Discount service
    /// </summary>
    public partial class DiscountService : IDiscountService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : discont ID
        /// </remarks>
        private const string DISCOUNTS_BY_ID_KEY = "Nop.discount.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : coupon code
        /// {2} : discount name
        /// </remarks>
        private const string DISCOUNTS_ALL_KEY = "Nop.discount.all-{0}-{1}-{2}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string DISCOUNTS_PATTERN_KEY = "Nop.discount.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTS_PATTERN_KEY = "Nop.product.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string MANUFACTURERS_PATTERN_KEY = "Nop.manufacturer.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CATEGORIES_PATTERN_KEY = "Nop.category.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string VENDORS_PATTERN_KEY = "Nop.vendor.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string STORES_PATTERN_KEY = "Nop.store.";

        #endregion

        #region Fields

        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<DiscountUsageHistory> _discountUsageHistoryRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<Store> _storeRepository;
        private readonly ILocalizationService _localizationService;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPluginFinder _pluginFinder;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public DiscountService(ICacheManager cacheManager,
            IRepository<Discount> discountRepository,
            IRepository<DiscountUsageHistory> discountUsageHistoryRepository,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            IPluginFinder pluginFinder,
            IEventPublisher eventPublisher,
            IRepository<Product> productRepository,
            IRepository<Category> categoryRepository,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<Vendor> vendorRepository,
            IRepository<Store> storeRepository
            )
        {
            this._cacheManager = cacheManager;
            this._discountRepository = discountRepository;
            this._discountUsageHistoryRepository = discountUsageHistoryRepository;
            this._localizationService = localizationService;
            this._storeContext = storeContext;
            this._genericAttributeService = genericAttributeService;
            this._pluginFinder = pluginFinder;
            this._eventPublisher = eventPublisher;
            this._productRepository = productRepository;
            this._categoryRepository = categoryRepository;
            this._manufacturerRepository = manufacturerRepository;
            this._vendorRepository = vendorRepository;
            this._storeRepository = storeRepository;
        }

        #endregion

        #region Nested classes

        [Serializable]
        public class DiscountRequirementForCaching
        {
            public string Id { get; set; }
            public string SystemName { get; set; }
            public string DiscountId { get; set; }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Checks discount limitation for customer
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <returns>Value indicating whether discount can be used</returns>
        protected virtual bool CheckDiscountLimitations(Discount discount, Customer customer)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            switch (discount.DiscountLimitation)
            {
                case DiscountLimitationType.Unlimited:
                    {
                        return true;
                    }
                case DiscountLimitationType.NTimesOnly:
                    {
                        var totalDuh = GetAllDiscountUsageHistory(discount.Id, null, null, 0, 1).TotalCount;
                        return totalDuh < discount.LimitationTimes;
                    }
                case DiscountLimitationType.NTimesPerCustomer:
                    {
                        if (customer != null && !customer.IsGuest())
                        {
                            //registered customer
                            var totalDuh = GetAllDiscountUsageHistory(discount.Id, customer.Id, null, 0, 1).TotalCount;
                            return totalDuh < discount.LimitationTimes;
                        }

                        //guest
                        return true;
                    }
                default:
                    break;
            }
            return false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete discount
        /// </summary>
        /// <param name="discount">Discount</param>
        public virtual void DeleteDiscount(Discount discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            _discountRepository.Delete(discount);

            if (discount.DiscountType == DiscountType.AssignedToSkus)
            {
                var builderproduct = Builders<Product>.Update;
                var updatefilter = builderproduct.PullFilter(x => x.AppliedDiscounts, y => y.Id == discount.Id);
                var result = _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;
                _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            }

            if (discount.DiscountType == DiscountType.AssignedToCategories)
            {
                var buildercategory = Builders<Category>.Update;
                var updatefilter = buildercategory.PullFilter(x => x.AppliedDiscounts, y => y.Id == discount.Id);
                var result = _categoryRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;
                _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            }

            if (discount.DiscountType == DiscountType.AssignedToManufacturers)
            {
                var buildermanufacturer = Builders<Manufacturer>.Update;
                var updatefilter = buildermanufacturer.PullFilter(x => x.AppliedDiscounts, y => y.Id == discount.Id);
                var result = _manufacturerRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;
                _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            }
            if (discount.DiscountType == DiscountType.AssignedToVendors)
            {
                var buildervendor = Builders<Vendor>.Update;
                var updatefilter = buildervendor.PullFilter(x => x.AppliedDiscounts, y => y.Id == discount.Id);
                var result = _vendorRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;
                _cacheManager.RemoveByPattern(VENDORS_PATTERN_KEY);
            }

            if (discount.DiscountType == DiscountType.AssignedToStores)
            {
                var builderstore = Builders<Store>.Update;
                var updatefilter = builderstore.PullFilter(x => x.AppliedDiscounts, y => y.Id == discount.Id);
                var result = _storeRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;
                _cacheManager.RemoveByPattern(STORES_PATTERN_KEY);
            }

            _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(discount);
        }

        /// <summary>
        /// Gets a discount
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <returns>Discount</returns>
        public virtual Discount GetDiscountById(string discountId)
        {
            string key = string.Format(DISCOUNTS_BY_ID_KEY, discountId);
            return _cacheManager.Get(key, () => _discountRepository.GetById(discountId));
        }

        /// <summary>
        /// Gets all discounts
        /// </summary>
        /// <param name="discountType">Discount type; null to load all discount</param>
        /// <param name="couponCode">Coupon code to find (exact match)</param>
        /// <param name="discountName">Discount name</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Discounts</returns>
        public virtual IList<Discount> GetAllDiscounts(DiscountType? discountType,
            string couponCode = "", string discountName = "", bool showHidden = false)
        {
            //we load all discounts, and filter them by passed "discountType" parameter later
            //we do it because we know that this method is invoked several times per HTTP request with distinct "discountType" parameter
            //that's why let's access the database only once
            string key = string.Format(DISCOUNTS_ALL_KEY, showHidden, couponCode, discountName);
            var result = _cacheManager.Get(key, () =>
            {
                var query = _discountRepository.Table;
                if (!showHidden)
                {
                    //The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
                    //That's why we pass the date value
                    var nowUtc = DateTime.UtcNow;
                    query = query.Where(d =>
                        (!d.StartDateUtc.HasValue || d.StartDateUtc <= nowUtc)
                        && (!d.EndDateUtc.HasValue || d.EndDateUtc >= nowUtc)
                        );
                }
                if (!String.IsNullOrEmpty(couponCode))
                {
                    query = query.Where(d => d.CouponCode!=null && d.CouponCode.ToLower().Contains(couponCode.ToLower()));
                }
                if (!String.IsNullOrEmpty(discountName))
                {
                    query = query.Where(d => d.Name!=null && d.Name.ToLower().Contains(discountName.ToLower()));
                }
                query = query.OrderBy(d => d.Name);
                
                var discounts = query.ToList();
                return discounts;
            });
            //we know that this method is usually inkoved multiple times
            //that's why we filter discounts by type on the application layer
            if (discountType.HasValue)
            {
                result = result.Where(d => d.DiscountType == discountType.Value).ToList();
            }
            return result;
        }

        /// <summary>
        /// Inserts a discount
        /// </summary>
        /// <param name="discount">Discount</param>
        public virtual void InsertDiscount(Discount discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            _discountRepository.Insert(discount);

            _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(discount);
        }

        /// <summary>
        /// Updates the discount
        /// </summary>
        /// <param name="discount">Discount</param>
        public virtual void UpdateDiscount(Discount discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            foreach(var req in discount.DiscountRequirements)
            {
                req.DiscountId = discount.Id;
            }

            _discountRepository.Update(discount);


            if (discount.DiscountType == DiscountType.AssignedToSkus)
            {
                var builder = Builders<Product>.Filter;
                var filter = builder.ElemMatch(x => x.AppliedDiscounts, y => y.Id == discount.Id);
                var update = Builders<Product>.Update
                    .Set(x => x.AppliedDiscounts.ElementAt(-1), discount);
                var result = _productRepository.Collection.UpdateManyAsync(filter, update).Result;
                _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            }

            if (discount.DiscountType == DiscountType.AssignedToCategories)
            {
                var builder = Builders<Category>.Filter;
                var filter = builder.ElemMatch(x => x.AppliedDiscounts, y => y.Id == discount.Id);
                var update = Builders<Category>.Update
                    .Set(x => x.AppliedDiscounts.ElementAt(-1), discount);
                var result = _categoryRepository.Collection.UpdateManyAsync(filter, update).Result;
                _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            }

            if (discount.DiscountType == DiscountType.AssignedToManufacturers)
            {
                var builder = Builders<Manufacturer>.Filter;
                var filter = builder.ElemMatch(x => x.AppliedDiscounts, y => y.Id == discount.Id);
                var update = Builders<Manufacturer>.Update
                    .Set(x => x.AppliedDiscounts.ElementAt(-1), discount);
                var result = _manufacturerRepository.Collection.UpdateManyAsync(filter, update).Result;
                _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            }
            if (discount.DiscountType == DiscountType.AssignedToVendors)
            {
                var builder = Builders<Vendor>.Filter;
                var filter = builder.ElemMatch(x => x.AppliedDiscounts, y => y.Id == discount.Id);
                var update = Builders<Vendor>.Update
                    .Set(x => x.AppliedDiscounts.ElementAt(-1), discount);
                var result = _vendorRepository.Collection.UpdateManyAsync(filter, update).Result;
                _cacheManager.RemoveByPattern(VENDORS_PATTERN_KEY);
            }

            if (discount.DiscountType == DiscountType.AssignedToStores)
            {
                var builder = Builders<Store>.Filter;
                var filter = builder.ElemMatch(x => x.AppliedDiscounts, y => y.Id == discount.Id);
                var update = Builders<Store>.Update
                    .Set(x => x.AppliedDiscounts.ElementAt(-1), discount);
                var result = _storeRepository.Collection.UpdateManyAsync(filter, update).Result;
                _cacheManager.RemoveByPattern(STORES_PATTERN_KEY);
            }

            _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(discount);
        }

        /// <summary>
        /// Delete discount requirement
        /// </summary>
        /// <param name="discountRequirement">Discount requirement</param>
        public virtual void DeleteDiscountRequirement(DiscountRequirement discountRequirement)
        {
            if (discountRequirement == null)
                throw new ArgumentNullException("discountRequirement");

            var discount = _discountRepository.GetById(discountRequirement.DiscountId);
            if(discount==null)
                throw new ArgumentNullException("discount");
            var req = discount.DiscountRequirements.FirstOrDefault(x => x.Id == discountRequirement.Id);
            if (req == null)
                throw new ArgumentNullException("discountRequirement");

            discount.DiscountRequirements.Remove(req);
            UpdateDiscount(discount);

            _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(discountRequirement);
        }

        /// <summary>
        /// Load discount requirement rule by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found discount requirement rule</returns>
        public virtual IDiscountRequirementRule LoadDiscountRequirementRuleBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IDiscountRequirementRule>(systemName);
            if (descriptor != null)
                return descriptor.Instance<IDiscountRequirementRule>();

            return null;
        }

        /// <summary>
        /// Load all discount requirement rules
        /// </summary>
        /// <returns>Discount requirement rules</returns>
        public virtual IList<IDiscountRequirementRule> LoadAllDiscountRequirementRules()
        {
            var rules = _pluginFinder.GetPlugins<IDiscountRequirementRule>();
            return rules.ToList();
        }

        /// <summary>
        /// Get discount by coupon code
        /// </summary>
        /// <param name="couponCode">Coupon code</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Discount</returns>
        public virtual Discount GetDiscountByCouponCode(string couponCode, bool showHidden = false)
        {
            if (String.IsNullOrWhiteSpace(couponCode))
                return null;

            var discount = GetAllDiscounts(null, couponCode, null, showHidden).FirstOrDefault();
            return discount;
        }

        /// <summary>
        /// Validate discount
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discount validation result</returns>
        public virtual DiscountValidationResult ValidateDiscount(Discount discount, Customer customer)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            string[] couponCodesToValidate = null;
            if (customer != null)
                couponCodesToValidate = customer.ParseAppliedDiscountCouponCodes();

            return ValidateDiscount(discount, customer, couponCodesToValidate);
        }

        /// <summary>
        /// Validate discount
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <param name="couponCodeToValidate">Coupon code to validate</param>
        /// <returns>Discount validation result</returns>
        public virtual DiscountValidationResult ValidateDiscount(Discount discount, Customer customer, string couponCodeToValidate)
        {
            if (!String.IsNullOrEmpty(couponCodeToValidate))
                return ValidateDiscount(discount, customer, new string[] { couponCodeToValidate });
            else
                return ValidateDiscount(discount, customer, new string[0]);

        }

        /// <summary>
        /// Validate discount
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <param name="couponCodesToValidate">Coupon codes to validate</param>
        /// <returns>Discount validation result</returns>
        public virtual DiscountValidationResult ValidateDiscount(Discount discount, Customer customer, string[] couponCodesToValidate)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            if (customer == null)
                throw new ArgumentNullException("customer");

            //invalid by default
            var result = new DiscountValidationResult();

            //check coupon code
            if (discount.RequiresCouponCode)
            {
                if (String.IsNullOrEmpty(discount.CouponCode))
                    return result;

                if (couponCodesToValidate == null)
                    return result;

                if (!couponCodesToValidate.Any(x => x.Equals(discount.CouponCode, StringComparison.InvariantCultureIgnoreCase)))
                    return result;
            }

            //Do not allow discounts applied to order subtotal or total when a customer has gift cards in the cart.
            //Otherwise, this customer can purchase gift cards with discount and get more than paid ("free money").
            if (discount.DiscountType == DiscountType.AssignedToOrderSubTotal ||
                discount.DiscountType == DiscountType.AssignedToOrderTotal)
            {
                var cart = customer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();

                var hasGiftCards = cart.Any(x => x.IsGiftCard);
                if (hasGiftCards)
                {
                    result.UserError = _localizationService.GetResource("ShoppingCart.Discount.CannotBeUsedWithGiftCards");
                    return result;
                }
            }

            //check date range
            DateTime now = DateTime.UtcNow;
            if (discount.StartDateUtc.HasValue)
            {
                DateTime startDate = DateTime.SpecifyKind(discount.StartDateUtc.Value, DateTimeKind.Utc);
                if (startDate.CompareTo(now) > 0)
                {
                    result.UserError = _localizationService.GetResource("ShoppingCart.Discount.NotStartedYet");
                    return result;
                }
            }
            if (discount.EndDateUtc.HasValue)
            {
                DateTime endDate = DateTime.SpecifyKind(discount.EndDateUtc.Value, DateTimeKind.Utc);
                if (endDate.CompareTo(now) < 0)
                {
                    result.UserError = _localizationService.GetResource("ShoppingCart.Discount.Expired");
                    return result;
                }
            }

            //discount limitation
            switch (discount.DiscountLimitation)
            {
                case DiscountLimitationType.NTimesOnly:
                    {
                        var usedTimes = GetAllDiscountUsageHistory(discount.Id, null, null, 0, 1).TotalCount;
                        if (usedTimes >= discount.LimitationTimes)
                            return result;
                    }
                    break;
                case DiscountLimitationType.NTimesPerCustomer:
                    {
                        if (customer.IsRegistered())
                        {
                            var usedTimes = GetAllDiscountUsageHistory(discount.Id, customer.Id, null, 0, 1).TotalCount;
                            if (usedTimes >= discount.LimitationTimes)
                            {
                                result.UserError = _localizationService.GetResource("ShoppingCart.Discount.CannotBeUsedAnymore");
                                return result;
                            }
                        }
                    }
                    break;
                case DiscountLimitationType.Unlimited:
                default:
                    break;
            }

            //discount requirements
            //UNDONE we should inject static cache manager into constructor. we we already have "per request" cache manager injected. better way to do it?
            //we cache meta info of discount requirements. this way we should not load them for each HTTP request
            var staticCacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
            string key = string.Format(DiscountRequirementEventConsumer.DISCOUNT_REQUIREMENT_MODEL_KEY, discount.Id);
            //var requirements = discount.DiscountRequirements;
            var requirements = staticCacheManager.Get(key, () =>
            {
                var cachedRequirements = new List<DiscountRequirementForCaching>();
                foreach (var dr in discount.DiscountRequirements)
                    cachedRequirements.Add(new DiscountRequirementForCaching
                    {
                        Id = dr.Id,
                        SystemName = dr.DiscountRequirementRuleSystemName
                    });
                return cachedRequirements;
            });
            foreach (var req in requirements)
            {
                //load a plugin
                var requirementRulePlugin = LoadDiscountRequirementRuleBySystemName(req.SystemName);
                if (requirementRulePlugin == null)
                    continue;

                //if (!_pluginFinder.AuthorizedForUser(requirementRulePlugin.PluginDescriptor, _workContext.CurrentCustomer))
                //    continue;

                if (!_pluginFinder.AuthenticateStore(requirementRulePlugin.PluginDescriptor, _storeContext.CurrentStore.Id))
                    continue;

                var ruleRequest = new DiscountRequirementValidationRequest
                {
                    DiscountRequirementId = req.Id,
                    Customer = customer,
                    Store = _storeContext.CurrentStore
                };
                var ruleResult = requirementRulePlugin.CheckRequirement(ruleRequest);
                if (!ruleResult.IsValid)
                {
                    result.UserError = ruleResult.UserError;
                    return result;
                }
            }

            result.IsValid = true;
            return result;
        }
        /// <summary>
        /// Gets a discount usage history record
        /// </summary>
        /// <param name="discountUsageHistoryId">Discount usage history record identifier</param>
        /// <returns>Discount usage history</returns>
        public virtual DiscountUsageHistory GetDiscountUsageHistoryById(string discountUsageHistoryId)
        {
            return _discountUsageHistoryRepository.GetById(discountUsageHistoryId);
        }

        /// <summary>
        /// Gets all discount usage history records
        /// </summary>
        /// <param name="discountId">Discount identifier; null to load all records</param>
        /// <param name="customerId">Customer identifier; null to load all records</param>
        /// <param name="orderId">Order identifier; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Discount usage history records</returns>
        public virtual IPagedList<DiscountUsageHistory> GetAllDiscountUsageHistory(string discountId = "",
            string customerId = "", string orderId = "", 
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _discountUsageHistoryRepository.Table;

            if (!String.IsNullOrEmpty(discountId))
                query = query.Where(duh => duh.DiscountId == discountId);
            if (!String.IsNullOrEmpty(customerId))
                query = query.Where(duh => duh.CustomerId == customerId);
            if (!String.IsNullOrEmpty(orderId))
                query = query.Where(duh => duh.OrderId == orderId);
            query = query.OrderByDescending(c => c.CreatedOnUtc);
            return new PagedList<DiscountUsageHistory>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Insert discount usage history record
        /// </summary>
        /// <param name="discountUsageHistory">Discount usage history record</param>
        public virtual void InsertDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            if (discountUsageHistory == null)
                throw new ArgumentNullException("discountUsageHistory");

            _discountUsageHistoryRepository.Insert(discountUsageHistory);

            _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(discountUsageHistory);
        }


        /// <summary>
        /// Update discount usage history record
        /// </summary>
        /// <param name="discountUsageHistory">Discount usage history record</param>
        public virtual void UpdateDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            if (discountUsageHistory == null)
                throw new ArgumentNullException("discountUsageHistory");

            _discountUsageHistoryRepository.Update(discountUsageHistory);

            _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(discountUsageHistory);
        }

        /// <summary>
        /// Delete discount usage history record
        /// </summary>
        /// <param name="discountUsageHistory">Discount usage history record</param>
        public virtual void DeleteDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            if (discountUsageHistory == null)
                throw new ArgumentNullException("discountUsageHistory");

            _discountUsageHistoryRepository.Delete(discountUsageHistory);

            _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(discountUsageHistory);
        }

        #endregion
    }
}
