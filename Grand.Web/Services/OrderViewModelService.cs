using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Tax;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Web.Models.Common;
using Grand.Web.Models.Order;
using System;
using System.Linq;

namespace Grand.Web.Services
{
    public partial class OrderViewModelService : IOrderViewModelService
    {
        private readonly IOrderService _orderService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDownloadService _downloadService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductService _productService;
        private readonly ICountryService _countryService;
        private readonly IAddressViewModelService _addressViewModelService;
        private readonly IShipmentService _shipmentService;
        private readonly IPaymentService _paymentService;
        private readonly IGiftCardService _giftCardService;
        private readonly IShippingService _shippingService;
        private readonly IRewardPointsService _rewardPointsService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        private readonly OrderSettings _orderSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;

        public OrderViewModelService(
            IOrderService orderService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            ICurrencyService currencyService,
            IOrderProcessingService orderProcessingService,
            IPriceFormatter priceFormatter,
            IDownloadService downloadService,
            IProductAttributeParser productAttributeParser,
            IProductService productService,
            ICountryService countryService,
            IAddressViewModelService addressViewModelService,
            IShipmentService shipmentService,
            IPaymentService paymentService,
            IGiftCardService giftCardService,
            IShippingService shippingService,
            IRewardPointsService rewardPointsService,
            IOrderTotalCalculationService orderTotalCalculationService,
            OrderSettings orderSettings,
            PdfSettings pdfSettings,
            TaxSettings taxSettings,
            CatalogSettings catalogSettings,
            ShippingSettings shippingSettings,
            RewardPointsSettings rewardPointsSettings)
        {
            this._orderService = orderService;
            this._storeContext = storeContext;
            this._workContext = workContext;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._currencyService = currencyService;
            this._orderProcessingService = orderProcessingService;
            this._priceFormatter = priceFormatter;
            this._downloadService = downloadService;
            this._productAttributeParser = productAttributeParser;
            this._productService = productService;
            this._countryService = countryService;
            this._addressViewModelService = addressViewModelService;
            this._shipmentService = shipmentService;
            this._paymentService = paymentService;
            this._giftCardService = giftCardService;
            this._shippingService = shippingService;
            this._rewardPointsService = rewardPointsService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._orderSettings = orderSettings;
            this._pdfSettings = pdfSettings;
            this._taxSettings = taxSettings;
            this._catalogSettings = catalogSettings;
            this._shippingSettings = shippingSettings;
            this._rewardPointsSettings = rewardPointsSettings;
        }

        public virtual CustomerOrderListModel PrepareCustomerOrderList()
        {
            var model = new CustomerOrderListModel();
            var orders = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id);
            foreach (var order in orders)
            {
                var orderModel = new CustomerOrderListModel.OrderDetailsModel
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
                    OrderStatusEnum = order.OrderStatus,
                    OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                    PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
                    ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
                    IsReturnRequestAllowed = _orderProcessingService.IsReturnRequestAllowed(order)
                };
                var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                orderModel.OrderTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);

