using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Moq;

namespace Nop.Services.Orders.Tests {
    [TestClass()]
    public class OrderTotalCalculationServiceTests {
        private IWorkContext _workContext;
        private IStoreContext _storeContext;
        private ITaxService _taxService;
        private IShippingService _shippingService;
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
        private ILocalizationService _localizationService;
        private ILogger _logger;
        private IRepository<ShippingMethod> _shippingMethodRepository;
        private IRepository<DeliveryDate> _deliveryDateRepository;
        private IRepository<Warehouse> _warehouseRepository;
        private ShoppingCartSettings _shoppingCartSettings;
        private CatalogSettings _catalogSettings;
        private IEventPublisher _eventPublisher;
        private Store _store;
        private IProductService _productService;
        private IGeoLookupService _geoLookupService;
        private ICountryService _countryService;
        private CustomerSettings _customerSettings;
        private AddressSettings _addressSettings;

        [TestInitialize()]
        public void TestInitialize() {

            new Nop.Services.Tests.ServiceTest().PluginInitializator();

            _workContext = new Mock<IWorkContext>().Object;

            _store = new Store { Id = "1" };
            var tempStoreContext = new Mock<IStoreContext>();
            {
                tempStoreContext.Setup(x => x.CurrentStore).Returns(_store);
                _storeContext = tempStoreContext.Object;
            }

            _productService = new Mock<IProductService>().Object;

            var pluginFinder = new PluginFinder();
            var cacheManager = new NopNullCache();

            _discountService = new Mock<IDiscountService>().Object;
            _categoryService = new Mock<ICategoryService>().Object;
            _manufacturerService = new Mock<IManufacturerService>().Object;
            _productAttributeParser = new Mock<IProductAttributeParser>().Object;

            _shoppingCartSettings = new ShoppingCartSettings();
            _catalogSettings = new CatalogSettings();

            _priceCalcService = new PriceCalculationService(_workContext, _storeContext,
                _discountService, _categoryService,
                _manufacturerService, _productAttributeParser,
                _productService, cacheManager,
                _shoppingCartSettings, _catalogSettings);

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
            _logger = new NullLogger();
            _shippingService = new ShippingService(_shippingMethodRepository,
                _deliveryDateRepository,
                _warehouseRepository,
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


            _paymentService = new Mock<IPaymentService>().Object;
            _checkoutAttributeParser = new Mock<ICheckoutAttributeParser>().Object;
            _giftCardService = new Mock<IGiftCardService>().Object;
            _genericAttributeService = new Mock<IGenericAttributeService>().Object;
            
            _geoLookupService = new Mock<IGeoLookupService>().Object;
            _countryService = new Mock<ICountryService>().Object;
            _customerSettings = new CustomerSettings();
            _addressSettings = new AddressSettings();

            //tax
            _taxSettings = new TaxSettings();
            _taxSettings.ShippingIsTaxable = true;
            _taxSettings.PaymentMethodAdditionalFeeIsTaxable = true;
            _taxSettings.DefaultTaxAddressId = "10";

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
                _checkoutAttributeParser, _discountService, _giftCardService, _genericAttributeService,
                null, _taxSettings, _rewardPointsSettings,
                _shippingSettings, _shoppingCartSettings, _catalogSettings);
        }

        
        
        [TestMethod()]
        public void Can_convert_reward_points_to_amount() {
            //when ExchangeRate is e.g. 512, then ConvertRewardPoints(44) will return 22528 (simple multyplying)
            _rewardPointsSettings.Enabled = true;
            _rewardPointsSettings.ExchangeRate = 512M;

            Assert.AreEqual(22528, _orderTotalCalcService.ConvertRewardPointsToAmount(44));
        }

        [TestMethod()]
        public void Can_convert_amount_to_reward_points() {
            _rewardPointsSettings.Enabled = true;
            _rewardPointsSettings.ExchangeRate = 512M;

            //vice versa
            Assert.AreEqual(44, _orderTotalCalcService.ConvertAmountToRewardPoints(22528));
        }

        [TestMethod()]
        public void Can_check_minimum_reward_points_to_use_requirement() {
            //if RewardPoints are enabled
            //MinimumRewardPointsToUse says from which quantity user can get RewardPoints
            //if 0 -> no constraints

            _rewardPointsSettings.Enabled = true;
            _rewardPointsSettings.MinimumRewardPointsToUse = 0;

            Assert.IsTrue(_orderTotalCalcService.CheckMinimumRewardPointsToUseRequirement(0));
            Assert.IsTrue(_orderTotalCalcService.CheckMinimumRewardPointsToUseRequirement(1));
            Assert.IsTrue(_orderTotalCalcService.CheckMinimumRewardPointsToUseRequirement(10));

            _rewardPointsSettings.MinimumRewardPointsToUse = 2123;
            Assert.IsFalse(_orderTotalCalcService.CheckMinimumRewardPointsToUseRequirement(0));
            Assert.IsTrue(_orderTotalCalcService.CheckMinimumRewardPointsToUseRequirement(3000));
            Assert.IsTrue(_orderTotalCalcService.CheckMinimumRewardPointsToUseRequirement(2123));
            Assert.IsFalse(_orderTotalCalcService.CheckMinimumRewardPointsToUseRequirement(2122));
            Assert.IsTrue(_orderTotalCalcService.CheckMinimumRewardPointsToUseRequirement(2124));

        }
    }
}