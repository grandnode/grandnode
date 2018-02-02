using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Discounts;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Stores;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Grand.Services.Catalog.Tests
{
    [TestClass()]
    public class PriceCalculationServiceTests
    {
        private Store _store;
        private Mock<IWorkContext> tempWorkContext;
        private IWorkContext _workContext;
        private IStoreContext _storeContext;
        private Mock<IDiscountService> tempDiscountServiceMock;
        private IDiscountService _discountService;
        private ICategoryService _categoryService;
        private IManufacturerService _manufacturerService;
        private IProductAttributeParser _productAttributeParser;
        private Mock<IProductService> tempProductService;
        private IProductService _productService;
        private ShoppingCartSettings _shoppingCartSettings;
        private CatalogSettings _catalogSettings;
        private CurrencySettings _currencySettings;
        private ICacheManager _cacheManager;
        private IPriceCalculationService _priceCalcService;
        private IVendorService _vendorService;
        private IStoreService _storeService;
        private ICustomerService _customerService;
        private ICurrencyService _currencyService;
        [TestInitialize()]
        public void TestInitialize()
        {
            tempWorkContext = new Mock<IWorkContext>();
            {
                _workContext = tempWorkContext.Object;
            }
            var tempStoreContext = new Mock<IStoreContext>();
            {
                _store = new Store { Id = "1" };
                tempStoreContext.Setup(instance => instance.CurrentStore).Returns(_store);
                _storeContext = tempStoreContext.Object;
            }
            tempDiscountServiceMock = new Mock<IDiscountService>();
            {
                _discountService = tempDiscountServiceMock.Object;
            }

            _categoryService = new Mock<ICategoryService>().Object;
            _manufacturerService = new Mock<IManufacturerService>().Object;
            _vendorService = new Mock<IVendorService>().Object;
            _storeService = new Mock<IStoreService>().Object;
            _customerService = new Mock<ICustomerService>().Object;
            _currencyService = new Mock<ICurrencyService>().Object;
            tempProductService = new Mock<IProductService>();
            {
                _productService = tempProductService.Object;
            }
            _productAttributeParser = new Mock<IProductAttributeParser>().Object;
            _shoppingCartSettings = new ShoppingCartSettings();
            _catalogSettings = new CatalogSettings();
            _currencySettings = new CurrencySettings();
            _cacheManager = new GrandNullCache();
            _priceCalcService = new PriceCalculationService(
                _workContext,
                _storeContext,
                _discountService,
                _categoryService,
                _manufacturerService,
                _productAttributeParser,
                _productService,
                _customerService,
                _cacheManager,
                _vendorService,
                _storeService,
                _currencyService,
                _shoppingCartSettings,
                _catalogSettings,
                _currencySettings);
        }

        [TestMethod()]
        public void Can_get_final_product_price()
        {
            var product = new Product
            {
                Id = "1",
                Name = "product name 01",
                Price = 49.99M,
                CustomerEntersPrice = false,
                Published = true
            };

            var customer = new Customer();

            Assert.AreEqual(49.99M, _priceCalcService.GetFinalPrice(product, customer, 0, false, 1));
            //returned price FOR ONE UNIT should be the same, even if quantity is different than 1
            Assert.AreEqual(49.99M, _priceCalcService.GetFinalPrice(product, customer, 0, false, 10));
        }

        [TestMethod()]
        public void Can_get_final_product_price_with_tier_prices()
        {
            var product = new Product
            {
                Id = "1",
                Name = "product name 01",
                Price = 49.99M,
                CustomerEntersPrice = false,
                Published = true,
                HasTierPrices = true //set to true - now Product has some TierPrices
            };

            //TierPrice is simply "the more you buy, the less you pay"
            product.TierPrices.Add(new TierPrice { Price = 10M, Quantity = 10, ProductId = "1" });
            product.TierPrices.Add(new TierPrice { Price = 2M, Quantity = 200, ProductId = "1" });

            Customer customer = new Customer();

            /*
            quantity: <=9           price: 49.99
            quantity: 10-199        price: 10
            quantity: >=200         price: 2
            */

            Assert.AreEqual(49.99M, _priceCalcService.GetFinalPrice(product, customer, 0, false, 1));
            Assert.AreEqual(49.99M, _priceCalcService.GetFinalPrice(product, customer, 0, false, 5));
            Assert.AreEqual(49.99M, _priceCalcService.GetFinalPrice(product, customer, 0, false, 9));

            Assert.AreEqual(10M, _priceCalcService.GetFinalPrice(product, customer, 0, false, 10));
            Assert.AreEqual(10M, _priceCalcService.GetFinalPrice(product, customer, 0, false, 11));
            Assert.AreEqual(10M, _priceCalcService.GetFinalPrice(product, customer, 0, false, 151));
            Assert.AreEqual(10M, _priceCalcService.GetFinalPrice(product, customer, 0, false, 199));

            Assert.AreEqual(2M, _priceCalcService.GetFinalPrice(product, customer, 0, false, 200));
            Assert.AreEqual(2M, _priceCalcService.GetFinalPrice(product, customer, 0, false, 201));
            Assert.AreEqual(2M, _priceCalcService.GetFinalPrice(product, customer, 0, false, 22201));
        }

        [TestMethod()]
        public void Can_get_final_product_price_with_tier_prices_by_customerRole()
        {
            /*
            this test shows how property "Price" of class "Product" can change in relation to:
                > TierPrice (the more you buy, the less you pay)
                > CustomerRole (some of our customers are more "valuable" than others e.g. wholesale customer
            */

            var product = new Product
            {
                Id = "1",
                Name = "product name 01",
                Price = 49.99M,
                CustomerEntersPrice = false,
                Published = true,
                HasTierPrices = true
            };

            //this is normal user - normal prices
            var customerRoleNormal = new CustomerRole { Id = "101", Name = "same role 01", Active = true };
            //very important person - extra off
            var customerRoleVip = new CustomerRole { Id = "201", Name = "vip role 02", Active = true };
            //king of the world - he will have everything for free
            var customerRoleKingOfWorld = new CustomerRole { Id = "301", Name = "king role 02", Active = true };

            //101 stands for normal user
            //201 vip
            //301 king
            product.TierPrices.Add(new TierPrice { Price = 40M, Quantity = 5, ProductId = "1", CustomerRoleId = "101" });
            product.TierPrices.Add(new TierPrice { Price = 2M, Quantity = 1000, ProductId = "1", CustomerRoleId = "101" });
            product.TierPrices.Add(new TierPrice { Price = 20M, Quantity = 5, ProductId = "1", CustomerRoleId = "201" });
            product.TierPrices.Add(new TierPrice { Price = 2M, Quantity = 1000, ProductId = "1", CustomerRoleId = "201" });
            product.TierPrices.Add(new TierPrice { Price = 10M, Quantity = 5, ProductId = "1", CustomerRoleId = "301" });
            product.TierPrices.Add(new TierPrice { Price = 2M, Quantity = 1000, ProductId = "1", CustomerRoleId = "301" });

            //all of these Customers will pay 49.99 for one product, if they buy less than 5 products
            //but when they buy exactly 5 or more..
            //then they will pay:
            //      normal 5x40     = 200
            //      vip 5x20        = 100
            //      king 5x10       = 50

            //but
            //all of them will pay only 2 per unit if they buy at least quantity of 1000 
            //2x1000                = 2000

            //lets try normal customer
            var normalCustomer = new Customer();
            normalCustomer.CustomerRoles.Add(customerRoleNormal);
            //49.99 - 40 - 2
            Assert.AreEqual(49.99M, _priceCalcService.GetFinalPrice(product, normalCustomer, 0, false, 1));
            Assert.AreEqual(40, _priceCalcService.GetFinalPrice(product, normalCustomer, 0, false, 5));
            Assert.AreEqual(2, _priceCalcService.GetFinalPrice(product, normalCustomer, 0, false, 1000));

            //lets try vip customer
            var vipCustomer = new Customer();
            vipCustomer.CustomerRoles.Add(customerRoleVip);
            //49.99 - 20 - 2
            Assert.AreEqual(49.99M, _priceCalcService.GetFinalPrice(product, vipCustomer, 0, false, 1));
            Assert.AreEqual(20, _priceCalcService.GetFinalPrice(product, vipCustomer, 0, false, 5));
            Assert.AreEqual(2, _priceCalcService.GetFinalPrice(product, vipCustomer, 0, false, 1000));

            //lets try king customer
            var kingCustomer = new Customer();
            kingCustomer.CustomerRoles.Add(customerRoleKingOfWorld);
            //49.99 - 10 - 2
            Assert.AreEqual(49.99M, _priceCalcService.GetFinalPrice(product, kingCustomer, 0, false, 1));
            Assert.AreEqual(10, _priceCalcService.GetFinalPrice(product, kingCustomer, 0, false, 5));
            Assert.AreEqual(2, _priceCalcService.GetFinalPrice(product, kingCustomer, 0, false, 1000));
            //I know it is intricate, but all works fine
        }

        [TestMethod()]
        public void Can_get_final_product_price_with_additionalFee()
        {
            //tests if price is valid for additional charge (additional fee) 
            var product = new Product
            {
                Id = "1",
                Name = "product name 01",
                Price = 49.99M,
                CustomerEntersPrice = false,
                Published = true
            };

            var customer = new Customer();

            //additional charge +1000
            //==1049.99
            Assert.AreEqual(1049.99M, _priceCalcService.GetFinalPrice(product, customer, 1000, false, 1));
        }

        [TestMethod()]
        public void Can_get_final_product_price_with_discount()
        {
            var product = new Product
            {
                Id = "1",
                Name = "product name 01",
                Price = 49.99M,
                CustomerEntersPrice = false,
                Published = true,
            };

            var customer = new Customer();

            var discount001 = new Discount
            {
                Id = "1",
                Name = "Discount 001",
                DiscountType = DiscountType.AssignedToSkus,
                DiscountAmount = 10,
                DiscountLimitation = DiscountLimitationType.Unlimited
            };

            tempDiscountServiceMock.Setup(x => x.GetDiscountById(discount001.Id)).Returns(discount001);

            product.AppliedDiscounts.Add(discount001.Id);

            tempDiscountServiceMock.Setup(x => x.ValidateDiscount(discount001, customer)).Returns(new DiscountValidationResult() { IsValid = true });
            tempDiscountServiceMock.Setup(x => x.GetAllDiscounts(DiscountType.AssignedToCategories, "", "", false)).Returns(new List<Discount>());
            tempDiscountServiceMock.Setup(x => x.GetAllDiscounts(DiscountType.AssignedToManufacturers, "", "", false)).Returns(new List<Discount>());
            tempDiscountServiceMock.Setup(x => x.GetAllDiscounts(DiscountType.AssignedToAllProducts, "", "", false)).Returns(new List<Discount>());

            var discountAmount = discount001.DiscountAmount;
            tempDiscountServiceMock.Setup(x => x.GetPreferredDiscount(It.IsAny<List<AppliedDiscount>>(), 49.99M, out discountAmount)).Returns(new List<AppliedDiscount>());

            //it should return 39.99 - price cheaper about 10 
            Assert.AreEqual(39.99M, _priceCalcService.GetFinalPrice(product, customer, 0, true, 1));
        }

        [TestMethod()]
        public void Can_get_shopping_cart_item_unitPrice()
        {
            var customer001 = new Customer { Id = "98767" };
            tempWorkContext.Setup(x => x.CurrentCustomer).Returns(customer001);

            var product001 = new Product
            {
                Id = "242422",
                Name = "product name 01",
                Price = 49.99M,
                CustomerEntersPrice = false,
                Published = true,
            };
            tempProductService.Setup(x => x.GetProductById("242422")).Returns(product001);

            var shoppingCartItem = new ShoppingCartItem
            {
                ProductId = "242422",// product001.Id, //222
                Quantity = 2
            };

            customer001.HasShoppingCartItems = true;
            customer001.ShoppingCartItems.Add(shoppingCartItem);

            tempDiscountServiceMock.Setup(x => x.GetAllDiscounts(DiscountType.AssignedToCategories, "", "", false)).Returns(new List<Discount>());
            tempDiscountServiceMock.Setup(x => x.GetAllDiscounts(DiscountType.AssignedToManufacturers, "", "", false)).Returns(new List<Discount>());
            tempDiscountServiceMock.Setup(x => x.GetAllDiscounts(DiscountType.AssignedToAllProducts, "", "", false)).Returns(new List<Discount>());

            Assert.AreEqual(49.99M, _priceCalcService.GetUnitPrice(shoppingCartItem));
        }

        [TestMethod()]
        public void Can_get_shopping_cart_item_subTotal()
        {
            var product001 = new Product
            {
                Id = "242422",
                Name = "product name 01",
                Price = 55.11M,
                CustomerEntersPrice = false,
                Published = true,
            };
            tempProductService.Setup(x => x.GetProductById("242422")).Returns(product001);

            var customer001 = new Customer { Id = "98767", HasShoppingCartItems = true };
            tempWorkContext.Setup(x => x.CurrentCustomer).Returns(customer001);

            var shoppingCartItem = new ShoppingCartItem
            {
                ProductId = product001.Id, //222
                Quantity = 2
            };

            customer001.ShoppingCartItems.Add(shoppingCartItem);

            tempDiscountServiceMock.Setup(x => x.GetAllDiscounts(DiscountType.AssignedToCategories, "", "", false)).Returns(new List<Discount>());
            tempDiscountServiceMock.Setup(x => x.GetAllDiscounts(DiscountType.AssignedToManufacturers, "", "", false)).Returns(new List<Discount>());
            tempDiscountServiceMock.Setup(x => x.GetAllDiscounts(DiscountType.AssignedToAllProducts, "", "", false)).Returns(new List<Discount>());

            Assert.AreEqual(110.22M, _priceCalcService.GetSubTotal(shoppingCartItem));
        }
    }
}