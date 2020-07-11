using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Discounts;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Domain.Tax;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Payments;
using Grand.Services.Shipping;
using Grand.Services.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Order service
    /// </summary>
    public partial class OrderTotalCalculationService : IOrderTotalCalculationService
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly IShippingService _shippingService;
        private readonly IPaymentService _paymentService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IDiscountService _discountService;
        private readonly IGiftCardService _giftCardService;
        private readonly IRewardPointsService _rewardPointsService;
        private readonly IProductService _productService;
        private readonly ICurrencyService _currencyService;
        private readonly TaxSettings _taxSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="priceCalculationService">Price calculation service</param>
        /// <param name="taxService">Tax service</param>
        /// <param name="shippingService">Shipping service</param>
        /// <param name="paymentService">Payment service</param>
        /// <param name="checkoutAttributeParser">Checkout attribute parser</param>
        /// <param name="discountService">Discount service</param>
        /// <param name="giftCardService">Gift card service</param>
        /// <param name="rewardPointsService">Reward points service</param>
        /// <param name="currencyService">Currency service</param>
        /// <param name="taxSettings">Tax settings</param>
        /// <param name="rewardPointsSettings">Reward points settings</param>
        /// <param name="shippingSettings">Shipping settings</param>
        /// <param name="shoppingCartSettings">Shopping cart settings</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public OrderTotalCalculationService(IWorkContext workContext,
            IStoreContext storeContext,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            IShippingService shippingService,
            IPaymentService paymentService,
            ICheckoutAttributeParser checkoutAttributeParser,
            IDiscountService discountService,
            IGiftCardService giftCardService,
            IRewardPointsService rewardPointsService,
            IProductService productService,
            ICurrencyService currencyService,
            TaxSettings taxSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _priceCalculationService = priceCalculationService;
            _taxService = taxService;
            _shippingService = shippingService;
            _paymentService = paymentService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _discountService = discountService;
            _giftCardService = giftCardService;
            _rewardPointsService = rewardPointsService;
            _productService = productService;
            _currencyService = currencyService;
            _taxSettings = taxSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _shippingSettings = shippingSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets an order discount (applied to order subtotal)
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="orderSubTotal">Order subtotal</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Order discount</returns>
        protected virtual async Task<(decimal ordersubtotaldiscount, List<AppliedDiscount> appliedDiscounts)> GetOrderSubtotalDiscount(Customer customer, decimal orderSubTotal)
        {
            var appliedDiscounts = new List<AppliedDiscount>();
            decimal discountAmount = decimal.Zero;
            if (_catalogSettings.IgnoreDiscounts)
                return (discountAmount, appliedDiscounts);

            var allDiscounts = await _discountService.GetAllDiscounts(DiscountType.AssignedToOrderSubTotal, storeId: _storeContext.CurrentStore.Id);
            var allowedDiscounts = new List<AppliedDiscount>();
            if (allDiscounts != null)
                foreach (var discount in allDiscounts)
                {
                    var validDiscount = await _discountService.ValidateDiscount(discount, customer);
                    if (validDiscount.IsValid &&
                        discount.DiscountType == DiscountType.AssignedToOrderSubTotal &&
                        !allowedDiscounts.Where(x => x.DiscountId == discount.Id).Any())
                    {
                        allowedDiscounts.Add(new AppliedDiscount
                        {
                            DiscountId = discount.Id,
                            IsCumulative = discount.IsCumulative,
                            CouponCode = validDiscount.CouponCode,
                        });
                    }
                }

            var preferredDiscounts = await _discountService.GetPreferredDiscount(allowedDiscounts, customer, orderSubTotal);
            appliedDiscounts = preferredDiscounts.appliedDiscount;
            discountAmount = preferredDiscounts.discountAmount;

            if (discountAmount < decimal.Zero)
                discountAmount = decimal.Zero;

            return (discountAmount, appliedDiscounts);
        }

        /// <summary>
        /// Gets a shipping discount
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shippingTotal">Shipping total</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Shipping discount</returns>
        protected virtual async Task<(decimal shippingDiscount, List<AppliedDiscount> appliedDiscounts)> GetShippingDiscount(Customer customer, decimal shippingTotal)
        {
            var appliedDiscounts = new List<AppliedDiscount>();
            decimal shippingDiscountAmount = decimal.Zero;
            if (_catalogSettings.IgnoreDiscounts)
                return (shippingDiscountAmount, appliedDiscounts);

            var allDiscounts = await _discountService.GetAllDiscounts(DiscountType.AssignedToShipping, storeId: _storeContext.CurrentStore.Id);
            var allowedDiscounts = new List<AppliedDiscount>();
            if (allDiscounts != null)
                foreach (var discount in allDiscounts)
                {
                    var validDiscount = await _discountService.ValidateDiscount(discount, customer);
                    if (validDiscount.IsValid &&
                        discount.DiscountType == DiscountType.AssignedToShipping &&
                        !allowedDiscounts.Where(x => x.DiscountId == discount.Id).Any())
                    {
                        allowedDiscounts.Add(new AppliedDiscount
                        {
                            DiscountId = discount.Id,
                            IsCumulative = discount.IsCumulative,
                            CouponCode = validDiscount.CouponCode,
                        });
                    }
                }

            var (appliedDiscount, discountAmount) = await _discountService.GetPreferredDiscount(allowedDiscounts, customer, shippingTotal);
            appliedDiscounts = appliedDiscount;
            shippingDiscountAmount = discountAmount;

            if (shippingDiscountAmount < decimal.Zero)
                shippingDiscountAmount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                var currency = await _currencyService.GetPrimaryExchangeRateCurrency();
                shippingDiscountAmount = RoundingHelper.RoundPrice(shippingDiscountAmount, currency);
            }
            return (shippingDiscountAmount, appliedDiscounts);
        }

        /// <summary>
        /// Gets an order discount (applied to order total)
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="orderTotal">Order total</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Order discount</returns>
        protected virtual async Task<(decimal orderTotalDiscount, List<AppliedDiscount> appliedDiscounts)> GetOrderTotalDiscount(Customer customer, decimal orderTotal)
        {
            var appliedDiscounts = new List<AppliedDiscount>();
            decimal discountAmount = decimal.Zero;
            if (_catalogSettings.IgnoreDiscounts)
                return (discountAmount, appliedDiscounts);

            var allDiscounts = await _discountService.GetAllDiscounts(DiscountType.AssignedToOrderTotal, storeId: _storeContext.CurrentStore.Id);
            var allowedDiscounts = new List<AppliedDiscount>();
            if (allDiscounts != null)
                foreach (var discount in allDiscounts)
                {
                    var validDiscount = await _discountService.ValidateDiscount(discount, customer);
                    if (validDiscount.IsValid &&
                               discount.DiscountType == DiscountType.AssignedToOrderTotal &&
                               !allowedDiscounts.Where(x => x.DiscountId == discount.Id).Any())
                    {
                        allowedDiscounts.Add(new AppliedDiscount
                        {
                            DiscountId = discount.Id,
                            IsCumulative = discount.IsCumulative,
                            CouponCode = validDiscount.CouponCode,
                        });
                    }
                }
            var preferredDiscount = await _discountService.GetPreferredDiscount(allowedDiscounts, customer, orderTotal);
            appliedDiscounts = preferredDiscount.appliedDiscount;
            discountAmount = preferredDiscount.discountAmount;

            if (discountAmount < decimal.Zero)
                discountAmount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                var primaryCurrency = await _currencyService.GetPrimaryExchangeRateCurrency();
                discountAmount = RoundingHelper.RoundPrice(discountAmount, primaryCurrency);
            }
            return (discountAmount, appliedDiscounts);
        }

        /// <summary>
        /// Get active gift cards that are applied by a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Active gift cards</returns>
        private async Task<IList<GiftCard>> GetActiveGiftCards(Customer customer)
        {
            var result = new List<GiftCard>();
            if (customer == null)
                return result;

            string[] couponCodes = customer.ParseAppliedCouponCodes(SystemCustomerAttributeNames.GiftCardCoupons);
            foreach (var couponCode in couponCodes)
            {
                var giftCards = await _giftCardService.GetAllGiftCards(isGiftCardActivated: true, giftCardCouponCode: couponCode);
                foreach (var gc in giftCards)
                {
                    if (gc.IsGiftCardValid())
                        result.Add(gc);
                }
            }

            return result;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Gets shopping cart subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <param name="subTotalWithoutDiscount">Sub total (without discount)</param>
        /// <param name="subTotalWithDiscount">Sub total (with discount)</param>
        /// <param name="taxRates">Tax rates (of order sub total)</param>
        public virtual async Task<(decimal discountAmount, List<AppliedDiscount> appliedDiscounts, decimal subTotalWithoutDiscount, decimal subTotalWithDiscount, SortedDictionary<decimal, decimal> taxRates)>
            GetShoppingCartSubTotal(IList<ShoppingCartItem> cart, bool includingTax)
        {
            var discountAmount = decimal.Zero;
            var appliedDiscounts = new List<AppliedDiscount>();
            var subTotalWithoutDiscount = decimal.Zero;
            var subTotalWithDiscount = decimal.Zero;
            var taxRates = new SortedDictionary<decimal, decimal>();

            if (!cart.Any())
                return (discountAmount, appliedDiscounts, subTotalWithoutDiscount, subTotalWithDiscount, taxRates);

            //get the customer 
            Customer customer = _workContext.CurrentCustomer;
            var currency = await _currencyService.GetPrimaryExchangeRateCurrency();
            //sub totals
            decimal subTotalExclTaxWithoutDiscount = decimal.Zero;
            decimal subTotalInclTaxWithoutDiscount = decimal.Zero;
            foreach (var shoppingCartItem in cart)
            {
                var product = await _productService.GetProductById(shoppingCartItem.ProductId);
                if (product == null)
                    continue;

                var subtotal = await _priceCalculationService.GetSubTotal(shoppingCartItem);
                decimal sciSubTotal = subtotal.subTotal;

                decimal taxRate;
                var pricesExcl = await _taxService.GetProductPrice(product, sciSubTotal, false, customer);
                decimal sciExclTax = pricesExcl.productprice;

                var pricesIncl = await _taxService.GetProductPrice(product, sciSubTotal, true, customer);
                decimal sciInclTax = pricesIncl.productprice;
                taxRate = pricesIncl.taxRate;

                subTotalExclTaxWithoutDiscount += sciExclTax;
                subTotalInclTaxWithoutDiscount += sciInclTax;

                //tax rates
                decimal sciTax = sciInclTax - sciExclTax;
                if (taxRate > decimal.Zero && sciTax > decimal.Zero)
                {
                    if (!taxRates.ContainsKey(taxRate))
                    {
                        taxRates.Add(taxRate, sciTax);
                    }
                    else
                    {
                        taxRates[taxRate] = taxRates[taxRate] + sciTax;
                    }
                }
            }

            //checkout attributes
            if (customer != null)
            {
                var checkoutAttributesXml = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CheckoutAttributes, _storeContext.CurrentStore.Id);
                var attributeValues = await _checkoutAttributeParser.ParseCheckoutAttributeValue(checkoutAttributesXml);
                    foreach (var attributeValue in attributeValues)
                    {
                        decimal taxRate;
                        var checkoutAttributePriceExclTax = await _taxService.GetCheckoutAttributePrice(attributeValue.ca, attributeValue.cav, false, customer);
                        decimal caExclTax = checkoutAttributePriceExclTax.checkoutPrice;

                        var checkoutAttributePriceInclTax = await _taxService.GetCheckoutAttributePrice(attributeValue.ca, attributeValue.cav, true, customer);
                        decimal caInclTax = checkoutAttributePriceInclTax.checkoutPrice;

                        taxRate = checkoutAttributePriceInclTax.taxRate;

                        subTotalExclTaxWithoutDiscount += caExclTax;
                        subTotalInclTaxWithoutDiscount += caInclTax;

                        //tax rates
                        decimal caTax = caInclTax - caExclTax;
                        if (taxRate > decimal.Zero && caTax > decimal.Zero)
                        {
                            if (!taxRates.ContainsKey(taxRate))
                            {
                                taxRates.Add(taxRate, caTax);
                            }
                            else
                            {
                                taxRates[taxRate] = taxRates[taxRate] + caTax;
                            }
                        }
                    }
            }

            //subtotal without discount
            subTotalWithoutDiscount = includingTax ? subTotalInclTaxWithoutDiscount : subTotalExclTaxWithoutDiscount;

            if (subTotalWithoutDiscount < decimal.Zero)
                subTotalWithoutDiscount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                subTotalWithoutDiscount = RoundingHelper.RoundPrice(subTotalWithoutDiscount, currency);
            }
            //We calculate discount amount on order subtotal excl tax (discount first)
            //calculate discount amount ('Applied to order subtotal' discount)
            var orderSubtotalDiscount = await GetOrderSubtotalDiscount(customer, subTotalExclTaxWithoutDiscount);
            decimal discountAmountExclTax = orderSubtotalDiscount.ordersubtotaldiscount;
            appliedDiscounts = orderSubtotalDiscount.appliedDiscounts;

            if (subTotalExclTaxWithoutDiscount < discountAmountExclTax)
                discountAmountExclTax = subTotalExclTaxWithoutDiscount;
            decimal discountAmountInclTax = discountAmountExclTax;
            //subtotal with discount (excl tax)
            decimal subTotalExclTaxWithDiscount = subTotalExclTaxWithoutDiscount - discountAmountExclTax;
            decimal subTotalInclTaxWithDiscount = subTotalExclTaxWithDiscount;

            //add tax for shopping items & checkout attributes
            var tempTaxRates = new Dictionary<decimal, decimal>(taxRates);
            foreach (KeyValuePair<decimal, decimal> kvp in tempTaxRates)
            {
                decimal taxRate = kvp.Key;
                decimal taxValue = kvp.Value;

                if (taxValue != decimal.Zero)
                {
                    //discount the tax amount that applies to subtotal items
                    if (subTotalExclTaxWithoutDiscount > decimal.Zero)
                    {
                        decimal discountTax = taxRates[taxRate] * (discountAmountExclTax / subTotalExclTaxWithoutDiscount);
                        discountAmountInclTax += discountTax;
                        taxValue = taxRates[taxRate] - discountTax;
                        if (_shoppingCartSettings.RoundPricesDuringCalculation)
                            taxValue = RoundingHelper.RoundPrice(taxValue, currency);
                        taxRates[taxRate] = taxValue;
                    }

                    //subtotal with discount (incl tax)
                    subTotalInclTaxWithDiscount += taxValue;
                }
            }

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                discountAmountInclTax = RoundingHelper.RoundPrice(discountAmountInclTax, currency);
                discountAmountExclTax = RoundingHelper.RoundPrice(discountAmountExclTax, currency);
            }

            if (includingTax)
            {
                subTotalWithDiscount = subTotalInclTaxWithDiscount;
                discountAmount = discountAmountInclTax;
            }
            else
            {
                subTotalWithDiscount = subTotalExclTaxWithDiscount;
                discountAmount = discountAmountExclTax;
            }

            if (subTotalWithDiscount < decimal.Zero)
                subTotalWithDiscount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                subTotalWithDiscount = RoundingHelper.RoundPrice(subTotalWithDiscount, currency);

            return (discountAmount, appliedDiscounts, subTotalWithoutDiscount, subTotalWithDiscount, taxRates);
        }


        /// <summary>
        /// Gets shopping cart additional shipping charge
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>Additional shipping charge</returns>
        public virtual async Task<decimal> GetShoppingCartAdditionalShippingCharge(IList<ShoppingCartItem> cart)
        {
            decimal additionalShippingCharge = decimal.Zero;

            bool isFreeShipping = await IsFreeShipping(cart);
            if (isFreeShipping)
                return decimal.Zero;

            foreach (var sci in cart)
                if (sci.IsShipEnabled && !sci.IsFreeShipping)
                    additionalShippingCharge += sci.AdditionalShippingCharge;

            return additionalShippingCharge;
        }

        /// <summary>
        /// Gets a value indicating whether shipping is free
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>A value indicating whether shipping is free</returns>
        public virtual async Task<bool> IsFreeShipping(IList<ShoppingCartItem> cart)
        {
            Customer customer = _workContext.CurrentCustomer;
            if (customer != null)
            {
                //check whether customer has a free shipping
                if (customer.FreeShipping)
                    return true;

                //check whether customer is in a customer role with free shipping applied
                var customerRoles = customer.CustomerRoles.Where(cr => cr.Active);
                foreach (var customerRole in customerRoles)
                    if (customerRole.FreeShipping)
                        return true;

            }

            bool shoppingCartRequiresShipping = cart.RequiresShipping();
            if (!shoppingCartRequiresShipping)
                return true;

            //check whether all shopping cart items are marked as free shipping
            bool allItemsAreFreeShipping = true;
            foreach (var sc in cart)
            {
                if (sc.IsShipEnabled && !sc.IsFreeShipping)
                {
                    allItemsAreFreeShipping = false;
                    break;
                }
            }
            if (allItemsAreFreeShipping)
                return true;

            //free shipping over $X
            if (_shippingSettings.FreeShippingOverXEnabled)
            {
                //check whether we have subtotal enough to have free shipping
                var (discountAmount, appliedDiscounts, subTotalWithoutDiscount, subTotalWithDiscount, taxRates) = await GetShoppingCartSubTotal(cart, _shippingSettings.FreeShippingOverXIncludingTax);
                var subTotalDiscountAmount = discountAmount;
                var subTotalAppliedDiscounts = appliedDiscounts;
                var subTotalWithoutDiscountBase = subTotalWithoutDiscount;
                var subTotalWithDiscountBase = subTotalWithDiscount;

                if (subTotalWithDiscountBase > _shippingSettings.FreeShippingOverXValue)
                    return true;
            }

            //otherwise, return false
            return false;
        }

        /// <summary>
        /// Adjust shipping rate (free shipping, additional charges, discounts)
        /// </summary>
        /// <param name="shippingRate">Shipping rate to adjust</param>
        /// <param name="cart">Cart</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Adjusted shipping rate</returns>
        public virtual async Task<(decimal shippingRate, List<AppliedDiscount> appliedDiscounts)> AdjustShippingRate(decimal shippingRate, IList<ShoppingCartItem> cart)
        {
            var appliedDiscounts = new List<AppliedDiscount>();

            //free shipping
            if (await IsFreeShipping(cart))
                return (decimal.Zero, appliedDiscounts);

            //additional shipping charges
            decimal additionalShippingCharge = await GetShoppingCartAdditionalShippingCharge(cart);
            var adjustedRate = shippingRate + additionalShippingCharge;

            //discount
            var customer = _workContext.CurrentCustomer;
            var shippingDiscount = await GetShippingDiscount(customer, adjustedRate);
            decimal discountAmount = shippingDiscount.shippingDiscount;
            appliedDiscounts = shippingDiscount.appliedDiscounts;

            adjustedRate = adjustedRate - discountAmount;

            if (adjustedRate < decimal.Zero)
                adjustedRate = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                var primaryCurrency = await _currencyService.GetPrimaryExchangeRateCurrency();
                adjustedRate = RoundingHelper.RoundPrice(adjustedRate, primaryCurrency);
            }

            return (adjustedRate, appliedDiscounts);
        }

        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>Shipping total</returns>
        public virtual async Task<(decimal? shoppingCartShippingTotal, decimal taxRate, List<AppliedDiscount> appliedDiscounts)> GetShoppingCartShippingTotal(IList<ShoppingCartItem> cart)
        {
            bool includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
            return await GetShoppingCartShippingTotal(cart, includingTax);
        }


        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="taxRate">Applied tax rate</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Shipping total</returns>
        public virtual async Task<(decimal? shoppingCartShippingTotal, decimal taxRate, List<AppliedDiscount> appliedDiscounts)> GetShoppingCartShippingTotal(IList<ShoppingCartItem> cart, bool includingTax)
        {
            decimal? shippingTotal = null;
            decimal? shippingTotalTaxed = null;
            var appliedDiscounts = new List<AppliedDiscount>();
            var taxRate = decimal.Zero;

            var customer = _workContext.CurrentCustomer;
            var currency = await _currencyService.GetPrimaryExchangeRateCurrency();

            bool isFreeShipping = await IsFreeShipping(cart);
            if (isFreeShipping)
                return (decimal.Zero, taxRate, appliedDiscounts);

            ShippingOption shippingOption = null;
            if (customer != null)
                shippingOption = customer.GetAttributeFromEntity<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);

            if (shippingOption != null)
            {
                var adjustshipingRate = await AdjustShippingRate(shippingOption.Rate, cart);
                shippingTotal = adjustshipingRate.shippingRate;
                appliedDiscounts = adjustshipingRate.appliedDiscounts;
            }
            else
            {
                //use fixed rate (if possible)
                Address shippingAddress = null;
                if (customer != null)
                    shippingAddress = customer.ShippingAddress;

                var shippingRateComputationMethods = await _shippingService.LoadActiveShippingRateComputationMethods(_storeContext.CurrentStore.Id, cart);

                if (!shippingRateComputationMethods.Any() && !_shippingSettings.AllowPickUpInStore)
                    throw new GrandException("Shipping rate computation method could not be loaded");

                if (shippingRateComputationMethods.Count == 1)
                {
                    var shippingRateComputationMethod = shippingRateComputationMethods[0];

                    var shippingOptionRequests = await _shippingService.CreateShippingOptionRequests(customer, cart,
                        shippingAddress,
                        _storeContext.CurrentStore);
                    bool shippingFromMultipleLocations = shippingOptionRequests.shippingFromMultipleLocations;
                    decimal? fixedRate = null;
                    foreach (var shippingOptionRequest in shippingOptionRequests.shippingOptionRequest)
                    {
                        //calculate fixed rates for each request-package
                        var fixedRateTmp = await shippingRateComputationMethod.GetFixedRate(shippingOptionRequest);
                        if (fixedRateTmp.HasValue)
                        {
                            if (!fixedRate.HasValue)
                                fixedRate = decimal.Zero;

                            fixedRate += fixedRateTmp.Value;
                        }
                    }

                    if (fixedRate.HasValue)
                    {
                        //adjust shipping rate
                        var adjustShippingRate = await AdjustShippingRate(fixedRate.Value, cart);
                        shippingTotal = adjustShippingRate.shippingRate;
                        appliedDiscounts = adjustShippingRate.appliedDiscounts;
                    }
                }
            }

            if (shippingTotal.HasValue)
            {
                if (shippingTotal.Value < decimal.Zero)
                    shippingTotal = decimal.Zero;

                //round
                if (_shoppingCartSettings.RoundPricesDuringCalculation)
                    shippingTotal = RoundingHelper.RoundPrice(shippingTotal.Value, currency);

                var shippingPrice = await _taxService.GetShippingPrice(shippingTotal.Value, includingTax, customer);
                shippingTotalTaxed = shippingPrice.shippingPrice;
                taxRate = shippingPrice.taxRate;

                //round
                if (_shoppingCartSettings.RoundPricesDuringCalculation)
                    shippingTotalTaxed = RoundingHelper.RoundPrice(shippingTotalTaxed.Value, currency);
            }

            return (shippingTotalTaxed, taxRate, appliedDiscounts);
        }

        /// <summary>
        /// Gets tax
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="taxRates">Tax rates</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating tax</param>
        /// <returns>Tax total</returns>
        public virtual async Task<(decimal taxtotal, SortedDictionary<decimal, decimal> taxRates)> GetTaxTotal(IList<ShoppingCartItem> cart, bool usePaymentMethodAdditionalFee = true)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            var taxRates = new SortedDictionary<decimal, decimal>();

            var customer = _workContext.CurrentCustomer;
            string paymentMethodSystemName = "";
            if (customer != null)
            {
                paymentMethodSystemName = customer.GetAttributeFromEntity<string>(
                    SystemCustomerAttributeNames.SelectedPaymentMethod,
                    _storeContext.CurrentStore.Id);
            }

            //order sub total (items + checkout attributes)
            decimal subTotalTaxTotal = decimal.Zero;

            var shoppingCartSubTotal = await GetShoppingCartSubTotal(cart, false);
            SortedDictionary<decimal, decimal> orderSubTotalTaxRates = shoppingCartSubTotal.taxRates;

            foreach (KeyValuePair<decimal, decimal> kvp in orderSubTotalTaxRates)
            {
                decimal taxRate = kvp.Key;
                decimal taxValue = kvp.Value;
                subTotalTaxTotal += taxValue;

                if (taxRate > decimal.Zero && taxValue > decimal.Zero)
                {
                    if (!taxRates.ContainsKey(taxRate))
                        taxRates.Add(taxRate, taxValue);
                    else
                        taxRates[taxRate] = taxRates[taxRate] + taxValue;
                }
            }

            //shipping
            decimal shippingTax = decimal.Zero;
            if (_taxSettings.ShippingIsTaxable)
            {
                decimal taxRate;
                var shippingTotalExcl = await GetShoppingCartShippingTotal(cart, false);
                decimal? shippingExclTax = shippingTotalExcl.shoppingCartShippingTotal;

                var shippingTotalIncl = await GetShoppingCartShippingTotal(cart, true);
                decimal? shippingInclTax = shippingTotalIncl.shoppingCartShippingTotal;

                taxRate = shippingTotalIncl.taxRate;

                if (shippingExclTax.HasValue && shippingInclTax.HasValue)
                {
                    shippingTax = shippingInclTax.Value - shippingExclTax.Value;
                    //ensure that tax is equal or greater than zero
                    if (shippingTax < decimal.Zero)
                        shippingTax = decimal.Zero;

                    //tax rates
                    if (taxRate > decimal.Zero && shippingTax > decimal.Zero)
                    {
                        if (!taxRates.ContainsKey(taxRate))
                            taxRates.Add(taxRate, shippingTax);
                        else
                            taxRates[taxRate] = taxRates[taxRate] + shippingTax;
                    }
                }
            }

            //payment method additional fee
            decimal paymentMethodAdditionalFeeTax = decimal.Zero;
            if (usePaymentMethodAdditionalFee && _taxSettings.PaymentMethodAdditionalFeeIsTaxable)
            {
                decimal taxRate;
                decimal paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFee(cart, paymentMethodSystemName);

                var additionalFeeExclTax = await _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, false, customer);
                decimal paymentMethodAdditionalFeeExclTax = additionalFeeExclTax.paymentPrice;

                var additionalFeeInclTax = await _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, true, customer);
                decimal paymentMethodAdditionalFeeInclTax = additionalFeeInclTax.paymentPrice;

                taxRate = additionalFeeInclTax.taxRate;

                paymentMethodAdditionalFeeTax = paymentMethodAdditionalFeeInclTax - paymentMethodAdditionalFeeExclTax;
                //ensure that tax is equal or greater than zero
                if (paymentMethodAdditionalFeeTax < decimal.Zero)
                    paymentMethodAdditionalFeeTax = decimal.Zero;

                //tax rates
                if (taxRate > decimal.Zero && paymentMethodAdditionalFeeTax > decimal.Zero)
                {
                    if (!taxRates.ContainsKey(taxRate))
                        taxRates.Add(taxRate, paymentMethodAdditionalFeeTax);
                    else
                        taxRates[taxRate] = taxRates[taxRate] + paymentMethodAdditionalFeeTax;
                }
            }

            //add at least one tax rate (0%)
            if (!taxRates.Any())
                taxRates.Add(decimal.Zero, decimal.Zero);

            //summarize taxes
            decimal taxTotal = subTotalTaxTotal + shippingTax + paymentMethodAdditionalFeeTax;
            //ensure that tax is equal or greater than zero
            if (taxTotal < decimal.Zero)
                taxTotal = decimal.Zero;
            //round tax
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                var currency = await _currencyService.GetPrimaryExchangeRateCurrency();
                taxTotal = RoundingHelper.RoundPrice(taxTotal, currency);
            }
            return (taxTotal, taxRates);
        }


        /// <summary>
        /// Gets shopping cart total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="appliedGiftCards">Applied gift cards</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <param name="redeemedRewardPoints">Reward points to redeem</param>
        /// <param name="redeemedRewardPointsAmount">Reward points amount in primary store currency to redeem</param>
        /// <param name="ignoreRewardPonts">A value indicating whether we should ignore reward points (if enabled and a customer is going to use them)</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating order total</param>
        /// <returns>Shopping cart total;Null if shopping cart total couldn't be calculated now</returns>
        public virtual async Task<(decimal? shoppingCartTotal, decimal discountAmount, List<AppliedDiscount> appliedDiscounts, List<AppliedGiftCard> appliedGiftCards,
            int redeemedRewardPoints, decimal redeemedRewardPointsAmount)>
            GetShoppingCartTotal(IList<ShoppingCartItem> cart, bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true)
        {

            var redeemedRewardPoints = 0;
            var redeemedRewardPointsAmount = decimal.Zero;

            var customer = _workContext.CurrentCustomer;
            var currency = await _currencyService.GetPrimaryExchangeRateCurrency();

            string paymentMethodSystemName = "";
            if (customer != null)
            {
                paymentMethodSystemName = customer.GetAttributeFromEntity<string>(
                    SystemCustomerAttributeNames.SelectedPaymentMethod,
                    _storeContext.CurrentStore.Id);
            }

            //subtotal without tax
            var subTotal = await GetShoppingCartSubTotal(cart, false);
            decimal subTotalWithDiscountBase = subTotal.subTotalWithDiscount;

            //subtotal with discount
            decimal subtotalBase = subTotalWithDiscountBase;

            //shipping without tax
            var shippingTotal = await GetShoppingCartShippingTotal(cart, false);
            decimal? shoppingCartShipping = shippingTotal.shoppingCartShippingTotal;

            //payment method additional fee without tax
            decimal paymentMethodAdditionalFeeWithoutTax = decimal.Zero;
            if (usePaymentMethodAdditionalFee && !String.IsNullOrEmpty(paymentMethodSystemName))
            {
                decimal paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFee(cart,
                    paymentMethodSystemName);
                paymentMethodAdditionalFeeWithoutTax = (await
                    _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee,
                        false, customer)).paymentPrice;
            }

            //tax
            decimal shoppingCartTax = (await GetTaxTotal(cart, usePaymentMethodAdditionalFee)).taxtotal;

            //order total
            decimal resultTemp = decimal.Zero;
            resultTemp += subtotalBase;
            if (shoppingCartShipping.HasValue)
            {
                resultTemp += shoppingCartShipping.Value;
            }
            resultTemp += paymentMethodAdditionalFeeWithoutTax;
            resultTemp += shoppingCartTax;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                resultTemp = RoundingHelper.RoundPrice(resultTemp, currency);
            }
            #region Order total discount

            var totalDiscount = await GetOrderTotalDiscount(customer, resultTemp);
            var discountAmount = totalDiscount.orderTotalDiscount;
            var appliedDiscounts = totalDiscount.appliedDiscounts;

            //sub totals with discount        
            if (resultTemp < discountAmount)
                discountAmount = resultTemp;

            //reduce subtotal
            resultTemp -= discountAmount;

            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = RoundingHelper.RoundPrice(resultTemp, currency);

            #endregion

            #region Applied gift cards

            //let's apply gift cards now (gift cards that can be used)
            var appliedGiftCards = new List<AppliedGiftCard>();
            if (!cart.IsRecurring())
            {
                //we don't apply gift cards for recurring products
                var giftCards = await GetActiveGiftCards(customer);
                if (giftCards != null)
                    foreach (var gc in giftCards)
                        if (resultTemp > decimal.Zero)
                        {
                            decimal remainingAmount = gc.GetGiftCardRemainingAmount();
                            decimal amountCanBeUsed = resultTemp > remainingAmount ?
                                remainingAmount :
                                resultTemp;

                            //reduce subtotal
                            resultTemp -= amountCanBeUsed;

                            var appliedGiftCard = new AppliedGiftCard
                            {
                                GiftCard = gc,
                                AmountCanBeUsed = amountCanBeUsed
                            };
                            appliedGiftCards.Add(appliedGiftCard);
                        }
            }

            #endregion

            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = RoundingHelper.RoundPrice(resultTemp, currency);

            if (!shoppingCartShipping.HasValue)
            {
                //we have errors
                return (null, discountAmount, appliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount);
            }

            decimal orderTotal = resultTemp;

            #region Reward points

            if (_rewardPointsSettings.Enabled)
            {
                if (!useRewardPoints.HasValue)
                    useRewardPoints = customer.GetAttributeFromEntity<bool>(SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, _storeContext.CurrentStore.Id);

                if (useRewardPoints.Value)
                {
                    int rewardPointsBalance = await _rewardPointsService.GetRewardPointsBalance(customer.Id, _storeContext.CurrentStore.Id);
                    if (CheckMinimumRewardPointsToUseRequirement(rewardPointsBalance))
                    {
                        decimal rewardPointsBalanceAmount = await ConvertRewardPointsToAmount(rewardPointsBalance);
                        if (orderTotal > decimal.Zero)
                        {
                            if (orderTotal > rewardPointsBalanceAmount)
                            {
                                redeemedRewardPoints = rewardPointsBalance;
                                redeemedRewardPointsAmount = rewardPointsBalanceAmount;
                            }
                            else
                            {
                                redeemedRewardPointsAmount = orderTotal;
                                redeemedRewardPoints = ConvertAmountToRewardPoints(redeemedRewardPointsAmount);
                            }
                        }
                    }
                }
            }

            #endregion

            orderTotal = orderTotal - redeemedRewardPointsAmount;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                orderTotal = RoundingHelper.RoundPrice(orderTotal, currency);

            return (orderTotal, discountAmount, appliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount);
        }

        /// <summary>
        /// Converts existing reward points to amount
        /// </summary>
        /// <param name="rewardPoints">Reward points</param>
        /// <returns>Converted value</returns>
        public virtual async Task<decimal> ConvertRewardPointsToAmount(int rewardPoints)
        {
            if (rewardPoints <= 0)
                return decimal.Zero;

            var result = rewardPoints * _rewardPointsSettings.ExchangeRate;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                var currency = await _currencyService.GetPrimaryExchangeRateCurrency();
                result = RoundingHelper.RoundPrice(result, currency);
            }
            return result;
        }

        /// <summary>
        /// Converts an amount to reward points
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <returns>Converted value</returns>
        public virtual int ConvertAmountToRewardPoints(decimal amount)
        {
            int result = 0;
            if (amount <= 0)
                return 0;

            if (_rewardPointsSettings.ExchangeRate > 0)
                result = (int)Math.Ceiling(amount / _rewardPointsSettings.ExchangeRate);
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether a customer has minimum amount of reward points to use (if enabled)
        /// </summary>
        /// <param name="rewardPoints">Reward points to check</param>
        /// <returns>true - reward points could use; false - cannot be used.</returns>
        public virtual bool CheckMinimumRewardPointsToUseRequirement(int rewardPoints)
        {
            if (_rewardPointsSettings.MinimumRewardPointsToUse <= 0)
                return true;

            return rewardPoints >= _rewardPointsSettings.MinimumRewardPointsToUse;
        }

        #endregion
    }
}
