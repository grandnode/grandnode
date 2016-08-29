using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Discounts;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Logging;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Tax;
using Grand.Core.Domain.Vendors;
using Grand.Services.Affiliates;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Events;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Payments;
using Grand.Services.Security;
using Grand.Services.Shipping;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using Grand.Core.Infrastructure;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Order processing service
    /// </summary>
    public partial class OrderProcessingService : IOrderProcessingService
    {
        #region Fields
        
        private readonly IOrderService _orderService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IProductService _productService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger _logger;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IGiftCardService _giftCardService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly IShippingService _shippingService;
        private readonly IShipmentService _shipmentService;
        private readonly ITaxService _taxService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IEncryptionService _encryptionService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IVendorService _vendorService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerActionEventService _customerActionEventService;
        private readonly ICurrencyService _currencyService;
        private readonly IAffiliateService _affiliateService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPdfService _pdfService;
        private readonly IRewardPointsService _rewardPointsService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly ShippingSettings _shippingSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly OrderSettings _orderSettings;
        private readonly TaxSettings _taxSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CurrencySettings _currencySettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="orderService">Order service</param>
        /// <param name="webHelper">Web helper</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="productService">Product service</param>
        /// <param name="paymentService">Payment service</param>
        /// <param name="logger">Logger</param>
        /// <param name="orderTotalCalculationService">Order total calculationservice</param>
        /// <param name="priceCalculationService">Price calculation service</param>
        /// <param name="priceFormatter">Price formatter</param>
        /// <param name="productAttributeParser">Product attribute parser</param>
        /// <param name="productAttributeFormatter">Product attribute formatter</param>
        /// <param name="giftCardService">Gift card service</param>
        /// <param name="shoppingCartService">Shopping cart service</param>
        /// <param name="checkoutAttributeFormatter">Checkout attribute service</param>
        /// <param name="shippingService">Shipping service</param>
        /// <param name="shipmentService">Shipment service</param>
        /// <param name="taxService">Tax service</param>
        /// <param name="customerService">Customer service</param>
        /// <param name="discountService">Discount service</param>
        /// <param name="encryptionService">Encryption service</param>
        /// <param name="workContext">Work context</param>
        /// <param name="workflowMessageService">Workflow message service</param>
        /// <param name="vendorService">Vendor service</param>
        /// <param name="customerActivityService">Customer activity service</param>
        /// <param name="currencyService">Currency service</param>
        /// <param name="affiliateService">Affiliate service</param>
        /// <param name="eventPublisher">Event published</param>
        /// <param name="pdfService">PDF service</param>
        /// <param name="rewardPointsService">Reward points service</param>
        /// <param name="paymentSettings">Payment settings</param>
        /// <param name="shippingSettings">Shipping settings</param>
        /// <param name="rewardPointsSettings">Reward points settings</param>
        /// <param name="orderSettings">Order settings</param>
        /// <param name="taxSettings">Tax settings</param>
        /// <param name="localizationSettings">Localization settings</param>
        /// <param name="currencySettings">Currency settings</param>
        public OrderProcessingService(IOrderService orderService,
            IWebHelper webHelper,
            ILocalizationService localizationService,
            ILanguageService languageService,
            IProductService productService,
            IPaymentService paymentService,
            ILogger logger,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            IGiftCardService giftCardService,
            IShoppingCartService shoppingCartService,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            IShippingService shippingService,
            IShipmentService shipmentService,
            ITaxService taxService,
            ICustomerService customerService,
            IDiscountService discountService,
            IEncryptionService encryptionService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            IVendorService vendorService,
            ICustomerActivityService customerActivityService,
            ICustomerActionEventService customerActionEventService,
            ICurrencyService currencyService,
            IAffiliateService affiliateService,
            IEventPublisher eventPublisher,
            IPdfService pdfService,
            IRewardPointsService rewardPointsService,
            IReturnRequestService returnRequestService,
            ShippingSettings shippingSettings,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            OrderSettings orderSettings,
            TaxSettings taxSettings,
            LocalizationSettings localizationSettings,
            CurrencySettings currencySettings)
        {
            this._orderService = orderService;
            this._webHelper = webHelper;
            this._localizationService = localizationService;
            this._languageService = languageService;
            this._productService = productService;
            this._paymentService = paymentService;
            this._logger = logger;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._productAttributeParser = productAttributeParser;
            this._productAttributeFormatter = productAttributeFormatter;
            this._giftCardService = giftCardService;
            this._shoppingCartService = shoppingCartService;
            this._checkoutAttributeFormatter = checkoutAttributeFormatter;
            this._workContext = workContext;
            this._workflowMessageService = workflowMessageService;
            this._vendorService = vendorService;
            this._shippingService = shippingService;
            this._shipmentService = shipmentService;
            this._taxService = taxService;
            this._customerService = customerService;
            this._discountService = discountService;
            this._encryptionService = encryptionService;
            this._customerActivityService = customerActivityService;
            this._customerActionEventService = customerActionEventService;
            this._currencyService = currencyService;
            this._affiliateService = affiliateService;
            this._eventPublisher = eventPublisher;
            this._pdfService = pdfService;
            this._rewardPointsService = rewardPointsService;
            this._returnRequestService = returnRequestService;
            this._paymentSettings = paymentSettings;
            this._shippingSettings = shippingSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._orderSettings = orderSettings;
            this._taxSettings = taxSettings;
            this._localizationSettings = localizationSettings;
            this._currencySettings = currencySettings;
        }

        #endregion

        #region Nested classes

        protected class PlaceOrderContainter
        {
            public PlaceOrderContainter()
            {
                this.Cart = new List<ShoppingCartItem>();
                this.AppliedDiscounts = new List<Discount>();
                this.AppliedGiftCards = new List<AppliedGiftCard>();
            }

            public Customer Customer { get; set; }
            public Language CustomerLanguage { get; set; }
            public string AffiliateId { get; set; }
            public TaxDisplayType CustomerTaxDisplayType {get; set; }
            public string CustomerCurrencyCode { get; set; }
            public decimal CustomerCurrencyRate { get; set; }

            public Address BillingAddress { get; set; }
            public Address ShippingAddress {get; set; }
            public ShippingStatus ShippingStatus { get; set; }
            public string ShippingMethodName { get; set; }
            public string ShippingRateComputationMethodSystemName { get; set; }
            public bool PickUpInStore { get; set; }
            public PickupPoint PickupPoint { get; set; }
            public bool IsRecurringShoppingCart { get; set; }
            //initial order (used with recurring payments)
            public Order InitialOrder { get; set; }

            public string CheckoutAttributeDescription { get; set; }
            public string CheckoutAttributesXml { get; set; }

            public IList<ShoppingCartItem> Cart { get; set; }
            public List<Discount> AppliedDiscounts { get; set; }
            public List<AppliedGiftCard> AppliedGiftCards { get; set; }

            public decimal OrderSubTotalInclTax { get; set; }
            public decimal OrderSubTotalExclTax { get; set; }
            public decimal OrderSubTotalDiscountInclTax { get; set; }
            public decimal OrderSubTotalDiscountExclTax { get; set; }
            public decimal OrderShippingTotalInclTax { get; set; }
            public decimal OrderShippingTotalExclTax { get; set; }
            public decimal PaymentAdditionalFeeInclTax {get; set; }
            public decimal PaymentAdditionalFeeExclTax { get; set; }
            public decimal OrderTaxTotal  {get; set; }
            public string VatNumber {get; set; }
            public string TaxRates {get; set; }
            public decimal OrderDiscountAmount { get; set; }
            public int RedeemedRewardPoints { get; set; }
            public decimal RedeemedRewardPointsAmount { get; set; }
            public decimal OrderTotal { get; set; }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare details to place an order. It also sets some properties to "processPaymentRequest"
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Details</returns>
        protected virtual PlaceOrderContainter PreparePlaceOrderDetails(ProcessPaymentRequest processPaymentRequest)
        {
            var details = new PlaceOrderContainter();

            //Recurring orders. Load initial order
            if (processPaymentRequest.IsRecurringPayment)
            {
                details.InitialOrder = _orderService.GetOrderById(processPaymentRequest.InitialOrderId);
                if (details.InitialOrder == null)
                    throw new ArgumentException("Initial order is not set for recurring payment");

                processPaymentRequest.PaymentMethodSystemName = details.InitialOrder.PaymentMethodSystemName;
            }

            //customer
            details.Customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            if (details.Customer == null)
                throw new ArgumentException("Customer is not set");

            //affiliate
            var affiliate = _affiliateService.GetAffiliateById(details.Customer.AffiliateId);
            if (affiliate != null && affiliate.Active && !affiliate.Deleted)
                details.AffiliateId = affiliate.Id;

            //customer currency
            if (!processPaymentRequest.IsRecurringPayment)
            {
                var currencyTmp = _currencyService.GetCurrencyById(details.Customer.GetAttribute<string>(SystemCustomerAttributeNames.CurrencyId, processPaymentRequest.StoreId));
                var customerCurrency = (currencyTmp != null && currencyTmp.Published) ? currencyTmp : _workContext.WorkingCurrency;
                details.CustomerCurrencyCode = customerCurrency.CurrencyCode;
                var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                details.CustomerCurrencyRate = customerCurrency.Rate / primaryStoreCurrency.Rate;
            }
            else
            {
                details.CustomerCurrencyCode = details.InitialOrder.CustomerCurrencyCode;
                details.CustomerCurrencyRate = details.InitialOrder.CurrencyRate;
            }

            //customer language
            if (!processPaymentRequest.IsRecurringPayment)
            {
                details.CustomerLanguage = _languageService.GetLanguageById(details.Customer.GetAttribute<string>(
                    SystemCustomerAttributeNames.LanguageId, processPaymentRequest.StoreId));
            }
            else
            {
                details.CustomerLanguage = _languageService.GetLanguageById(details.InitialOrder.CustomerLanguageId);
            }
            if (details.CustomerLanguage == null || !details.CustomerLanguage.Published)
                details.CustomerLanguage = _workContext.WorkingLanguage;

            //check whether customer is guest
            if (details.Customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                throw new NopException("Anonymous checkout is not allowed");

            //billing address
            if (!processPaymentRequest.IsRecurringPayment)
            {
                if (details.Customer.BillingAddress == null)
                    throw new NopException("Billing address is not provided");

                if (!CommonHelper.IsValidEmail(details.Customer.BillingAddress.Email))
                    throw new NopException("Email is not valid");

                //clone billing address
                details.BillingAddress = (Address)details.Customer.BillingAddress.Clone();
                if (!String.IsNullOrEmpty(details.BillingAddress.CountryId))
                {
                    var country = EngineContext.Current.Resolve<ICountryService>().GetCountryById(details.BillingAddress.CountryId);
                    if (country != null)
                        if (!country.AllowsBilling)
                            throw new NopException(string.Format("Country '{0}' is not allowed for billing", country.Name));
                }
            }
            else
            {
                if (details.InitialOrder.BillingAddress == null)
                    throw new NopException("Billing address is not available");

                //clone billing address
                details.BillingAddress = (Address)details.InitialOrder.BillingAddress.Clone();
                if (!String.IsNullOrEmpty(details.BillingAddress.CountryId))
                {
                    var country = EngineContext.Current.Resolve<ICountryService>().GetCountryById(details.BillingAddress.CountryId);
                    if (country != null)
                        if (!country.AllowsBilling)
                            throw new NopException(string.Format("Country '{0}' is not allowed for billing", country.Name));
                }
            }

            //checkout attributes
            if (!processPaymentRequest.IsRecurringPayment)
            {
                details.CheckoutAttributesXml = details.Customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, processPaymentRequest.StoreId);
                details.CheckoutAttributeDescription = _checkoutAttributeFormatter.FormatAttributes(details.CheckoutAttributesXml, details.Customer);
            }
            else
            {
                details.CheckoutAttributesXml = details.InitialOrder.CheckoutAttributesXml;
                details.CheckoutAttributeDescription = details.InitialOrder.CheckoutAttributeDescription;
            }

            //load and validate customer shopping cart
            if (!processPaymentRequest.IsRecurringPayment)
            {
                //load shopping cart
                details.Cart = details.Customer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(processPaymentRequest.StoreId)
                    .ToList();

                if (!details.Cart.Any())
                    throw new NopException("Cart is empty");

                //validate the entire shopping cart
                var warnings = _shoppingCartService.GetShoppingCartWarnings(details.Cart,
                    details.CheckoutAttributesXml,
                    true);
                if (warnings.Any())
                {
                    var warningsSb = new StringBuilder();
                    foreach (string warning in warnings)
                    {
                        warningsSb.Append(warning);
                        warningsSb.Append(";");
                    }
                    throw new NopException(warningsSb.ToString());
                }

                //validate individual cart items
                foreach (var sci in details.Cart)
                {
                    var product = _productService.GetProductById(sci.ProductId);
                    var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(details.Customer, sci.ShoppingCartType,
                        product, processPaymentRequest.StoreId, sci.AttributesXml,
                        sci.CustomerEnteredPrice, sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                        sci.Quantity, false);
                    if (sciWarnings.Any())
                    {
                        var warningsSb = new StringBuilder();
                        foreach (string warning in sciWarnings)
                        {
                            warningsSb.Append(warning);
                            warningsSb.Append(";");
                        }
                        throw new NopException(warningsSb.ToString());
                    }
                }
            }

            //min totals validation
            if (!processPaymentRequest.IsRecurringPayment)
            {
                bool minOrderSubtotalAmountOk = ValidateMinOrderSubtotalAmount(details.Cart);
                if (!minOrderSubtotalAmountOk)
                {
                    decimal minOrderSubtotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
                    throw new NopException(string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false)));
                }
                bool minOrderTotalAmountOk = ValidateMinOrderTotalAmount(details.Cart);
                if (!minOrderTotalAmountOk)
                {
                    decimal minOrderTotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, _workContext.WorkingCurrency);
                    throw new NopException(string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"), _priceFormatter.FormatPrice(minOrderTotalAmount, true, false)));
                }
            }

            //tax display type
            if (!processPaymentRequest.IsRecurringPayment)
            {
                if (_taxSettings.AllowCustomersToSelectTaxDisplayType)
                    details.CustomerTaxDisplayType = (TaxDisplayType)details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.TaxDisplayTypeId, processPaymentRequest.StoreId);
                else
                    details.CustomerTaxDisplayType = _taxSettings.TaxDisplayType;
            }
            else
            {
                details.CustomerTaxDisplayType = details.InitialOrder.CustomerTaxDisplayType;
            }
            
            //sub total
            if (!processPaymentRequest.IsRecurringPayment)
            {
                //sub total (incl tax)
                decimal orderSubTotalDiscountAmount;
                List<Discount> orderSubTotalAppliedDiscounts;
                decimal subTotalWithoutDiscountBase;
                decimal subTotalWithDiscountBase;
                _orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart,
                    true, out orderSubTotalDiscountAmount, out orderSubTotalAppliedDiscounts,
                    out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
                details.OrderSubTotalInclTax = subTotalWithoutDiscountBase;
                details.OrderSubTotalDiscountInclTax = orderSubTotalDiscountAmount;

                foreach (var disc in orderSubTotalAppliedDiscounts)
                    if (!details.AppliedDiscounts.ContainsDiscount(disc))
                        details.AppliedDiscounts.Add(disc);

                //sub total (excl tax)
                _orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart, false, out orderSubTotalDiscountAmount,
                out orderSubTotalAppliedDiscounts, out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
                details.OrderSubTotalExclTax = subTotalWithoutDiscountBase;
                details.OrderSubTotalDiscountExclTax = orderSubTotalDiscountAmount;
            }
            else
            {
                details.OrderSubTotalInclTax = details.InitialOrder.OrderSubtotalInclTax;
                details.OrderSubTotalExclTax = details.InitialOrder.OrderSubtotalExclTax;
            }


            //shipping info
            bool shoppingCartRequiresShipping;
            if (!processPaymentRequest.IsRecurringPayment)
            {
                shoppingCartRequiresShipping = details.Cart.RequiresShipping();
            }
            else
            {
                shoppingCartRequiresShipping = details.InitialOrder.ShippingStatus != ShippingStatus.ShippingNotRequired;
            }
            if (shoppingCartRequiresShipping)
            {
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    var pickupPoint = details.Customer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPickupPoint, processPaymentRequest.StoreId);
                    if (_shippingSettings.AllowPickUpInStore && pickupPoint != null)
                    {
                        details.PickUpInStore = true;
                        details.PickupPoint = _shippingService.GetPickupPointById(pickupPoint);
                    }
                    else
                    {
                        if (details.Customer.ShippingAddress == null)
                            throw new NopException("Shipping address is not provided");

                        if (!CommonHelper.IsValidEmail(details.Customer.ShippingAddress.Email))
                            throw new NopException("Email is not valid");

                        //clone shipping address
                        details.ShippingAddress = (Address)details.Customer.ShippingAddress.Clone();
                        if (!String.IsNullOrEmpty(details.ShippingAddress.CountryId)) 
                        {
                            var country = EngineContext.Current.Resolve<ICountryService>().GetCountryById(details.ShippingAddress.CountryId);
                            if (country != null)
                                if (!country.AllowsShipping)
                                    throw new NopException(string.Format("Country '{0}' is not allowed for shipping", country.Name));
                        }
                    }

                    var shippingOption = details.Customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, processPaymentRequest.StoreId);
                    if (shippingOption != null)
                    {
                        details.ShippingMethodName = shippingOption.Name;
                        details.ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName;
                    }
                }
                else
                {
                    var pickupPoint = details.Customer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPickupPoint, processPaymentRequest.StoreId);
                    if (_shippingSettings.AllowPickUpInStore && pickupPoint != null)
                    {
                        details.PickUpInStore = true;
                        details.PickupPoint = _shippingService.GetPickupPointById(pickupPoint);
                    }
                    else
                    {
                        if (details.InitialOrder.ShippingAddress == null)
                            throw new NopException("Shipping address is not available");

                        //clone shipping address
                        details.ShippingAddress = (Address)details.InitialOrder.ShippingAddress.Clone();
                        if (!String.IsNullOrEmpty(details.ShippingAddress.CountryId))
                        {
                            var country = EngineContext.Current.Resolve<ICountryService>().GetCountryById(details.ShippingAddress.CountryId);
                            if (country != null)
                                if (!country.AllowsShipping)
                                    throw new NopException(string.Format("Country '{0}' is not allowed for shipping", country.Name));
                        }
                    }

                    details.ShippingMethodName = details.InitialOrder.ShippingMethod;
                    details.ShippingRateComputationMethodSystemName = details.InitialOrder.ShippingRateComputationMethodSystemName;
                }
            }
            details.ShippingStatus = shoppingCartRequiresShipping
                ? ShippingStatus.NotYetShipped
                : ShippingStatus.ShippingNotRequired;

            //shipping total
            decimal tax;
            List<Discount> shippingTotalDiscounts;
            var orderShippingTotalInclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, true, out tax, out shippingTotalDiscounts);
            var orderShippingTotalExclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, false);
            if (!orderShippingTotalInclTax.HasValue || !orderShippingTotalExclTax.HasValue)
                throw new NopException("Shipping total couldn't be calculated");

            details.OrderShippingTotalInclTax = orderShippingTotalInclTax.Value;
            details.OrderShippingTotalExclTax = orderShippingTotalExclTax.Value;

            foreach (var disc in shippingTotalDiscounts)
                if (!details.AppliedDiscounts.ContainsDiscount(disc))
                    details.AppliedDiscounts.Add(disc);

            //payment total
            if (!processPaymentRequest.IsRecurringPayment)
            {
                decimal paymentAdditionalFee = _paymentService.GetAdditionalHandlingFee(details.Cart, processPaymentRequest.PaymentMethodSystemName);
                details.PaymentAdditionalFeeInclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, true, details.Customer);
                details.PaymentAdditionalFeeExclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, false, details.Customer);
            }
            else
            {
                details.PaymentAdditionalFeeInclTax = details.InitialOrder.PaymentMethodAdditionalFeeInclTax;
                details.PaymentAdditionalFeeExclTax = details.InitialOrder.PaymentMethodAdditionalFeeExclTax;
            }


            //tax total
            if (!processPaymentRequest.IsRecurringPayment)
            {
                //tax amount
                SortedDictionary<decimal, decimal> taxRatesDictionary;
                details.OrderTaxTotal = _orderTotalCalculationService.GetTaxTotal(details.Cart, out taxRatesDictionary);

                //VAT number
                var customerVatStatus = (VatNumberStatus)details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId);
                if (_taxSettings.EuVatEnabled && customerVatStatus == VatNumberStatus.Valid)
                    details.VatNumber = details.Customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);

                //tax rates
                foreach (var kvp in taxRatesDictionary)
                {
                    var taxRate = kvp.Key;
                    var taxValue = kvp.Value;
                    details.TaxRates += string.Format("{0}:{1};   ", taxRate.ToString(CultureInfo.InvariantCulture), taxValue.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                details.OrderTaxTotal = details.InitialOrder.OrderTax;
                //VAT number
                details.VatNumber = details.InitialOrder.VatNumber;
            }


            //order total (and applied discounts, gift cards, reward points)
            List<AppliedGiftCard> appliedGiftCards;
            List<Discount> orderAppliedDiscounts;
            decimal orderDiscountAmount;
            int redeemedRewardPoints;
            decimal redeemedRewardPointsAmount;
            var orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(details.Cart, out orderDiscountAmount,
                out orderAppliedDiscounts, out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount);
            if (!orderTotal.HasValue)
                throw new NopException("Order total couldn't be calculated");

            details.OrderDiscountAmount = orderDiscountAmount;
            details.RedeemedRewardPoints = redeemedRewardPoints;
            details.RedeemedRewardPointsAmount = redeemedRewardPointsAmount;
            details.AppliedGiftCards = appliedGiftCards;
            details.OrderTotal = orderTotal.Value;

            //discount history
            foreach (var disc in orderAppliedDiscounts)
                if (!details.AppliedDiscounts.ContainsDiscount(disc))
                    details.AppliedDiscounts.Add(disc);

            processPaymentRequest.OrderTotal = details.OrderTotal;


            //recurring or standard shopping cart?
            if (!processPaymentRequest.IsRecurringPayment)
            {
                details.IsRecurringShoppingCart = details.Cart.IsRecurring();
                if (details.IsRecurringShoppingCart)
                {
                    int recurringCycleLength;
                    RecurringProductCyclePeriod recurringCyclePeriod;
                    int recurringTotalCycles;
                    string recurringCyclesError = details.Cart.GetRecurringCycleInfo(_localizationService, _productService,
                        out recurringCycleLength, out recurringCyclePeriod, out recurringTotalCycles);
                    if (!string.IsNullOrEmpty(recurringCyclesError))
                        throw new NopException(recurringCyclesError);

                    processPaymentRequest.RecurringCycleLength = recurringCycleLength;
                    processPaymentRequest.RecurringCyclePeriod = recurringCyclePeriod;
                    processPaymentRequest.RecurringTotalCycles = recurringTotalCycles;
                }
            }
            else
            {
                details.IsRecurringShoppingCart = true;
            }

            return details;
        }

        /// <summary>
        /// Award reward points
        /// </summary>
        /// <param name="order">Order</param>
        protected virtual void AwardRewardPoints(Order order)
        {
            var customer = _customerService.GetCustomerById(order.CustomerId);

            int points = _orderTotalCalculationService.CalculateRewardPoints(customer, order.OrderTotal);
            if (points == 0)
                return;

            //Ensure that reward points were not added before. We should not add reward points if they were already earned for this order
            if (order.RewardPointsWereAdded)
                return;

            //add reward points
            _rewardPointsService.AddRewardPointsHistory(customer.Id, points, order.StoreId, string.Format(_localizationService.GetResource("RewardPoints.Message.EarnedForOrder"), order.OrderNumber));
            //order.RewardPointsWereAdded = true;
            //order.RedeemedRewardPointsEntry = rph;
            //_orderService.UpdateOrder(order);
        }

        /// <summary>
        /// Award reward points
        /// </summary>
        /// <param name="order">Order</param>
        protected virtual void ReduceRewardPoints(Order order)
        {
            var customer = _customerService.GetCustomerById(order.CustomerId);
            int points = _orderTotalCalculationService.CalculateRewardPoints(customer, order.OrderTotal);
            if (points == 0)
                return;

            //ensure that reward points were already earned for this order before
            if (!order.RewardPointsWereAdded)
                return;

            //reduce reward points
            _rewardPointsService.AddRewardPointsHistory(customer.Id, -points, order.StoreId,
                string.Format(_localizationService.GetResource("RewardPoints.Message.ReducedForOrder"), order.OrderNumber));
            _orderService.UpdateOrder(order);

        }

        /// <summary>
        /// Return back redeemded reward points to a customer (spent when placing an order)
        /// </summary>
        /// <param name="order">Order</param>
        protected virtual void ReturnBackRedeemedRewardPoints(Order order)
        {
            //were some points redeemed when placing an order?
            if (order.RedeemedRewardPointsEntry == null)
                return;

            //return back
            _rewardPointsService.AddRewardPointsHistory(order.CustomerId, -order.RedeemedRewardPointsEntry.Points, order.StoreId,
                string.Format(_localizationService.GetResource("RewardPoints.Message.ReturnedForOrder"), order.OrderNumber));
            _orderService.UpdateOrder(order);
        }

        /// <summary>
        /// Set IsActivated value for purchase gift cards for particular order
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="activate">A value indicating whether to activate gift cards; true - activate, false - deactivate</param>
        protected virtual void SetActivatedValueForPurchasedGiftCards(Order order, bool activate)
        {
            var giftCards = _giftCardService.GetAllGiftCards(purchasedWithOrderId: order.Id, 
                isGiftCardActivated: !activate);
            foreach (var gc in giftCards)
            {
                if (activate)
                {
                    //activate
                    bool isRecipientNotified = gc.IsRecipientNotified;
                    if (gc.GiftCardType == GiftCardType.Virtual)
                    {
                        //send email for virtual gift card
                        if (!String.IsNullOrEmpty(gc.RecipientEmail) &&
                            !String.IsNullOrEmpty(gc.SenderEmail))
                        {
                            var customerLang = _languageService.GetLanguageById(order.CustomerLanguageId);
                            if (customerLang == null)
                                customerLang = _languageService.GetAllLanguages().FirstOrDefault();
                            if (customerLang == null)
                                throw new Exception("No languages could be loaded");
                            int queuedEmailId = _workflowMessageService.SendGiftCardNotification(gc, customerLang.Id);
                            if (queuedEmailId > 0)
                                isRecipientNotified = true;
                        }
                    }
                    gc.IsGiftCardActivated = true;
                    gc.IsRecipientNotified = isRecipientNotified;
                    _giftCardService.UpdateGiftCard(gc);
                }
                else
                {
                    //deactivate
                    gc.IsGiftCardActivated = false;
                    _giftCardService.UpdateGiftCard(gc);
                }
            }
        }

        /// <summary>
        /// Sets an order status
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="os">New order status</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        protected virtual void SetOrderStatus(Order order, OrderStatus os, bool notifyCustomer)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            OrderStatus prevOrderStatus = order.OrderStatus;
            if (prevOrderStatus == os)
                return;

            //set and save new order status
            order.OrderStatusId = (int)os;
            _orderService.UpdateOrder(order);

            //order notes, notifications
            _orderService.InsertOrderNote(new OrderNote
                {
                    Note = string.Format("Order status has been changed to {0}", os.ToString()),
                    DisplayToCustomer = false,
                    OrderId = order.Id,
                    CreatedOnUtc = DateTime.UtcNow
                });

            if (prevOrderStatus != OrderStatus.Complete &&
                os == OrderStatus.Complete
                && notifyCustomer)
            {
                //notification
                var orderCompletedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderCompletedEmail ?
                    _pdfService.PrintOrderToPdf(order, "") : null;
                var orderCompletedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderCompletedEmail ?
                    "order.pdf" : null;
                int orderCompletedCustomerNotificationQueuedEmailId = _workflowMessageService
                    .SendOrderCompletedCustomerNotification(order, order.CustomerLanguageId, orderCompletedAttachmentFilePath,
                    orderCompletedAttachmentFileName);
                if (orderCompletedCustomerNotificationQueuedEmailId > 0)
                {
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        Note = string.Format("\"Order completed\" email (to customer) has been queued. Queued email identifier: {0}.", orderCompletedCustomerNotificationQueuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                    });
                }
            }

            if (prevOrderStatus != OrderStatus.Cancelled &&
                os == OrderStatus.Cancelled
                && notifyCustomer)
            {
                //notification
                int orderCancelledCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderCancelledCustomerNotification(order, order.CustomerLanguageId);
                if (orderCancelledCustomerNotificationQueuedEmailId > 0)
                {
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        Note = string.Format("\"Order cancelled\" email (to customer) has been queued. Queued email identifier: {0}.", orderCancelledCustomerNotificationQueuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                    });
                }
            }

            //reward points
            if (_rewardPointsSettings.PointsForPurchases_Awarded == order.OrderStatus)
            {
                AwardRewardPoints(order);
            }
            if (_rewardPointsSettings.PointsForPurchases_Canceled == order.OrderStatus)
            {
                ReduceRewardPoints(order);
            }

            //gift cards activation
            if (_orderSettings.GiftCards_Activated_OrderStatusId > 0 &&
               _orderSettings.GiftCards_Activated_OrderStatusId == (int)order.OrderStatus)
            {
                SetActivatedValueForPurchasedGiftCards(order, true);
            }

            //gift cards deactivation
            if (_orderSettings.GiftCards_Deactivated_OrderStatusId > 0 &&
               _orderSettings.GiftCards_Deactivated_OrderStatusId == (int)order.OrderStatus)
            {
                SetActivatedValueForPurchasedGiftCards(order, false);
            }
        }

        /// <summary>
        /// Process order paid status
        /// </summary>
        /// <param name="order">Order</param>
        protected virtual void ProcessOrderPaid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //raise event
            _eventPublisher.Publish(new OrderPaidEvent(order));

            //order paid email notification
            if (order.OrderTotal != decimal.Zero)
            {
                //we should not send it for free ($0 total) orders?
                //remove this "if" statement if you want to send it in this case

                var orderPaidAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPaidEmail ?
                    _pdfService.PrintOrderToPdf(order, "") : null;
                var orderPaidAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPaidEmail ?
                    "order.pdf" : null;
                _workflowMessageService.SendOrderPaidCustomerNotification(order, order.CustomerLanguageId,
                    orderPaidAttachmentFilePath, orderPaidAttachmentFileName);

                _workflowMessageService.SendOrderPaidStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
                var vendors = GetVendorsInOrder(order);
                foreach (var vendor in vendors)
                {
                    _workflowMessageService.SendOrderPaidVendorNotification(order, vendor, _localizationSettings.DefaultAdminLanguageId);
                }
                //TODO add "order paid email sent" order note
            }

            //customer roles with "purchased with product" specified
            ProcessCustomerRolesWithPurchasedProductSpecified(order, true);
        }

        /// <summary>
        /// Process customer roles with "Purchased with Product" property configured
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="add">A value indicating whether to add configured customer role; true - add, false - remove</param>
        protected virtual void ProcessCustomerRolesWithPurchasedProductSpecified(Order order, bool add)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //purchased product identifiers
            var purchasedProductIds = new List<string>();
            foreach (var orderItem in order.OrderItems)
            {
                //standard items
                purchasedProductIds.Add(orderItem.ProductId);

                //bundled (associated) products
                var product = _productService.GetProductById(orderItem.ProductId);
                var attributeValues = _productAttributeParser.ParseProductAttributeValues(product, orderItem.AttributesXml);
                foreach (var attributeValue in attributeValues)
                {
                    if (attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                    {
                       purchasedProductIds.Add(attributeValue.AssociatedProductId);
                    }
                }
            }

            //list of customer roles
            var customerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Where(cr => purchasedProductIds.Contains(cr.PurchasedWithProductId))
                .ToList();

            if (customerRoles.Any())
            {
                var customer = _customerService.GetCustomerById(order.CustomerId);
                foreach (var customerRole in customerRoles)
                {
                    if (customer.CustomerRoles.Count(cr => cr.Id == customerRole.Id) == 0)
                    {
                        //not in the list yet
                        if (add)
                        {
                            //add
                            customerRole.CustomerId = customer.Id;
                            customer.CustomerRoles.Add(customerRole);
                            _customerService.InsertCustomerRoleInCustomer(customerRole);
                        }
                    }
                    else
                    {
                        //already in the list
                        if (!add)
                        {
                            //remove
                            customer.CustomerRoles.Remove(customerRole);
                            customerRole.CustomerId = customer.Id;
                            _customerService.InsertCustomerRoleInCustomer(customerRole);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get a list of vendors in order (order items)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Vendors</returns>
        protected virtual IList<Vendor> GetVendorsInOrder(Order order)
        {
            var vendors = new List<Vendor>();
            foreach (var orderItem in order.OrderItems)
            {
                //find existing
                var vendor = vendors.FirstOrDefault(v => v.Id == orderItem.VendorId);
                if (vendor == null)
                {
                    //not found. load by Id
                    vendor = _vendorService.GetVendorById(orderItem.VendorId);
                    if (vendor != null && !vendor.Deleted && vendor.Active)
                    {
                        vendors.Add(vendor);
                    }
                }
            }

            return vendors;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks order status
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Validated order</returns>
        public virtual void CheckOrderStatus(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.PaymentStatus == PaymentStatus.Paid && !order.PaidDateUtc.HasValue)
            {
                //ensure that paid date is set
                order.PaidDateUtc = DateTime.UtcNow;
                _orderService.UpdateOrder(order);
            }

            if (order.OrderStatus == OrderStatus.Pending)
            {
                if (order.PaymentStatus == PaymentStatus.Authorized ||
                    order.PaymentStatus == PaymentStatus.Paid)
                {
                    SetOrderStatus(order, OrderStatus.Processing, false);
                }
            }

            if (order.OrderStatus == OrderStatus.Pending)
            {
                if (order.ShippingStatus == ShippingStatus.PartiallyShipped ||
                    order.ShippingStatus == ShippingStatus.Shipped ||
                    order.ShippingStatus == ShippingStatus.Delivered)
                {
                    SetOrderStatus(order, OrderStatus.Processing, false);
                }
            }

            if (order.OrderStatus != OrderStatus.Cancelled &&
                order.OrderStatus != OrderStatus.Complete)
            {
                if (order.PaymentStatus == PaymentStatus.Paid)
                {
                    if (order.ShippingStatus == ShippingStatus.ShippingNotRequired || order.ShippingStatus == ShippingStatus.Delivered)
                    {
                        SetOrderStatus(order, OrderStatus.Complete, true);
                    }
                }
            }
        }

        /// <summary>
        /// Places an order
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Place order result</returns>
        public virtual PlaceOrderResult PlaceOrder(ProcessPaymentRequest processPaymentRequest)
        {
            //think about moving functionality of processing recurring orders (after the initial order was placed) to ProcessNextRecurringPayment() method
            if (processPaymentRequest == null)
                throw new ArgumentNullException("processPaymentRequest");

            var result = new PlaceOrderResult();
            try
            {
                if (processPaymentRequest.OrderGuid == Guid.Empty)
                    processPaymentRequest.OrderGuid = Guid.NewGuid();

                //prepare order details
                var details = PreparePlaceOrderDetails(processPaymentRequest);
                    
                #region Payment workflow


                //process payment
                ProcessPaymentResult processPaymentResult = null;
                //skip payment workflow if order total equals zero
                var skipPaymentWorkflow = details.OrderTotal == decimal.Zero;
                if (!skipPaymentWorkflow)
                {
                    var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);
                    if (paymentMethod == null)
                        throw new NopException("Payment method couldn't be loaded");

                    //ensure that payment method is active
                    if (!paymentMethod.IsPaymentMethodActive(_paymentSettings))
                        throw new NopException("Payment method is not active");

                    if (!processPaymentRequest.IsRecurringPayment)
                    {
                        if (details.IsRecurringShoppingCart)
                        {
                            //recurring cart
                            var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                            switch (recurringPaymentType)
                            {
                                case RecurringPaymentType.NotSupported:
                                    throw new NopException("Recurring payments are not supported by selected payment method");
                                case RecurringPaymentType.Manual:
                                case RecurringPaymentType.Automatic:
                                    processPaymentResult = _paymentService.ProcessRecurringPayment(processPaymentRequest);
                                    break;
                                default:
                                    throw new NopException("Not supported recurring payment type");
                            }
                        }
                        else
                        {
                            //standard cart
                            processPaymentResult = _paymentService.ProcessPayment(processPaymentRequest);
                        }
                    }
                    else
                    {
                        if (details.IsRecurringShoppingCart)
                        {
                            //Old credit card info
                            processPaymentRequest.CreditCardType = details.InitialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(details.InitialOrder.CardType) : "";
                            processPaymentRequest.CreditCardName = details.InitialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(details.InitialOrder.CardName) : "";
                            processPaymentRequest.CreditCardNumber = details.InitialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(details.InitialOrder.CardNumber) : "";
                            processPaymentRequest.CreditCardCvv2 = details.InitialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(details.InitialOrder.CardCvv2) : "";
                            try
                            {
                                processPaymentRequest.CreditCardExpireMonth = details.InitialOrder.AllowStoringCreditCardNumber ? Convert.ToInt32(_encryptionService.DecryptText(details.InitialOrder.CardExpirationMonth)) : 0;
                                processPaymentRequest.CreditCardExpireYear = details.InitialOrder.AllowStoringCreditCardNumber ? Convert.ToInt32(_encryptionService.DecryptText(details.InitialOrder.CardExpirationYear)) : 0;
                            }
                            catch {}

                            var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                            switch (recurringPaymentType)
                            {
                                case RecurringPaymentType.NotSupported:
                                    throw new NopException("Recurring payments are not supported by selected payment method");
                                case RecurringPaymentType.Manual:
                                    processPaymentResult = _paymentService.ProcessRecurringPayment(processPaymentRequest);
                                    break;
                                case RecurringPaymentType.Automatic:
                                    //payment is processed on payment gateway site
                                    processPaymentResult = new ProcessPaymentResult();
                                    break;
                                default:
                                    throw new NopException("Not supported recurring payment type");
                            }
                        }
                        else
                        {
                            throw new NopException("No recurring products");
                        }
                    }
                }
                else
                {
                    //payment is not required
                    if (processPaymentResult == null)
                        processPaymentResult = new ProcessPaymentResult();
                    processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;
                }

                if (processPaymentResult == null)
                    throw new NopException("processPaymentResult is not available");

                #endregion

                if (processPaymentResult.Success)
                {
                    #region Save order details

                    var order = new Order
                    {
                        StoreId = processPaymentRequest.StoreId,
                        OrderGuid = processPaymentRequest.OrderGuid,
                        CustomerId = details.Customer.Id,
                        CustomerLanguageId = details.CustomerLanguage.Id,
                        CustomerTaxDisplayType = details.CustomerTaxDisplayType,
                        CustomerIp = _webHelper.GetCurrentIpAddress(),
                        OrderSubtotalInclTax = details.OrderSubTotalInclTax,
                        OrderSubtotalExclTax = details.OrderSubTotalExclTax,
                        OrderSubTotalDiscountInclTax = details.OrderSubTotalDiscountInclTax,
                        OrderSubTotalDiscountExclTax = details.OrderSubTotalDiscountExclTax,
                        OrderShippingInclTax = details.OrderShippingTotalInclTax,
                        OrderShippingExclTax = details.OrderShippingTotalExclTax,
                        PaymentMethodAdditionalFeeInclTax = details.PaymentAdditionalFeeInclTax,
                        PaymentMethodAdditionalFeeExclTax = details.PaymentAdditionalFeeExclTax,
                        TaxRates = details.TaxRates,
                        OrderTax = details.OrderTaxTotal,
                        OrderTotal = details.OrderTotal,
                        RefundedAmount = decimal.Zero,
                        OrderDiscount = details.OrderDiscountAmount,
                        CheckoutAttributeDescription = details.CheckoutAttributeDescription,
                        CheckoutAttributesXml = details.CheckoutAttributesXml,
                        CustomerCurrencyCode = details.CustomerCurrencyCode,
                        CurrencyRate = details.CustomerCurrencyRate,
                        AffiliateId = details.AffiliateId,
                        OrderStatus = OrderStatus.Pending,
                        AllowStoringCreditCardNumber = processPaymentResult.AllowStoringCreditCardNumber,
                        CardType = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardType) : string.Empty,
                        CardName = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardName) : string.Empty,
                        CardNumber = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardNumber) : string.Empty,
                        MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(processPaymentRequest.CreditCardNumber)),
                        CardCvv2 = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardCvv2) : string.Empty,
                        CardExpirationMonth = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireMonth.ToString()) : string.Empty,
                        CardExpirationYear = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireYear.ToString()) : string.Empty,
                        PaymentMethodSystemName = processPaymentRequest.PaymentMethodSystemName,
                        AuthorizationTransactionId = processPaymentResult.AuthorizationTransactionId,
                        AuthorizationTransactionCode = processPaymentResult.AuthorizationTransactionCode,
                        AuthorizationTransactionResult = processPaymentResult.AuthorizationTransactionResult,
                        CaptureTransactionId = processPaymentResult.CaptureTransactionId,
                        CaptureTransactionResult = processPaymentResult.CaptureTransactionResult,
                        SubscriptionTransactionId = processPaymentResult.SubscriptionTransactionId,
                        PaymentStatus = processPaymentResult.NewPaymentStatus,
                        PaidDateUtc = null,
                        BillingAddress = details.BillingAddress,
                        ShippingAddress = details.ShippingAddress,
                        ShippingStatus = details.ShippingStatus,
                        ShippingMethod = details.ShippingMethodName,
                        PickUpInStore = details.PickUpInStore,
                        PickupPoint = details.PickupPoint,
                        ShippingRateComputationMethodSystemName = details.ShippingRateComputationMethodSystemName,
                        CustomValuesXml = processPaymentRequest.SerializeCustomValues(),
                        VatNumber = details.VatNumber,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    _orderService.InsertOrder(order);
                    result.PlacedOrder = order;

                    if (!processPaymentRequest.IsRecurringPayment)
                    {
                        //move shopping cart items to order items
                        foreach (var sc in details.Cart)
                        {
                            //prices
                            decimal taxRate;
                            List<Discount> scDiscounts;
                            decimal discountAmount;
                            decimal scUnitPrice = _priceCalculationService.GetUnitPrice(sc);
                            var product = _productService.GetProductById(sc.ProductId);
                            decimal scSubTotal = _priceCalculationService.GetSubTotal(sc, true, out discountAmount, out scDiscounts);

                            var prices = _taxService.GetTaxProductPrice(product, details.Customer, out taxRate, scUnitPrice, scSubTotal, discountAmount, _taxSettings.PricesIncludeTax);
                            decimal scUnitPriceInclTax = prices.UnitPriceInclTax;
                            decimal scUnitPriceExclTax = prices.UnitPriceExclTax;
                            decimal scSubTotalInclTax = prices.SubTotalInclTax;
                            decimal scSubTotalExclTax = prices.SubTotalExclTax;
                            decimal discountAmountInclTax = prices.discountAmountInclTax;
                            decimal discountAmountExclTax = prices.discountAmountExclTax;

                            /* old method
                            decimal scUnitPriceInclTax = _taxService.GetProductPrice(product, scUnitPrice, true, details.Customer, out taxRate);
                            decimal scUnitPriceExclTax = _taxService.GetProductPrice(product, scUnitPrice, false, details.Customer, out taxRate);
                            decimal scSubTotalInclTax = _taxService.GetProductPrice(product, scSubTotal, true, details.Customer, out taxRate);
                            decimal scSubTotalExclTax = _taxService.GetProductPrice(product, scSubTotal, false, details.Customer, out taxRate);
                            decimal discountAmountInclTax = _taxService.GetProductPrice(product, discountAmount, true, details.Customer, out taxRate);
                            decimal discountAmountExclTax = _taxService.GetProductPrice(product, discountAmount, false, details.Customer, out taxRate);
                            */

                            foreach (var disc in scDiscounts)
                                if (!details.AppliedDiscounts.ContainsDiscount(disc))
                                    details.AppliedDiscounts.Add(disc);

                            //attributes
                            string attributeDescription = _productAttributeFormatter.FormatAttributes(product, sc.AttributesXml, details.Customer);

                            var itemWeight = _shippingService.GetShoppingCartItemWeight(sc);

                            //save order item
                            var orderItem = new OrderItem
                            {
                                OrderItemGuid = Guid.NewGuid(),
                                ProductId = sc.ProductId,
                                VendorId = product.VendorId,
                                UnitPriceInclTax = scUnitPriceInclTax,
                                UnitPriceExclTax = scUnitPriceExclTax,
                                PriceInclTax = scSubTotalInclTax,
                                PriceExclTax = scSubTotalExclTax,
                                OriginalProductCost = _priceCalculationService.GetProductCost(product, sc.AttributesXml),
                                AttributeDescription = attributeDescription,
                                AttributesXml = sc.AttributesXml,
                                Quantity = sc.Quantity,
                                DiscountAmountInclTax = discountAmountInclTax,
                                DiscountAmountExclTax = discountAmountExclTax,
                                DownloadCount = 0,
                                IsDownloadActivated = false,
                                LicenseDownloadId = "",
                                ItemWeight = itemWeight,
                                RentalStartDateUtc = sc.RentalStartDateUtc,
                                RentalEndDateUtc = sc.RentalEndDateUtc,
                                CreatedOnUtc = DateTime.UtcNow,
                            };
                            order.OrderItems.Add(orderItem);
                            _orderService.UpdateOrder(order);
                            _productService.UpdateSold(sc.ProductId, sc.Quantity);

                            //gift cards
                            if (product.IsGiftCard)
                            {
                                string giftCardRecipientName, giftCardRecipientEmail,
                                    giftCardSenderName, giftCardSenderEmail, giftCardMessage;
                                _productAttributeParser.GetGiftCardAttribute(sc.AttributesXml,
                                    out giftCardRecipientName, out giftCardRecipientEmail,
                                    out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                                for (int i = 0; i < sc.Quantity; i++)
                                {
                                    var gc = new GiftCard
                                    {
                                        GiftCardType = product.GiftCardType,
                                        PurchasedWithOrderItem = orderItem,
                                        Amount = product.OverriddenGiftCardAmount.HasValue ? product.OverriddenGiftCardAmount.Value : scUnitPriceExclTax,
                                        IsGiftCardActivated = false,
                                        GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                                        RecipientName = giftCardRecipientName,
                                        RecipientEmail = giftCardRecipientEmail,
                                        SenderName = giftCardSenderName,
                                        SenderEmail = giftCardSenderEmail,
                                        Message = giftCardMessage,
                                        IsRecipientNotified = false,
                                        CreatedOnUtc = DateTime.UtcNow
                                    };
                                    _giftCardService.InsertGiftCard(gc);
                                }
                            }

                            //inventory
                            _productService.AdjustInventory(product, -sc.Quantity, sc.AttributesXml);
                        }

                        //clear shopping cart
                        _customerService.ClearShoppingCartItem(details.Customer.Id, processPaymentRequest.StoreId, ShoppingCartType.ShoppingCart);
                        //product also purchased
                        _orderService.InsertProductAlsoPurchased(order);

                        if(!details.Customer.IsHasOrders)
                        {
                            details.Customer.IsHasOrders = true;
                            _customerService.UpdateHasOrders(details.Customer.Id);
                        }

                    }
                    else
                    {
                        //recurring payment
                        var initialOrderItems = details.InitialOrder.OrderItems;
                        foreach (var orderItem in initialOrderItems)
                        {
                            //save item
                            var newOrderItem = new OrderItem
                            {

                                OrderItemGuid = Guid.NewGuid(),
                                ProductId = orderItem.ProductId,
                                VendorId = orderItem.VendorId,
                                UnitPriceInclTax = orderItem.UnitPriceInclTax,
                                UnitPriceExclTax = orderItem.UnitPriceExclTax,
                                PriceInclTax = orderItem.PriceInclTax,
                                PriceExclTax = orderItem.PriceExclTax,
                                OriginalProductCost = orderItem.OriginalProductCost,
                                AttributeDescription = orderItem.AttributeDescription,
                                AttributesXml = orderItem.AttributesXml,
                                Quantity = orderItem.Quantity,
                                DiscountAmountInclTax = orderItem.DiscountAmountInclTax,
                                DiscountAmountExclTax = orderItem.DiscountAmountExclTax,
                                DownloadCount = 0,
                                IsDownloadActivated = false,
                                LicenseDownloadId = "",
                                ItemWeight = orderItem.ItemWeight,
                                RentalStartDateUtc = orderItem.RentalStartDateUtc,
                                RentalEndDateUtc = orderItem.RentalEndDateUtc,
                                CreatedOnUtc = DateTime.UtcNow,
                            };
                            order.OrderItems.Add(newOrderItem);
                            _orderService.UpdateOrder(order);

                            //gift cards
                            var product = _productService.GetProductById(orderItem.ProductId);
                            if (product.IsGiftCard)
                            {
                                string giftCardRecipientName, giftCardRecipientEmail,
                                    giftCardSenderName, giftCardSenderEmail, giftCardMessage;
                                _productAttributeParser.GetGiftCardAttribute(orderItem.AttributesXml,
                                    out giftCardRecipientName, out giftCardRecipientEmail,
                                    out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                                for (int i = 0; i < orderItem.Quantity; i++)
                                {
                                    var gc = new GiftCard
                                    {
                                        GiftCardType = product.GiftCardType,
                                        PurchasedWithOrderItem = newOrderItem,
                                        Amount = orderItem.UnitPriceExclTax,
                                        IsGiftCardActivated = false,
                                        GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                                        RecipientName = giftCardRecipientName,
                                        RecipientEmail = giftCardRecipientEmail,
                                        SenderName = giftCardSenderName,
                                        SenderEmail = giftCardSenderEmail,
                                        Message = giftCardMessage,
                                        IsRecipientNotified = false,
                                        CreatedOnUtc = DateTime.UtcNow
                                    };
                                    _giftCardService.InsertGiftCard(gc);
                                }
                            }

                            //inventory
                            _productService.AdjustInventory(product, -orderItem.Quantity, orderItem.AttributesXml);
                        }
                    }

                    //discount usage history
                    if (!processPaymentRequest.IsRecurringPayment)
                        foreach (var discount in details.AppliedDiscounts)
                        {
                            var duh = new DiscountUsageHistory
                            {
                                DiscountId = discount.Id,
                                OrderId = order.Id,
                                CustomerId = order.CustomerId,
                                CreatedOnUtc = DateTime.UtcNow
                            };
                            _discountService.InsertDiscountUsageHistory(duh);
                        }

                    //gift card usage history
                    if (!processPaymentRequest.IsRecurringPayment)
                        if (details.AppliedGiftCards != null)
                            foreach (var agc in details.AppliedGiftCards)
                            {
                                decimal amountUsed = agc.AmountCanBeUsed;
                                var gcuh = new GiftCardUsageHistory
                                {
                                    GiftCardId = agc.GiftCard.Id,
                                    UsedWithOrderId = order.Id,
                                    UsedValue = amountUsed,
                                    CreatedOnUtc = DateTime.UtcNow
                                };
                                agc.GiftCard.GiftCardUsageHistory.Add(gcuh);
                                _giftCardService.UpdateGiftCard(agc.GiftCard);
                            }

                    //reward points history
                    if (details.RedeemedRewardPointsAmount > decimal.Zero)
                    {

                        var rph = _rewardPointsService.AddRewardPointsHistory(details.Customer.Id,
                            -details.RedeemedRewardPoints, order.StoreId,
                            string.Format(_localizationService.GetResource("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId), order.OrderNumber),
                            order.Id, details.RedeemedRewardPointsAmount);
                        order.RewardPointsWereAdded = true;
                        order.RedeemedRewardPointsEntry = rph;
                        _orderService.UpdateOrder(order);
                    }

                    //recurring orders
                    if (!processPaymentRequest.IsRecurringPayment && details.IsRecurringShoppingCart)
                    {
                        //create recurring payment (the first payment)
                        var rp = new RecurringPayment
                        {
                            CycleLength = processPaymentRequest.RecurringCycleLength,
                            CyclePeriod = processPaymentRequest.RecurringCyclePeriod,
                            TotalCycles = processPaymentRequest.RecurringTotalCycles,
                            StartDateUtc = DateTime.UtcNow,
                            IsActive = true,
                            CreatedOnUtc = DateTime.UtcNow,
                            InitialOrder = order,
                        };
                        _orderService.InsertRecurringPayment(rp);


                        var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                        switch (recurringPaymentType)
                        {
                            case RecurringPaymentType.NotSupported:
                                {
                                    //not supported
                                }
                                break;
                            case RecurringPaymentType.Manual:
                                {
                                    //first payment
                                    var rph = new RecurringPaymentHistory
                                    {
                                        RecurringPayment = rp,
                                        CreatedOnUtc = DateTime.UtcNow,
                                        OrderId = order.Id,
                                    };
                                    rp.RecurringPaymentHistory.Add(rph);
                                    _orderService.UpdateRecurringPayment(rp);
                                }
                                break;
                            case RecurringPaymentType.Automatic:
                                {
                                    //will be created later (process is automated)
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    #endregion

                    #region Notifications & notes
                    Task.Run(() =>
                    {

                        //notes, messages
                        if (_workContext.OriginalCustomerIfImpersonated != null)
                        {
                            //this order is placed by a store administrator impersonating a customer
                            _orderService.InsertOrderNote(new OrderNote
                            {
                                Note = string.Format("Order placed by a store owner ('{0}'. ID = {1}) impersonating the customer.",
                                    _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id),
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow,
                                OrderId = order.Id,
                            });
                        }
                        else
                        {
                            _orderService.InsertOrderNote(new OrderNote
                            {
                                Note = "Order placed",
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow,
                                OrderId = order.Id,

                            });
                        }


                        //send email notifications
                        int orderPlacedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
                        if (orderPlacedStoreOwnerNotificationQueuedEmailId > 0)
                        {
                            _orderService.InsertOrderNote(new OrderNote
                            {
                                Note = "\"Order placed\" email (to store owner) has been queued",
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow,
                                OrderId = order.Id,

                            });
                        }

                        var orderPlacedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
                            _pdfService.PrintOrderToPdf(order, order.CustomerLanguageId) : null;
                        var orderPlacedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
                            "order.pdf" : null;
                        int orderPlacedCustomerNotificationQueuedEmailId = _workflowMessageService
                            .SendOrderPlacedCustomerNotification(order, order.CustomerLanguageId, orderPlacedAttachmentFilePath, orderPlacedAttachmentFileName);
                        if (orderPlacedCustomerNotificationQueuedEmailId > 0)
                        {
                            _orderService.InsertOrderNote(new OrderNote
                            {
                                Note = "\"Order placed\" email (to customer) has been queued",
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow,
                                OrderId = order.Id,

                            });
                        }

                        var vendors = GetVendorsInOrder(order);
                        foreach (var vendor in vendors)
                        {
                            int orderPlacedVendorNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedVendorNotification(order, vendor, order.CustomerLanguageId);
                            if (orderPlacedVendorNotificationQueuedEmailId > 0)
                            {
                                _orderService.InsertOrderNote(new OrderNote
                                {
                                    Note = "\"Order placed\" email (to vendor) has been queued",
                                    DisplayToCustomer = false,
                                    CreatedOnUtc = DateTime.UtcNow,
                                    OrderId = order.Id,
                                });
                            }
                        }

                        //check order status
                        CheckOrderStatus(order);

                        //reset checkout data
                        if (!processPaymentRequest.IsRecurringPayment)
                            _customerService.ResetCheckoutData(details.Customer, processPaymentRequest.StoreId, clearCouponCodes: true, clearCheckoutAttributes: true);

                        if (!processPaymentRequest.IsRecurringPayment)
                        {
                            _customerActivityService.InsertActivity(
                                "PublicStore.PlaceOrder", "",
                                _localizationService.GetResource("ActivityLog.PublicStore.PlaceOrder"),
                                order.Id);
                        }

                        //Updated field "free shipping" after added a new order
                        _customerService.UpdateFreeShipping(order.CustomerId, false);

                        //Update customer reminder history
                        _customerService.UpdateCustomerReminderHistory(order.CustomerId, order.Id);

                        //Update field Last purchase date after added a new order
                        _customerService.UpdateCustomerLastPurchaseDate(order.CustomerId, order.CreatedOnUtc);

                        //Update field last update cart
                        _customerService.UpdateCustomerLastUpdateCartDate(order.CustomerId, null);
                        //raise event       
                        _eventPublisher.Publish(new OrderPlacedEvent(order));
                        _customerActionEventService.AddOrder(order, _workContext.CurrentCustomer);
                        if (order.PaymentStatus == PaymentStatus.Paid)
                        {
                            ProcessOrderPaid(order);
                        }
                    });
                    #endregion
                }
                else
                {
                    foreach (var paymentError in processPaymentResult.Errors)
                        result.AddError(string.Format(_localizationService.GetResource("Checkout.PaymentError"), paymentError));
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc.Message, exc);
                result.AddError(exc.Message);
            }

            #region Process errors

            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i + 1, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //log it
                string logError = string.Format("Error while placing order. {0}", error);
                var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
                _logger.Error(logError, customer: customer);
            }

            #endregion

            return result;
        }

        /// <summary>
        /// Deletes an order
        /// </summary>
        /// <param name="order">The order</param>
        public virtual void DeleteOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //check whether the order wasn't cancelled before
            //if it already was cancelled, then there's no need to make the following adjustments
            //(such as reward points, inventory, recurring payments)
            //they already was done when cancelling the order
            if (order.OrderStatus != OrderStatus.Cancelled)
            {
                //return (add) back redeemded reward points
                ReturnBackRedeemedRewardPoints(order);
                //reduce (cancel) back reward points (previously awarded for this order)
                ReduceRewardPoints(order);

                //cancel recurring payments
                var recurringPayments = _orderService.SearchRecurringPayments(initialOrderId: order.Id);
                foreach (var rp in recurringPayments)
                {
                    var errors = CancelRecurringPayment(rp);
                    //use "errors" variable?
                }

                //Adjust inventory for already shipped shipments
                //only products with "use multiple warehouses"
                foreach (var shipment in _shipmentService.GetShipmentsByOrder(order.Id))
                {
                    //var order = _orderService.GetOrderById(shipment.OrderId);
                    foreach (var shipmentItem in shipment.ShipmentItems)
                    {
                        var product = _productService.GetProductById(shipmentItem.ProductId);
                        shipmentItem.ShipmentId = shipment.Id;
                        _productService.ReverseBookedInventory(product, shipmentItem);
                    }
                }
                //Adjust inventory
                foreach (var orderItem in order.OrderItems)
                {
                    var product = _productService.GetProductById(orderItem.ProductId);
                    _productService.AdjustInventory(product, orderItem.Quantity, orderItem.AttributesXml);
                }

            }

            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                Note = "Order has been deleted",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,

            });
            
            //now delete an order
            _orderService.DeleteOrder(order);
        }

        /// <summary>
        /// Process next recurring psayment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual void ProcessNextRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");
            try
            {
                if (!recurringPayment.IsActive)
                    throw new NopException("Recurring payment is not active");

                var initialOrder = recurringPayment.InitialOrder;
                if (initialOrder == null)
                    throw new NopException("Initial order could not be loaded");

                var customer = _customerService.GetCustomerById(initialOrder.CustomerId);
                if (customer == null)
                    throw new NopException("Customer could not be loaded");

                var nextPaymentDate = recurringPayment.NextPaymentDate;
                if (!nextPaymentDate.HasValue)
                    throw new NopException("Next payment date could not be calculated");

                //payment info
                var paymentInfo = new ProcessPaymentRequest
                {
                    StoreId = initialOrder.StoreId,
                    CustomerId = customer.Id,
                    OrderGuid = Guid.NewGuid(),
                    IsRecurringPayment = true,
                    InitialOrderId = initialOrder.Id,
                    RecurringCycleLength = recurringPayment.CycleLength,
                    RecurringCyclePeriod = recurringPayment.CyclePeriod,
                    RecurringTotalCycles = recurringPayment.TotalCycles,
                };

                //place a new order
                var result = this.PlaceOrder(paymentInfo);
                if (result.Success)
                {
                    if (result.PlacedOrder == null)
                        throw new NopException("Placed order could not be loaded");

                    var rph = new RecurringPaymentHistory
                    {
                        RecurringPayment = recurringPayment,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = result.PlacedOrder.Id,
                    };
                    recurringPayment.RecurringPaymentHistory.Add(rph);
                    _orderService.UpdateRecurringPayment(recurringPayment);
                }
                else
                {
                    string error = "";
                    for (int i = 0; i < result.Errors.Count; i++)
                    {
                        error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                        if (i != result.Errors.Count - 1)
                            error += ". ";
                    }
                    throw new NopException(error);
                }
            }
            catch (Exception exc)
            {
                _logger.Error(string.Format("Error while processing recurring order. {0}", exc.Message), exc);
                throw;
            }
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual IList<string> CancelRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            var initialOrder = recurringPayment.InitialOrder;
            if (initialOrder == null)
                return new List<string> { "Initial order could not be loaded" };


            var request = new CancelRecurringPaymentRequest();
            CancelRecurringPaymentResult result = null;
            try
            {
                request.Order = initialOrder;
                result = _paymentService.CancelRecurringPayment(request);
                if (result.Success)
                {
                    //update recurring payment
                    recurringPayment.IsActive = false;
                    _orderService.UpdateRecurringPayment(recurringPayment);


                    //add a note
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        Note = "Recurring payment has been cancelled",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = initialOrder.Id,

                    });

                    //notify a store owner
                    _workflowMessageService
                        .SendRecurringPaymentCancelledStoreOwnerNotification(recurringPayment, 
                        _localizationSettings.DefaultAdminLanguageId);
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new CancelRecurringPaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }


            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                _orderService.InsertOrderNote(new OrderNote
                {
                    Note = string.Format("Unable to cancel recurring payment. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = initialOrder.Id,

                });

                //log it
                string logError = string.Format("Error cancelling recurring payment. Order #{0}. Error: {1}", initialOrder.Id, error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether a customer can cancel recurring payment
        /// </summary>
        /// <param name="customerToValidate">Customer</param>
        /// <param name="recurringPayment">Recurring Payment</param>
        /// <returns>value indicating whether a customer can cancel recurring payment</returns>
        public virtual bool CanCancelRecurringPayment(Customer customerToValidate, RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                return false;

            if (customerToValidate == null)
                return false;

            var initialOrder = recurringPayment.InitialOrder;
            if (initialOrder == null)
                return false;

            var customer = _customerService.GetCustomerById(recurringPayment.InitialOrder.CustomerId);
            if (customer == null)
                return false;

            if (initialOrder.OrderStatus == OrderStatus.Cancelled)
                return false;

            if (!customerToValidate.IsAdmin())
            {
                if (customer.Id != customerToValidate.Id)
                    return false;
            }

            if (!recurringPayment.NextPaymentDate.HasValue)
                return false;

            return true;
        }



        /// <summary>
        /// Send a shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        public virtual void Ship(Shipment shipment, bool notifyCustomer)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                throw new Exception("Order cannot be loaded");

            if (shipment.ShippedDateUtc.HasValue)
                throw new Exception("This shipment is already shipped");

            shipment.ShippedDateUtc = DateTime.UtcNow;
            _shipmentService.UpdateShipment(shipment);

            //process products with "Multiple warehouse" support enabled
            foreach (var item in shipment.ShipmentItems)
            {
                var orderItem = order.OrderItems.Where(x => x.Id == item.OrderItemId).FirstOrDefault(); 
                var product = _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                _productService.BookReservedInventory(product, item.WarehouseId, -item.Quantity);
            }

            //check whether we have more items to ship
            if (order.HasItemsToAddToShipment() || order.HasItemsToShip())
                order.ShippingStatusId = (int)ShippingStatus.PartiallyShipped;
            else
                order.ShippingStatusId = (int)ShippingStatus.Shipped;
            _orderService.UpdateOrder(order);

            //add a note
            _orderService.InsertOrderNote(new OrderNote
                {
                    Note = string.Format("Shipment# {0} has been sent", shipment.Id),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                });

            if (notifyCustomer)
            {
                //notify customer
                int queuedEmailId = _workflowMessageService.SendShipmentSentCustomerNotification(shipment, order.CustomerLanguageId);
                if (queuedEmailId > 0)
                {
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        Note = string.Format("\"Shipped\" email (to customer) has been queued. Queued email identifier: {0}.", queuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                    });
                }
            }

            //event
            _eventPublisher.PublishShipmentSent(shipment);

            //check order status
            CheckOrderStatus(order);
        }

        /// <summary>
        /// Marks a shipment as delivered
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        public virtual void Deliver(Shipment shipment, bool notifyCustomer)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            var order = EngineContext.Current.Resolve<IOrderService>().GetOrderById(shipment.OrderId);
            if (order == null)
                throw new Exception("Order cannot be loaded");

            if (!shipment.ShippedDateUtc.HasValue)
                throw new Exception("This shipment is not shipped yet");

            if (shipment.DeliveryDateUtc.HasValue)
                throw new Exception("This shipment is already delivered");

            shipment.DeliveryDateUtc = DateTime.UtcNow;
            _shipmentService.UpdateShipment(shipment);

            if (!order.HasItemsToAddToShipment() && !order.HasItemsToShip() && !order.HasItemsToDeliver())
                order.ShippingStatusId = (int)ShippingStatus.Delivered;
            _orderService.UpdateOrder(order);

            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                Note = string.Format("Shipment# {0} has been delivered", shipment.Id),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });

            if (notifyCustomer)
            {
                //send email notification
                int queuedEmailId = _workflowMessageService.SendShipmentDeliveredCustomerNotification(shipment, order.CustomerLanguageId);
                if (queuedEmailId > 0)
                {
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        Note = string.Format("\"Delivered\" email (to customer) has been queued. Queued email identifier: {0}.", queuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                    });
                }
            }

            //event
            _eventPublisher.PublishShipmentDelivered(shipment);

            //check order status
            CheckOrderStatus(order);
        }



        /// <summary>
        /// Gets a value indicating whether cancel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether cancel is allowed</returns>
        public virtual bool CanCancelOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderStatus == OrderStatus.Cancelled)
                return false;

            return true;
        }

        /// <summary>
        /// Cancels order
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        public virtual void CancelOrder(Order order, bool notifyCustomer)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanCancelOrder(order))
                throw new NopException("Cannot do cancel for order.");

            //Cancel order
            SetOrderStatus(order, OrderStatus.Cancelled, notifyCustomer);

            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                Note = "Order has been cancelled",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,

            });

            //return (add) back redeemded reward points
            ReturnBackRedeemedRewardPoints(order);

            //cancel recurring payments
            var recurringPayments = _orderService.SearchRecurringPayments(initialOrderId: order.Id);
            foreach (var rp in recurringPayments)
            {
                var errors = CancelRecurringPayment(rp);
                //use "errors" variable?
            }

            //Adjust inventory for already shipped shipments
            //only products with "use multiple warehouses"
            var shipments = EngineContext.Current.Resolve<IShipmentService>().GetShipmentsByOrder(order.Id);
            foreach (var shipment in shipments)
            {
                foreach (var shipmentItem in shipment.ShipmentItems)
                {
                    var product = _productService.GetProductById(shipmentItem.ProductId);
                    shipmentItem.ShipmentId = shipment.Id;
                    _productService.ReverseBookedInventory(product, shipmentItem);
                }
            }
            //Adjust inventory
            foreach (var orderItem in order.OrderItems)
            {
                var product = _productService.GetProductById(orderItem.ProductId);
                _productService.AdjustInventory(product, orderItem.Quantity, orderItem.AttributesXml);
            }

            _eventPublisher.Publish(new OrderCancelledEvent(order));

        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as authorized
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as authorized</returns>
        public virtual bool CanMarkOrderAsAuthorized(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderStatus == OrderStatus.Cancelled)
                return false;

            if (order.PaymentStatus == PaymentStatus.Pending)
                return true;

            return false;
        }

        /// <summary>
        /// Marks order as authorized
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void MarkAsAuthorized(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            order.PaymentStatusId = (int)PaymentStatus.Authorized;
            _orderService.UpdateOrder(order);

            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                Note = "Order has been marked as authorized",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });

            //check order status
            CheckOrderStatus(order);
        }



        /// <summary>
        /// Gets a value indicating whether capture from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether capture from admin panel is allowed</returns>
        public virtual bool CanCapture(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderStatus == OrderStatus.Cancelled ||
                order.OrderStatus == OrderStatus.Pending)
                return false;

            if (order.PaymentStatus == PaymentStatus.Authorized &&
                _paymentService.SupportCapture(order.PaymentMethodSystemName))
                return true;

            return false;
        }

        /// <summary>
        /// Capture an order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        public virtual IList<string> Capture(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanCapture(order))
                throw new NopException("Cannot do capture for order.");

            var request = new CapturePaymentRequest();
            CapturePaymentResult result = null;
            try
            {
                //old info from placing order
                request.Order = order;
                result = _paymentService.Capture(request);

                if (result.Success)
                {
                    var paidDate = order.PaidDateUtc;
                    if (result.NewPaymentStatus == PaymentStatus.Paid)
                        paidDate = DateTime.UtcNow;

                    order.CaptureTransactionId = result.CaptureTransactionId;
                    order.CaptureTransactionResult = result.CaptureTransactionResult;
                    order.PaymentStatus = result.NewPaymentStatus;
                    order.PaidDateUtc = paidDate;
                    _orderService.UpdateOrder(order);

                    //add a note
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        Note = "Order has been captured",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,

                    });

                    CheckOrderStatus(order);
     
                    if (order.PaymentStatus == PaymentStatus.Paid)
                    {
                        ProcessOrderPaid(order);
                    }
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new CapturePaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }


            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                _orderService.InsertOrderNote(new OrderNote
                {
                    Note = string.Format("Unable to capture order. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                });

                //log it
                string logError = string.Format("Error capturing order #{0}. Error: {1}", order.Id, error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as paid
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as paid</returns>
        public virtual bool CanMarkOrderAsPaid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderStatus == OrderStatus.Cancelled)
                return false;

            if (order.PaymentStatus == PaymentStatus.Paid ||
                order.PaymentStatus == PaymentStatus.Refunded ||
                order.PaymentStatus == PaymentStatus.Voided)
                return false;

            return true;
        }

        /// <summary>
        /// Marks order as paid
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void MarkOrderAsPaid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanMarkOrderAsPaid(order))
                throw new NopException("You can't mark this order as paid");

            order.PaymentStatusId = (int)PaymentStatus.Paid;
            order.PaidDateUtc = DateTime.UtcNow;
            _orderService.UpdateOrder(order);

            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                Note = "Order has been marked as paid",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,

            });

            CheckOrderStatus(order);
   
            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                ProcessOrderPaid(order);
            }
        }



        /// <summary>
        /// Gets a value indicating whether refund from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether refund from admin panel is allowed</returns>
        public virtual bool CanRefund(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderTotal == decimal.Zero)
                return false;

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            if (order.PaymentStatus == PaymentStatus.Paid &&
                _paymentService.SupportRefund(order.PaymentMethodSystemName))
                return true;

            return false;
        }
        
        /// <summary>
        /// Refunds an order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        public virtual IList<string> Refund(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanRefund(order))
                throw new NopException("Cannot do refund for order.");

            var request = new RefundPaymentRequest();
            RefundPaymentResult result = null;
            try
            {
                request.Order = order;
                request.AmountToRefund = order.OrderTotal;
                request.IsPartialRefund = false;
                result = _paymentService.Refund(request);
                if (result.Success)
                {
                    //total amount refunded
                    decimal totalAmountRefunded = order.RefundedAmount + request.AmountToRefund;

                    //update order info
                    order.RefundedAmount = totalAmountRefunded;
                    order.PaymentStatus = result.NewPaymentStatus;
                    _orderService.UpdateOrder(order);

                    //add a note
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        Note = string.Format("Order has been refunded. Amount = {0}", request.AmountToRefund),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                    });

                    //check order status
                    CheckOrderStatus(order);

                    //notifications
                    var orderRefundedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedStoreOwnerNotification(order, request.AmountToRefund, _localizationSettings.DefaultAdminLanguageId);
                    if (orderRefundedStoreOwnerNotificationQueuedEmailId > 0)
                    {
                        _orderService.InsertOrderNote(new OrderNote
                        {
                            Note = string.Format("\"Order refunded\" email (to store owner) has been queued. Queued email identifier: {0}.", orderRefundedStoreOwnerNotificationQueuedEmailId),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id,
                        });
                    }

                    //notifications
                    var orderRefundedCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedCustomerNotification(order, request.AmountToRefund, order.CustomerLanguageId);
                    if (orderRefundedCustomerNotificationQueuedEmailId > 0)
                    {
                        _orderService.InsertOrderNote(new OrderNote
                        {
                            Note = string.Format("\"Order refunded\" email (to customer) has been queued. Queued email identifier: {0}.", orderRefundedCustomerNotificationQueuedEmailId),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id,
                        });
                    }

                    //raise event       
                    _eventPublisher.Publish(new OrderRefundedEvent(order, request.AmountToRefund));
                }

            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new RefundPaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }

            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                _orderService.InsertOrderNote(new OrderNote
                {
                    Note = string.Format("Unable to refund order. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                });

                //log it
                string logError = string.Format("Error refunding order #{0}. Error: {1}", order.Id, error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as refunded
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as refunded</returns>
        public virtual bool CanRefundOffline(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderTotal == decimal.Zero)
                return false;

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //     return false;

            if (order.PaymentStatus == PaymentStatus.Paid)
                return true;

            return false;
        }

        /// <summary>
        /// Refunds an order (offline)
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void RefundOffline(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanRefundOffline(order))
                throw new NopException("You can't refund this order");

            //amout to refund
            decimal amountToRefund = order.OrderTotal;

            //total amount refunded
            decimal totalAmountRefunded = order.RefundedAmount + amountToRefund;

            //update order info
            order.RefundedAmount = totalAmountRefunded;
            order.PaymentStatus = PaymentStatus.Refunded;
            _orderService.UpdateOrder(order);

            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                Note = string.Format("Order has been marked as refunded. Amount = {0}", amountToRefund),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });

            //check order status
            CheckOrderStatus(order);

            //notifications
            var orderRefundedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedStoreOwnerNotification(order, amountToRefund, _localizationSettings.DefaultAdminLanguageId);
            if (orderRefundedStoreOwnerNotificationQueuedEmailId > 0)
            {
                _orderService.InsertOrderNote(new OrderNote
                {
                    Note = string.Format("\"Order refunded\" email (to store owner) has been queued. Queued email identifier: {0}.", orderRefundedStoreOwnerNotificationQueuedEmailId),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                });

            }


            var orderRefundedCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedCustomerNotification(order, amountToRefund, order.CustomerLanguageId);
            if (orderRefundedCustomerNotificationQueuedEmailId > 0)
            {
                _orderService.InsertOrderNote(new OrderNote
                {
                    Note = string.Format("\"Order refunded\" email (to customer) has been queued. Queued email identifier: {0}.", orderRefundedCustomerNotificationQueuedEmailId),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                });
            }

            //raise event       
            _eventPublisher.Publish(new OrderRefundedEvent(order, amountToRefund));
        }

        /// <summary>
        /// Gets a value indicating whether partial refund from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A value indicating whether refund from admin panel is allowed</returns>
        public virtual bool CanPartiallyRefund(Order order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderTotal == decimal.Zero)
                return false;

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            decimal canBeRefunded = order.OrderTotal - order.RefundedAmount;
            if (canBeRefunded <= decimal.Zero)
                return false;

            if (amountToRefund > canBeRefunded)
                return false;

            if ((order.PaymentStatus == PaymentStatus.Paid ||
                order.PaymentStatus == PaymentStatus.PartiallyRefunded) &&
                _paymentService.SupportPartiallyRefund(order.PaymentMethodSystemName))
                return true;

            return false;
        }

        /// <summary>
        /// Partially refunds an order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        public virtual IList<string> PartiallyRefund(Order order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanPartiallyRefund(order, amountToRefund))
                throw new NopException("Cannot do partial refund for order.");

            var request = new RefundPaymentRequest();
            RefundPaymentResult result = null;
            try
            {
                request.Order = order;
                request.AmountToRefund = amountToRefund;
                request.IsPartialRefund = true;

                result = _paymentService.Refund(request);

                if (result.Success)
                {
                    //total amount refunded
                    decimal totalAmountRefunded = order.RefundedAmount + amountToRefund;

                    //update order info
                    order.RefundedAmount = totalAmountRefunded;
                    order.PaymentStatus = result.NewPaymentStatus;
                    _orderService.UpdateOrder(order);


                    //add a note
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        Note = string.Format("Order has been partially refunded. Amount = {0}", amountToRefund),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                    });

                    //check order status
                    CheckOrderStatus(order);

                    //notifications
                    var orderRefundedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedStoreOwnerNotification(order, amountToRefund, _localizationSettings.DefaultAdminLanguageId);
                    if (orderRefundedStoreOwnerNotificationQueuedEmailId > 0)
                    {
                        _orderService.InsertOrderNote(new OrderNote
                        {
                            Note = string.Format("\"Order refunded\" email (to store owner) has been queued. Queued email identifier: {0}.", orderRefundedStoreOwnerNotificationQueuedEmailId),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id,
                        });
                    }


                    var orderRefundedCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedCustomerNotification(order, amountToRefund, order.CustomerLanguageId);
                    if (orderRefundedCustomerNotificationQueuedEmailId > 0)
                    {
                        _orderService.InsertOrderNote(new OrderNote
                        {
                            Note = string.Format("\"Order refunded\" email (to customer) has been queued. Queued email identifier: {0}.", orderRefundedCustomerNotificationQueuedEmailId),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id,
                        });
                    }

                    //raise event       
                    _eventPublisher.Publish(new OrderRefundedEvent(order, amountToRefund));
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new RefundPaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }

            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                _orderService.InsertOrderNote(new OrderNote
                {
                    Note = string.Format("Unable to partially refund order. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                });

                //log it
                string logError = string.Format("Error refunding order #{0}. Error: {1}", order.Id, error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as partially refunded
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A value indicating whether order can be marked as partially refunded</returns>
        public virtual bool CanPartiallyRefundOffline(Order order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderTotal == decimal.Zero)
                return false;

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            decimal canBeRefunded = order.OrderTotal - order.RefundedAmount;
            if (canBeRefunded <= decimal.Zero)
                return false;

            if (amountToRefund > canBeRefunded)
                return false;

            if (order.PaymentStatus == PaymentStatus.Paid ||
                order.PaymentStatus == PaymentStatus.PartiallyRefunded)
                return true;

            return false;
        }

        /// <summary>
        /// Partially refunds an order (offline)
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        public virtual void PartiallyRefundOffline(Order order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException("order");
            
            if (!CanPartiallyRefundOffline(order, amountToRefund))
                throw new NopException("You can't partially refund (offline) this order");

            //total amount refunded
            decimal totalAmountRefunded = order.RefundedAmount + amountToRefund;

            //update order info
            order.RefundedAmount = totalAmountRefunded;
            //if (order.OrderTotal == totalAmountRefunded), then set order.PaymentStatus = PaymentStatus.Refunded;
            order.PaymentStatus = PaymentStatus.PartiallyRefunded;
            _orderService.UpdateOrder(order);

            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                Note = string.Format("Order has been marked as partially refunded. Amount = {0}", amountToRefund),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });

            //check order status
            CheckOrderStatus(order);

            //notifications
            var orderRefundedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedStoreOwnerNotification(order, amountToRefund, _localizationSettings.DefaultAdminLanguageId);
            if (orderRefundedStoreOwnerNotificationQueuedEmailId > 0)
            {
                _orderService.InsertOrderNote(new OrderNote
                {
                    Note = string.Format("\"Order refunded\" email (to store owner) has been queued. Queued email identifier: {0}.", orderRefundedStoreOwnerNotificationQueuedEmailId),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                });
            }

            var orderRefundedCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedCustomerNotification(order, amountToRefund, order.CustomerLanguageId);
            if (orderRefundedCustomerNotificationQueuedEmailId > 0)
            {
                _orderService.InsertOrderNote(new OrderNote
                {
                    Note = string.Format("\"Order refunded\" email (to customer) has been queued. Queued email identifier: {0}.", orderRefundedCustomerNotificationQueuedEmailId),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                });
            }
            //raise event       
            _eventPublisher.Publish(new OrderRefundedEvent(order, amountToRefund));
        }



        /// <summary>
        /// Gets a value indicating whether void from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether void from admin panel is allowed</returns>
        public virtual bool CanVoid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderTotal == decimal.Zero)
                return false;

            if (order.PaymentStatus == PaymentStatus.Authorized &&
                _paymentService.SupportVoid(order.PaymentMethodSystemName))
                return true;

            return false;
        }

        /// <summary>
        /// Voids order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Voided order</returns>
        public virtual IList<string> Void(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanVoid(order))
                throw new NopException("Cannot do void for order.");

            var request = new VoidPaymentRequest();
            VoidPaymentResult result = null;
            try
            {
                request.Order = order;
                result = _paymentService.Void(request);

                if (result.Success)
                {
                    //update order info
                    order.PaymentStatus = result.NewPaymentStatus;
                    _orderService.UpdateOrder(order);

                    //add a note
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        Note = "Order has been voided",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                    });

                    //check order status
                    CheckOrderStatus(order);
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new VoidPaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }

            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                _orderService.InsertOrderNote(new OrderNote
                {
                    Note = string.Format("Unable to voiding order. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                });

                //log it
                string logError = string.Format("Error voiding order #{0}. Error: {1}", order.Id, error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as voided
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as voided</returns>
        public virtual bool CanVoidOffline(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderTotal == decimal.Zero)
                return false;

            if (order.PaymentStatus == PaymentStatus.Authorized)
                return true;

            return false;
        }

        /// <summary>
        /// Voids order (offline)
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void VoidOffline(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanVoidOffline(order))
                throw new NopException("You can't void this order");

            order.PaymentStatusId = (int)PaymentStatus.Voided;
            _orderService.UpdateOrder(order);

            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                Note = "Order has been marked as voided",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });
            //check orer status
            CheckOrderStatus(order);
        }



        /// <summary>
        /// Place order items in current user shopping cart.
        /// </summary>
        /// <param name="order">The order</param>
        public virtual void ReOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var customer = _customerService.GetCustomerById(order.CustomerId);

            foreach (var orderItem in order.OrderItems)
            {
                if (_productService.GetProductById(orderItem.ProductId) != null)
                {
                    _shoppingCartService.AddToCart(customer, orderItem.ProductId,
                        ShoppingCartType.ShoppingCart, order.StoreId,
                        orderItem.AttributesXml, orderItem.UnitPriceExclTax,
                        orderItem.RentalStartDateUtc, orderItem.RentalEndDateUtc,
                        orderItem.Quantity, false);
                }
            }
        }
        
        /// <summary>
        /// Check whether return request is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public virtual bool IsReturnRequestAllowed(Order order)
        {
            if (!_orderSettings.ReturnRequestsEnabled)
                return false;

            if (order == null || order.Deleted)
                return false;

            var shipments = _shipmentService.GetShipmentsByOrder(order.Id);

            //validate allowed number of days
            if (_orderSettings.NumberOfDaysReturnRequestAvailable > 0)
            {
                var daysPassed = (DateTime.UtcNow - order.CreatedOnUtc).TotalDays;
                if (daysPassed >= _orderSettings.NumberOfDaysReturnRequestAvailable)
                    return false;
            }
            foreach (var item in order.OrderItems)
            {
                var product = _productService.GetProductById(item.ProductId);
                var qtyDelivery = shipments.Where(x => x.DeliveryDateUtc.HasValue).SelectMany(x => x.ShipmentItems).Where(x => x.OrderItemId == item.Id).Sum(x => x.Quantity);
                var qtyReturn = _returnRequestService.SearchReturnRequests(customerId:order.CustomerId, orderItemId: item.Id).Sum(x=>x.Quantity);

                if (!product.NotReturnable && qtyDelivery - qtyReturn > 0)
                    return true;
            }
            return false;
        }
        


        /// <summary>
        /// Valdiate minimum order sub-total amount
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - OK; false - minimum order sub-total amount is not reached</returns>
        public virtual bool ValidateMinOrderSubtotalAmount(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            //min order amount sub-total validation
            if (cart.Any() && _orderSettings.MinOrderSubtotalAmount > decimal.Zero)
            {
                //subtotal
                decimal orderSubTotalDiscountAmountBase;
                List<Discount> orderSubTotalAppliedDiscounts;
                decimal subTotalWithoutDiscountBase;
                decimal subTotalWithDiscountBase;
                _orderTotalCalculationService.GetShoppingCartSubTotal(cart, false,
                    out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscounts,
                    out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

                if (subTotalWithoutDiscountBase < _orderSettings.MinOrderSubtotalAmount)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Valdiate minimum order total amount
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - OK; false - minimum order total amount is not reached</returns>
        public virtual bool ValidateMinOrderTotalAmount(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (cart.Any() && _orderSettings.MinOrderTotalAmount > decimal.Zero)
            {
                decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart);
                if (shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value < _orderSettings.MinOrderTotalAmount)
                    return false;
            }

            return true;
        }

        #endregion
    }
}
