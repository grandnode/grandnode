using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Tax;
using Grand.Core.Plugins;
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
using Moq;
using Grand.Services.Stores;

namespace Grand.Services.Orders.Tests
{
    [TestClass()]
    public class OrderProcessingServiceTests
    {
        private IWorkContext _workContext;
        private IStoreContext _storeContext;
        private ITaxService _taxService;
        private IShippingService _shippingService;
        private IShipmentService _shipmentService;
        private Mock<IPaymentService> tempPaymentService;
        private IPaymentService _paymentService;
        private ICheckoutAttributeParser _checkoutAttributeParser;
        private IDiscountService _discountService;
        private IGiftCardService _giftCardService;
        private IGenericAttributeService _genericAttributeService;
        private TaxSettings _taxSettings;
        private RewardPointsSettings _rewardPointsSettings;
        private ICategoryService _categoryService;
        private IManufacturerService _manufacturerService;
        private IProductAttributeParser _productAttributeParser;
        private IPriceCalculationService _priceCalcService;
        private IOrderTotalCalculationService _orderTotalCalcService;
        private IAddressService _addressService;
        private ShippingSettings _shippingSettings;
        private ILogger _logger;
        private IRepository<ShippingMethod> _shippingMethodRepository;
        private IRepository<DeliveryDate> _deliveryDateRepository;
        private IRepository<Warehouse> _warehouseRepository;
        private IRepository<PickupPoint> _pickupPointRepository;
        private IOrderService _orderService;
        private IWebHelper _webHelper;
        private ILocalizationService _localizationService;
        private ILanguageService _languageService;
        private IProductService _productService;
        private IPriceFormatter _priceFormatter;
        private IProductAttributeFormatter _productAttributeFormatter;
        private IShoppingCartService _shoppingCartService;
        private ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private ICustomerService _customerService;
        private IEncryptionService _encryptionService;
        private IWorkflowMessageService _workflowMessageService;
        private ICustomerActivityService _customerActivityService;
        private ICurrencyService _currencyService;
        private PaymentSettings _paymentSettings;
        private OrderSettings _orderSettings;
        private LocalizationSettings _localizationSettings;
        private ShoppingCartSettings _shoppingCartSettings;
        private CatalogSettings _catalogSettings;
        private IOrderProcessingService _orderProcessingService;
        private IEventPublisher _eventPublisher;
        private CurrencySettings _currencySettings;
        private IAffiliateService _affiliateService;
        private IVendorService _vendorService;
        private IPdfService _pdfService;
        private IStoreService _storeService;
        private IGeoLookupService _geoLookupService;
        private ICountryService _countryService;
        private CustomerSettings _customerSettings;
        private AddressSettings _addressSettings;
        private Store _store;
        private IProductReservationService _productReservationService;
        private IAuctionService _auctionService;

