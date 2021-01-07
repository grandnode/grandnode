using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Discounts;
using Grand.Domain.Localization;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using Grand.Domain.Tax;
using Grand.Services.Affiliates;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Customers;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Events.Extensions;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Notifications.Orders;
using Grand.Services.Payments;
using Grand.Services.Queries.Models.Orders;
using Grand.Services.Security;
using Grand.Services.Shipping;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    public class OrderConfirmationService : IOrderConfirmationService
    {
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IProductService _productService;
        private readonly IInventoryManageService _inventoryManageService;
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
        private readonly ITaxService _taxService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IEncryptionService _encryptionService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IVendorService _vendorService;
        private readonly ISalesEmployeeService _salesEmployeeService;
        private readonly ICurrencyService _currencyService;
        private readonly IAffiliateService _affiliateService;
        private readonly IMediator _mediator;
        private readonly IPdfService _pdfService;
        private readonly IRewardPointsService _rewardPointsService;
        private readonly IStoreContext _storeContext;
        private readonly IProductReservationService _productReservationService;
        private readonly IAuctionService _auctionService;
        private readonly ICountryService _countryService;
        private readonly ShippingSettings _shippingSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly OrderSettings _orderSettings;
        private readonly TaxSettings _taxSettings;
        private readonly LocalizationSettings _localizationSettings;

        public OrderConfirmationService(
            IOrderService orderService,
            ILocalizationService localizationService,
            ILanguageService languageService,
            IProductService productService,
            IInventoryManageService inventoryManageService,
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
            ITaxService taxService,
            ICustomerService customerService,
            IDiscountService discountService,
            IEncryptionService encryptionService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            IVendorService vendorService,
            ISalesEmployeeService salesEmployeeService,
            ICurrencyService currencyService,
            IAffiliateService affiliateService,
            IMediator mediator,
            IPdfService pdfService,
            IRewardPointsService rewardPointsService,
            IStoreContext storeContext,
            IProductReservationService productReservationService,
            IAuctionService auctionService,
            ICountryService countryService,
            ShippingSettings shippingSettings,
            ShoppingCartSettings shoppingCartSettings,
            PaymentSettings paymentSettings,
            OrderSettings orderSettings,
            TaxSettings taxSettings,
            LocalizationSettings localizationSettings)
        {
            _orderService = orderService;
            _localizationService = localizationService;
            _languageService = languageService;
            _productService = productService;
            _inventoryManageService = inventoryManageService;
            _paymentService = paymentService;
            _logger = logger;
            _orderTotalCalculationService = orderTotalCalculationService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeFormatter = productAttributeFormatter;
            _giftCardService = giftCardService;
            _shoppingCartService = shoppingCartService;
            _checkoutAttributeFormatter = checkoutAttributeFormatter;
            _shippingService = shippingService;
            _taxService = taxService;
            _customerService = customerService;
            _discountService = discountService;
            _encryptionService = encryptionService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _vendorService = vendorService;
            _salesEmployeeService = salesEmployeeService;
            _currencyService = currencyService;
            _affiliateService = affiliateService;
            _mediator = mediator;
            _pdfService = pdfService;
            _rewardPointsService = rewardPointsService;
            _storeContext = storeContext;
            _productReservationService = productReservationService;
            _auctionService = auctionService;
            _countryService = countryService;
            _shippingSettings = shippingSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _paymentSettings = paymentSettings;
            _orderSettings = orderSettings;
            _taxSettings = taxSettings;
            _localizationSettings = localizationSettings;
        }

        private async Task<decimal?> PrepareCommissionRate(Product product, PlaceOrderContainter details)
        {
            var commissionRate = default(decimal?);
            if (!string.IsNullOrEmpty(product.VendorId))
            {
                var vendor = await _vendorService.GetVendorById(product.VendorId);
                if (vendor != null && vendor.Commission.HasValue)
                    commissionRate = vendor.Commission.Value;
            }

            if (!commissionRate.HasValue)
            {
                if (!string.IsNullOrEmpty(details.Customer.SeId))
                {
                    var salesEmployee = await _salesEmployeeService.GetSalesEmployeeById(details.Customer.SeId);
                    if (salesEmployee != null && salesEmployee.Active && salesEmployee.Commission.HasValue)
                        commissionRate = salesEmployee.Commission.Value;
                }
            }

            return commissionRate;
        }

        protected virtual async Task UpdateCustomer(Order order, PlaceOrderContainter details)
        {
            //Update customer reminder history
            await _mediator.Send(new UpdateCustomerReminderHistoryCommand() { CustomerId = order.CustomerId, OrderId = order.Id });

            //Updated field "free shipping" after added a new order
            await _customerService.UpdateFreeShipping(order.CustomerId, false);

            //Update field Last purchase date after added a new order
            await _customerService.UpdateCustomerLastPurchaseDate(order.CustomerId, order.CreatedOnUtc);

            //Update field Last purchase date after added a new order
            await _customerService.UpdateCustomerLastUpdateCartDate(order.CustomerId, null);

            if (!details.Customer.HasContributions)
            {
                await _customerService.UpdateContributions(details.Customer);
            }

        }

        protected virtual async Task<PlaceOrderContainter> PreparePlaceOrderDetailsForRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var details = new PlaceOrderContainter();
            //customer
            details.Customer = await _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            if (details.Customer == null)
                throw new ArgumentException("Customer is not set");

            //affiliate
            if (!string.IsNullOrEmpty(details.Customer.AffiliateId))
            {
                var affiliate = await _affiliateService.GetAffiliateById(details.Customer.AffiliateId);
                if (affiliate != null && affiliate.Active && !affiliate.Deleted)
                    details.AffiliateId = affiliate.Id;
            }

            // Recurring orders.Load initial order
            details.InitialOrder = await _orderService.GetOrderById(processPaymentRequest.InitialOrderId);
            if (details.InitialOrder == null)
                throw new ArgumentException("Initial order is not set for recurring payment");

            var currency = await _currencyService.GetCurrencyByCode(details.InitialOrder.CustomerCurrencyCode);
            if (currency == null)
                throw new ArgumentException("Initial order has not set correct currency code");

            details.InitialOrder.Code = await _mediator.Send(new PrepareOrderCodeCommand());
            processPaymentRequest.PaymentMethodSystemName = details.InitialOrder.PaymentMethodSystemName;
            details.Currency = currency;
            details.CustomerLanguage = await _languageService.GetLanguageById(details.InitialOrder.CustomerLanguageId);
            details.CheckoutAttributes = details.InitialOrder.CheckoutAttributes;
            details.CheckoutAttributeDescription = details.InitialOrder.CheckoutAttributeDescription;
            details.CustomerTaxDisplayType = details.InitialOrder.CustomerTaxDisplayType;
            details.OrderSubTotalInclTax = details.InitialOrder.OrderSubtotalInclTax;
            details.OrderSubTotalExclTax = details.InitialOrder.OrderSubtotalExclTax;
            details.OrderDiscountAmount = details.InitialOrder.OrderDiscount;
            details.OrderSubTotalDiscountExclTax = details.InitialOrder.OrderSubTotalDiscountExclTax;
            details.OrderSubTotalDiscountInclTax = details.InitialOrder.OrderSubTotalDiscountInclTax;
            details.OrderTotal = details.InitialOrder.OrderTotal;
            details.BillingAddress = details.InitialOrder.BillingAddress;
            details.ShippingAddress = details.InitialOrder.ShippingAddress;
            details.PickupPoint = details.InitialOrder.PickupPoint;
            details.ShippingMethodName = details.InitialOrder.ShippingMethod;
            details.ShippingRateComputationMethodSystemName = details.InitialOrder.ShippingRateComputationMethodSystemName;
            var shoppingCartRequiresShipping = details.InitialOrder.ShippingStatus != ShippingStatus.ShippingNotRequired;
            details.ShippingStatus = shoppingCartRequiresShipping ? ShippingStatus.NotYetShipped : ShippingStatus.ShippingNotRequired;
            details.OrderShippingTotalInclTax = details.InitialOrder.OrderShippingInclTax;
            details.OrderShippingTotalExclTax = details.InitialOrder.OrderShippingExclTax;
            details.PaymentAdditionalFeeInclTax = details.InitialOrder.PaymentMethodAdditionalFeeInclTax;
            details.PaymentAdditionalFeeExclTax = details.InitialOrder.PaymentMethodAdditionalFeeExclTax;
            details.OrderTaxTotal = details.InitialOrder.OrderTax;
            details.Taxes = details.InitialOrder.OrderTaxes.ToList();
            details.IsRecurringShoppingCart = true;
            processPaymentRequest.OrderTotal = details.OrderTotal;

            return details;
        }

        protected virtual async Task<ProcessPaymentResult> PrepareProcessPaymentResult(ProcessPaymentRequest processPaymentRequest, PlaceOrderContainter details)
        {
            //process payment
            ProcessPaymentResult processPaymentResult = null;
            //skip payment workflow if order total equals zero
            var skipPaymentWorkflow = details.OrderTotal == decimal.Zero;
            if (!skipPaymentWorkflow)
            {
                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);
                if (paymentMethod == null)
                    throw new GrandException("Payment method couldn't be loaded");

                //ensure that payment method is active
                if (!paymentMethod.IsPaymentMethodActive(_paymentSettings))
                    throw new GrandException("Payment method is not active");

                if (!processPaymentRequest.IsRecurringPayment)
                {
                    if (details.IsRecurringShoppingCart)
                    {
                        //recurring cart
                        var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                        switch (recurringPaymentType)
                        {
                            case RecurringPaymentType.NotSupported:
                                throw new GrandException("Recurring payments are not supported by selected payment method");
                            case RecurringPaymentType.Manual:
                            case RecurringPaymentType.Automatic:
                                processPaymentResult = await _paymentService.ProcessRecurringPayment(processPaymentRequest);
                                break;
                            default:
                                throw new GrandException("Not supported recurring payment type");
                        }
                    }
                    else
                    {
                        //standard cart
                        processPaymentResult = await _paymentService.ProcessPayment(processPaymentRequest);
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
                        catch { }

                        var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                        switch (recurringPaymentType)
                        {
                            case RecurringPaymentType.NotSupported:
                                throw new GrandException("Recurring payments are not supported by selected payment method");
                            case RecurringPaymentType.Manual:
                                processPaymentResult = await _paymentService.ProcessRecurringPayment(processPaymentRequest);
                                break;
                            case RecurringPaymentType.Automatic:
                                //payment is processed on payment gateway site
                                processPaymentResult = new ProcessPaymentResult();
                                break;
                            default:
                                throw new GrandException("Not supported recurring payment type");
                        }
                    }
                    else
                    {
                        throw new GrandException("No recurring products");
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
                throw new GrandException("processPaymentResult is not available");

            return processPaymentResult;
        }

        protected virtual async Task<PlaceOrderContainter> PreparePlaceOrderDetails(ProcessPaymentRequest processPaymentRequest)
        {
            var details = new PlaceOrderContainter();

            //customer
            details.Customer = await _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            if (details.Customer == null)
                throw new ArgumentException("Customer is not set");

            //check whether customer is guest
            if (details.Customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                throw new GrandException("Anonymous checkout is not allowed");

            //billing address
            if (details.Customer.BillingAddress == null)
                throw new GrandException("Billing address is not provided");

            if (!CommonHelper.IsValidEmail(details.Customer.BillingAddress.Email))
                throw new GrandException("Email is not valid");

            //affiliate
            if (!string.IsNullOrEmpty(details.Customer.AffiliateId))
            {
                var affiliate = await _affiliateService.GetAffiliateById(details.Customer.AffiliateId);
                if (affiliate != null && affiliate.Active && !affiliate.Deleted)
                    details.AffiliateId = affiliate.Id;
            }

            //customer currency
            var currencyTmp = await _currencyService.GetCurrencyById(details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CurrencyId, processPaymentRequest.StoreId));
            var customerCurrency = (currencyTmp != null && currencyTmp.Published) ? currencyTmp : _workContext.WorkingCurrency;
            details.Currency = customerCurrency;
            var primaryStoreCurrency = await _currencyService.GetPrimaryStoreCurrency();
            details.CustomerCurrencyRate = customerCurrency.Rate / primaryStoreCurrency.Rate;
            details.PrimaryCurrencyCode = primaryStoreCurrency.CurrencyCode;

            //customer language
            details.CustomerLanguage = await _languageService.GetLanguageById(details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LanguageId, processPaymentRequest.StoreId));

            if (details.CustomerLanguage == null || !details.CustomerLanguage.Published)
                details.CustomerLanguage = _workContext.WorkingLanguage;



            details.BillingAddress = details.Customer.BillingAddress;
            if (!string.IsNullOrEmpty(details.BillingAddress.CountryId))
            {
                var country = await _countryService.GetCountryById(details.BillingAddress.CountryId);
                if (country != null)
                    if (!country.AllowsBilling)
                        throw new GrandException(string.Format("Country '{0}' is not allowed for billing", country.Name));
            }

            //checkout attributes
            details.CheckoutAttributes = details.Customer.GetAttributeFromEntity<List<CustomAttribute>>(SystemCustomerAttributeNames.CheckoutAttributes, processPaymentRequest.StoreId);
            details.CheckoutAttributeDescription = await _checkoutAttributeFormatter.FormatAttributes(details.CheckoutAttributes, details.Customer);

            //load and validate customer shopping cart
            details.Cart = details.Customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
                .LimitPerStore(_shoppingCartSettings.CartsSharedBetweenStores, processPaymentRequest.StoreId)
                .ToList();

            if (!details.Cart.Any())
                throw new GrandException("Cart is empty");

            //validate the entire shopping cart
            var warnings = await _shoppingCartService.GetShoppingCartWarnings(details.Cart, details.CheckoutAttributes.ToList(), true);
            if (warnings.Any())
            {
                var warningsSb = new StringBuilder();
                foreach (string warning in warnings)
                {
                    warningsSb.Append(warning);
                    warningsSb.Append(";");
                }
                throw new GrandException(warningsSb.ToString());
            }

            //validate individual cart items
            foreach (var sci in details.Cart)
            {
                var product = await _productService.GetProductById(sci.ProductId);
                var sciWarnings = await _shoppingCartService.GetShoppingCartItemWarnings(details.Customer, sci, product, false);
                if (sciWarnings.Any())
                {
                    var warningsSb = new StringBuilder();
                    foreach (string warning in sciWarnings)
                    {
                        warningsSb.Append(warning);
                        warningsSb.Append(";");
                    }
                    throw new GrandException(warningsSb.ToString());
                }
            }

            //min totals validation
            bool minOrderSubtotalAmountOk = await _mediator.Send(new ValidateMinShoppingCartSubtotalAmountCommand() { Customer = _workContext.CurrentCustomer, Cart = details.Cart });
            if (!minOrderSubtotalAmountOk)
            {
                decimal minOrderSubtotalAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
                throw new GrandException(string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false)));
            }

            bool minmaxOrderTotalAmountOk = await _mediator.Send(new ValidateShoppingCartTotalAmountCommand() { Customer = details.Customer, Cart = details.Cart });
            if (!minmaxOrderTotalAmountOk)
            {
                throw new GrandException(_localizationService.GetResource("Checkout.MinMaxOrderTotalAmount"));
            }

            //tax display type
            if (_taxSettings.AllowCustomersToSelectTaxDisplayType)
                details.CustomerTaxDisplayType = (TaxDisplayType)details.Customer.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.TaxDisplayTypeId, processPaymentRequest.StoreId);
            else
                details.CustomerTaxDisplayType = _taxSettings.TaxDisplayType;

            //sub total
            //sub total (incl tax)
            var shoppingCartSubTotal = await _orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart, true);
            decimal orderSubTotalDiscountAmount = shoppingCartSubTotal.discountAmount;
            List<AppliedDiscount> orderSubTotalAppliedDiscounts = shoppingCartSubTotal.appliedDiscounts;
            decimal subTotalWithoutDiscountBase = shoppingCartSubTotal.subTotalWithoutDiscount;
            decimal subTotalWithDiscountBase = shoppingCartSubTotal.subTotalWithDiscount;

            details.OrderSubTotalInclTax = subTotalWithoutDiscountBase;
            details.OrderSubTotalDiscountInclTax = orderSubTotalDiscountAmount;

            foreach (var disc in orderSubTotalAppliedDiscounts)
                if (!details.AppliedDiscounts.Where(x => x.DiscountId == disc.DiscountId).Any())
                    details.AppliedDiscounts.Add(disc);

            //sub total (excl tax)
            shoppingCartSubTotal = await _orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart, false);
            orderSubTotalDiscountAmount = shoppingCartSubTotal.discountAmount;
            orderSubTotalAppliedDiscounts = shoppingCartSubTotal.appliedDiscounts;
            subTotalWithoutDiscountBase = shoppingCartSubTotal.subTotalWithoutDiscount;
            subTotalWithDiscountBase = shoppingCartSubTotal.subTotalWithDiscount;

            details.OrderSubTotalExclTax = subTotalWithoutDiscountBase;
            details.OrderSubTotalDiscountExclTax = orderSubTotalDiscountAmount;

            //shipping info
            bool shoppingCartRequiresShipping = shoppingCartRequiresShipping = details.Cart.RequiresShipping();

            if (shoppingCartRequiresShipping)
            {
                var pickupPoint = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.SelectedPickupPoint, processPaymentRequest.StoreId);
                if (_shippingSettings.AllowPickUpInStore && pickupPoint != null)
                {
                    details.PickUpInStore = true;
                    details.PickupPoint = await _mediator.Send(new GetPickupPointById() { Id = pickupPoint });
                }
                else
                {
                    if (details.Customer.ShippingAddress == null)
                        throw new GrandException("Shipping address is not provided");

                    if (!CommonHelper.IsValidEmail(details.Customer.ShippingAddress.Email))
                        throw new GrandException("Email is not valid");

                    //clone shipping address
                    details.ShippingAddress = details.Customer.ShippingAddress;
                    if (!String.IsNullOrEmpty(details.ShippingAddress.CountryId))
                    {
                        var country = await _countryService.GetCountryById(details.ShippingAddress.CountryId);
                        if (country != null)
                            if (!country.AllowsShipping)
                                throw new GrandException(string.Format("Country '{0}' is not allowed for shipping", country.Name));
                    }
                }
                var shippingOption = details.Customer.GetAttributeFromEntity<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, processPaymentRequest.StoreId);
                if (shippingOption != null)
                {
                    details.ShippingMethodName = shippingOption.Name;
                    details.ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName;
                }
            }
            details.ShippingStatus = shoppingCartRequiresShipping
                ? ShippingStatus.NotYetShipped
                : ShippingStatus.ShippingNotRequired;

            //shipping total

            var shoppingCartShippingTotal = await _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, true);
            decimal tax = shoppingCartShippingTotal.taxRate;
            List<AppliedDiscount> shippingTotalDiscounts = shoppingCartShippingTotal.appliedDiscounts;
            var orderShippingTotalInclTax = shoppingCartShippingTotal.shoppingCartShippingTotal;
            var orderShippingTotalExclTax = (await _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, false)).shoppingCartShippingTotal;
            if (!orderShippingTotalInclTax.HasValue || !orderShippingTotalExclTax.HasValue)
                throw new GrandException("Shipping total couldn't be calculated");

            foreach (var disc in shippingTotalDiscounts)
            {
                if (!details.AppliedDiscounts.Where(x => x.DiscountId == disc.DiscountId).Any())
                    details.AppliedDiscounts.Add(disc);
            }


            details.OrderShippingTotalInclTax = orderShippingTotalInclTax.Value;
            details.OrderShippingTotalExclTax = orderShippingTotalExclTax.Value;

            //payment total
            decimal paymentAdditionalFee = await _paymentService.GetAdditionalHandlingFee(details.Cart, processPaymentRequest.PaymentMethodSystemName);
            paymentAdditionalFee = await _currencyService.ConvertFromPrimaryStoreCurrency(paymentAdditionalFee, _workContext.WorkingCurrency);
            details.PaymentAdditionalFeeInclTax = (await _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, true, details.Customer)).paymentPrice;
            details.PaymentAdditionalFeeExclTax = (await _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, false, details.Customer)).paymentPrice;

            //tax total
            //tax amount
            var (taxtotal, taxRates) = await _orderTotalCalculationService.GetTaxTotal(details.Cart);
            details.OrderTaxTotal = taxtotal;

            //tax rates
            foreach (var kvp in taxRates)
            {
                details.Taxes.Add(new OrderTax() {
                    Percent = Math.Round(kvp.Key, 2),
                    Amount = kvp.Value
                });
            }

            //order total (and applied discounts, gift cards, reward points)
            var shoppingCartTotal = await _orderTotalCalculationService.GetShoppingCartTotal(details.Cart);
            List<AppliedGiftCard> appliedGiftCards = shoppingCartTotal.appliedGiftCards;
            List<AppliedDiscount> orderAppliedDiscounts = shoppingCartTotal.appliedDiscounts;
            decimal orderDiscountAmount = shoppingCartTotal.discountAmount;
            int redeemedRewardPoints = shoppingCartTotal.redeemedRewardPoints;
            var orderTotal = shoppingCartTotal.shoppingCartTotal;
            if (!orderTotal.HasValue)
                throw new GrandException("Order total couldn't be calculated");

            details.OrderDiscountAmount = orderDiscountAmount;
            details.RedeemedRewardPoints = redeemedRewardPoints;
            details.RedeemedRewardPointsAmount = shoppingCartTotal.redeemedRewardPointsAmount;
            details.AppliedGiftCards = appliedGiftCards;
            details.OrderTotal = orderTotal.Value;

            //discount history
            foreach (var disc in orderAppliedDiscounts)
            {
                if (!details.AppliedDiscounts.Where(x => x.DiscountId == disc.DiscountId).Any())
                    details.AppliedDiscounts.Add(disc);
            }

            details.IsRecurringShoppingCart = details.Cart.IsRecurring();
            if (details.IsRecurringShoppingCart)
            {
                var (info, cycleLength, cyclePeriod, totalCycles) = await details.Cart.GetRecurringCycleInfo(_localizationService, _productService);

                int recurringCycleLength = cycleLength;
                RecurringProductCyclePeriod recurringCyclePeriod = cyclePeriod;
                int recurringTotalCycles = totalCycles;
                string recurringCyclesError = info;
                if (!string.IsNullOrEmpty(recurringCyclesError))
                    throw new GrandException(recurringCyclesError);

                processPaymentRequest.RecurringCycleLength = recurringCycleLength;
                processPaymentRequest.RecurringCyclePeriod = recurringCyclePeriod;
                processPaymentRequest.RecurringTotalCycles = recurringTotalCycles;
            }

            processPaymentRequest.OrderTotal = details.OrderTotal;

            return details;
        }

        protected virtual async Task<Order> SaveOrderDetailsForReccuringPayment(PlaceOrderContainter details, Order order)
        {
            #region recurring payment

            var initialOrderItems = details.InitialOrder.OrderItems;
            foreach (var orderItem in initialOrderItems)
            {
                //save item
                var newOrderItem = new OrderItem {
                    OrderItemGuid = Guid.NewGuid(),
                    ProductId = orderItem.ProductId,
                    VendorId = orderItem.VendorId,
                    WarehouseId = orderItem.WarehouseId,
                    SeId = orderItem.SeId,
                    UnitPriceWithoutDiscInclTax = orderItem.UnitPriceWithoutDiscInclTax,
                    UnitPriceWithoutDiscExclTax = orderItem.UnitPriceWithoutDiscExclTax,
                    UnitPriceInclTax = orderItem.UnitPriceInclTax,
                    UnitPriceExclTax = orderItem.UnitPriceExclTax,
                    PriceInclTax = orderItem.PriceInclTax,
                    PriceExclTax = orderItem.PriceExclTax,
                    OriginalProductCost = orderItem.OriginalProductCost,
                    AttributeDescription = orderItem.AttributeDescription,
                    Attributes = orderItem.Attributes,
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
                    Commission = orderItem.Commission
                };
                order.OrderItems.Add(newOrderItem);

                //gift cards
                var product = await _productService.GetProductById(orderItem.ProductId);
                if (product.IsGiftCard)
                {
                    _productAttributeParser.GetGiftCardAttribute(orderItem.Attributes,
                        out string giftCardRecipientName, out string giftCardRecipientEmail,
                        out string giftCardSenderName, out string giftCardSenderEmail, out string giftCardMessage);

                    for (int i = 0; i < orderItem.Quantity; i++)
                    {
                        var gc = new GiftCard {
                            GiftCardType = product.GiftCardType,
                            PurchasedWithOrderItem = newOrderItem,
                            Amount = orderItem.UnitPriceExclTax,
                            CurrencyCode = order.CustomerCurrencyCode,
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
                        await _giftCardService.InsertGiftCard(gc);
                    }
                }

                //inventory
                await _inventoryManageService.AdjustInventory(product, -orderItem.Quantity, orderItem.Attributes, orderItem.WarehouseId);
            }

            //insert order
            await _orderService.InsertOrder(order);

            return order;

            #endregion
        }

        protected virtual async Task CreateRecurringPayment(ProcessPaymentRequest processPaymentRequest, Order order)
        {
            var rp = new RecurringPayment {
                CycleLength = processPaymentRequest.RecurringCycleLength,
                CyclePeriod = processPaymentRequest.RecurringCyclePeriod,
                TotalCycles = processPaymentRequest.RecurringTotalCycles,
                StartDateUtc = DateTime.UtcNow,
                IsActive = true,
                CreatedOnUtc = DateTime.UtcNow,
                InitialOrder = order,
            };
            await _orderService.InsertRecurringPayment(rp);


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
                        var rph = new RecurringPaymentHistory {
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id,
                            RecurringPaymentId = rp.Id
                        };
                        rp.RecurringPaymentHistory.Add(rph);
                        await _orderService.UpdateRecurringPayment(rp);
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

        protected virtual async Task<OrderItem> PrepareOrderItem(ShoppingCartItem sc, Product product, PlaceOrderContainter details)
        {
            //prices
            decimal taxRate;
            List<AppliedDiscount> scDiscounts;
            decimal discountAmount;
            decimal scUnitPrice = (await _priceCalculationService.GetUnitPrice(sc, product)).unitprice;
            decimal scUnitPriceWithoutDisc = (await _priceCalculationService.GetUnitPrice(sc, product, false)).unitprice;

            var subtotal = await _priceCalculationService.GetSubTotal(sc, product, true);
            decimal scSubTotal = subtotal.subTotal;
            discountAmount = subtotal.discountAmount;
            scDiscounts = subtotal.appliedDiscounts;


            var prices = await _taxService.GetTaxProductPrice(product, details.Customer, scUnitPrice, scUnitPriceWithoutDisc, sc.Quantity, scSubTotal, discountAmount, _taxSettings.PricesIncludeTax);
            taxRate = prices.taxRate;
            decimal scUnitPriceWithoutDiscInclTax = prices.UnitPriceWihoutDiscInclTax;
            decimal scUnitPriceWithoutDiscExclTax = prices.UnitPriceWihoutDiscExclTax;
            decimal scUnitPriceInclTax = prices.UnitPriceInclTax;
            decimal scUnitPriceExclTax = prices.UnitPriceExclTax;
            decimal scSubTotalInclTax = prices.SubTotalInclTax;
            decimal scSubTotalExclTax = prices.SubTotalExclTax;
            decimal discountAmountInclTax = prices.discountAmountInclTax;
            decimal discountAmountExclTax = prices.discountAmountExclTax;

            foreach (var disc in scDiscounts)
            {
                if (!details.AppliedDiscounts.Where(x => x.DiscountId == disc.DiscountId).Any())
                    details.AppliedDiscounts.Add(disc);
            }

            //attributes
            string attributeDescription = await _productAttributeFormatter.FormatAttributes(product, sc.Attributes, details.Customer);

            if (string.IsNullOrEmpty(attributeDescription) && sc.ShoppingCartType == ShoppingCartType.Auctions)
                attributeDescription = _localizationService.GetResource("ShoppingCart.auctionwonon") + " " + product.AvailableEndDateTimeUtc;

            var itemWeight = await _shippingService.GetShoppingCartItemWeight(sc);

            var warehouseId = !string.IsNullOrEmpty(sc.WarehouseId) ? sc.WarehouseId : _storeContext.CurrentStore.DefaultWarehouseId;
            if (!product.UseMultipleWarehouses && string.IsNullOrEmpty(warehouseId))
            {
                if (!string.IsNullOrEmpty(product.WarehouseId))
                {
                    warehouseId = product.WarehouseId;
                }
            }

            var commissionRate = await PrepareCommissionRate(product, details);
            var commision = commissionRate.HasValue ?
                Math.Round((commissionRate.Value * scSubTotal / 100), 2) : 0;

            //save order item
            var orderItem = new OrderItem {
                OrderItemGuid = Guid.NewGuid(),
                ProductId = sc.ProductId,
                VendorId = product.VendorId,
                WarehouseId = warehouseId,
                SeId = details.Customer.SeId,
                UnitPriceWithoutDiscInclTax = Math.Round(scUnitPriceWithoutDiscInclTax, 6),
                UnitPriceWithoutDiscExclTax = Math.Round(scUnitPriceWithoutDiscExclTax, 6),
                UnitPriceInclTax = Math.Round(scUnitPriceInclTax, 6),
                UnitPriceExclTax = Math.Round(scUnitPriceExclTax, 6),
                PriceInclTax = Math.Round(scSubTotalInclTax, 6),
                PriceExclTax = Math.Round(scSubTotalExclTax, 6),
                OriginalProductCost = await _priceCalculationService.GetProductCost(product, sc.Attributes),
                AttributeDescription = attributeDescription,
                Attributes = sc.Attributes,
                Quantity = sc.Quantity,
                DiscountAmountInclTax = Math.Round(discountAmountInclTax, 6),
                DiscountAmountExclTax = Math.Round(discountAmountExclTax, 6),
                DownloadCount = 0,
                IsDownloadActivated = false,
                LicenseDownloadId = "",
                ItemWeight = itemWeight,
                RentalStartDateUtc = sc.RentalStartDateUtc,
                RentalEndDateUtc = sc.RentalEndDateUtc,
                CreatedOnUtc = DateTime.UtcNow,
                Commission = commision,
            };

            string reservationInfo = "";
            if (product.ProductType == ProductType.Reservation)
            {
                if (sc.RentalEndDateUtc == default(DateTime) || sc.RentalEndDateUtc == null)
                {
                    reservationInfo = sc.RentalStartDateUtc.ToString();
                }
                else
                {
                    reservationInfo = sc.RentalStartDateUtc + " - " + sc.RentalEndDateUtc;
                }
                if (!string.IsNullOrEmpty(sc.Parameter))
                {
                    reservationInfo += "<br>" + string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Option"), sc.Parameter);
                }
                if (!string.IsNullOrEmpty(sc.Duration))
                {
                    reservationInfo += "<br>" + _localizationService.GetResource("Products.Duration") + ": " + sc.Duration;
                }
            }
            if (!string.IsNullOrEmpty(reservationInfo))
            {
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    orderItem.AttributeDescription += "<br>" + reservationInfo;
                }
                else
                {
                    orderItem.AttributeDescription = reservationInfo;
                }
            }
            return orderItem;
        }

        protected virtual async Task GenerateGiftCard(PlaceOrderContainter details, ShoppingCartItem sc, Order order, OrderItem orderItem, Product product)
        {
            _productAttributeParser.GetGiftCardAttribute(sc.Attributes,
                        out string giftCardRecipientName, out string giftCardRecipientEmail,
                        out string giftCardSenderName, out string giftCardSenderEmail, out string giftCardMessage);

            for (int i = 0; i < sc.Quantity; i++)
            {
                var amount = orderItem.UnitPriceInclTax;
                if (product.OverriddenGiftCardAmount.HasValue)
                    amount = await _currencyService.ConvertFromPrimaryStoreCurrency(product.OverriddenGiftCardAmount.Value, details.Currency);

                var gc = new GiftCard {
                    GiftCardType = product.GiftCardType,
                    PurchasedWithOrderItem = orderItem,
                    Amount = amount,
                    CurrencyCode = order.CustomerCurrencyCode,
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
                await _giftCardService.InsertGiftCard(gc);
            }
        }

        protected virtual async Task UpdateProductReservation(Order order, PlaceOrderContainter details)
        {
            var reservationsToUpdate = new List<ProductReservation>();
            foreach (var sc in details.Cart.Where(x => (x.RentalStartDateUtc.HasValue && x.RentalEndDateUtc.HasValue) || !string.IsNullOrEmpty(x.ReservationId)))
            {
                var product = await _productService.GetProductById(sc.ProductId);
                if (!string.IsNullOrEmpty(sc.ReservationId))
                {
                    var reservation = await _productReservationService.GetProductReservation(sc.ReservationId);
                    reservationsToUpdate.Add(reservation);
                }

                if (sc.RentalStartDateUtc.HasValue && sc.RentalEndDateUtc.HasValue)
                {
                    var reservations = await _productReservationService.GetProductReservationsByProductId(product.Id, true, null);
                    var grouped = reservations.GroupBy(x => x.Resource);

                    IGrouping<string, ProductReservation> groupToBook = null;
                    foreach (var group in grouped)
                    {
                        bool groupCanBeBooked = true;
                        if (product.IncBothDate && product.IntervalUnitType == IntervalUnit.Day)
                        {
                            for (DateTime iterator = sc.RentalStartDateUtc.Value; iterator <= sc.RentalEndDateUtc.Value; iterator += new TimeSpan(24, 0, 0))
                            {
                                if (!group.Select(x => x.Date).Contains(iterator))
                                {
                                    groupCanBeBooked = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            for (DateTime iterator = sc.RentalStartDateUtc.Value; iterator < sc.RentalEndDateUtc.Value; iterator += new TimeSpan(24, 0, 0))
                            {
                                if (!group.Select(x => x.Date).Contains(iterator))
                                {
                                    groupCanBeBooked = false;
                                    break;
                                }
                            }
                        }
                        if (groupCanBeBooked)
                        {
                            groupToBook = group;
                            break;
                        }
                    }

                    if (groupToBook == null)
                    {
                        throw new Exception("ShoppingCart.Reservation.Nofreereservationsinthisperiod");
                    }
                    else
                    {
                        var temp = groupToBook.AsQueryable();
                        if (product.IncBothDate && product.IntervalUnitType == IntervalUnit.Day)
                        {
                            temp = temp.Where(x => x.Date >= sc.RentalStartDateUtc && x.Date <= sc.RentalEndDateUtc);
                        }
                        else
                        {
                            temp = temp.Where(x => x.Date >= sc.RentalStartDateUtc && x.Date < sc.RentalEndDateUtc);
                        }

                        foreach (var item in temp)
                        {
                            item.OrderId = order.OrderGuid.ToString();
                            await _productReservationService.UpdateProductReservation(item);
                        }

                        reservationsToUpdate.AddRange(temp);
                    }
                }
            }
            var reserved = await _productReservationService.GetCustomerReservationsHelpers(order.CustomerId);
            foreach (var res in reserved)
            {
                await _productReservationService.DeleteCustomerReservationsHelper(res);
            }

            foreach (var resToUpdate in reservationsToUpdate)
            {
                resToUpdate.OrderId = order.Id;
                await _productReservationService.UpdateProductReservation(resToUpdate);
            }

        }

        protected virtual async Task UpdateAuctionEnded(ShoppingCartItem sc, Product product, Order order)
        {
            if (product.ProductType == ProductType.Auction && sc.ShoppingCartType == ShoppingCartType.ShoppingCart)
            {
                await _auctionService.UpdateAuctionEnded(product, true, true);
                await _auctionService.UpdateHighestBid(product, product.Price, order.CustomerId);
                await _workflowMessageService.SendAuctionEndedCustomerNotificationBin(product, order.CustomerId, order.CustomerLanguageId, order.StoreId);
                await _auctionService.InsertBid(new Bid() {
                    CustomerId = order.CustomerId,
                    OrderId = order.Id,
                    Amount = product.Price,
                    Date = DateTime.UtcNow,
                    ProductId = product.Id,
                    StoreId = order.StoreId,
                    Win = true,
                    Bin = true,
                });
            }
            if (product.ProductType == ProductType.Auction && _orderSettings.UnpublishAuctionProduct)
            {
                await _productService.UnpublishProduct(product);
            }
        }

        protected virtual async Task UpdateBids(Order order, PlaceOrderContainter details)
        {
            foreach (var sc in details.Cart.Where(x => x.ShoppingCartType == ShoppingCartType.Auctions))
            {
                var bid = (await _auctionService.GetBidsByProductId(sc.Id)).Where(x => x.CustomerId == details.Customer.Id).FirstOrDefault();
                if (bid != null)
                {
                    bid.OrderId = order.Id;
                    await _auctionService.UpdateBid(bid);
                }
            }
        }
        protected virtual async Task InsertDiscountUsageHistory(Order order, PlaceOrderContainter details)
        {
            foreach (var discount in details.AppliedDiscounts)
            {
                var duh = new DiscountUsageHistory {
                    DiscountId = discount.DiscountId,
                    CouponCode = discount.CouponCode,
                    OrderId = order.Id,
                    CustomerId = order.CustomerId,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _discountService.InsertDiscountUsageHistory(duh);
            }
        }

        protected virtual async Task AppliedGiftCards(Order order, PlaceOrderContainter details)
        {
            foreach (var agc in details.AppliedGiftCards)
            {
                decimal amountUsed = agc.AmountCanBeUsed;
                var gcuh = new GiftCardUsageHistory {
                    GiftCardId = agc.GiftCard.Id,
                    UsedWithOrderId = order.Id,
                    UsedValue = amountUsed,
                    CreatedOnUtc = DateTime.UtcNow
                };
                agc.GiftCard.GiftCardUsageHistory.Add(gcuh);
                await _giftCardService.UpdateGiftCard(agc.GiftCard);
            }
        }

        /// <summary>
        /// Prepare order header
        /// </summary>
        /// <param name="processPaymentRequest">Process Payment Request</param>
        /// <param name="processPaymentResult">Process Payment Result</param>
        /// <param name="details">Place order containter</param>
        /// <returns>Order</returns>
        protected virtual Order PrepareOrderHeader(ProcessPaymentRequest processPaymentRequest, ProcessPaymentResult processPaymentResult, PlaceOrderContainter details)
        {
            var order = new Order {
                StoreId = processPaymentRequest.StoreId,
                OrderGuid = processPaymentRequest.OrderGuid,
                Code = processPaymentRequest.OrderCode,
                CustomerId = details.Customer.Id,
                OwnerId = string.IsNullOrEmpty(details.Customer.OwnerId) ? details.Customer.Id : details.Customer.OwnerId,
                SeId = details.Customer.SeId,
                CustomerLanguageId = details.CustomerLanguage.Id,
                CustomerTaxDisplayType = details.CustomerTaxDisplayType,
                CustomerIp = details.Customer.LastIpAddress,
                OrderSubtotalInclTax = Math.Round(details.OrderSubTotalInclTax, 6),
                OrderSubtotalExclTax = Math.Round(details.OrderSubTotalExclTax, 6),
                OrderSubTotalDiscountInclTax = Math.Round(details.OrderSubTotalDiscountInclTax, 6),
                OrderSubTotalDiscountExclTax = Math.Round(details.OrderSubTotalDiscountExclTax, 6),
                OrderShippingInclTax = Math.Round(details.OrderShippingTotalInclTax, 6),
                OrderShippingExclTax = Math.Round(details.OrderShippingTotalExclTax, 6),
                PaymentMethodAdditionalFeeInclTax = Math.Round(details.PaymentAdditionalFeeInclTax, 6),
                PaymentMethodAdditionalFeeExclTax = Math.Round(details.PaymentAdditionalFeeExclTax, 6),
                OrderTax = Math.Round(details.OrderTaxTotal, 6),
                OrderTotal = Math.Round(details.OrderTotal, 6),
                RefundedAmount = decimal.Zero,
                OrderDiscount = Math.Round(details.OrderDiscountAmount, 6),
                CheckoutAttributeDescription = details.CheckoutAttributeDescription,
                CheckoutAttributes = details.CheckoutAttributes,
                CustomerCurrencyCode = details.Currency.CurrencyCode,
                PrimaryCurrencyCode = details.PrimaryCurrencyCode,
                CurrencyRate = details.CustomerCurrencyRate,
                Rate = details.Currency.Rate,
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
                VatNumber = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.VatNumber),
                VatNumberStatusId = details.Customer.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.VatNumberStatusId),
                CompanyName = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Company),
                FirstName = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.FirstName),
                LastName = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastName),
                CustomerEmail = details.Customer.Email,
                UrlReferrer = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastUrlReferrer),
                ShippingOptionAttributeDescription = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.ShippingOptionAttributeDescription, processPaymentRequest.StoreId),
                ShippingOptionAttributeXml = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.ShippingOptionAttributeXml, processPaymentRequest.StoreId),
                CreatedOnUtc = DateTime.UtcNow,
            };

            foreach (var item in details.Taxes)
            {
                order.OrderTaxes.Add(item);
            }
            return order;
        }

        #region Methods
        /// <summary>
        /// Save order details
        /// </summary>
        /// <param name="details">Place order containter</param>
        /// <param name="order">Order</param>
        /// <returns>Order</returns>
        public virtual async Task<Order> SaveOrderDetails(PlaceOrderContainter details, Order order)
        {
            //move shopping cart items to order items
            foreach (var sc in details.Cart)
            {
                var product = await _productService.GetProductById(sc.ProductId);

                var orderItem = await PrepareOrderItem(sc, product, details);

                order.OrderItems.Add(orderItem);

                //gift cards
                if (product.IsGiftCard)
                {
                    await GenerateGiftCard(details, sc, order, orderItem, product);
                }

                //update auction ended
                await UpdateAuctionEnded(sc, product, order);

                //inventory
                await _inventoryManageService.AdjustInventory(product, -sc.Quantity, sc.Attributes, orderItem.WarehouseId);

                //update sold
                await _productService.UpdateSold(sc.ProductId, sc.Quantity);
            }

            //insert order
            await _orderService.InsertOrder(order);

            //update reservation
            await UpdateProductReservation(order, details);

            //update bids
            await UpdateBids(order, details);

            //clear shopping cart
            await _customerService.ClearShoppingCartItem(order.CustomerId, details.Cart);

            //product also purchased
            await _orderService.InsertProductAlsoPurchased(order);

            //discount usage history
            await InsertDiscountUsageHistory(order, details);

            //gift card usage history
            if (details.AppliedGiftCards != null)
                await AppliedGiftCards(order, details);

            //reset checkout data
            await _customerService.ResetCheckoutData(details.Customer, order.StoreId, clearCouponCodes: true, clearCheckoutAttributes: true);

            return order;
        }

        /// <summary>
        /// Send notification order 
        /// </summary>
        /// <param name="order">Order</param>
        public virtual async Task SendNotification(Order order)
        {
            //notes, messages
            if (_workContext.OriginalCustomerIfImpersonated != null)
            {
                //this order is placed by a store administrator impersonating a customer
                await _orderService.InsertOrderNote(new OrderNote {
                    Note = string.Format("Order placed by a store owner ('{0}'. ID = {1}) impersonating the customer.",
                        _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                });
            }
            else
            {
                await _orderService.InsertOrderNote(new OrderNote {
                    Note = "Order placed",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,

                });
            }

            //send email notifications
            int orderPlacedStoreOwnerNotificationQueuedEmailId = await _workflowMessageService.SendOrderPlacedStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
            if (orderPlacedStoreOwnerNotificationQueuedEmailId > 0)
            {
                await _orderService.InsertOrderNote(new OrderNote {
                    Note = "\"Order placed\" email (to store owner) has been queued",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,

                });
            }

            var orderPlacedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail && !_orderSettings.AttachPdfInvoiceToBinary ?
                await _pdfService.PrintOrderToPdf(order, order.CustomerLanguageId) : null;
            var orderPlacedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail && !_orderSettings.AttachPdfInvoiceToBinary ?
                "order.pdf" : null;
            var orderPlacedAttachments = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail && _orderSettings.AttachPdfInvoiceToBinary ?
                new List<string> { await _pdfService.SaveOrderToBinary(order, order.CustomerLanguageId) } : new List<string>();

            int orderPlacedCustomerNotificationQueuedEmailId = await _workflowMessageService
                .SendOrderPlacedCustomerNotification(order, order.CustomerLanguageId, orderPlacedAttachmentFilePath, orderPlacedAttachmentFileName, orderPlacedAttachments);
            if (orderPlacedCustomerNotificationQueuedEmailId > 0)
            {
                await _orderService.InsertOrderNote(new OrderNote {
                    Note = "\"Order placed\" email (to customer) has been queued",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,

                });
            }
            if (order.OrderItems.Any(x => !string.IsNullOrEmpty(x.VendorId)))
            {
                var vendors = await _mediator.Send(new GetVendorsInOrderQuery() { Order = order });
                foreach (var vendor in vendors)
                {
                    int orderPlacedVendorNotificationQueuedEmailId = await _workflowMessageService.SendOrderPlacedVendorNotification(order, vendor, _localizationSettings.DefaultAdminLanguageId);
                    if (orderPlacedVendorNotificationQueuedEmailId > 0)
                    {
                        await _orderService.InsertOrderNote(new OrderNote {
                            Note = "\"Order placed\" email (to vendor) has been queued",
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id,
                        });
                    }
                }
            }
        }
        /// <summary>
        /// Places an order
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Place order result</returns>
        public virtual async Task<PlaceOrderResult> PlaceOrder(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentNullException("processPaymentRequest");

            var result = new PlaceOrderResult();
            try
            {
                if (processPaymentRequest.OrderGuid == Guid.Empty)
                    processPaymentRequest.OrderGuid = Guid.NewGuid();

                if (string.IsNullOrEmpty(processPaymentRequest.OrderCode))
                    processPaymentRequest.OrderCode = await _mediator.Send(new PrepareOrderCodeCommand());

                //prepare order details
                var details = !processPaymentRequest.IsRecurringPayment ?
                    await PreparePlaceOrderDetails(processPaymentRequest) :
                    await PreparePlaceOrderDetailsForRecurringPayment(processPaymentRequest);

                //event notification
                await _mediator.PlaceOrderDetailsEvent(result, details);

                //return if exist errors
                if (result.Errors.Any())
                    return result;

                #region Payment workflow

                var processPaymentResult = await PrepareProcessPaymentResult(processPaymentRequest, details);

                #endregion

                if (processPaymentResult.Success)
                {
                    #region Save order details

                    var order = PrepareOrderHeader(processPaymentRequest, processPaymentResult, details);

                    if (!processPaymentRequest.IsRecurringPayment)
                    {
                        result.PlacedOrder = await SaveOrderDetails(details, order);
                    }
                    else
                    {
                        result.PlacedOrder = await SaveOrderDetailsForReccuringPayment(details, order);

                    }
                    //recurring orders
                    if (details.IsRecurringShoppingCart && !processPaymentRequest.IsRecurringPayment)
                    {
                        //create recurring payment (the first payment)
                        await CreateRecurringPayment(processPaymentRequest, order);
                    }
                    //reward points history
                    if (details.RedeemedRewardPointsAmount > decimal.Zero)
                    {
                        var rph = await _rewardPointsService.AddRewardPointsHistory(details.Customer.Id,
                            -details.RedeemedRewardPoints, order.StoreId,
                            string.Format(_localizationService.GetResource("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId), order.OrderNumber),
                            order.Id, details.RedeemedRewardPointsAmount);
                        order.RewardPointsWereAdded = true;
                        order.RedeemedRewardPointsEntry = rph;
                        await _orderService.UpdateOrder(order);
                    }

                    #endregion

                    #region Notifications & notes

                    await SendNotification(order);

                    //check order status
                    await _mediator.Send(new CheckOrderStatusCommand() { Order = order });

                    //update customer 
                    await UpdateCustomer(order, details);

                    //raise event       
                    await _mediator.Publish(new OrderPlacedEvent(order));

                    if (order.PaymentStatus == PaymentStatus.Paid)
                    {
                        await _mediator.Send(new ProcessOrderPaidCommand() { Order = order });
                    }

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
            if (!string.IsNullOrEmpty(error))
            {
                //log it
                string logError = string.Format("Error while placing order. {0}", error);
                _logger.Error(logError);
            }

            #endregion

            return result;
        }

        #endregion
    }
}
