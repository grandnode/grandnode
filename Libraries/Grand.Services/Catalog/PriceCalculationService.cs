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
        private readonly ICacheManager _cacheManager;
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
            ICacheManager cacheManager,
            ShoppingCartSettings shoppingCartSettings, 
            CatalogSettings catalogSettings)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._discountService = discountService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productAttributeParser = productAttributeParser;
            this._productService = productService;
            this._cacheManager = cacheManager;
            this._shoppingCartSettings = shoppingCartSettings;
            this._catalogSettings = catalogSettings;
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
        protected virtual IList<Discount> GetAllowedDiscountsAppliedToProduct(Product product, Customer customer)
        {
            var allowedDiscounts = new List<Discount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            if (product.HasDiscountsApplied)
            {
                //we use this property ("HasDiscountsApplied") for performance optimziation to avoid unnecessary database calls
                foreach (var discount in product.AppliedDiscounts)
                {
                    if (_discountService.ValidateDiscount(discount, customer).IsValid &&
                        discount.DiscountType == DiscountType.AssignedToSkus &&
                        !allowedDiscounts.ContainsDiscount(discount))
                        allowedDiscounts.Add(discount);
                }
            }
            return allowedDiscounts;
        }



        /// <summary>
        /// Gets allowed discounts applied to categories
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discounts</returns>
        protected virtual IList<Discount> GetAllowedDiscountsAppliedToCategories(Product product, Customer customer)
        {
            var allowedDiscounts = new List<Discount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            

            foreach (var productCategory in product.ProductCategories)
            {

                var category = _categoryService.GetCategoryById(productCategory.CategoryId);
                if (category.AppliedDiscounts.Count() > 0)
                {
                    var categoryDiscounts = category.AppliedDiscounts;
                    foreach (var discount in categoryDiscounts)
                    {
                        if (_discountService.ValidateDiscount(discount, customer).IsValid &&
                                discount.DiscountType == DiscountType.AssignedToCategories &&
                                !allowedDiscounts.ContainsDiscount(discount))
                            allowedDiscounts.Add(discount);
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
        protected virtual IList<Discount> GetAllowedDiscountsAppliedToManufacturers(Product product, Customer customer)
        {
            var allowedDiscounts = new List<Discount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            foreach (var productManufacturer in product.ProductManufacturers)
            {
                var manufacturer = _manufacturerService.GetManufacturerById(productManufacturer.ManufacturerId);
                if (manufacturer.AppliedDiscounts.Count() > 0)
                {
                    //we use this property ("HasDiscountsApplied") for performance optimziation to avoid unnecessary database calls
                    var manufacturerDiscounts = manufacturer.AppliedDiscounts;
                    foreach (var discount in manufacturerDiscounts)
                    {
                        if (_discountService.ValidateDiscount(discount, customer).IsValid &&
                                 discount.DiscountType == DiscountType.AssignedToManufacturers &&
                                 !allowedDiscounts.ContainsDiscount(discount))
                            allowedDiscounts.Add(discount);
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
        protected virtual IList<Discount> GetAllowedDiscounts(Product product, Customer customer)
        {
            var allowedDiscounts = new List<Discount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            //discounts applied to products
            foreach (var discount in GetAllowedDiscountsAppliedToProduct(product, customer))
                if (!allowedDiscounts.ContainsDiscount(discount))
                    allowedDiscounts.Add(discount);

            //discounts applied to categories
            foreach (var discount in GetAllowedDiscountsAppliedToCategories(product, customer))
                if (!allowedDiscounts.ContainsDiscount(discount))
                    allowedDiscounts.Add(discount);

            //discounts applied to manufacturers
            foreach (var discount in GetAllowedDiscountsAppliedToManufacturers(product, customer))
                if (!allowedDiscounts.ContainsDiscount(discount))
                    allowedDiscounts.Add(discount);

            return allowedDiscounts;
        }

        //performance optimization
        //load all category discounts just to ensure that we have at least one
        //if (_discountService.GetAllDiscounts(DiscountType.AssignedToCategories).Any())
        //    {
        //        var productCategories = product.ProductCategories; //_categoryService.GetProductCategoriesByProductId(product.Id);
        //        foreach (var productCategory in productCategories)
        //        {
        //            var category = _categoryService.GetCategoryById(productCategory.CategoryId);
        //            if (category.HasDiscountsApplied)
        //            {
        //                //we use this property ("HasDiscountsApplied") for performance optimziation to avoid unnecessary database calls
        //                var categoryDiscounts = category.AppliedDiscounts;
        //                foreach (var discount in categoryDiscounts)
        //                {
        //                    if (_discountService.IsDiscountValid(discount, customer) &&
        //                        discount.DiscountType == DiscountType.AssignedToCategories &&
        //                        !allowedDiscounts.ContainsDiscount(discount))
        //                        allowedDiscounts.Add(discount);
        //                }
        //            }
        //        }
        //    }

        //    //performance optimization
        //    //load all manufacturer discounts just to ensure that we have at least one
        //    //if (_discountService.GetAllDiscounts(DiscountType.AssignedToManufacturers).Any())
        //    {
        //        var productManufacturers = product.ProductManufacturers; // _manufacturerService.GetProductManufacturersByProductId(product.Id);
        //        foreach (var productManufacturer in productManufacturers)
        //        {
        //            var manufacturer = _manufacturerService.GetManufacturerById(productManufacturer.ManufacturerId);
        //            if (manufacturer.HasDiscountsApplied)
        //            {
        //                //we use this property ("HasDiscountsApplied") for performance optimziation to avoid unnecessary database calls
        //                var manufacturerDiscounts = manufacturer.AppliedDiscounts;
        //                foreach (var discount in manufacturerDiscounts)
        //                {
        //                    if (_discountService.IsDiscountValid(discount, customer) &&
        //                        discount.DiscountType == DiscountType.AssignedToManufacturers &&
        //                        !allowedDiscounts.ContainsDiscount(discount))
        //                        allowedDiscounts.Add(discount);
        //                }
        //            }
        //        }
        //    }
        //    return allowedDiscounts;
        //}

        /// <summary>
        /// Gets a tier price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <param name="quantity">Quantity</param>
        /// <returns>Price</returns>
        protected virtual decimal? GetMinimumTierPrice(Product product, Customer customer, int quantity)
        {
            if (!product.HasTierPrices)
                return decimal.Zero;

            var tierPrices = product.TierPrices
                .OrderBy(tp => tp.Quantity)
                .ToList()
                .FilterByStore(_storeContext.CurrentStore.Id)
                .FilterForCustomer(customer)
                .RemoveDuplicatedQuantities();

            int previousQty = 1;
            decimal? previousPrice = null;
            foreach (var tierPrice in tierPrices)
            {
                //check quantity
                if (quantity < tierPrice.Quantity)
                    continue;
                if (tierPrice.Quantity < previousQty)
                    continue;

                //save new price
                previousPrice = tierPrice.Price;
                previousQty = tierPrice.Quantity;
            }
            
            return previousPrice;
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
            out List<Discount> appliedDiscounts)
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

            appliedDiscounts = allowedDiscounts.GetPreferredDiscount(productPriceWithoutDiscount, out appliedDiscountAmount);
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
            List<Discount> appliedDiscounts;
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
            out List<Discount> appliedDiscounts)
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
            out List<Discount> appliedDiscounts)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            discountAmount = decimal.Zero;
            appliedDiscounts = new List<Discount>();

            var cacheKey = string.Format(PriceCacheEventConsumer.PRODUCT_PRICE_MODEL_KEY,
                product.Id, 
                additionalCharge.ToString(CultureInfo.InvariantCulture),
                includeDiscounts, 
                quantity,
                string.Join(",", customer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var cacheTime = _catalogSettings.CacheProductPrices ? 60 : 0;
            //we do not cache price for rental products
            //otherwise, it can cause memory leaks (to store all possible date period combinations)
            if (product.IsRental)
                cacheTime = 0;
            var cachedPrice = _cacheManager.Get(cacheKey, cacheTime, () =>
            {
                var result = new ProductPriceForCaching();

                //initial price
                decimal price = product.Price;

                //special price
                var specialPrice = product.GetSpecialPrice();
                if (specialPrice.HasValue)
                    price = specialPrice.Value;

                //tier prices
                if (product.HasTierPrices)
                {
                    decimal? tierPrice = GetMinimumTierPrice(product, customer, quantity);
                    if (tierPrice.HasValue)
                        price = Math.Min(price, tierPrice.Value);
                }

                //additional charge
                price = price + additionalCharge;

                //rental products
                if (product.IsRental)
                    if (rentalStartDate.HasValue && rentalEndDate.HasValue)
                        price = price * product.GetRentalPeriods(rentalStartDate.Value, rentalEndDate.Value);

                if (includeDiscounts)
                {
                    //discount
                    List<Discount> tmpAppliedDiscounts;
                    decimal tmpDiscountAmount = GetDiscountAmount(product, customer, price, out tmpAppliedDiscounts);
                    price = price - tmpDiscountAmount;

                    if (tmpAppliedDiscounts != null)
                    {
                        result.AppliedDiscountIds = tmpAppliedDiscounts.Select(x => x.Id).ToList();
                        result.AppliedDiscountAmount = tmpDiscountAmount;
                    }
                }

                if (price < decimal.Zero)
                    price = decimal.Zero;

                result.Price = price;
                return result;
            });

            if (includeDiscounts)
            {
                //Discount instance cannnot be cached between requests (when "catalogSettings.CacheProductPrices" is "true)
                //This is limitation of Entity Framework
                //That's why we load it here after working with cache
                foreach (var appliedDiscountId in cachedPrice.AppliedDiscountIds)
                {
                    var appliedDiscount = _discountService.GetDiscountById(appliedDiscountId);
                    if (appliedDiscount != null)
                        appliedDiscounts.Add(appliedDiscount);
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
            List<Discount> appliedDiscounts;
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
            out List<Discount> appliedDiscounts)
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
            out List<Discount> appliedDiscounts)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (customer == null)
                throw new ArgumentNullException("customer");

            discountAmount = decimal.Zero;
            appliedDiscounts = new List<Discount>();

            decimal finalPrice;

            var combination = _productAttributeParser.FindProductAttributeCombination(product, attributesXml);
            if (combination != null && combination.OverriddenPrice.HasValue)
            {
                finalPrice = combination.OverriddenPrice.Value;
            }
            else
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
                        product.IsRental ? rentalStartDate : null,
                        product.IsRental ? rentalEndDate : null,
                        out discountAmount, out appliedDiscounts);
                }
            }
            
            //rounding
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                finalPrice = RoundingHelper.RoundPrice(finalPrice);

            return finalPrice;
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
            List<Discount> appliedDiscounts;
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
           out List<Discount> appliedDiscounts)
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
                    oneAndOnlyDiscount = appliedDiscounts.First();

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