        [TestInitialize()]
        public void TestInitialize()
        {
            _workContext = null;

            _store = new Store { Id = "1" };
            var tempStoreContext = new Mock<IStoreContext>();
            {
                tempStoreContext.Setup(x => x.CurrentStore).Returns(_store);
                _storeContext = tempStoreContext.Object;
            }

            var pluginFinder = new PluginFinder();

            _shoppingCartSettings = new ShoppingCartSettings();
            _catalogSettings = new CatalogSettings();
            _currencySettings = new CurrencySettings();

            var cacheManager = new GrandNullCache();

            _productService = new Mock<IProductService>().Object;

            //price calculation service
            _discountService = new Mock<IDiscountService>().Object;
            _categoryService = new Mock<ICategoryService>().Object;
            _manufacturerService = new Mock<IManufacturerService>().Object;
            _storeService = new Mock<IStoreService>().Object;
            _customerService = new Mock<ICustomerService>().Object;
            _productReservationService = new Mock<IProductReservationService>().Object;
            _currencyService = new Mock<ICurrencyService>().Object;
            _auctionService = new Mock<IAuctionService>().Object;

            _productAttributeParser = new Mock<IProductAttributeParser>().Object;
            _priceCalcService = new PriceCalculationService(_workContext, _storeContext,
                _discountService, _categoryService, _manufacturerService,
                _productAttributeParser, _productService, _customerService,
                cacheManager, _vendorService, _storeService, _currencyService, _shoppingCartSettings, _catalogSettings, _currencySettings);

            var tempEventPublisher = new Mock<IEventPublisher>();
            {
                tempEventPublisher.Setup(x => x.Publish(It.IsAny<object>()));
                _eventPublisher = tempEventPublisher.Object;
            }

            _localizationService = new Mock<ILocalizationService>().Object;

            //shipping
            _shippingSettings = new ShippingSettings();
            _shippingSettings.ActiveShippingRateComputationMethodSystemNames = new List<string>();
            _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add("FixedRateTestShippingRateComputationMethod");
            _shippingMethodRepository = new Mock<IRepository<ShippingMethod>>().Object;
            _deliveryDateRepository = new Mock<IRepository<DeliveryDate>>().Object;
            _warehouseRepository = new Mock<IRepository<Warehouse>>().Object;
            _pickupPointRepository = new Mock<IRepository<PickupPoint>>().Object;

            _logger = new NullLogger();
            _shippingService = new ShippingService(_shippingMethodRepository,
                _deliveryDateRepository,
                _warehouseRepository,
                _pickupPointRepository,
                _logger,
                _productService,
                _productAttributeParser,
                _checkoutAttributeParser,
                _genericAttributeService,
                _localizationService,
                _addressService,
                _shippingSettings,
                pluginFinder,
                _storeContext,
                _eventPublisher,
                _shoppingCartSettings,
                cacheManager,
                null);
            _shipmentService = new Mock<IShipmentService>().Object;


            tempPaymentService = new Mock<IPaymentService>();
            {
                _paymentService = tempPaymentService.Object;
            }
            _checkoutAttributeParser = new Mock<ICheckoutAttributeParser>().Object;
            _giftCardService = new Mock<IGiftCardService>().Object;
            _genericAttributeService = new Mock<IGenericAttributeService>().Object;

            _geoLookupService = new Mock<IGeoLookupService>().Object;
            _countryService = new Mock<ICountryService>().Object;
            _customerSettings = new CustomerSettings();
            _addressSettings = new AddressSettings();

            //tax
            _taxSettings = new TaxSettings
            {
                ShippingIsTaxable = true,
                PaymentMethodAdditionalFeeIsTaxable = true,
                DefaultTaxAddressId = "10"
            };

            var tempAddressService = new Mock<IAddressService>();
            {
                tempAddressService.Setup(x => x.GetAddressByIdSettings(_taxSettings.DefaultTaxAddressId))
                    .Returns(new Address { Id = _taxSettings.DefaultTaxAddressId });
                _addressService = tempAddressService.Object;
            }

            _taxService = new TaxService(_addressService, _workContext, _taxSettings,
                pluginFinder, _geoLookupService, _countryService, _logger, _customerSettings, _addressSettings);

            _rewardPointsSettings = new RewardPointsSettings();

            _orderTotalCalcService = new OrderTotalCalculationService(_workContext, _storeContext,
                _priceCalcService, _taxService, _shippingService, _paymentService,
                _checkoutAttributeParser, _discountService, _giftCardService,
                _genericAttributeService, null, _productService, _currencyService,
                _taxSettings, _rewardPointsSettings, _shippingSettings, _shoppingCartSettings, _catalogSettings, _currencySettings);

            _orderService = new Mock<IOrderService>().Object;
            _webHelper = new Mock<IWebHelper>().Object;
            _languageService = new Mock<ILanguageService>().Object;
            _priceFormatter = new Mock<IPriceFormatter>().Object;
            _productAttributeFormatter = new Mock<IProductAttributeFormatter>().Object;
            _shoppingCartService = new Mock<IShoppingCartService>().Object;
            _checkoutAttributeFormatter = new Mock<ICheckoutAttributeFormatter>().Object;
            _encryptionService = new Mock<IEncryptionService>().Object;
            _workflowMessageService = new Mock<IWorkflowMessageService>().Object;
            _customerActivityService = new Mock<ICustomerActivityService>().Object;
            _currencyService = new Mock<ICurrencyService>().Object;
            _affiliateService = new Mock<IAffiliateService>().Object;
            _vendorService = new Mock<IVendorService>().Object;
            _pdfService = new Mock<IPdfService>().Object;

            _paymentSettings = new PaymentSettings
            {
                ActivePaymentMethodSystemNames = new List<string>
                {
                    "Payments.TestMethod"
                }
            };
            _orderSettings = new OrderSettings();
            _localizationSettings = new LocalizationSettings();
            ICustomerActionEventService tempICustomerActionEventService = new Mock<ICustomerActionEventService>().Object;
            _currencySettings = new CurrencySettings();

            _orderProcessingService = new OrderProcessingService(_orderService, _webHelper,
                _localizationService, _languageService,
                _productService, _paymentService, _logger,
                _orderTotalCalcService, _priceCalcService, _priceFormatter,
                _productAttributeParser, _productAttributeFormatter,
                _giftCardService, _shoppingCartService, _checkoutAttributeFormatter,
                _shippingService, _shipmentService, _taxService,
                _customerService, _discountService,
                _encryptionService, _workContext,
                _workflowMessageService, _vendorService,
                _customerActivityService, tempICustomerActionEventService,
                _currencyService, _affiliateService,
                _eventPublisher, _pdfService, null, null, _storeContext, _productReservationService, _auctionService,
                _shippingSettings, _paymentSettings, _rewardPointsSettings,
                _orderSettings, _taxSettings, _localizationSettings,
                _currencySettings);
        }

