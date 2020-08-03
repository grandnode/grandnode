using Grand.Core;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Domain.Stores;
using Grand.Domain.Tax;
using Grand.Core.Plugins;
using Grand.Core.Tests.Caching;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Payments;
using Grand.Services.Shipping;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Orders.Tests
{
    [TestClass()]
    public class OrderTotalCalculationServiceTests
    {
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
        private IMediator _eventPublisher;
        private Store _store;
        private IProductService _productService;
        private IGeoLookupService _geoLookupService;
        private ICountryService _countryService;
        private CustomerSettings _customerSettings;
        private AddressSettings _addressSettings;
        private IVendorService _vendorService;
        private ICustomerService _customerService;
        private ICustomerProductService _customerProductService;
        private ICurrencyService _currencyService;
        private IServiceProvider _serviceProvider;
        private IStateProvinceService _stateProvinceService;

        [TestInitialize()]
        public void TestInitialize()
        {

            new Grand.Services.Tests.ServiceTest().PluginInitializator();

            _workContext = new Mock<IWorkContext>().Object;
            _stateProvinceService = new Mock<IStateProvinceService>().Object;

            _store = new Store { Id = "1" };
            var tempStoreContext = new Mock<IStoreContext>();
            {
                tempStoreContext.Setup(x => x.CurrentStore).Returns(_store);
                _storeContext = tempStoreContext.Object;
            }

            _productService = new Mock<IProductService>().Object;
            var tempEventPublisher = new Mock<IMediator>();
            {
                //tempEventPublisher.Setup(x => x.PublishAsync(It.IsAny<object>()));
                _eventPublisher = tempEventPublisher.Object;
            }

            var pluginFinder = new PluginFinder(_serviceProvider);
            var cacheManager = new TestMemoryCacheManager(new Mock<IMemoryCache>().Object, _eventPublisher);

            _discountService = new Mock<IDiscountService>().Object;
            _categoryService = new Mock<ICategoryService>().Object;
            _manufacturerService = new Mock<IManufacturerService>().Object;
            _productAttributeParser = new Mock<IProductAttributeParser>().Object;
            _vendorService = new Mock<IVendorService>().Object;
            _currencyService = new Mock<ICurrencyService>().Object;
            _serviceProvider = new Mock<IServiceProvider>().Object;

            _shoppingCartSettings = new ShoppingCartSettings();
            _catalogSettings = new CatalogSettings();
            _customerService = new Mock<ICustomerService>().Object;
            _customerProductService = new Mock<ICustomerProductService>().Object;

            _priceCalcService = new PriceCalculationService(_workContext, _storeContext,
                _discountService, _categoryService,
                _manufacturerService, _productAttributeParser, _productService, _customerProductService,
                _vendorService, _currencyService,
                _shoppingCartSettings, _catalogSettings);

           
            _localizationService = new Mock<ILocalizationService>().Object;

            //shipping
            _shippingSettings = new ShippingSettings();
            _shippingSettings.ActiveShippingRateComputationMethodSystemNames = new List<string>();
            _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add("FixedRateTestShippingRateComputationMethod");
            _shippingMethodRepository = new Mock<IRepository<ShippingMethod>>().Object;
            _deliveryDateRepository = new Mock<IRepository<DeliveryDate>>().Object;
            _warehouseRepository = new Mock<IRepository<Warehouse>>().Object;
            _logger = new NullLogger();
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

            _shippingService = new ShippingService(_shippingMethodRepository,
            _deliveryDateRepository,
            _warehouseRepository,
            null,
            _logger,
            _productService,
            _productAttributeParser,
            _checkoutAttributeParser,
            _localizationService,
            _addressService,
            _countryService,
            _stateProvinceService,
            pluginFinder,
            _eventPublisher,
            _currencyService,
            cacheManager,
            _shoppingCartSettings,
            _shippingSettings);



            var tempAddressService = new Mock<IAddressService>();
            {
                tempAddressService.Setup(x => x.GetAddressByIdSettings(_taxSettings.DefaultTaxAddressId))
                    .ReturnsAsync(new Address { Id = _taxSettings.DefaultTaxAddressId });
                _addressService = tempAddressService.Object;
            }

            _taxService = new TaxService(_addressService, _workContext, pluginFinder, _geoLookupService, _countryService, _logger, _taxSettings, _customerSettings, _addressSettings);

            _rewardPointsSettings = new RewardPointsSettings();

            _orderTotalCalcService = new OrderTotalCalculationService(_workContext, _storeContext,
                _priceCalcService, _taxService, _shippingService, _paymentService,
                _checkoutAttributeParser, _discountService, _giftCardService,
                null, _productService, _currencyService, _taxSettings, _rewardPointsSettings,
                _shippingSettings, _shoppingCartSettings, _catalogSettings);
        }



        [TestMethod()]
        public async Task Can_convert_reward_points_to_amount()
        {
            //when ExchangeRate is e.g. 512, then ConvertRewardPoints(44) will return 22528 (simple multyplying)
            _rewardPointsSettings.Enabled = true;
            _rewardPointsSettings.ExchangeRate = 512M;

            Assert.AreEqual(22528, await _orderTotalCalcService.ConvertRewardPointsToAmount(44));
        }

        [TestMethod()]
        public void Can_convert_amount_to_reward_points()
        {
            _rewardPointsSettings.Enabled = true;
            _rewardPointsSettings.ExchangeRate = 512M;

            //vice versa
            Assert.AreEqual(44, _orderTotalCalcService.ConvertAmountToRewardPoints(22528));
        }

        [TestMethod()]
        public void Can_check_minimum_reward_points_to_use_requirement()
        {
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