using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Discounts;
using Grand.Domain.Orders;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Vendors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly ICustomerProductService _customerProductService;
        private readonly IVendorService _vendorService;
        private readonly ICurrencyService _currencyService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        public PriceCalculationService(IWorkContext workContext,
            IStoreContext storeContext,
            IDiscountService discountService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductAttributeParser productAttributeParser,
            IProductService productService,
            ICustomerProductService customerProductService,
            IVendorService vendorService,
            ICurrencyService currencyService,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _discountService = discountService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _productAttributeParser = productAttributeParser;
            _productService = productService;
            _customerProductService = customerProductService;
            _vendorService = vendorService;
            _currencyService = currencyService;
            _shoppingCartSettings = shoppingCartSettings;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Nested classes

        [Serializable]
        protected class ProductPriceForCaching
        {
            public ProductPriceForCaching()
            {
                AppliedDiscounts = new List<AppliedDiscount>();
            }
            public decimal Price { get; set; }
            public decimal AppliedDiscountAmount { get; set; }
            public IList<AppliedDiscount> AppliedDiscounts { get; set; }
            public TierPrice PreferredTierPrice { get; set; }

        }
        #endregion

        #region Utilities

        /// <summary>
        /// Gets allowed discounts
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discounts</returns>
        protected virtual async Task<IList<AppliedDiscount>> GetAllowedDiscountsAppliedToProduct(Product product, Customer customer)
        {
            var allowedDiscounts = new List<AppliedDiscount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            if (product.AppliedDiscounts.Any())
            {
                foreach (var appliedDiscount in product.AppliedDiscounts)
                {
                    var discount = await _discountService.GetDiscountById(appliedDiscount);
                    if (discount != null)
                    {
                        var validDiscount = await _discountService.ValidateDiscount(discount, customer);
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

        protected virtual async Task<IList<AppliedDiscount>> GetAllowedDiscountsAppliedToAllProduct(Product product, Customer customer)
        {
            var allowedDiscounts = new List<AppliedDiscount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            var discounts = await _discountService.GetAllDiscounts(DiscountType.AssignedToAllProducts, storeId: _storeContext.CurrentStore.Id);
            foreach (var discount in discounts)
            {
                var validDiscount = await _discountService.ValidateDiscount(discount, customer);
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
        protected virtual async Task<IList<AppliedDiscount>> GetAllowedDiscountsAppliedToCategories(Product product, Customer customer)
        {
            var allowedDiscounts = new List<AppliedDiscount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            foreach (var productCategory in product.ProductCategories)
            {
                var category = await _categoryService.GetCategoryById(productCategory.CategoryId);
                if (category != null && category.AppliedDiscounts.Any())
                {
                    foreach (var appliedDiscount in category.AppliedDiscounts)
                    {
                        var discount = await _discountService.GetDiscountById(appliedDiscount);
                        if (discount != null)
                        {
                            var validDiscount = await _discountService.ValidateDiscount(discount, customer);
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
        protected virtual async Task<IList<AppliedDiscount>> GetAllowedDiscountsAppliedToManufacturers(Product product, Customer customer)
        {
            var allowedDiscounts = new List<AppliedDiscount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            foreach (var productManufacturer in product.ProductManufacturers)
            {
                var manufacturer = await _manufacturerService.GetManufacturerById(productManufacturer.ManufacturerId);
                if (manufacturer != null && manufacturer.AppliedDiscounts.Any())
                {
                    foreach (var appliedDiscount in manufacturer.AppliedDiscounts)
                    {
                        var discount = await _discountService.GetDiscountById(appliedDiscount);
                        if (discount != null)
                        {
                            var validDiscount = await _discountService.ValidateDiscount(discount, customer);
                            if (validDiscount.IsValid &&
                                     discount.DiscountType == DiscountType.AssignedToManufacturers)
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
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        protected virtual async Task<IList<AppliedDiscount>> GetAllowedDiscountsAppliedToVendors(Product product, Customer customer)
        {
            var allowedDiscounts = new List<AppliedDiscount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            if (!string.IsNullOrEmpty(product.VendorId))
            {
                var vendor = await _vendorService.GetVendorById(product.VendorId);
                if (vendor != null)
                {
                    if (vendor.AppliedDiscounts.Any())
                    {
                        foreach (var appliedDiscount in vendor.AppliedDiscounts)
                        {
                            var discount = await _discountService.GetDiscountById(appliedDiscount);
                            if (discount != null)
                            {
                                var validDiscount = await _discountService.ValidateDiscount(discount, customer);
                                if (validDiscount.IsValid &&
                                         discount.DiscountType == DiscountType.AssignedToVendors)
                                    allowedDiscounts.Add(new AppliedDiscount() {
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
        /// Gets allowed discounts
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discounts</returns>
        protected virtual async Task<IList<AppliedDiscount>> GetAllowedDiscounts(Product product, Customer customer)
        {
            var allowedDiscounts = new List<AppliedDiscount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            //discounts applied to products
            foreach (var discount in await GetAllowedDiscountsAppliedToProduct(product, customer))
                if (!allowedDiscounts.Where(x => x.DiscountId == discount.DiscountId).Any())
                    allowedDiscounts.Add(discount);

            //discounts applied to all products
            foreach (var discount in await GetAllowedDiscountsAppliedToAllProduct(product, customer))
                if (!allowedDiscounts.Where(x => x.DiscountId == discount.DiscountId).Any())
                    allowedDiscounts.Add(discount);

            //discounts applied to categories
            foreach (var discount in await GetAllowedDiscountsAppliedToCategories(product, customer))
                if (!allowedDiscounts.Where(x => x.DiscountId == discount.DiscountId).Any())
                    allowedDiscounts.Add(discount);

            //discounts applied to manufacturers
            foreach (var discount in await GetAllowedDiscountsAppliedToManufacturers(product, customer))
                if (!allowedDiscounts.Where(x => x.DiscountId == discount.DiscountId).Any())
                    allowedDiscounts.Add(discount);

            //discounts applied to vendors
            foreach (var discount in await GetAllowedDiscountsAppliedToVendors(product, customer))
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
        protected virtual async Task<(decimal discountAmount, List<AppliedDiscount> appliedDiscounts)> GetDiscountAmount(Product product,
            Customer customer,
            decimal productPriceWithoutDiscount)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            List<AppliedDiscount> appliedDiscounts = null;
            decimal appliedDiscountAmount = decimal.Zero;

            //we don't apply discounts to products with price entered by a customer
            if (product.CustomerEntersPrice)
                return (appliedDiscountAmount, appliedDiscounts);

            //discounts are disabled
            if (_catalogSettings.IgnoreDiscounts)
                return (appliedDiscountAmount, appliedDiscounts);

            var allowedDiscounts = await GetAllowedDiscounts(product, customer);

            //no discounts
            if (!allowedDiscounts.Any())
                return (appliedDiscountAmount, appliedDiscounts);

            var preferredDiscount = (await _discountService.GetPreferredDiscount(allowedDiscounts, customer, product, productPriceWithoutDiscount));
            appliedDiscounts = preferredDiscount.appliedDiscount;
            appliedDiscountAmount = preferredDiscount.discountAmount;
            return (appliedDiscountAmount, appliedDiscounts);
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
        public virtual async Task<(decimal finalPrice, decimal discountAmount, List<AppliedDiscount> appliedDiscounts, TierPrice preferredTierPrice)> GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge = decimal.Zero,
            bool includeDiscounts = true,
            int quantity = 1)
        {
            return await GetFinalPrice(product, customer, additionalCharge, includeDiscounts, quantity, null, null);
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
        public virtual async Task<(decimal finalPrice, decimal discountAmount, List<AppliedDiscount> appliedDiscounts, TierPrice preferredTierPrice)> GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            DateTime? rentalStartDate,
            DateTime? rentalEndDate)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var discountAmount = decimal.Zero;
            var appliedDiscounts = new List<AppliedDiscount>();

            async Task<ProductPriceForCaching> PrepareModel()
            {
                var result = new ProductPriceForCaching();

                //initial price
                decimal price = product.Price;

                //tier prices
                var tierPrice = product.GetPreferredTierPrice(customer, _storeContext.CurrentStore.Id, quantity);
                if (tierPrice != null)
                {
                    price = tierPrice.Price;
                    result.PreferredTierPrice = tierPrice;
                }

                //customer product price
                if (_catalogSettings.CustomerProductPrice)
                {
                    var customerPrice = await _customerProductService.GetPriceByCustomerProduct(customer.Id, product.Id);
                    if (customerPrice.HasValue && customerPrice.Value < price)
                        price = customerPrice.Value;
                }

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
                    var discountamount = await GetDiscountAmount(product, customer, price);
                    decimal tmpDiscountAmount = discountamount.discountAmount;
                    List<AppliedDiscount> tmpAppliedDiscounts = discountamount.appliedDiscounts;
                    price = price - tmpDiscountAmount;

                    if (tmpAppliedDiscounts != null)
                    {
                        result.AppliedDiscounts = tmpAppliedDiscounts.ToList();
                        result.AppliedDiscountAmount = tmpDiscountAmount;
                    }
                }

                if (price < decimal.Zero)
                    price = decimal.Zero;

                //rounding
                if (_shoppingCartSettings.RoundPricesDuringCalculation)
                {
                    var primaryCurrency = await _currencyService.GetPrimaryExchangeRateCurrency();
                    result.Price = RoundingHelper.RoundPrice(price, primaryCurrency);
                }
                else
                    result.Price = price;

                return result;
            }

            var modelprice = await PrepareModel();

            if (includeDiscounts)
            {
                appliedDiscounts = modelprice.AppliedDiscounts.ToList();
                if (appliedDiscounts.Any())
                {
                    discountAmount = modelprice.AppliedDiscountAmount;
                }
            }

            return (modelprice.Price, discountAmount, appliedDiscounts, modelprice.PreferredTierPrice);
        }


        /// <summary>
        /// Gets the shopping cart unit price (one item)
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Shopping cart unit price (one item)</returns>
        public virtual async Task<(decimal unitprice, decimal discountAmount, List<AppliedDiscount> appliedDiscounts)> GetUnitPrice(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts = true)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");
            var product = await _productService.GetProductById(shoppingCartItem.ProductId);
            return await GetUnitPrice(product,
                _workContext.CurrentCustomer,
                shoppingCartItem.ShoppingCartType,
                shoppingCartItem.Quantity,
                shoppingCartItem.AttributesXml,
                shoppingCartItem.CustomerEnteredPrice,
                shoppingCartItem.RentalStartDateUtc,
                shoppingCartItem.RentalEndDateUtc,
                includeDiscounts);
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
        public virtual async Task<(decimal unitprice, decimal discountAmount, List<AppliedDiscount> appliedDiscounts)> GetUnitPrice(Product product,
            Customer customer,
            ShoppingCartType shoppingCartType,
            int quantity,
            string attributesXml,
            decimal customerEnteredPrice,
            DateTime? rentalStartDate, DateTime? rentalEndDate,
            bool includeDiscounts)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (customer == null)
                throw new ArgumentNullException("customer");

            var discountAmount = decimal.Zero;
            var appliedDiscounts = new List<AppliedDiscount>();

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
            if (!finalPrice.HasValue)
            {
                //summarize price of all attributes
                decimal attributesTotalPrice = decimal.Zero;
                var attributeValues = _productAttributeParser.ParseProductAttributeValues(product, attributesXml);
                if (attributeValues != null)
                {
                    foreach (var attributeValue in attributeValues)
                    {
                        attributesTotalPrice += await GetProductAttributeValuePriceAdjustment(attributeValue);
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
                    var getfinalPrice = await GetFinalPrice(product,
                        customer,
                        attributesTotalPrice,
                        includeDiscounts,
                        qty,
                        product.ProductType == ProductType.Reservation ? rentalStartDate : null,
                        product.ProductType == ProductType.Reservation ? rentalEndDate : null);
                    finalPrice = getfinalPrice.finalPrice;
                    discountAmount = getfinalPrice.discountAmount;
                    appliedDiscounts = getfinalPrice.appliedDiscounts;
                }
            }

            if (!finalPrice.HasValue)
                finalPrice = 0;

            //rounding
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                var primaryCurrency = await _currencyService.GetPrimaryExchangeRateCurrency();
                finalPrice = RoundingHelper.RoundPrice(finalPrice.Value, primaryCurrency);
            }
            return (finalPrice.Value, discountAmount, appliedDiscounts);
        }

        /// <summary>
        /// Gets the shopping cart item sub total
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Shopping cart item sub total</returns>
        public virtual async Task<(decimal subTotal, decimal discountAmount, List<AppliedDiscount> appliedDiscounts)> GetSubTotal(ShoppingCartItem shoppingCartItem,
           bool includeDiscounts = true)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");

            decimal subTotal;
            //unit price
            var getunitPrice = await GetUnitPrice(shoppingCartItem, includeDiscounts);
            var unitPrice = getunitPrice.unitprice;
            decimal discountAmount = getunitPrice.discountAmount;
            List<AppliedDiscount> appliedDiscounts = getunitPrice.appliedDiscounts;

            //discount
            if (appliedDiscounts.Any())
            {
                //we can properly use "MaximumDiscountedQuantity" property only for one discount (not cumulative ones)
                Discount oneAndOnlyDiscount = null;
                if (appliedDiscounts.Count == 1)
                    oneAndOnlyDiscount = await _discountService.GetDiscountById(appliedDiscounts.FirstOrDefault().DiscountId);

                if (oneAndOnlyDiscount != null &&
                    oneAndOnlyDiscount.MaximumDiscountedQuantity.HasValue &&
                    shoppingCartItem.Quantity > oneAndOnlyDiscount.MaximumDiscountedQuantity.Value)
                {
                    //we cannot apply discount for all shopping cart items
                    var discountedQuantity = oneAndOnlyDiscount.MaximumDiscountedQuantity.Value;
                    var discountedSubTotal = unitPrice * discountedQuantity;
                    discountAmount = discountAmount * discountedQuantity;

                    var notDiscountedQuantity = shoppingCartItem.Quantity - discountedQuantity;
                    var notDiscountedUnitPrice = await GetUnitPrice(shoppingCartItem, false);
                    var notDiscountedSubTotal = notDiscountedUnitPrice.unitprice * notDiscountedQuantity;

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
            return (subTotal, discountAmount, appliedDiscounts);
        }



        /// <summary>
        /// Gets the product cost (one item)
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Shopping cart item attributes in XML</param>
        /// <returns>Product cost (one item)</returns>
        public virtual async Task<decimal> GetProductCost(Product product, string attributesXml)
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
                            var associatedProduct = await _productService.GetProductById(attributeValue.AssociatedProductId);
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
        public virtual async Task<decimal> GetProductAttributeValuePriceAdjustment(ProductAttributeValue value)
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
                        var associatedProduct = await _productService.GetProductById(value.AssociatedProductId);
                        if (associatedProduct != null)
                        {
                            adjustment = (await GetFinalPrice(associatedProduct, _workContext.CurrentCustomer, additionalCharge: value.PriceAdjustment, includeDiscounts: true)).finalPrice * value.Quantity;
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
