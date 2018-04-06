using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Discounts;
using Grand.Core.Domain.Orders;
using Grand.Services.Catalog.Cache;
using Grand.Services.Customers;
using Grand.Services.Discounts;
using Grand.Services.Vendors;
using Grand.Services.Stores;
using Grand.Core.Domain.Directory;
using Grand.Services.Directory;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Price calculation service
    /// </summary>
    public partial class PriceCalculationService : IPriceCalculationService
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IDiscountService _discountService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly ICacheManager _cacheManager;
        private readonly IVendorService _vendorService;
        private readonly IStoreService _storeService;
        private readonly ICurrencyService _currencyService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        #endregion

        #region Ctor

        public PriceCalculationService(IWorkContext workContext,
            IStoreContext storeContext,
            IDiscountService discountService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductAttributeParser productAttributeParser,
            IProductService productService,
            ICustomerService customerService,
            ICacheManager cacheManager,
            IVendorService vendorService,
            IStoreService storeService,
            ICurrencyService currencyService,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._discountService = discountService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productAttributeParser = productAttributeParser;
            this._productService = productService;
            this._customerService = customerService;
            this._cacheManager = cacheManager;
            this._vendorService = vendorService;
            this._storeService = storeService;
            this._currencyService = currencyService;
            this._shoppingCartSettings = shoppingCartSettings;
            this._catalogSettings = catalogSettings;
            this._currencySettings = currencySettings;
        }

        #endregion

        #region Nested classes

        [Serializable]
        protected class ProductPriceForCaching
        {
            public ProductPriceForCaching()
            {
                AppliedDiscountIds = new List<string>();
            }
            public decimal Price { get; set; }
            public decimal AppliedDiscountAmount { get; set; }
            public List<string> AppliedDiscountIds { get; set; }
        }
        #endregion

        #region Utilities

        /// <summary>
        /// Gets allowed discounts
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discounts</returns>
        protected virtual IList<AppliedDiscount> GetAllowedDiscountsAppliedToProduct(Product product, Customer customer)
        {
            var allowedDiscounts = new List<AppliedDiscount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            if (product.AppliedDiscounts.Any())
            {
                foreach (var appliedDiscount in product.AppliedDiscounts)
                {
                    var discount = _discountService.GetDiscountById(appliedDiscount);
                    if (discount != null)
                    {
                        var validDiscount = _discountService.ValidateDiscount(discount, customer);
                        if (validDiscount.IsValid &&
                            discount.DiscountType == DiscountType.AssignedToSkus)
                            allowedDiscounts.Add(new AppliedDiscount() {
                                CouponCode = validDiscount.CouponCode,
                                DiscountId = discount.Id,
                                IsCumulative = discount.IsCumulative
                            });
                    }
                }
            }
            return allowedDiscounts;
        }

        protected virtual IList<AppliedDiscount> GetAllowedDiscountsAppliedToAllProduct(Product product, Customer customer)
        {
            var allowedDiscounts = new List<AppliedDiscount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            var discounts = _discountService.GetAllDiscounts(DiscountType.AssignedToAllProducts);
            foreach (var discount in discounts)
            {
                var validDiscount = _discountService.ValidateDiscount(discount, customer);
                if (validDiscount.IsValid)
                    allowedDiscounts.Add(new AppliedDiscount() {
                        CouponCode = validDiscount.CouponCode,
                        DiscountId = discount.Id,
                        IsCumulative = discount.IsCumulative
                    });
            }
            return allowedDiscounts;
        }


        /// <summary>
        /// Gets allowed discounts applied to categories
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discounts</returns>
        protected virtual IList<AppliedDiscount> GetAllowedDiscountsAppliedToCategories(Product product, Customer customer)
        {
            var allowedDiscounts = new List<AppliedDiscount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            foreach (var productCategory in product.ProductCategories)
            {
                var category = _categoryService.GetCategoryById(productCategory.CategoryId);
                if (category!=null && category.AppliedDiscounts.Any())
                {
                    foreach (var appliedDiscount in category.AppliedDiscounts)
                    {
                        var discount = _discountService.GetDiscountById(appliedDiscount);
                        if (discount != null)
                        {
                            var validDiscount = _discountService.ValidateDiscount(discount, customer);
                            if (validDiscount.IsValid && discount.DiscountType == DiscountType.AssignedToCategories)
                                allowedDiscounts.Add(new AppliedDiscount() {
                                    CouponCode = validDiscount.CouponCode,
                                    DiscountId = discount.Id,
                                    IsCumulative = discount.IsCumulative
                                });
                        }
                    }
                }
            }
            return allowedDiscounts;
        }

        /// <summary>
        /// Gets allowed discounts applied to manufacturers
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discounts</returns>
        protected virtual IList<AppliedDiscount> GetAllowedDiscountsAppliedToManufacturers(Product product, Customer customer)
        {
            var allowedDiscounts = new List<AppliedDiscount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            foreach (var productManufacturer in product.ProductManufacturers)
            {
                var manufacturer = _manufacturerService.GetManufacturerById(productManufacturer.ManufacturerId);
                if (manufacturer !=null && manufacturer.AppliedDiscounts.Any())
                {
                    foreach (var appliedDiscount in manufacturer.AppliedDiscounts)
                    {
                        var discount = _discountService.GetDiscountById(appliedDiscount);
                        if (discount != null)
                        {
                            var validDiscount = _discountService.ValidateDiscount(discount, customer);
                            if (validDiscount.IsValid &&
                                     discount.DiscountType == DiscountType.AssignedToManufacturers)
                                allowedDiscounts.Add(new AppliedDiscount()
                                {
                                    CouponCode = validDiscount.CouponCode,
                                    DiscountId = discount.Id,
                                    IsCumulative = discount.IsCumulative
                                });
                        }
                    }
                }
            }
            return allowedDiscounts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        protected virtual IList<AppliedDiscount> GetAllowedDiscountsAppliedToVendors(Product product, Customer customer)
        {
            var allowedDiscounts = new List<AppliedDiscount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            if (!string.IsNullOrEmpty(product.VendorId))
            {
                var vendor = _vendorService.GetVendorById(product.VendorId);
                if (vendor != null)
                {
                    if (vendor.AppliedDiscounts.Any())
                    {
                        foreach (var appliedDiscount in vendor.AppliedDiscounts)
                        {
                            var discount = _discountService.GetDiscountById(appliedDiscount);
                            if (discount != null)
                            {
                                var validDiscount = _discountService.ValidateDiscount(discount, customer);
                                if (validDiscount.IsValid &&
                                         discount.DiscountType == DiscountType.AssignedToVendors)
                                    allowedDiscounts.Add(new AppliedDiscount()
                                    {
                                        CouponCode = validDiscount.CouponCode,
                                        DiscountId = discount.Id,
                                        IsCumulative = discount.IsCumulative,
                                    });
                            }
                        }
                    }
                }
            }
            return allowedDiscounts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        protected virtual IList<AppliedDiscount> GetAllowedDiscountsAppliedToStores(Product product, Customer customer)
        {
            var allowedDiscounts = new List<AppliedDiscount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            if (product.LimitedToStores == false)
            {
                //Products that don't have any Store (inside Stores collection) shouldn't have any Discount by Store
            }
            else
            {
                //if it is limited to store, it means it should have at least one Store assigned
                foreach (var storeID in product.Stores)
                {
                    if (!(string.IsNullOrEmpty(storeID)))
                    {
                        var store = _storeService.GetStoreById(storeID);
                        if (store!=null && store.AppliedDiscounts.Any())
                        {
                            foreach (var appliedDiscount in store.AppliedDiscounts)
                            {
                                var discount = _discountService.GetDiscountById(appliedDiscount);
                                if (discount != null)
                                {
                                    var validDiscount = _discountService.ValidateDiscount(discount, customer);
                                    if (validDiscount.IsValid &&
                                             discount.DiscountType == DiscountType.AssignedToStores)
                                        allowedDiscounts.Add(new AppliedDiscount() {
                                            CouponCode = validDiscount.CouponCode,
                                            DiscountId = discount.Id,
                                            IsCumulative = discount.IsCumulative
                                        });
                                }
                            }
                        }
                    }
                }
            }
            return allowedDiscounts;
        }

        /// <summary>
        /// Gets allowed discounts
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discounts</returns>
        protected virtual IList<AppliedDiscount> GetAllowedDiscounts(Product product, Customer customer)
        {
            var allowedDiscounts = new List<AppliedDiscount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            //discounts applied to products
            foreach (var discount in GetAllowedDiscountsAppliedToProduct(product, customer))
                if(!allowedDiscounts.Where(x=>x.DiscountId == discount.DiscountId).Any())
                    allowedDiscounts.Add(discount);

            //discounts applied to all products
            foreach (var discount in GetAllowedDiscountsAppliedToAllProduct(product, customer))
                if (!allowedDiscounts.Where(x => x.DiscountId == discount.DiscountId).Any())
                    allowedDiscounts.Add(discount);

            //discounts applied to categories
            foreach (var discount in GetAllowedDiscountsAppliedToCategories(product, customer))
                if (!allowedDiscounts.Where(x => x.DiscountId == discount.DiscountId).Any())
                    allowedDiscounts.Add(discount);

            //discounts applied to manufacturers
            foreach (var discount in GetAllowedDiscountsAppliedToManufacturers(product, customer))
                if (!allowedDiscounts.Where(x => x.DiscountId == discount.DiscountId).Any())
                    allowedDiscounts.Add(discount);

            //discounts applied to vendors
            foreach (var discount in GetAllowedDiscountsAppliedToVendors(product, customer))
                if (!allowedDiscounts.Where(x => x.DiscountId == discount.DiscountId).Any())
                    allowedDiscounts.Add(discount);

            //discounts applied to stores
            foreach (var discount in GetAllowedDiscountsAppliedToStores(product, customer))
                if (!allowedDiscounts.Where(x => x.DiscountId == discount.DiscountId).Any())
                    allowedDiscounts.Add(discount);

            return allowedDiscounts;
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="productPriceWithoutDiscount">Already calculated product price without discount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Discount amount</returns>
        protected virtual decimal GetDiscountAmount(Product product,
            Customer customer,
            decimal productPriceWithoutDiscount,
            out List<AppliedDiscount> appliedDiscounts)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            appliedDiscounts = null;
            decimal appliedDiscountAmount = decimal.Zero;

            //we don't apply discounts to products with price entered by a customer
            if (product.CustomerEntersPrice)
                return appliedDiscountAmount;

            //discounts are disabled
            if (_catalogSettings.IgnoreDiscounts)
                return appliedDiscountAmount;

            var allowedDiscounts = GetAllowedDiscounts(product, customer);

            //no discounts
            if (!allowedDiscounts.Any())
                return appliedDiscountAmount;

            appliedDiscounts = _discountService.GetPreferredDiscount(allowedDiscounts, productPriceWithoutDiscount, out appliedDiscountAmount);
            return appliedDiscountAmount;
        }


        #endregion

        #region Methods

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge = decimal.Zero,
            bool includeDiscounts = true,
            int quantity = 1)
        {
            decimal discountAmount;
            List<AppliedDiscount> appliedDiscounts;
            return GetFinalPrice(product, customer, additionalCharge, includeDiscounts,
                quantity, out discountAmount, out appliedDiscounts);
        }
        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            out decimal discountAmount,
            out List<AppliedDiscount> appliedDiscounts)
        {
            return GetFinalPrice(product, customer,
                additionalCharge, includeDiscounts, quantity,
                null, null,
                out discountAmount, out appliedDiscounts);
        }
        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <param name="rentalStartDate">Rental period start date (for rental products)</param>
        /// <param name="rentalEndDate">Rental period end date (for rental products)</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            DateTime? rentalStartDate,
            DateTime? rentalEndDate,
            out decimal discountAmount,
            out List<AppliedDiscount> appliedDiscounts)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            discountAmount = decimal.Zero;
            appliedDiscounts = new List<AppliedDiscount>();

            var cacheKey = string.Format(PriceCacheEventConsumer.PRODUCT_PRICE_MODEL_KEY,
                product.Id,
                additionalCharge.ToString(CultureInfo.InvariantCulture),
                includeDiscounts,
                quantity,
                string.Join(",", customer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var cacheTime = _catalogSettings.CacheProductPrices ? 60 : 0;
            //we do not cache price for reservation products
            //otherwise, it can cause memory leaks (to store all possible date period combinations)
            if (product.ProductType == ProductType.Reservation)
                cacheTime = 0;

            ProductPriceForCaching PrepareModel() {
                var result = new ProductPriceForCaching();

                //initial price
                decimal price = product.Price;

                //customer product price
                var customerPrice = _customerService.GetPriceByCustomerProduct(customer.Id, product.Id);
                if (customerPrice.HasValue && customerPrice.Value < price)
                    price = customerPrice.Value;

                //tier prices
                var tierPrice = product.GetPreferredTierPrice(customer, _storeContext.CurrentStore.Id, quantity);
                if (tierPrice != null)
                    price = tierPrice.Price;

                //additional charge
                price = price + additionalCharge;

                //reservations
                if (product.ProductType == ProductType.Reservation)
                    if (rentalStartDate.HasValue && rentalEndDate.HasValue)
                    {
                        decimal d = 0;
                        if (product.IncBothDate)
                        {
                            decimal.TryParse(((rentalEndDate - rentalStartDate).Value.TotalDays + 1).ToString(), out d);
                        }
                        else
                        {
                            decimal.TryParse((rentalEndDate - rentalStartDate).Value.TotalDays.ToString(), out d);
                        }
                        price = price * d;
                    }

                if (includeDiscounts)
                {
                    //discount
                    List<AppliedDiscount> tmpAppliedDiscounts;
                    decimal tmpDiscountAmount = GetDiscountAmount(product, customer, price, out tmpAppliedDiscounts);
                    price = price - tmpDiscountAmount;

                    if (tmpAppliedDiscounts != null)
                    {
                        result.AppliedDiscountIds = tmpAppliedDiscounts.Select(x => x.DiscountId).ToList();
                        result.AppliedDiscountAmount = tmpDiscountAmount;
                    }
                }

                if (price < decimal.Zero)
                    price = decimal.Zero;

                //rounding
                if (_shoppingCartSettings.RoundPricesDuringCalculation)
                {
                    var primaryCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryExchangeRateCurrencyId);
                    result.Price = RoundingHelper.RoundPrice(price, primaryCurrency);
                }
                else
                    result.Price = price;

                return result;
            }

            var cachedPrice = cacheTime > 0 ? _cacheManager.Get(cacheKey, cacheTime, () => { return PrepareModel(); }) : PrepareModel();

            if (includeDiscounts)
            {
                foreach (var appliedDiscountId in cachedPrice.AppliedDiscountIds)
                {
                    var appliedDiscount = _discountService.GetDiscountById(appliedDiscountId);
                    if (appliedDiscount != null)
                        appliedDiscounts.Add(new AppliedDiscount() { DiscountId = appliedDiscount.Id, IsCumulative = appliedDiscount.IsCumulative });
                }
                if (appliedDiscounts.Any())
                {
                    discountAmount = cachedPrice.AppliedDiscountAmount;
                }
            }

            return cachedPrice.Price;
        }



        /// <summary>
        /// Gets the shopping cart unit price (one item)
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>Shopping cart unit price (one item)</returns>
        public virtual decimal GetUnitPrice(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts = true)
        {
            decimal discountAmount;
            List<AppliedDiscount> appliedDiscounts;
            return GetUnitPrice(shoppingCartItem, includeDiscounts,
                out discountAmount, out appliedDiscounts);
        }
        /// <summary>
        /// Gets the shopping cart unit price (one item)
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Shopping cart unit price (one item)</returns>
        public virtual decimal GetUnitPrice(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts,
            out decimal discountAmount,
            out List<AppliedDiscount> appliedDiscounts)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");
            var product = _productService.GetProductById(shoppingCartItem.ProductId);
            return GetUnitPrice(product,
                _workContext.CurrentCustomer,
                shoppingCartItem.ShoppingCartType,
                shoppingCartItem.Quantity,
                shoppingCartItem.AttributesXml,
                shoppingCartItem.CustomerEnteredPrice,
                shoppingCartItem.RentalStartDateUtc,
                shoppingCartItem.RentalEndDateUtc,
                includeDiscounts,
                out discountAmount,
                out appliedDiscounts);
        }
        /// <summary>
        /// Gets the shopping cart unit price (one item)
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="attributesXml">Product atrributes (XML format)</param>
        /// <param name="customerEnteredPrice">Customer entered price (if specified)</param>
        /// <param name="rentalStartDate">Rental start date (null for not rental products)</param>
        /// <param name="rentalEndDate">Rental end date (null for not rental products)</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Shopping cart unit price (one item)</returns>
        public virtual decimal GetUnitPrice(Product product,
            Customer customer,
            ShoppingCartType shoppingCartType,
            int quantity,
            string attributesXml,
            decimal customerEnteredPrice,
            DateTime? rentalStartDate, DateTime? rentalEndDate,
            bool includeDiscounts,
            out decimal discountAmount,
            out List<AppliedDiscount> appliedDiscounts)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (customer == null)
                throw new ArgumentNullException("customer");

            discountAmount = decimal.Zero;
            appliedDiscounts = new List<AppliedDiscount>();

            decimal? finalPrice = null;

            if (shoppingCartType == ShoppingCartType.Auctions && product.ProductType == ProductType.Auction)
                finalPrice = customerEnteredPrice;

            if (!finalPrice.HasValue)
            {
                var combination = _productAttributeParser.FindProductAttributeCombination(product, attributesXml);
                if (combination != null)
                {
                    if (combination.OverriddenPrice.HasValue)
                        finalPrice = combination.OverriddenPrice.Value;
                    if (combination.TierPrices.Any())
                    {
                        var storeId = _storeContext.CurrentStore.Id;
                        var actualTierPrices = combination.TierPrices.Where(x => string.IsNullOrEmpty(x.StoreId) || x.StoreId == storeId)
                            .Where(x => string.IsNullOrEmpty(x.CustomerRoleId) ||
                            customer.CustomerRoles.Where(role => role.Active).Select(role => role.Id).Contains(x.CustomerRoleId)).ToList();
                        var tierPrice = actualTierPrices.LastOrDefault(price => quantity >= price.Quantity);
                        if (tierPrice != null)
                            finalPrice = tierPrice.Price;
                    }
                }
            }
            if(!finalPrice.HasValue)
            {
                //summarize price of all attributes
                decimal attributesTotalPrice = decimal.Zero;
                var attributeValues = _productAttributeParser.ParseProductAttributeValues(product, attributesXml);
                if (attributeValues != null)
                {
                    foreach (var attributeValue in attributeValues)
                    {
                        attributesTotalPrice += GetProductAttributeValuePriceAdjustment(attributeValue);
                    }
                }

                //get price of a product (with previously calculated price of all attributes)
                if (product.CustomerEntersPrice)
                {
                    finalPrice = customerEnteredPrice;
                }
                else
                {
                    int qty;
                    if (_shoppingCartSettings.GroupTierPricesForDistinctShoppingCartItems)
                    {
                        //the same products with distinct product attributes could be stored as distinct "ShoppingCartItem" records
                        //so let's find how many of the current products are in the cart
                        qty = customer.ShoppingCartItems
                            .Where(x => x.ProductId == product.Id)
                            .Where(x => x.ShoppingCartType == shoppingCartType)
                            .Sum(x => x.Quantity);
                        if (qty == 0)
                        {
                            qty = quantity;
                        }
                    }
                    else
                    {
                        qty = quantity;
                    }
                    finalPrice = GetFinalPrice(product,
                        customer,
                        attributesTotalPrice,
                        includeDiscounts,
                        qty,
                        product.ProductType == ProductType.Reservation ? rentalStartDate : null,
                        product.ProductType == ProductType.Reservation ? rentalEndDate : null,
                        out discountAmount, out appliedDiscounts);
                }
            }

            if (!finalPrice.HasValue)
                finalPrice = 0;

            //rounding
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                var primaryCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryExchangeRateCurrencyId);
                finalPrice = RoundingHelper.RoundPrice(finalPrice.Value, primaryCurrency);
            }
            return finalPrice.Value;
        }
        /// <summary>
        /// Gets the shopping cart item sub total
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>Shopping cart item sub total</returns>
        public virtual decimal GetSubTotal(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts = true)
        {
            decimal discountAmount;
            List<AppliedDiscount> appliedDiscounts;
            return GetSubTotal(shoppingCartItem, includeDiscounts, out discountAmount, out appliedDiscounts);
        }
        /// <summary>
        /// Gets the shopping cart item sub total
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Shopping cart item sub total</returns>
        public virtual decimal GetSubTotal(ShoppingCartItem shoppingCartItem,
           bool includeDiscounts,
           out decimal discountAmount,
           out List<AppliedDiscount> appliedDiscounts)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");

            decimal subTotal;

            //unit price
            var unitPrice = GetUnitPrice(shoppingCartItem, includeDiscounts,
                out discountAmount, out appliedDiscounts);

            //discount
            if (appliedDiscounts.Any())
            {
                //we can properly use "MaximumDiscountedQuantity" property only for one discount (not cumulative ones)
                Discount oneAndOnlyDiscount = null;
                if (appliedDiscounts.Count == 1)
                    oneAndOnlyDiscount = _discountService.GetDiscountById(appliedDiscounts.FirstOrDefault().DiscountId);

                if (oneAndOnlyDiscount != null &&
                    oneAndOnlyDiscount.MaximumDiscountedQuantity.HasValue &&
                    shoppingCartItem.Quantity > oneAndOnlyDiscount.MaximumDiscountedQuantity.Value)
                {
                    //we cannot apply discount for all shopping cart items
                    var discountedQuantity = oneAndOnlyDiscount.MaximumDiscountedQuantity.Value;
                    var discountedSubTotal = unitPrice * discountedQuantity;
                    discountAmount = discountAmount * discountedQuantity;

                    var notDiscountedQuantity = shoppingCartItem.Quantity - discountedQuantity;
                    var notDiscountedUnitPrice = GetUnitPrice(shoppingCartItem, false);
                    var notDiscountedSubTotal = notDiscountedUnitPrice * notDiscountedQuantity;

                    subTotal = discountedSubTotal + notDiscountedSubTotal;
                }
                else
                {
                    //discount is applied to all items (quantity)
                    //calculate discount amount for all items
                    discountAmount = discountAmount * shoppingCartItem.Quantity;

                    subTotal = unitPrice * shoppingCartItem.Quantity;
                }
            }
            else
            {
                subTotal = unitPrice * shoppingCartItem.Quantity;
            }
            return subTotal;
        }



        /// <summary>
        /// Gets the product cost (one item)
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Shopping cart item attributes in XML</param>
        /// <returns>Product cost (one item)</returns>
        public virtual decimal GetProductCost(Product product, string attributesXml)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            decimal cost = product.ProductCost;
            var attributeValues = _productAttributeParser.ParseProductAttributeValues(product, attributesXml);
            foreach (var attributeValue in attributeValues)
            {
                switch (attributeValue.AttributeValueType)
                {
                    case AttributeValueType.Simple:
                        {
                            //simple attribute
                            cost += attributeValue.Cost;
                        }
                        break;
                    case AttributeValueType.AssociatedToProduct:
                        {
                            //bundled product
                            var associatedProduct = _productService.GetProductById(attributeValue.AssociatedProductId);
                            if (associatedProduct != null)
                                cost += associatedProduct.ProductCost * attributeValue.Quantity;
                        }
                        break;
                    default:
                        break;
                }
            }

            return cost;
        }



        /// <summary>
        /// Get a price adjustment of a product attribute value
        /// </summary>
        /// <param name="value">Product attribute value</param>
        /// <returns>Price adjustment</returns>
        public virtual decimal GetProductAttributeValuePriceAdjustment(ProductAttributeValue value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            var adjustment = decimal.Zero;
            switch (value.AttributeValueType)
            {
                case AttributeValueType.Simple:
                    {
                        //simple attribute
                        adjustment = value.PriceAdjustment;
                    }
                    break;
                case AttributeValueType.AssociatedToProduct:
                    {
                        //bundled product
                        var associatedProduct = _productService.GetProductById(value.AssociatedProductId);
                        if (associatedProduct != null)
                        {
                            adjustment = GetFinalPrice(associatedProduct, _workContext.CurrentCustomer, includeDiscounts: true) * value.Quantity;
                        }
                    }
                    break;
                default:
                    break;
            }

            return adjustment;
        }

        #endregion
    }
}
