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
        private const string DISCOUNTS_BY_ID_KEY = "Grand.discount.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : coupon code
        /// {2} : discount name
        /// </remarks>
        private const string DISCOUNTS_ALL_KEY = "Grand.discount.all-{0}-{1}-{2}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string DISCOUNTS_PATTERN_KEY = "Grand.discount.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTS_PATTERN_KEY = "Grand.product.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string MANUFACTURERS_PATTERN_KEY = "Grand.manufacturer.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CATEGORIES_PATTERN_KEY = "Grand.category.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string VENDORS_PATTERN_KEY = "Grand.vendor.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string STORES_PATTERN_KEY = "Grand.store.";

        #endregion

        #region Fields

        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<DiscountCoupon> _discountCouponRepository;
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
        private readonly PerRequestCacheManager _perRequestCache;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public DiscountService(ICacheManager cacheManager,
            IRepository<Discount> discountRepository,
            IRepository<DiscountCoupon> discountCouponRepository,
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
            IRepository<Store> storeRepository,
            PerRequestCacheManager perRequestCache
            )
        {
            this._cacheManager = cacheManager;
            this._discountRepository = discountRepository;
            this._discountCouponRepository = discountCouponRepository;
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
            this._perRequestCache = perRequestCache;
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

            var usagehistory = GetAllDiscountUsageHistory(discount.Id);
            if (usagehistory.Count > 0)
                throw new ArgumentNullException("discount has a history");

            var builder = Builders<BsonDocument>.Filter;
            if (discount.DiscountType == DiscountType.AssignedToSkus)
            {
                var builderproduct = Builders<Product>.Update;
                var updatefilter = builderproduct.Pull(x => x.AppliedDiscounts, discount.Id);
                var result = _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;
                _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            }

            if (discount.DiscountType == DiscountType.AssignedToCategories)
            {
                var buildercategory = Builders<Category>.Update;
                var updatefilter = buildercategory.Pull(x => x.AppliedDiscounts, discount.Id);
                var result = _categoryRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;
                _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            }

            if (discount.DiscountType == DiscountType.AssignedToManufacturers)
            {
                var buildermanufacturer = Builders<Manufacturer>.Update;
                var updatefilter = buildermanufacturer.Pull(x => x.AppliedDiscounts, discount.Id);
                var result = _manufacturerRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;
                _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            }
            if (discount.DiscountType == DiscountType.AssignedToVendors)
            {
                var buildervendor = Builders<Vendor>.Update;
                var updatefilter = buildervendor.Pull(x => x.AppliedDiscounts, discount.Id);
                var result = _vendorRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;
                _cacheManager.RemoveByPattern(VENDORS_PATTERN_KEY);
            }

            if (discount.DiscountType == DiscountType.AssignedToStores)
            {
                var builderstore = Builders<Store>.Update;
                var updatefilter = builderstore.Pull(x => x.AppliedDiscounts, discount.Id);
                var result = _storeRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;
                _cacheManager.RemoveByPattern(STORES_PATTERN_KEY);
            }


            //remove coupon codes
            var filtersCoupon = Builders<DiscountCoupon>.Filter;
            var filterCrp = filtersCoupon.Eq(x => x.DiscountId, discount.Id);
            _discountCouponRepository.Collection.DeleteMany(filterCrp);

            _discountRepository.Delete(discount);



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
                        && d.IsEnabled);
                }
                if (!String.IsNullOrEmpty(couponCode))
                {
                    var _coupon = _discountCouponRepository.Table.FirstOrDefault(x => x.CouponCode == couponCode);
                    if(_coupon!=null)
                        query = query.Where(d => d.Id == _coupon.DiscountId);
                }
                if (!String.IsNullOrEmpty(discountName))
                {
                    query = query.Where(d => d.Name != null && d.Name.ToLower().Contains(discountName.ToLower()));
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

            foreach (var req in discount.DiscountRequirements)
            {
                req.DiscountId = discount.Id;
            }

            _discountRepository.Update(discount);

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
            if (discount == null)
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
        /// Load discount by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found discount</returns>
        public virtual IDiscount LoadDiscountPluginBySystemName(string systemName)
        {
            var discountPlugins = LoadAllDiscountPlugins();
            foreach (var discountPlugin in discountPlugins)
            {
                var rules = discountPlugin.GetRequirementRules();

                if (!rules.Any(x => x.SystemName == systemName))
                    continue;
                return discountPlugin;
            }
            return null;
        }

        /// <summary>
        /// Load all discount requirement rules
        /// </summary>
        /// <returns>Discount requirement rules</returns>
        public virtual IList<IDiscount> LoadAllDiscountPlugins()
        {
            var discountPlugins = _pluginFinder.GetPlugins<IDiscount>();
            return discountPlugins.ToList();
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

            var builder = Builders<DiscountCoupon>.Filter;
            var filter = builder.Eq(x => x.CouponCode, couponCode);
            var query = _discountCouponRepository.Collection.Find(filter);

            var coupon = query.FirstOrDefault();
            if(coupon == null)
                return null;

            var discount = GetDiscountById(coupon.DiscountId);
            return discount;
        }

        /// <summary>
        /// Exist coupon code in discount
        /// </summary>
        /// <param name="couponCode"></param>
        /// <param name="discountId"></param>
        /// <returns></returns>
        public virtual bool ExistsCodeInDiscount(string couponCode, string discountId, bool? used)
        {
            if (String.IsNullOrWhiteSpace(couponCode))
                return false;

            var builder = Builders<DiscountCoupon>.Filter;
            var filter = builder.Eq(x => x.CouponCode, couponCode);
            filter = filter & builder.Eq(x => x.DiscountId, discountId);
            if(used.HasValue)
                filter = filter & builder.Eq(x => x.Used, used.Value);
            var query = _discountCouponRepository.Collection.Find(filter);
            if (query.Any())
                return true;
            else
                return false;
        }

        /// <summary>
        /// Get all coupon codes for discount
        /// </summary>
        /// <param name="discountId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<DiscountCoupon> GetAllCouponCodesByDiscountId(string discountId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _discountCouponRepository.Table;

            if (!String.IsNullOrEmpty(discountId))
                query = query.Where(duh => duh.DiscountId == discountId);
            query = query.OrderByDescending(c => c.CouponCode);
            return new PagedList<DiscountCoupon>(query, pageIndex, pageSize);
        }


        /// <summary>
        /// Gets a discount
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <returns>Discount</returns>
        public virtual DiscountCoupon GetDiscountCodeById(string id)
        {
            return _discountCouponRepository.GetById(id);
        }

        /// <summary>
        /// Get discount code by discount code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public DiscountCoupon GetDiscountCodeByCode(string couponCode)
        {
            var builder = Builders<DiscountCoupon>.Filter;
            var filter = builder.Eq(x => x.CouponCode, couponCode);
            var query = _discountCouponRepository.Collection.Find(filter);
            return query.FirstOrDefault();
        }


        /// <summary>
        /// Delete discount code
        /// </summary>
        /// <param name="coupon"></param>
        public virtual void DeleteDiscountCoupon(DiscountCoupon coupon)
        {
            _discountCouponRepository.Delete(coupon);
        }

        /// <summary>
        /// Insert discount code
        /// </summary>
        /// <param name="coupon"></param>
        public virtual void InsertDiscountCoupon(DiscountCoupon coupon)
        {
            _discountCouponRepository.Insert(coupon);
        }

        /// <summary>
        /// Update discount code - set as used or not
        /// </summary>
        /// <param name="coupon"></param>
        public virtual void DiscountCouponSetAsUsed(string couponCode, bool used)
        {
            if (string.IsNullOrEmpty(couponCode))
                return;

            var coupon = GetDiscountCodeByCode(couponCode);
            if (coupon != null)
            {
                if (used)
                {
                    coupon.Used = used;
                    coupon.Qty = coupon.Qty + 1;
                }
                else
                {
                    coupon.Qty = coupon.Qty - 1;
                    coupon.Used = coupon.Qty > 0 ? true : false;
                }
                _discountCouponRepository.Update(coupon);
            }
        }

        /// <summary>
        /// Cancel discount if order was canceled or deleted
        /// </summary>
        /// <param name="orderId"></param>
        public virtual void CancelDiscount(string orderId)
        {
            var discountUsage = _discountUsageHistoryRepository.Table.Where(x => x.OrderId == orderId);
            foreach (var item in discountUsage)
            {
                DiscountCouponSetAsUsed(item.CouponCode, false);
                item.Canceled = true;
                UpdateDiscountUsageHistory(item);
            }
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
            {
                return ValidateDiscount(discount, customer, new string[] { couponCodeToValidate });
            }
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

            string key = $"DiscountValidationResult_{customer.Id}_{discount.Id}_{string.Join("_", couponCodesToValidate)}";
            var validationResult = _perRequestCache.Get(key, () =>
            {
                var result = new DiscountValidationResult();

                //check is enabled
                if (!discount.IsEnabled)
                    return result;

                //check coupon code
                if (discount.RequiresCouponCode)
                {
                    if (couponCodesToValidate == null || !couponCodesToValidate.Any())
                        return result;
                    var exists = false;
                    foreach (var item in couponCodesToValidate)
                    {
                        if (discount.Reused)
                        {
                            if (ExistsCodeInDiscount(item, discount.Id, null))
                            {
                                result.CouponCode = item;
                                exists = true;
                            }
                        }
                        else
                        {
                            if (ExistsCodeInDiscount(item, discount.Id, false))
                            {
                                result.CouponCode = item;
                                exists = true;
                            }
                        }
                    }
                    if (!exists)
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
                            var usedTimes = GetAllDiscountUsageHistory(discount.Id, null, null, false, 0, 1).TotalCount;
                            if (usedTimes >= discount.LimitationTimes)
                                return result;
                        }
                        break;
                    case DiscountLimitationType.NTimesPerCustomer:
                        {
                            if (customer.IsRegistered())
                            {
                                var usedTimes = GetAllDiscountUsageHistory(discount.Id, customer.Id, null, false, 0, 1).TotalCount;
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
                string keyReq = string.Format(DiscountRequirementEventConsumer.DISCOUNT_REQUIREMENT_MODEL_KEY, discount.Id);
                var requirements = _cacheManager.Get(keyReq, () =>
                {
                    return discount.DiscountRequirements.ToList();
                });
                foreach (var req in requirements)
                {
                    //load a plugin
                    var discountRequirementPlugin = LoadDiscountPluginBySystemName(req.DiscountRequirementRuleSystemName);

                    if (discountRequirementPlugin == null)
                        continue;

                    if (!_pluginFinder.AuthenticateStore(discountRequirementPlugin.PluginDescriptor, _storeContext.CurrentStore.Id))
                        continue;

                    var ruleRequest = new DiscountRequirementValidationRequest
                    {
                        DiscountRequirementId = req.Id,
                        DiscountId = req.DiscountId,
                        Customer = customer,
                        Store = _storeContext.CurrentStore
                    };

                    var singleRequirementRule = discountRequirementPlugin.GetRequirementRules().Single(x => x.SystemName == req.DiscountRequirementRuleSystemName);
                    var ruleResult = singleRequirementRule.CheckRequirement(ruleRequest);
                    if (!ruleResult.IsValid)
                    {
                        result.UserError = ruleResult.UserError;
                        return result;
                    }
                }

                result.IsValid = true;
                return result;
            });
            
            return validationResult;
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
            string customerId = "", string orderId = "", bool? canceled = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _discountUsageHistoryRepository.Table;

            if (!String.IsNullOrEmpty(discountId))
                query = query.Where(duh => duh.DiscountId == discountId);
            if (!String.IsNullOrEmpty(customerId))
                query = query.Where(duh => duh.CustomerId == customerId);
            if (!String.IsNullOrEmpty(orderId))
                query = query.Where(duh => duh.OrderId == orderId);
            if(canceled.HasValue)
                query = query.Where(duh => duh.Canceled == canceled.Value);
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

            //Support for couponcode
            DiscountCouponSetAsUsed(discountUsageHistory.CouponCode, true);

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

        /// <summary>
        /// Get discount amount
        /// </summary>
        /// <param name="discount"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public decimal GetDiscountAmount(Discount discount, decimal amount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            //calculate discount amount
            decimal result = decimal.Zero;
            if (!discount.CalculateByPlugin)
            {
                if (discount.UsePercentage)
                    result = (decimal)((((float)amount) * ((float)discount.DiscountPercentage)) / 100f);
                else
                    result = discount.DiscountAmount;
            }
            else
            {
                result = GetDiscountAmountProvider(discount, amount);
            }

            //validate maximum disocunt amount
            if (discount.UsePercentage &&
                discount.MaximumDiscountAmount.HasValue &&
                result > discount.MaximumDiscountAmount.Value)
                result = discount.MaximumDiscountAmount.Value;

            if (result < decimal.Zero)
                result = decimal.Zero;

            return result;
        }

        /// <summary>
        /// Get preferred discount (with maximum discount value)
        /// </summary>
        /// <param name="discounts">A list of discounts to check</param>
        /// <param name="amount">Amount</param>
        /// <returns>Preferred discount</returns>
        public List<AppliedDiscount> GetPreferredDiscount(IList<AppliedDiscount> discounts,
            decimal amount, out decimal discountAmount)
        {
            if (discounts == null)
                throw new ArgumentNullException("discounts");

            var result = new List<AppliedDiscount>();
            discountAmount = decimal.Zero;
            if (!discounts.Any())
                return result;

            //first we check simple discounts
            foreach (var applieddiscount in discounts)
            {
                var discount = GetDiscountById(applieddiscount.DiscountId);
                decimal currentDiscountValue = GetDiscountAmount(discount, amount);
                if (currentDiscountValue > discountAmount)
                {
                    discountAmount = currentDiscountValue;
                    result.Clear();
                    result.Add(applieddiscount);
                }
            }
            //now let's check cumulative discounts
            //right now we calculate discount values based on the original amount value
            //please keep it in mind if you're going to use discounts with "percentage"
            var cumulativeDiscounts = discounts.Where(x => x.IsCumulative).ToList();
            if (cumulativeDiscounts.Count > 1)
            {
                var cumulativeDiscountAmount = cumulativeDiscounts.Sum(d => GetDiscountAmount(GetDiscountById(d.DiscountId), amount));
                if (cumulativeDiscountAmount > discountAmount)
                {
                    discountAmount = cumulativeDiscountAmount;

                    result.Clear();
                    result.AddRange(cumulativeDiscounts);
                }
            }

            return result;
        }


        /// <summary>
        /// Get amount from discount amount provider 
        /// </summary>
        /// <param name="discount"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public virtual decimal GetDiscountAmountProvider(Discount discount, decimal amount)
        {
            var discountAmountProvider = LoadDiscountAmountProviderBySystemName(discount.DiscountPluginName);
            if (discountAmountProvider == null)
                return 0;
            return discountAmountProvider.DiscountAmount(discount, amount);
        }


        public virtual IDiscountAmountProvider LoadDiscountAmountProviderBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IDiscountAmountProvider>(systemName);
            if (descriptor != null)
                return descriptor.Instance<IDiscountAmountProvider>();

            return null;
        }

        /// <summary>
        /// Get all discount amount providers
        /// </summary>
        /// <returns></returns>
        public IList<IDiscountAmountProvider> LoadDiscountAmountProviders()
        {
            var discountAmountProviders = _pluginFinder.GetPlugins<IDiscountAmountProvider>();
            return discountAmountProviders
                .OrderBy(tp => tp.PluginDescriptor)
                .ToList();
        }

        #endregion
    }
}