        [TestMethod()]
        public void Ensure_order_can_only_be_cancelled_when_orderStatus_is_not_cancelled_yet()
        {
            /*
            if OrderStatus hasn't status of "cancelled"
            then OrderStatus.CanCancelOrder should return true
            */

            var order = new Order();
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;
                        if (os != OrderStatus.Cancelled)
                            Assert.IsTrue(_orderProcessingService.CanCancelOrder(order));
                        else
                            Assert.IsFalse(_orderProcessingService.CanCancelOrder(order));
                    }
        }

        [TestMethod()]
        public void Ensure_order_can_only_be_marked_as_authorized_when_orderStatus_is_not_cancelled_and_paymentStatus_is_pending()
        {
            /*
            if OrderStatus != Cancelled, it PaymentStaus == Pending
            it should be able to authorize order
            */

            var order = new Order();
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;
                        if (os != OrderStatus.Cancelled && ps == PaymentStatus.Pending)
                            Assert.IsTrue(_orderProcessingService.CanMarkOrderAsAuthorized(order));
                        else
                            Assert.IsFalse(_orderProcessingService.CanMarkOrderAsAuthorized(order));
                    }
        }

        [TestMethod()]
        public void Ensure_order_can_only_be_captured_when_orderStatus_is_not_cancelled_or_pending_and_paymentstatus_is_authorized_and_paymentModule_supports_capture()
        {
            //Property SupportCapture should be returning true (if supports) or false (if dosen't support)
            tempPaymentService.Setup(ps => ps.SupportCapture("paymentMethodSystemName_that_supports_capture")).Returns(true);
            tempPaymentService.Setup(ps => ps.SupportCapture("paymentMethodSystemName_that_doesn't_support_capture")).Returns(false);
            var order = new Order();

            //if
            //  PaymetMethodSystemName == "supports caputre"
            //  OrderStatus != cancelled
            //  PaymentStatus != pending
            //  PaymentStatus == authorized
            //then
            //  CanCapture() should return true
            //  in other case it should return false

            order.PaymentMethodSystemName = "paymentMethodSystemName_that_supports_capture";
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        if ((os != OrderStatus.Cancelled && os != OrderStatus.Pending)
                            && (ps == PaymentStatus.Authorized))
                            Assert.IsTrue(_orderProcessingService.CanCapture(order));
                        else
                            Assert.IsFalse(_orderProcessingService.CanCapture(order));
                    }

            //in this case always: CanCapture() == false
            order.PaymentMethodSystemName = "paymentMethodSystemName_that_doesn't_support_capture";
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        Assert.IsFalse(_orderProcessingService.CanCapture(order));
                    }
        }

        [TestMethod()]
        public void Ensure_order_cannot_be_marked_as_paid_when_orderStatus_is_cancelled_or_paymentStatus_is_paid_or_refunded_or_voided()
        {
            var order = new Order();
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;
                        if (os == OrderStatus.Cancelled
                            || (ps == PaymentStatus.Paid || ps == PaymentStatus.Refunded || ps == PaymentStatus.Voided))
                            Assert.IsFalse(_orderProcessingService.CanMarkOrderAsPaid(order));
                        else
                            //even if it is Unpaid - it can be marked as paid
                            //even if it is Unrefunded - it can be marked as paid
                            //the only "can't" is when order status is: Cancelled
                            Assert.IsTrue(_orderProcessingService.CanMarkOrderAsPaid(order));
                    }
        }

        [TestMethod()]
        public void Ensure_order_can_only_be_refunded_when_paymentstatus_is_paid_and_paymentModule_supports_refund()
        {
            //method SupportRefund() is expected to return true or false - it depends on string
            tempPaymentService.Setup(ps => ps.SupportRefund("paymentMethodSystemName_that_supports_refund")).Returns(true);
            tempPaymentService.Setup(ps => ps.SupportRefund("paymentMethodSystemName_that_doesn't_support_refund")).Returns(false);

            var order = new Order();
            order.OrderTotal = 1;
            order.PaymentMethodSystemName = "paymentMethodSystemName_that_supports_refund";

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        //if product is paid - you can take cost refund
                        if (ps == PaymentStatus.Paid)
                            Assert.IsTrue(_orderProcessingService.CanRefund(order));
                        else
                            Assert.IsFalse(_orderProcessingService.CanRefund(order));
                    }

            order.PaymentMethodSystemName = "paymentMethodSystemName_that_doesn't_support_refund";
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        //but you can't take refund, when it is not supported
                        Assert.IsFalse(_orderProcessingService.CanRefund(order));
                    }
        }

        [TestMethod()]
        public void Ensure_order_cannot_be_refunded_when_orderTotal_is_zero()
        {
            tempPaymentService.Setup(ps => ps.SupportRefund("paymentMethodSystemName_that_supports_refund")).Returns(true);
            var order = new Order();
            order.PaymentMethodSystemName = "paymentMethodSystemName_that_supports_refund";

            //OrderTotal = 0
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        Assert.IsFalse(_orderProcessingService.CanRefund(order));
                    }
        }

        [TestMethod()]
        public void Ensure_order_can_only_be_refunded_offline_when_paymentstatus_is_paid()
        {
            //if paid, can get online refund
            var order = new Order
            {
                OrderTotal = 1,
            };
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        if (ps == PaymentStatus.Paid)
                            Assert.IsTrue(_orderProcessingService.CanRefundOffline(order));
                        else
                            Assert.IsFalse(_orderProcessingService.CanRefundOffline(order));
                    }
        }

        [TestMethod()]
        public void Ensure_order_cannot_be_refunded_offline_when_orderTotal_is_zero()
        {
            var order = new Order { OrderTotal = 0 }; //no values in ORderTotal property
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
            {
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                {
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        Assert.IsFalse(_orderProcessingService.CanRefundOffline(order));
                    }
                }
            }
        }

        [TestMethod()]
        public void Ensure_order_can_only_be_voided_when_paymentstatus_is_authorized_and_paymentModule_supports_void()
        {
            tempPaymentService.Setup(ps => ps.SupportVoid("paymentMethodSystemName_that_supports_void")).Returns(true);
            tempPaymentService.Setup(ps => ps.SupportVoid("paymentMethodSystemName_that_doesn't_support_void")).Returns(false);

            var order = new Order();
            order.OrderTotal = 1;
            order.PaymentMethodSystemName = "paymentMethodSystemName_that_supports_void";

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        if (ps == PaymentStatus.Authorized)
                            Assert.IsTrue(_orderProcessingService.CanVoid(order));
                        else
                            Assert.IsFalse(_orderProcessingService.CanVoid(order));
                    }

            order.PaymentMethodSystemName = "paymentMethodSystemName_that_doesn't_support_void";
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        Assert.IsFalse(_orderProcessingService.CanVoid(order));
                    }
        }

        [TestMethod()]
        public void Ensure_order_cannot_be_voided_when_orderTotal_is_zero()
        {
            tempPaymentService.Setup(ps => ps.SupportVoid("paymentMethodSystemName_that_supports_void")).Returns(true);
            var order = new Order(); //nothing inside OrderTotal !
            order.PaymentMethodSystemName = "paymentMethodSystemName_that_supports_void";

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        Assert.IsFalse(_orderProcessingService.CanVoid(order));
                    }
        }

        [TestMethod()]
        public void Ensure_order_can_only_be_voided_offline_when_paymentstatus_is_authorized()
        {
            //payment status == authorized && OrderTotal >0
            var order = new Order
            {
                OrderTotal = 1,
            };
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        if (ps == PaymentStatus.Authorized)
                            Assert.IsTrue(_orderProcessingService.CanVoidOffline(order));
                        else
                            Assert.IsFalse(_orderProcessingService.CanVoidOffline(order));
                    }
        }

        [TestMethod()]
        public void Ensure_order_cannot_be_voided_offline_when_orderTotal_is_zero()
        {
            //CanVoidOffline() ==false when nothing in ORderTotal
            var order = new Order();

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        Assert.IsFalse(_orderProcessingService.CanVoidOffline(order));
                    }
        }

        [TestMethod()]
        public void Ensure_order_can_only_be_partially_refunded_when_paymentstatus_is_paid_or_partiallyRefunded_and_paymentModule_supports_partialRefund()
        {
            //SupportPartiallyRefund()
            tempPaymentService.Setup(ps => ps.SupportPartiallyRefund("paymentMethodSystemName_that_supports_partialrefund")).Returns(true);
            tempPaymentService.Setup(ps => ps.SupportPartiallyRefund("paymentMethodSystemName_that_doesn't_support_partialrefund")).Returns(false);
            var order = new Order();
            order.OrderTotal = 100;
            order.PaymentMethodSystemName = "paymentMethodSystemName_that_supports_partialrefund";

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;
                        //when paid and partiallyrefund -> you can get partially refund 
                        if (ps == PaymentStatus.Paid || order.PaymentStatus == PaymentStatus.PartiallyRefunded)
                            Assert.IsTrue(_orderProcessingService.CanPartiallyRefund(order, 10));
                        else
                            Assert.IsFalse(_orderProcessingService.CanPartiallyRefund(order, 10));
                    }

            //this string shouldn't allow you in any case permit to have partial refund
            order.PaymentMethodSystemName = "paymentMethodSystemName_that_doesn't_support_partialrefund"; //changed!
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        Assert.IsFalse(_orderProcessingService.CanPartiallyRefund(order, 10));
                    }
        }

        [TestMethod()]
        public void Ensure_order_cannot_be_partially_refunded_when_amountToRefund_is_greater_than_amount_that_can_be_refunded()
        {
            tempPaymentService.Setup(ps => ps.SupportPartiallyRefund("paymentMethodSystemName_that_supports_partialrefund")).Returns(true);

            var order = new Order
            {
                OrderTotal = 100,
                RefundedAmount = 30, //100-30=70 can be refunded
            };
            order.PaymentMethodSystemName = "paymentMethodSystemName_that_supports_partialrefund";

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        Assert.IsFalse(_orderProcessingService.CanPartiallyRefund(order, 80));
                    }
        }

        [TestMethod()]
        public void Ensure_order_can_only_be_partially_refunded_offline_when_paymentstatus_is_paid_or_partiallyRefunded()
        {
            var order = new Order();
            order.OrderTotal = 100;

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        //paid or partiallyrefund returns true
                        if (ps == PaymentStatus.Paid || order.PaymentStatus == PaymentStatus.PartiallyRefunded)
                            Assert.IsTrue(_orderProcessingService.CanPartiallyRefundOffline(order, 10));
                        else
                            Assert.IsFalse(_orderProcessingService.CanPartiallyRefundOffline(order, 10));

                    }
        }

        [TestMethod()]
        public void Ensure_order_cannot_be_partially_refunded_offline_when_amountToRefund_is_greater_than_amount_that_can_be_refunded()
        {
            var order = new Order
            {
                OrderTotal = 100,
                RefundedAmount = 30, //100-30=70 can be refunded
            };

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        Assert.IsFalse(_orderProcessingService.CanPartiallyRefundOffline(order, 80));
                    }
        }
    }
}