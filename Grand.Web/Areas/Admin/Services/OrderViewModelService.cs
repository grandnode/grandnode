using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Tax;
using Grand.Framework.Extensions;
using Grand.Services.Affiliates;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Security;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class OrderViewModelService : IOrderViewModelService
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IOrderReportService _orderReportService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IEncryptionService _encryptionService;
        private readonly IPaymentService _paymentService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IProductService _productService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IGiftCardService _giftCardService;
        private readonly IDownloadService _downloadService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IShippingService _shippingService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAffiliateService _affiliateService;
        private readonly IPictureService _pictureService;
        private readonly ITaxService _taxService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly CurrencySettings _currencySettings;
        private readonly TaxSettings _taxSettings;
        private readonly AddressSettings _addressSettings;
        #endregion

        #region Ctor

        public OrderViewModelService(IOrderService orderService,
            IOrderReportService orderReportService,
            IOrderProcessingService orderProcessingService,
            IPriceCalculationService priceCalculationService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            IDiscountService discountService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ICurrencyService currencyService,
            IEncryptionService encryptionService,
            IPaymentService paymentService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IProductService productService,
            IWorkflowMessageService workflowMessageService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IGiftCardService giftCardService,
            IDownloadService downloadService,
            IShippingService shippingService,
            IStoreService storeService,
            IVendorService vendorService,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAffiliateService affiliateService,
            IPictureService pictureService,
            ITaxService taxService,
            IReturnRequestService returnRequestService,
            ICustomerService customerService,
            ICustomerActivityService customerActivityService,
            CurrencySettings currencySettings,
            TaxSettings taxSettings,
            AddressSettings addressSettings)
        {
            this._orderService = orderService;
            this._orderReportService = orderReportService;
            this._orderProcessingService = orderProcessingService;
            this._priceCalculationService = priceCalculationService;
            this._dateTimeHelper = dateTimeHelper;
            this._priceFormatter = priceFormatter;
            this._discountService = discountService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._currencyService = currencyService;
            this._encryptionService = encryptionService;
            this._paymentService = paymentService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._productService = productService;
            this._workflowMessageService = workflowMessageService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._giftCardService = giftCardService;
            this._downloadService = downloadService;
            this._shippingService = shippingService;
            this._storeService = storeService;
            this._vendorService = vendorService;
            this._addressAttributeParser = addressAttributeParser;
            this._addressAttributeService = addressAttributeService;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._affiliateService = affiliateService;
            this._pictureService = pictureService;
            this._taxService = taxService;
            this._returnRequestService = returnRequestService;
            this._customerActivityService = customerActivityService;
            this._currencySettings = currencySettings;
            this._taxSettings = taxSettings;
            this._addressSettings = addressSettings;
            this._customerService = customerService;
        }

        #endregion

        public virtual OrderListModel PrepareOrderListModel(int? orderStatusId = null, int? paymentStatusId = null, int? shippingStatusId = null, DateTime? startDate = null)
        {
            //order statuses
            var model = new OrderListModel();
            model.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            if (orderStatusId.HasValue)
            {
                //pre-select value?
                var item = model.AvailableOrderStatuses.FirstOrDefault(x => x.Value == orderStatusId.Value.ToString());
                if (item != null)
                    item.Selected = true;
            }

            //payment statuses
            model.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            if (paymentStatusId.HasValue)
            {
                //pre-select value?
                var item = model.AvailablePaymentStatuses.FirstOrDefault(x => x.Value == paymentStatusId.Value.ToString());
                if (item != null)
                    item.Selected = true;
            }

            //shipping statuses
            model.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();
            model.AvailableShippingStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            if (shippingStatusId.HasValue)
            {
                //pre-select value?
                var item = model.AvailableShippingStatuses.FirstOrDefault(x => x.Value == shippingStatusId.Value.ToString());
                if (item != null)
                    item.Selected = true;
            }

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var w in _shippingService.GetAllWarehouses())
                model.AvailableWarehouses.Add(new SelectListItem { Text = w.Name, Value = w.Id.ToString() });

            //payment methods
            model.AvailablePaymentMethods.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var pm in _paymentService.LoadAllPaymentMethods())
                model.AvailablePaymentMethods.Add(new SelectListItem { Text = pm.PluginDescriptor.FriendlyName, Value = pm.PluginDescriptor.SystemName });

            //billing countries
            foreach (var c in _countryService.GetAllCountriesForBilling(showHidden: true))
            {
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            }
            model.AvailableCountries.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });

            //a vendor should have access only to orders with his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            if (startDate.HasValue)
                model.StartDate = startDate.Value;

            return model;
        }
        public virtual (IEnumerable<OrderModel> orderModels, OrderAggreratorModel aggreratorModel, int totalCount) PrepareOrderModel(OrderListModel model, int pageIndex, int pageSize)
        {
            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;

            var filterByProductId = "";
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && _workContext.HasAccessToProduct(product))
                filterByProductId = model.ProductId;

            //load orders
            var orders = _orderService.SearchOrders(storeId: model.StoreId,
                vendorId: model.VendorId,
                productId: filterByProductId,
                warehouseId: model.WarehouseId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                os: orderStatus,
                ps: paymentStatus,
                ss: shippingStatus,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderGuid: model.OrderGuid,
                pageIndex: pageIndex - 1,
                pageSize: pageSize);



            //summary report
            //currently we do not support productId and warehouseId parameters for this report
            var reportSummary = _orderReportService.GetOrderAverageReportLine(
                storeId: model.StoreId,
                vendorId: model.VendorId,
                orderId: "",
                paymentMethodSystemName: model.PaymentMethodSystemName,
                os: orderStatus,
                ps: paymentStatus,
                ss: shippingStatus,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId
                );
            var profit = _orderReportService.ProfitReport(
                storeId: model.StoreId,
                vendorId: model.VendorId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                os: orderStatus,
                ps: paymentStatus,
                ss: shippingStatus,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId
                );
            var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (primaryStoreCurrency == null)
                throw new Exception("Cannot load primary store currency");

            var aggregate = new OrderAggreratorModel
            {
                aggregatorprofit = _priceFormatter.FormatPrice(profit, true, false),
                aggregatorshipping = _priceFormatter.FormatShippingPrice(reportSummary.SumShippingExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false),
                aggregatortax = _priceFormatter.FormatPrice(reportSummary.SumTax, true, false),
                aggregatortotal = _priceFormatter.FormatPrice(reportSummary.SumOrders, true, false)
            };

            return (orders.Select(x =>
            {
                var store = _storeService.GetStoreById(x.StoreId);
                return new OrderModel
                {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber,
                    StoreName = store != null ? store.Name : "Unknown",
                    OrderTotal = _priceFormatter.FormatPrice(x.OrderTotal, true, false),
                    OrderStatus = x.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                    PaymentStatus = x.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
                    ShippingStatus = x.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
                    CustomerEmail = x.BillingAddress?.Email,
                    CustomerId = x.CustomerId,
                    CustomerFullName = string.Format("{0} {1}", x.BillingAddress?.FirstName, x.BillingAddress?.LastName),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                };
            }), aggregate, orders.TotalCount);
        }

       
        public virtual void PrepareOrderDetailsModel(OrderModel model, Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (model == null)
                throw new ArgumentNullException("model");

            model.Id = order.Id;
            model.OrderNumber = order.OrderNumber;
            model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.OrderStatusId = order.OrderStatusId;
            model.OrderGuid = order.OrderGuid;
            var store = _storeService.GetStoreById(order.StoreId);
            model.StoreName = store != null ? store.Name : "Unknown";
            model.CustomerId = order.CustomerId;

            var customer = _customerService.GetCustomerById(order.CustomerId);
            model.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
            model.CustomerIp = order.CustomerIp;
            model.UrlReferrer = order.UrlReferrer;
            model.VatNumber = order.VatNumber;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
            model.AllowCustomersToSelectTaxDisplayType = _taxSettings.AllowCustomersToSelectTaxDisplayType;
            model.TaxDisplayType = _taxSettings.TaxDisplayType;

            var affiliate = _affiliateService.GetAffiliateById(order.AffiliateId);
            if (affiliate != null)
            {
                model.AffiliateId = affiliate.Id;
                model.AffiliateName = affiliate.GetFullName();
            }

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            //custom values
            model.CustomValues = order.DeserializeCustomValues();

            #region Order totals

            var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (primaryStoreCurrency == null)
                throw new Exception("Cannot load primary store currency");

            //subtotal
            model.OrderSubtotalInclTax = _priceFormatter.FormatPrice(order.OrderSubtotalInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            model.OrderSubtotalExclTax = _priceFormatter.FormatPrice(order.OrderSubtotalExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            model.OrderSubtotalInclTaxValue = order.OrderSubtotalInclTax;
            model.OrderSubtotalExclTaxValue = order.OrderSubtotalExclTax;
            //discount (applied to order subtotal)
            string orderSubtotalDiscountInclTaxStr = _priceFormatter.FormatPrice(order.OrderSubTotalDiscountInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            string orderSubtotalDiscountExclTaxStr = _priceFormatter.FormatPrice(order.OrderSubTotalDiscountExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            if (order.OrderSubTotalDiscountInclTax > decimal.Zero)
                model.OrderSubTotalDiscountInclTax = orderSubtotalDiscountInclTaxStr;
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
                model.OrderSubTotalDiscountExclTax = orderSubtotalDiscountExclTaxStr;
            model.OrderSubTotalDiscountInclTaxValue = order.OrderSubTotalDiscountInclTax;
            model.OrderSubTotalDiscountExclTaxValue = order.OrderSubTotalDiscountExclTax;

            //shipping
            model.OrderShippingInclTax = _priceFormatter.FormatShippingPrice(order.OrderShippingInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            model.OrderShippingExclTax = _priceFormatter.FormatShippingPrice(order.OrderShippingExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            model.OrderShippingInclTaxValue = order.OrderShippingInclTax;
            model.OrderShippingExclTaxValue = order.OrderShippingExclTax;

            //payment method additional fee
            if (order.PaymentMethodAdditionalFeeInclTax > decimal.Zero)
            {
                model.PaymentMethodAdditionalFeeInclTax = _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
                model.PaymentMethodAdditionalFeeExclTax = _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            }
            model.PaymentMethodAdditionalFeeInclTaxValue = order.PaymentMethodAdditionalFeeInclTax;
            model.PaymentMethodAdditionalFeeExclTaxValue = order.PaymentMethodAdditionalFeeExclTax;


            //tax
            model.Tax = _priceFormatter.FormatPrice(order.OrderTax, true, false);
            SortedDictionary<decimal, decimal> taxRates = order.TaxRatesDictionary;
            bool displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count > 0;
            bool displayTax = !displayTaxRates;
            foreach (var tr in order.TaxRatesDictionary)
            {
                model.TaxRates.Add(new OrderModel.TaxRate
                {
                    Rate = _priceFormatter.FormatTaxRate(tr.Key),
                    Value = _priceFormatter.FormatPrice(tr.Value, true, false),
                });
            }
            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
            model.TaxValue = order.OrderTax;
            model.TaxRatesValue = order.TaxRates;

            //discount
            if (order.OrderDiscount > 0)
                model.OrderTotalDiscount = _priceFormatter.FormatPrice(-order.OrderDiscount, true, false);
            model.OrderTotalDiscountValue = order.OrderDiscount;

            //gift cards
            foreach (var gcuh in _giftCardService.GetAllGiftCardUsageHistory(order.Id))
            {
                var giftCard = _giftCardService.GetGiftCardById(gcuh.GiftCardId);
                if (giftCard != null)
                {
                    model.GiftCards.Add(new OrderModel.GiftCard
                    {
                        CouponCode = giftCard.GiftCardCouponCode,
                        Amount = _priceFormatter.FormatPrice(-gcuh.UsedValue, true, false),
                    });
                }
            }

            //reward points
            if (order.RedeemedRewardPointsEntry != null)
            {
                model.RedeemedRewardPoints = -order.RedeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-order.RedeemedRewardPointsEntry.UsedAmount, true, false);
            }

            //total
            model.OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false);
            model.OrderTotalValue = order.OrderTotal;

            //refunded amount
            if (order.RefundedAmount > decimal.Zero)
                model.RefundedAmount = _priceFormatter.FormatPrice(order.RefundedAmount, true, false);

            //used discounts
            var duh = _discountService.GetAllDiscountUsageHistory(orderId: order.Id);
            foreach (var d in duh)
            {
                var discount = _discountService.GetDiscountById(d.DiscountId);
                if (discount != null)
                {
                    model.UsedDiscounts.Add(new OrderModel.UsedDiscountModel
                    {
                        DiscountId = d.DiscountId,
                        DiscountName = discount.Name,
                    });
                }
            }

            //profit (hide for vendors)
            if (_workContext.CurrentVendor == null)
            {
                var profit = _orderReportService.ProfitReport(orderId: order.Id);
                model.Profit = _priceFormatter.FormatPrice(profit, true, false);
            }

            #endregion

            #region Payment info

            if (order.AllowStoringCreditCardNumber)
            {
                //card type
                model.CardType = _encryptionService.DecryptText(order.CardType);
                //cardholder name
                model.CardName = _encryptionService.DecryptText(order.CardName);
                //card number
                model.CardNumber = _encryptionService.DecryptText(order.CardNumber);
                //cvv
                model.CardCvv2 = _encryptionService.DecryptText(order.CardCvv2);
                //expiry date
                string cardExpirationMonthDecrypted = _encryptionService.DecryptText(order.CardExpirationMonth);
                if (!String.IsNullOrEmpty(cardExpirationMonthDecrypted) && cardExpirationMonthDecrypted != "0")
                    model.CardExpirationMonth = cardExpirationMonthDecrypted;
                string cardExpirationYearDecrypted = _encryptionService.DecryptText(order.CardExpirationYear);
                if (!String.IsNullOrEmpty(cardExpirationYearDecrypted) && cardExpirationYearDecrypted != "0")
                    model.CardExpirationYear = cardExpirationYearDecrypted;

                model.AllowStoringCreditCardNumber = true;
            }
            else
            {
                string maskedCreditCardNumberDecrypted = _encryptionService.DecryptText(order.MaskedCreditCardNumber);
                if (!String.IsNullOrEmpty(maskedCreditCardNumberDecrypted))
                    model.CardNumber = maskedCreditCardNumberDecrypted;
            }


            //payment transaction info
            model.AuthorizationTransactionId = order.AuthorizationTransactionId;
            model.CaptureTransactionId = order.CaptureTransactionId;
            model.SubscriptionTransactionId = order.SubscriptionTransactionId;

            //payment method info
            var pm = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            model.PaymentMethod = pm != null ? pm.PluginDescriptor.FriendlyName : order.PaymentMethodSystemName;
            model.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);

            //payment method buttons
            model.CanCancelOrder = _orderProcessingService.CanCancelOrder(order);
            model.CanCapture = _orderProcessingService.CanCapture(order);
            model.CanMarkOrderAsPaid = _orderProcessingService.CanMarkOrderAsPaid(order);
            model.CanRefund = _orderProcessingService.CanRefund(order);
            model.CanRefundOffline = _orderProcessingService.CanRefundOffline(order);
            model.CanPartiallyRefund = _orderProcessingService.CanPartiallyRefund(order, decimal.Zero);
            model.CanPartiallyRefundOffline = _orderProcessingService.CanPartiallyRefundOffline(order, decimal.Zero);
            model.CanVoid = _orderProcessingService.CanVoid(order);
            model.CanVoidOffline = _orderProcessingService.CanVoidOffline(order);

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.MaxAmountToRefund = order.OrderTotal - order.RefundedAmount;

            //recurring payment record
            var recurringPayment = _orderService.SearchRecurringPayments(initialOrderId: order.Id, showHidden: true).FirstOrDefault();
            if (recurringPayment != null)
            {
                model.RecurringPaymentId = recurringPayment.Id;
            }
            #endregion

            #region Billing & shipping info

            model.BillingAddress = order.BillingAddress.ToModel();
            model.BillingAddress.FormattedCustomAddressAttributes = _addressAttributeFormatter.FormatAttributes(order.BillingAddress.CustomAttributes);
            model.BillingAddress.FirstNameEnabled = true;
            model.BillingAddress.FirstNameRequired = true;
            model.BillingAddress.LastNameEnabled = true;
            model.BillingAddress.LastNameRequired = true;
            model.BillingAddress.EmailEnabled = true;
            model.BillingAddress.EmailRequired = true;
            model.BillingAddress.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.BillingAddress.CompanyRequired = _addressSettings.CompanyRequired;
            model.BillingAddress.VatNumberEnabled = _addressSettings.VatNumberEnabled;
            model.BillingAddress.VatNumberRequired = _addressSettings.VatNumberRequired;
            model.BillingAddress.CountryEnabled = _addressSettings.CountryEnabled;
            model.BillingAddress.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.BillingAddress.CityEnabled = _addressSettings.CityEnabled;
            model.BillingAddress.CityRequired = _addressSettings.CityRequired;
            model.BillingAddress.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.BillingAddress.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.BillingAddress.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.BillingAddress.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.BillingAddress.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.BillingAddress.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.BillingAddress.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.BillingAddress.PhoneRequired = _addressSettings.PhoneRequired;
            model.BillingAddress.FaxEnabled = _addressSettings.FaxEnabled;
            model.BillingAddress.FaxRequired = _addressSettings.FaxRequired;

            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext);
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                model.IsShippable = true;

                model.PickUpInStore = order.PickUpInStore;
                if (!order.PickUpInStore)
                {
                    if (order.ShippingAddress != null)
                    {
                        model.ShippingAddress = order.ShippingAddress.ToModel();
                        model.ShippingAddress.FormattedCustomAddressAttributes = _addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes);
                        model.ShippingAddress.FirstNameEnabled = true;
                        model.ShippingAddress.FirstNameRequired = true;
                        model.ShippingAddress.LastNameEnabled = true;
                        model.ShippingAddress.LastNameRequired = true;
                        model.ShippingAddress.EmailEnabled = true;
                        model.ShippingAddress.EmailRequired = true;
                        model.ShippingAddress.CompanyEnabled = _addressSettings.CompanyEnabled;
                        model.ShippingAddress.CompanyRequired = _addressSettings.CompanyRequired;
                        model.ShippingAddress.VatNumberEnabled = _addressSettings.VatNumberEnabled;
                        model.ShippingAddress.VatNumberRequired = _addressSettings.VatNumberRequired;
                        model.ShippingAddress.CountryEnabled = _addressSettings.CountryEnabled;
                        model.ShippingAddress.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
                        model.ShippingAddress.CityEnabled = _addressSettings.CityEnabled;
                        model.ShippingAddress.CityRequired = _addressSettings.CityRequired;
                        model.ShippingAddress.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
                        model.ShippingAddress.StreetAddressRequired = _addressSettings.StreetAddressRequired;
                        model.ShippingAddress.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
                        model.ShippingAddress.StreetAddress2Required = _addressSettings.StreetAddress2Required;
                        model.ShippingAddress.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
                        model.ShippingAddress.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
                        model.ShippingAddress.PhoneEnabled = _addressSettings.PhoneEnabled;
                        model.ShippingAddress.PhoneRequired = _addressSettings.PhoneRequired;
                        model.ShippingAddress.FaxEnabled = _addressSettings.FaxEnabled;
                        model.ShippingAddress.FaxRequired = _addressSettings.FaxRequired;

                        model.ShippingAddressGoogleMapsUrl = string.Format("http://maps.google.com/maps?f=q&hl=en&ie=UTF8&oe=UTF8&geocode=&q={0}", WebUtility.UrlEncode(order.ShippingAddress.Address1 + " " + order.ShippingAddress.ZipPostalCode + " " + order.ShippingAddress.City + " " + (!String.IsNullOrEmpty(order.ShippingAddress.CountryId) ? _countryService.GetCountryById(order.ShippingAddress.CountryId)?.Name : "")));
                    }
                }
                else
                {
                    if (order.PickupPoint != null)
                    {
                        if (order.PickupPoint.Address != null)
                        {
                            model.PickupAddress = order.PickupPoint.Address.ToModel();
                            var country = _countryService.GetCountryById(order.PickupPoint.Address.CountryId);
                            if (country != null)
                                model.PickupAddress.CountryName = country.Name;
                        }
                    }
                }
                model.ShippingMethod = order.ShippingMethod;
                model.ShippingAdditionDescription = order.ShippingOptionAttributeDescription;
                model.CanAddNewShipments = false;

                foreach (var orderItem in order.OrderItems)
                {
                    //we can ship only shippable products
                    var product = _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                    if (product != null)
                    {
                        if (!product.IsShipEnabled)
                            continue;
                    }
                    var totalNumberOfItemsCanBeAddedToShipment = orderItem.GetTotalNumberOfItemsCanBeAddedToShipment();
                    if (totalNumberOfItemsCanBeAddedToShipment <= 0)
                        continue;
                    model.CanAddNewShipments = true;
                }

            }

            #endregion

            #region Products

            model.CheckoutAttributeInfo = order.CheckoutAttributeDescription;
            bool hasDownloadableItems = false;
            var products = order.OrderItems;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                products = products
                    .Where(orderItem => orderItem.VendorId == _workContext.CurrentVendor.Id)
                    .ToList();
            }
            foreach (var orderItem in products)
            {
                var product = _productService.GetProductByIdIncludeArch(orderItem.ProductId);

                if (product != null)
                {

                    if (product.IsDownload)
                        hasDownloadableItems = true;

                    var orderItemModel = new OrderModel.OrderItemModel
                    {
                        Id = orderItem.Id,
                        ProductId = orderItem.ProductId,
                        ProductName = product.Name,
                        Sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser),
                        Quantity = orderItem.Quantity,
                        IsDownload = product.IsDownload,
                        DownloadCount = orderItem.DownloadCount,
                        DownloadActivationType = product.DownloadActivationType,
                        IsDownloadActivated = orderItem.IsDownloadActivated
                    };
                    //picture
                    var orderItemPicture = product.GetProductPicture(orderItem.AttributesXml, _pictureService, _productAttributeParser);
                    orderItemModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(orderItemPicture, 75, true);

                    //license file
                    if (!String.IsNullOrEmpty(orderItem.LicenseDownloadId))
                    {
                        var licenseDownload = _downloadService.GetDownloadById(orderItem.LicenseDownloadId);
                        if (licenseDownload != null)
                        {
                            orderItemModel.LicenseDownloadGuid = licenseDownload.DownloadGuid;
                        }
                    }
                    //vendor
                    var vendor = _vendorService.GetVendorById(orderItem.VendorId);
                    orderItemModel.VendorName = vendor != null ? vendor.Name : "";

                    //unit price
                    orderItemModel.UnitPriceInclTaxValue = orderItem.UnitPriceInclTax;
                    orderItemModel.UnitPriceExclTaxValue = orderItem.UnitPriceExclTax;
                    orderItemModel.UnitPriceInclTax = _priceFormatter.FormatPrice(orderItem.UnitPriceInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                    orderItemModel.UnitPriceExclTax = _priceFormatter.FormatPrice(orderItem.UnitPriceExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);
                    //discounts
                    orderItemModel.DiscountInclTaxValue = orderItem.DiscountAmountInclTax;
                    orderItemModel.DiscountExclTaxValue = orderItem.DiscountAmountExclTax;
                    orderItemModel.DiscountInclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                    orderItemModel.DiscountExclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);
                    //subtotal
                    orderItemModel.SubTotalInclTaxValue = orderItem.PriceInclTax;
                    orderItemModel.SubTotalExclTaxValue = orderItem.PriceExclTax;
                    orderItemModel.SubTotalInclTax = _priceFormatter.FormatPrice(orderItem.PriceInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                    orderItemModel.SubTotalExclTax = _priceFormatter.FormatPrice(orderItem.PriceExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);

                    orderItemModel.AttributeInfo = orderItem.AttributeDescription;
                    if (product.IsRecurring)
                        orderItemModel.RecurringInfo = string.Format(_localizationService.GetResource("Admin.Orders.Products.RecurringPeriod"), product.RecurringCycleLength, product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));

                    //return requests
                    orderItemModel.ReturnRequestIds = _returnRequestService.SearchReturnRequests(orderItemId: orderItem.Id)
                        .Select(rr => rr.Id).ToList();
                    //gift cards
                    orderItemModel.PurchasedGiftCardIds = _giftCardService.GetGiftCardsByPurchasedWithOrderItemId(orderItem.Id)
                        .Select(gc => gc.Id).ToList();

                    model.Items.Add(orderItemModel);
                }
            }
            model.HasDownloadableProducts = hasDownloadableItems;
            #endregion
        }
        
        public virtual OrderModel.AddOrderProductModel PrepareAddOrderProductModel(Order order)
        {
            var model = new OrderModel.AddOrderProductModel();
            model.OrderId = order.Id;
            model.OrderNumber = order.OrderNumber;
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });

            return model;
        }

        public virtual OrderModel.AddOrderProductModel.ProductDetailsModel PrepareAddProductToOrderModel(string orderId, string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            var customer = _customerService.GetCustomerById(order.CustomerId);

            var presetQty = 1;
            var presetPrice = _priceCalculationService.GetFinalPrice(product, customer, decimal.Zero, true, presetQty);
            decimal taxRate;
            decimal presetPriceInclTax = _taxService.GetProductPrice(product, presetPrice, true, customer, out taxRate);
            decimal presetPriceExclTax = _taxService.GetProductPrice(product, presetPrice, false, customer, out taxRate);


            var model = new OrderModel.AddOrderProductModel.ProductDetailsModel
            {
                ProductId = productId,
                OrderId = orderId,
                OrderNumber = order.OrderNumber,
                Name = product.Name,
                ProductType = product.ProductType,
                UnitPriceExclTax = presetPriceExclTax,
                UnitPriceInclTax = presetPriceInclTax,
                Quantity = presetQty,
                SubTotalExclTax = presetPriceExclTax,
                SubTotalInclTax = presetPriceInclTax
            };

            //attributes
            var attributes = product.ProductAttributeMappings;
            foreach (var attribute in attributes)
            {
                var productAttribute = _productAttributeService.GetProductAttributeById(attribute.ProductAttributeId);
                var attributeModel = new OrderModel.AddOrderProductModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = productAttribute.Name,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = attribute.ProductAttributeValues;
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new OrderModel.AddOrderProductModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                model.ProductAttributes.Add(attributeModel);
            }
            //gift card
            model.GiftCard.IsGiftCard = product.IsGiftCard;
            if (model.GiftCard.IsGiftCard)
            {
                model.GiftCard.GiftCardType = product.GiftCardType;
            }
            return model;
        }
        public virtual OrderAddressModel PrepareOrderAddressModel(Order order, Address address)
        {
            var model = new OrderAddressModel();
            model.OrderId = order.Id;
            model.Address = address.ToModel();
            model.Address.Id = address.Id;
            model.Address.FirstNameEnabled = true;
            model.Address.FirstNameRequired = true;
            model.Address.LastNameEnabled = true;
            model.Address.LastNameRequired = true;
            model.Address.EmailEnabled = true;
            model.Address.EmailRequired = true;
            model.Address.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.Address.CompanyRequired = _addressSettings.CompanyRequired;
            model.Address.VatNumberEnabled = _addressSettings.VatNumberEnabled;
            model.Address.VatNumberRequired = _addressSettings.VatNumberRequired;
            model.Address.CountryEnabled = _addressSettings.CountryEnabled;
            model.Address.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.Address.CityEnabled = _addressSettings.CityEnabled;
            model.Address.CityRequired = _addressSettings.CityRequired;
            model.Address.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.Address.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.Address.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.Address.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.Address.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.Address.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.Address.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.Address.PhoneRequired = _addressSettings.PhoneRequired;
            model.Address.FaxEnabled = _addressSettings.FaxEnabled;
            model.Address.FaxRequired = _addressSettings.FaxRequired;

            //countries
            model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == address.CountryId) });
            //states
            var states = !String.IsNullOrEmpty(address.CountryId) ? _stateProvinceService.GetStateProvincesByCountryId(address.CountryId, showHidden: true).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "" });
            //customer attribute services
            model.Address.PrepareCustomAddressAttributes(address, _addressAttributeService, _addressAttributeParser);

            return model;
        }

        public virtual IList<OrderModel.OrderNote> PrepareOrderNotes(Order order)
        {
            //order notes
            var orderNoteModels = new List<OrderModel.OrderNote>();
            foreach (var orderNote in _orderService.GetOrderNotes(order.Id)
                .OrderByDescending(on => on.CreatedOnUtc))
            {
                var download = _downloadService.GetDownloadById(orderNote.DownloadId);
                orderNoteModels.Add(new OrderModel.OrderNote
                {
                    Id = orderNote.Id,
                    OrderId = order.Id,
                    DownloadId = String.IsNullOrEmpty(orderNote.DownloadId) ? "" : orderNote.DownloadId,
                    DownloadGuid = download != null ? download.DownloadGuid : Guid.Empty,
                    DisplayToCustomer = orderNote.DisplayToCustomer,
                    Note = orderNote.FormatOrderNoteText(),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc),
                    CreatedByCustomer = orderNote.CreatedByCustomer
                });
            }
            return orderNoteModels;
        }

        public virtual void InsertOrderNote(Order order, string downloadId, bool displayToCustomer, string message)
        {
            var orderNote = new OrderNote
            {
                DisplayToCustomer = displayToCustomer,
                Note = message,
                DownloadId = downloadId,
                OrderId = order.Id,
                CreatedOnUtc = DateTime.UtcNow,
            };
            _orderService.InsertOrderNote(orderNote);

            //new order notification
            if (displayToCustomer)
            {
                //email
                _workflowMessageService.SendNewOrderNoteAddedCustomerNotification(
                    orderNote, _workContext.WorkingLanguage.Id);

            }
        }

        public virtual void DeleteOrderNote(Order order, string id  )
        {
            var orderNote = _orderService.GetOrderNotes(order.Id).FirstOrDefault(on => on.Id == id);
            if (orderNote == null)
                throw new ArgumentException("No order note found with the specified id");

            orderNote.OrderId = order.Id;
            _orderService.DeleteOrderNote(orderNote);

        }

        public virtual void LogEditOrder(string orderId)
        {
            _customerActivityService.InsertActivity("EditOrder", orderId, _localizationService.GetResource("ActivityLog.EditOrder"), orderId);
        }
        public virtual Address UpdateOrderAddress(Order order, Address address, OrderAddressModel model, string customAttributes)
        {
            address = model.Address.ToEntity(address);
            address.CustomAttributes = customAttributes;
            _orderService.UpdateOrder(order);

            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                Note = "Address has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });
            LogEditOrder(order.Id);
            return address;
        }
        public virtual IList<string> AddProductToOrderDetails(string orderId, string productId, IFormCollection form)
        {
            var order = _orderService.GetOrderById(orderId);
            var product = _productService.GetProductById(productId);
            var customer = _customerService.GetCustomerById(order.CustomerId);
            //save order item

            //basic properties
            decimal unitPriceInclTax;
            decimal.TryParse(form["UnitPriceInclTax"], out unitPriceInclTax);
            decimal unitPriceExclTax;
            decimal.TryParse(form["UnitPriceExclTax"], out unitPriceExclTax);
            int quantity;
            int.TryParse(form["Quantity"], out quantity);
            decimal priceInclTax;
            decimal.TryParse(form["SubTotalInclTax"], out priceInclTax);
            decimal priceExclTax;
            decimal.TryParse(form["SubTotalExclTax"], out priceExclTax);

            //attributes
            //warnings
            var warnings = new List<string>();
            string attributesXml = "";

            #region Product attributes

            var attributes = product.ProductAttributeMappings;
            foreach (var attribute in attributes)
            {
                string controlId = string.Format("product_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, ctrlAttributes);
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                foreach (var item in ctrlAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, item);
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = attribute.ProductAttributeValues;
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var day = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            Guid downloadGuid;
                            Guid.TryParse(form[controlId], out downloadGuid);
                            var download = _downloadService.GetDownloadByGuid(downloadGuid);
                            if (download != null)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, download.DownloadGuid.ToString());
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            //validate conditional attributes (if specified)
            foreach (var attribute in attributes)
            {
                var conditionMet = _productAttributeParser.IsConditionMet(product, attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                }
            }

            #endregion

            #region Gift cards

            string recipientName = "";
            string recipientEmail = "";
            string senderName = "";
            string senderEmail = "";
            string giftCardMessage = "";
            if (product.IsGiftCard)
            {
                foreach (string formKey in form.Keys)
                {
                    if (formKey.Equals("giftcard.RecipientName", StringComparison.OrdinalIgnoreCase))
                    {
                        recipientName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.RecipientEmail", StringComparison.OrdinalIgnoreCase))
                    {
                        recipientEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.SenderName", StringComparison.OrdinalIgnoreCase))
                    {
                        senderName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.SenderEmail", StringComparison.OrdinalIgnoreCase))
                    {
                        senderEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.Message", StringComparison.OrdinalIgnoreCase))
                    {
                        giftCardMessage = form[formKey];
                        continue;
                    }
                }

                attributesXml = _productAttributeParser.AddGiftCardAttribute(attributesXml,
                    recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);
            }

            #endregion

            //warnings
            var shoppingCartService = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IShoppingCartService>();
            warnings.AddRange(shoppingCartService.GetShoppingCartItemAttributeWarnings(customer, ShoppingCartType.ShoppingCart, product, quantity, attributesXml));
            warnings.AddRange(shoppingCartService.GetShoppingCartItemGiftCardWarnings(ShoppingCartType.ShoppingCart, product, attributesXml));
            if (warnings.Count == 0)
            {
                //no errors
                var productAttributeFormatter = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IProductAttributeFormatter>();
                //attributes
                string attributeDescription = productAttributeFormatter.FormatAttributes(product, attributesXml, customer);

                //save item
                var orderItem = new OrderItem
                {
                    OrderItemGuid = Guid.NewGuid(),
                    ProductId = product.Id,
                    VendorId = product.VendorId,
                    UnitPriceInclTax = unitPriceInclTax,
                    UnitPriceExclTax = unitPriceExclTax,
                    PriceInclTax = priceInclTax,
                    PriceExclTax = priceExclTax,
                    OriginalProductCost = _priceCalculationService.GetProductCost(product, attributesXml),
                    AttributeDescription = attributeDescription,
                    AttributesXml = attributesXml,
                    Quantity = quantity,
                    DiscountAmountInclTax = decimal.Zero,
                    DiscountAmountExclTax = decimal.Zero,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = "",
                    CreatedOnUtc = DateTime.UtcNow,

                };
                order.OrderItems.Add(orderItem);
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);
                //adjust inventory
                _productService.AdjustInventory(product, -orderItem.Quantity, orderItem.AttributesXml, orderItem.WarehouseId);

                //add a note
                _orderService.InsertOrderNote(new OrderNote
                {
                    Note = "A new order item has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                });

                //gift cards
                if (product.IsGiftCard)
                {
                    for (int i = 0; i < orderItem.Quantity; i++)
                    {
                        var gc = new GiftCard
                        {
                            GiftCardType = product.GiftCardType,
                            PurchasedWithOrderItem = orderItem,
                            Amount = unitPriceExclTax,
                            IsGiftCardActivated = false,
                            GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                            RecipientName = recipientName,
                            RecipientEmail = recipientEmail,
                            SenderName = senderName,
                            SenderEmail = senderEmail,
                            Message = giftCardMessage,
                            IsRecipientNotified = false,
                            CreatedOnUtc = DateTime.UtcNow
                        };
                        _giftCardService.InsertGiftCard(gc);
                    }
                }
                
            }
            return warnings;
        }
        public void EditCreditCardInfo(Order order, OrderModel model)
        {
            string cardType = model.CardType;
            string cardName = model.CardName;
            string cardNumber = model.CardNumber;
            string cardCvv2 = model.CardCvv2;
            string cardExpirationMonth = model.CardExpirationMonth;
            string cardExpirationYear = model.CardExpirationYear;

            order.CardType = _encryptionService.EncryptText(cardType);
            order.CardName = _encryptionService.EncryptText(cardName);
            order.CardNumber = _encryptionService.EncryptText(cardNumber);
            order.MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(cardNumber));
            order.CardCvv2 = _encryptionService.EncryptText(cardCvv2);
            order.CardExpirationMonth = _encryptionService.EncryptText(cardExpirationMonth);
            order.CardExpirationYear = _encryptionService.EncryptText(cardExpirationYear);
            _orderService.UpdateOrder(order);
        }
        public virtual IList<Order> PrepareOrders(OrderListModel model)
        {
            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;

            var filterByProductId = "";
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && _workContext.HasAccessToProduct(product))
                filterByProductId = model.ProductId;

            //load orders
            var orders = _orderService.SearchOrders(storeId: model.StoreId,
                vendorId: model.VendorId,
                productId: filterByProductId,
                warehouseId: model.WarehouseId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                os: orderStatus,
                ps: paymentStatus,
                ss: shippingStatus,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderGuid: model.OrderGuid);

            return orders;
        }
    }
}