                model.Orders.Add(orderModel);
            }

            var recurringPayments = _orderService.SearchRecurringPayments(_storeContext.CurrentStore.Id,
                _workContext.CurrentCustomer.Id);
            foreach (var recurringPayment in recurringPayments)
            {
                var recurringPaymentModel = new CustomerOrderListModel.RecurringOrderModel
                {
                    Id = recurringPayment.Id,
                    StartDate = _dateTimeHelper.ConvertToUserTime(recurringPayment.StartDateUtc, DateTimeKind.Utc).ToString(),
                    CycleInfo = string.Format("{0} {1}", recurringPayment.CycleLength, recurringPayment.CyclePeriod.GetLocalizedEnum(_localizationService, _workContext)),
                    NextPayment = recurringPayment.NextPaymentDate.HasValue ? _dateTimeHelper.ConvertToUserTime(recurringPayment.NextPaymentDate.Value, DateTimeKind.Utc).ToString() : "",
                    TotalCycles = recurringPayment.TotalCycles,
                    CyclesRemaining = recurringPayment.CyclesRemaining,
                    InitialOrderId = recurringPayment.InitialOrder.Id,
                    CanCancel = _orderProcessingService.CanCancelRecurringPayment(_workContext.CurrentCustomer, recurringPayment),
                };

                model.RecurringOrders.Add(recurringPaymentModel);
            }

            return model;

        }
        public virtual OrderDetailsModel PrepareOrderDetails(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");
            var model = new OrderDetailsModel();

            model.Id = order.Id;
            model.OrderNumber = order.OrderNumber;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
            model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.IsReOrderAllowed = _orderSettings.IsReOrderAllowed;
            model.IsReturnRequestAllowed = _orderProcessingService.IsReturnRequestAllowed(order);
            model.PdfInvoiceDisabled = _pdfSettings.DisablePdfInvoicesForPendingOrders && order.OrderStatus == OrderStatus.Pending;
            model.ShowAddOrderNote = _orderSettings.AllowCustomerToAddOrderNote;

            //shipping info
            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext);
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                model.IsShippable = true;
                model.PickUpInStore = order.PickUpInStore;
                if (!order.PickUpInStore)
                {
                    _addressViewModelService.PrepareModel(model: model.ShippingAddress,
                        address: order.ShippingAddress,
                        excludeProperties: false);
                }
                else
                {
                    if (order.PickupPoint != null)
                    {
                        if (order.PickupPoint.Address != null)
                        {
                            var country = _countryService.GetCountryById(order.PickupPoint.Address.CountryId);
                            model.PickupAddress = new AddressModel
                            {
                                Address1 = order.PickupPoint.Address.Address1,
                                City = order.PickupPoint.Address.City,
                                CountryName = country != null ? country.Name : string.Empty,
                                ZipPostalCode = order.PickupPoint.Address.ZipPostalCode
                            };
                        }
                    }
                }
                model.ShippingMethod = order.ShippingMethod;
                model.ShippingAdditionDescription = order.ShippingOptionAttributeDescription;
                //shipments (only already shipped)
                var shipments = _shipmentService.GetShipmentsByOrder(order.Id).Where(x => x.ShippedDateUtc.HasValue).OrderBy(x => x.CreatedOnUtc).ToList();
                foreach (var shipment in shipments)
                {
                    var shipmentModel = new OrderDetailsModel.ShipmentBriefModel
                    {
                        Id = shipment.Id,
                        ShipmentNumber = shipment.ShipmentNumber,
                        TrackingNumber = shipment.TrackingNumber,
                    };
                    if (shipment.ShippedDateUtc.HasValue)
                        shipmentModel.ShippedDate = _dateTimeHelper.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
                    if (shipment.DeliveryDateUtc.HasValue)
                        shipmentModel.DeliveryDate = _dateTimeHelper.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);
                    model.Shipments.Add(shipmentModel);
                }
            }


            //billing info
            _addressViewModelService.PrepareModel(model: model.BillingAddress,
                address: order.BillingAddress,
                excludeProperties: false);

            //VAT number
            model.VatNumber = order.VatNumber;

            //payment method
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            model.PaymentMethod = paymentMethod != null ? paymentMethod.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id) : order.PaymentMethodSystemName;
            model.PaymentMethodStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.CanRePostProcessPayment = _paymentService.CanRePostProcessPayment(order);
            //custom values
            model.CustomValues = order.DeserializeCustomValues();

            //order subtotal
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax

                //order subtotal
                var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                model.OrderSubtotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                //discount (applied to order subtotal)
                var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                    model.OrderSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
            }
            else
            {
                //excluding tax

                //order subtotal
                var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                model.OrderSubtotal = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                //discount (applied to order subtotal)
                var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
                    model.OrderSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
            }

            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax

                //order shipping
                var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                model.OrderShipping = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                //payment method additional fee
                var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                if (paymentMethodAdditionalFeeInclTaxInCustomerCurrency > decimal.Zero)
                    model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
            }
            else
            {
                //excluding tax

                //order shipping
                var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                model.OrderShipping = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                //payment method additional fee
                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                if (paymentMethodAdditionalFeeExclTaxInCustomerCurrency > decimal.Zero)
                    model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
            }

            //tax
            bool displayTax = true;
            bool displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    displayTaxRates = _taxSettings.DisplayTaxRates && order.TaxRatesDictionary.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    //TODO pass languageId to _priceFormatter.FormatPrice
                    model.Tax = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);

                    foreach (var tr in order.TaxRatesDictionary)
                    {
                        model.TaxRates.Add(new OrderDetailsModel.TaxRate
                        {
                            Rate = _priceFormatter.FormatTaxRate(tr.Key),
                            //TODO pass languageId to _priceFormatter.FormatPrice
                            Value = _priceFormatter.FormatPrice(_currencyService.ConvertCurrency(tr.Value, order.CurrencyRate), true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage),
                        });
                    }
                }
            }
            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
            model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoOrderDetailsPage;
            model.PricesIncludeTax = order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax;

            //discount (applied to order total)
            var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
            if (orderDiscountInCustomerCurrency > decimal.Zero)
                model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);


            //gift cards
            foreach (var gcuh in _giftCardService.GetAllGiftCardUsageHistory(order.Id))
            {
                var giftCard = _giftCardService.GetGiftCardById(gcuh.GiftCardId);
                model.GiftCards.Add(new OrderDetailsModel.GiftCard
                {
                    CouponCode = giftCard.GiftCardCouponCode,
                    Amount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage),
                });
            }

            //reward points           
            if (order.RedeemedRewardPointsEntry != null)
            {
                model.RedeemedRewardPoints = -order.RedeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);
            }

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            model.OrderTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);

            //checkout attributes
            model.CheckoutAttributeInfo = order.CheckoutAttributeDescription;

            //order notes
            foreach (var orderNote in _orderService.GetOrderNotes(order.Id)
                .Where(on => on.DisplayToCustomer)
                .OrderByDescending(on => on.CreatedOnUtc)
                .ToList())
            {
                model.OrderNotes.Add(new OrderDetailsModel.OrderNote
                {
                    Id = orderNote.Id,
                    OrderId = orderNote.OrderId,
                    HasDownload = !String.IsNullOrEmpty(orderNote.DownloadId),
                    Note = orderNote.FormatOrderNoteText(),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }

            //allow cancel order
            if (_orderSettings.UserCanCancelUnpaidOrder)
            {
                if (order.OrderStatus == OrderStatus.Pending && order.PaymentStatus == Core.Domain.Payments.PaymentStatus.Pending
                    && (order.ShippingStatus == ShippingStatus.ShippingNotRequired || order.ShippingStatus == ShippingStatus.NotYetShipped))
                    model.UserCanCancelUnpaidOrder = true;
            }

            //purchased products
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
            foreach (var orderItem in order.OrderItems)
            {
                var product = _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                var orderItemModel = new OrderDetailsModel.OrderItemModel
                {
                    Id = orderItem.Id,
                    OrderItemGuid = orderItem.OrderItemGuid,
                    Sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser),
                    ProductId = product.Id,
                    ProductName = product.GetLocalized(x => x.Name),
                    ProductSeName = product.SeName,
                    Quantity = orderItem.Quantity,
                    AttributeInfo = orderItem.AttributeDescription,
                };

                model.Items.Add(orderItemModel);

                //unit price, subtotal
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                    orderItemModel.UnitPriceValue = unitPriceInclTaxInCustomerCurrency;

                    var unitPriceWithDiscInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceWithoutDiscInclTax, order.CurrencyRate);
                    orderItemModel.UnitPriceWithoutDiscount = _priceFormatter.FormatPrice(unitPriceWithDiscInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                    orderItemModel.UnitPriceWithoutDiscountValue = unitPriceWithDiscInclTaxInCustomerCurrency;

                    var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                    orderItemModel.SubTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                    if (orderItem.DiscountAmountInclTax > 0)
                    {
                        var discountCustomerCurrency = _currencyService.ConvertCurrency(orderItem.DiscountAmountInclTax, order.CurrencyRate);
                        orderItemModel.Discount = _priceFormatter.FormatPrice(discountCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                    }
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                    orderItemModel.UnitPriceValue = unitPriceExclTaxInCustomerCurrency;

                    var unitPriceExclWithDiscTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceWithoutDiscInclTax, order.CurrencyRate);
                    orderItemModel.UnitPriceWithoutDiscount = _priceFormatter.FormatPrice(unitPriceExclWithDiscTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                    orderItemModel.UnitPriceWithoutDiscountValue = unitPriceExclWithDiscTaxInCustomerCurrency;

                    var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                    orderItemModel.SubTotal = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                    if (orderItem.DiscountAmountExclTax > 0)
                    {
                        var discountCustomerCurrency = _currencyService.ConvertCurrency(orderItem.DiscountAmountExclTax, order.CurrencyRate);
                        orderItemModel.Discount = _priceFormatter.FormatPrice(discountCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                    }
                }

                //downloadable products
                if (_downloadService.IsDownloadAllowed(orderItem))
                    orderItemModel.DownloadId = product.DownloadId;
                if (_downloadService.IsLicenseDownloadAllowed(orderItem))
                    orderItemModel.LicenseId = !String.IsNullOrEmpty(orderItem.LicenseDownloadId) ? orderItem.LicenseDownloadId : "";
            }

            return model;

        }
        public virtual ShipmentDetailsModel PrepareShipmentDetails(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                throw new Exception("order cannot be loaded");
            var model = new ShipmentDetailsModel();

            model.Id = shipment.Id;
            model.ShipmentNumber = shipment.ShipmentNumber;
            if (shipment.ShippedDateUtc.HasValue)
                model.ShippedDate = _dateTimeHelper.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
            if (shipment.DeliveryDateUtc.HasValue)
                model.DeliveryDate = _dateTimeHelper.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);

            //tracking number and shipment information
            if (!String.IsNullOrEmpty(shipment.TrackingNumber))
            {
                model.TrackingNumber = shipment.TrackingNumber;
                var srcm = _shippingService.LoadShippingRateComputationMethodBySystemName(order.ShippingRateComputationMethodSystemName);
                if (srcm != null &&
                    srcm.PluginDescriptor.Installed &&
                    srcm.IsShippingRateComputationMethodActive(_shippingSettings))
                {
                    var shipmentTracker = srcm.ShipmentTracker;
                    if (shipmentTracker != null)
                    {
                        model.TrackingNumberUrl = shipmentTracker.GetUrl(shipment.TrackingNumber);
                        if (_shippingSettings.DisplayShipmentEventsToCustomers)
                        {
                            var shipmentEvents = shipmentTracker.GetShipmentEvents(shipment.TrackingNumber);
                            if (shipmentEvents != null)
                            {
                                foreach (var shipmentEvent in shipmentEvents)
                                {
                                    var shipmentStatusEventModel = new ShipmentDetailsModel.ShipmentStatusEventModel();
                                    var shipmentEventCountry = _countryService.GetCountryByTwoLetterIsoCode(shipmentEvent.CountryCode);
                                    shipmentStatusEventModel.Country = shipmentEventCountry != null
                                                                           ? shipmentEventCountry.GetLocalized(x => x.Name)
                                                                           : shipmentEvent.CountryCode;
                                    shipmentStatusEventModel.Date = shipmentEvent.Date;
                                    shipmentStatusEventModel.EventName = shipmentEvent.EventName;
                                    shipmentStatusEventModel.Location = shipmentEvent.Location;
                                    model.ShipmentStatusEvents.Add(shipmentStatusEventModel);
                                }
                            }
                        }
                    }
                }
            }

            //products in this shipment
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
            foreach (var shipmentItem in shipment.ShipmentItems)
            {
                var orderItem = order.OrderItems.Where(x => x.Id == shipmentItem.OrderItemId).FirstOrDefault();
                if (orderItem == null)
                    continue;
                var product = _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                var shipmentItemModel = new ShipmentDetailsModel.ShipmentItemModel
                {
                    Id = shipmentItem.Id,
                    Sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser),
                    ProductId = orderItem.ProductId,
                    ProductName = product.GetLocalized(x => x.Name),
                    ProductSeName = product.GetSeName(),
                    AttributeInfo = orderItem.AttributeDescription,
                    QuantityOrdered = orderItem.Quantity,
                    QuantityShipped = shipmentItem.Quantity,
                };

                model.Items.Add(shipmentItemModel);
            }

            //order details model
            model.Order = PrepareOrderDetails(order);

            return model;

        }

        public virtual CustomerRewardPointsModel PrepareCustomerRewardPoints(Customer customer)
        {
            var model = new CustomerRewardPointsModel();
            foreach (var rph in _rewardPointsService.GetRewardPointsHistory(customer.Id))
            {
                model.RewardPoints.Add(new CustomerRewardPointsModel.RewardPointsHistoryModel
                {
                    Points = rph.Points,
                    PointsBalance = rph.PointsBalance,
                    Message = rph.Message,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(rph.CreatedOnUtc, DateTimeKind.Utc)
                });
            }
            //current amount/balance
            int rewardPointsBalance = _rewardPointsService.GetRewardPointsBalance(customer.Id, _storeContext.CurrentStore.Id);
            decimal rewardPointsAmountBase = _orderTotalCalculationService.ConvertRewardPointsToAmount(rewardPointsBalance);
            decimal rewardPointsAmount = _currencyService.ConvertFromPrimaryStoreCurrency(rewardPointsAmountBase, _workContext.WorkingCurrency);
            model.RewardPointsBalance = rewardPointsBalance;
            model.RewardPointsAmount = _priceFormatter.FormatPrice(rewardPointsAmount, true, false);
            //minimum amount/balance
            int minimumRewardPointsBalance = _rewardPointsSettings.MinimumRewardPointsToUse;
            decimal minimumRewardPointsAmountBase = _orderTotalCalculationService.ConvertRewardPointsToAmount(minimumRewardPointsBalance);
            decimal minimumRewardPointsAmount = _currencyService.ConvertFromPrimaryStoreCurrency(minimumRewardPointsAmountBase, _workContext.WorkingCurrency);
            model.MinimumRewardPointsBalance = minimumRewardPointsBalance;
            model.MinimumRewardPointsAmount = _priceFormatter.FormatPrice(minimumRewardPointsAmount, true, false);

            return model;
        }

    }
}