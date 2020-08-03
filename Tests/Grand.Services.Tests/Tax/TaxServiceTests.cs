using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Tax;
using Grand.Core.Plugins;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Logging;
using Grand.Services.Tests.Tax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Grand.Services.Tax.Tests
{
    [TestClass()]
    public class TaxServiceTests
    {
        private IAddressService _addressService;
        private IWorkContext _workContext;
        private TaxSettings _taxSettings;
        private ITaxService _taxService;
        private IVatService _vatService;
        private IGeoLookupService _geoLookupService;
        private ICountryService _countryService;
        private CustomerSettings _customerSettings;
        private AddressSettings _addressSettings;
        private ILogger _logger;
        private IPluginFinder _pluginFinder;
        private IServiceProvider _serviceProvider;
        [TestInitialize()]
        public void TestInitialize()
        {
            //plugin initialization
            new Services.Tests.ServiceTest().PluginInitializator();
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(FixedRateTestTaxProvider))).Returns(new FixedRateTestTaxProvider());
            _serviceProvider = serviceProvider.Object;
            
            _pluginFinder = new PluginFinder(_serviceProvider);
            _taxSettings = new TaxSettings();
            _taxSettings.ActiveTaxProviderSystemName = "FixedTaxRateTest";
            _workContext = null;
            _addressService = new Mock<IAddressService>().Object;
            _geoLookupService = new Mock<IGeoLookupService>().Object;
            _countryService = new Mock<ICountryService>().Object;
            _customerSettings = new CustomerSettings();
            _addressSettings = new AddressSettings();
            _logger = new NullLogger();
            
            _taxService = new TaxService(_addressService, _workContext, 
                _pluginFinder, _geoLookupService, _countryService, _logger,
                _taxSettings, _customerSettings, _addressSettings);

            _vatService = new VatService(_taxSettings);
        }

        [TestMethod()]
        public void Can_check_taxExempt_product()
        {

            var product = new Product();
            product.IsTaxExempt = true;
            Assert.IsTrue(_taxService.IsTaxExempt(product, null));
            product.IsTaxExempt = false;
            Assert.IsFalse(_taxService.IsTaxExempt(product, null));
        }

        [TestMethod()]
        public void Can_check_taxExempt_customer()
        {

            var customer = new Customer();
            customer.IsTaxExempt = true;
            Assert.IsTrue(_taxService.IsTaxExempt(null, customer));
            customer.IsTaxExempt = false;
            Assert.IsFalse(_taxService.IsTaxExempt(null, customer));
        }

        [TestMethod()]
        public void Can_check_taxExempt_customer_in_taxExemptCustomerRole()
        {
            var customer = new Customer();
            customer.IsTaxExempt = false;
            Assert.IsFalse(_taxService.IsTaxExempt(null, customer));

            var customerRole = new CustomerRole
            {
                TaxExempt = true,
                Active = true
            };
            customer.CustomerRoles.Add(customerRole);
            Assert.IsTrue(_taxService.IsTaxExempt(null, customer));
            customerRole.TaxExempt = false;
            Assert.IsFalse(_taxService.IsTaxExempt(null, customer));

            customerRole.Active = false;
            Assert.IsFalse(_taxService.IsTaxExempt(null, customer));
        }

        [TestMethod()]
        public async Task Can_get_productPrice_priceIncludesTax_includingTax_taxable()
        {
            var customer = new Customer();
            var product = new Product();
            var pp = await _taxService.GetProductPrice(product, "0", 1000M, true, customer, true);
            var price = pp.productprice;
            Assert.AreEqual(1000, price);
        }

        [TestMethod()]
        public async Task Can_get_productPrice_priceIncludesTax_includingTax_non_taxable()
        {

            var customer = new Customer { IsTaxExempt = true }; //not taxable
            var product = new Product();

            Assert.AreEqual(Math.Round(909.0909090909090909090909091M,2), (await _taxService.GetProductPrice(product, "0", 1000M, true, customer, true)).productprice);
            Assert.AreEqual(1000M, (await _taxService.GetProductPrice(product, "0", 1000M, true, customer, false)).productprice);
            Assert.AreEqual(Math.Round(909.0909090909090909090909091M,2), (await _taxService.GetProductPrice(product, "0", 1000M, false, customer, true)).productprice);
            Assert.AreEqual(1000, (await _taxService.GetProductPrice(product, "0", 1000M, false, customer, false)).productprice);
        }

        [TestMethod()]
        public async Task Should_assume_valid_VAT_number_if_EuVatAssumeValid_setting_is_true()
        {
            _taxSettings.EuVatAssumeValid = true;
            VatNumberStatus vatNumberStatus = (await _vatService.GetVatNumberStatus("GB", "000 0000 00")).status;
            Assert.AreEqual(VatNumberStatus.Valid, vatNumberStatus);
        }

        [TestMethod()]
        public async Task GetProductPriceQuickly_NonTaxExemptAndPriceIncludingTax_ShouldReturnTheSameValues()
        {
            var product = new Product();
            product.IsTaxExempt = false;
            var customer = new Customer();
            customer.IsTaxExempt = false;

            string taxCategoryId = "";  //as in code
            decimal scUnitPriceWithoutDiscount = 1000.00M;
            decimal scUnitPrice = 1000.00M;
            decimal scSubTotal = 5000.00M;
            decimal discountAmount = 7000.00M;

            //these 6 methods..
            var scUnitPriceInclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, true, customer, true)).productprice;
            var scUnitPriceExclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, false, customer, true)).productprice;
            var scSubTotalInclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, true, customer, true)).productprice;
            var scSubTotalExclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, false, customer, true)).productprice;
            var discountAmountInclTax = (await _taxService.GetProductPrice(product, taxCategoryId, discountAmount, true, customer, true)).productprice;
            var discountAmountExclTax = (await _taxService.GetProductPrice(product, taxCategoryId, discountAmount, false, customer, true)).productprice;
            //..should return the same value as this one method's properties are having
            var result02 = (await _taxService.GetTaxProductPrice(product, customer, scUnitPrice, scUnitPriceWithoutDiscount, scSubTotal, discountAmount, true));

            Assert.AreEqual(scUnitPriceInclTax, result02.UnitPriceInclTax, "unit price including tax");
            Assert.AreEqual(scUnitPriceExclTax, result02.UnitPriceExclTax, "unit price excluding tax");
            Assert.AreEqual(scSubTotalInclTax, result02.SubTotalInclTax, "sub total including tax");
            Assert.AreEqual(scSubTotalExclTax, result02.SubTotalExclTax, "sub total excluding tax");
            Assert.AreEqual(discountAmountInclTax, result02.discountAmountInclTax, "discount including tax");
            Assert.AreEqual(discountAmountExclTax, result02.discountAmountExclTax, "discount excluding tax");
        }

        [TestMethod()]
        public async Task GetProductPriceQuickly_NonTaxExemptAndPriceExcludingTax_ShouldReturnTheSameValues()
        {
            var product = new Product();
            product.TaxCategoryId = "";
            product.IsTele = false;
            product.IsTaxExempt = false;
            var customer = new Customer();
            customer.IsTaxExempt = false;
            string taxCategoryId = "";  //as in code

            decimal scUnitPriceWithoutDiscount = 1000.00M;
            decimal scUnitPrice = 1000.00M;
            decimal scSubTotal = 5000.00M;
            decimal discountAmount = 7000.00M;

            var scUnitPriceInclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, true, customer, false)).productprice;
            var scUnitPriceExclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, false, customer, false)).productprice;
            var scSubTotalInclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, true, customer, false)).productprice;
            var scSubTotalExclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, false, customer, false)).productprice;
            var discountAmountInclTax = (await _taxService.GetProductPrice(product, taxCategoryId, discountAmount, true, customer, false)).productprice;
            var discountAmountExclTax = (await _taxService.GetProductPrice(product, taxCategoryId, discountAmount, false, customer, false)).productprice;

            var result02 = (await _taxService.GetTaxProductPrice(product, customer, scUnitPrice, scUnitPriceWithoutDiscount, scSubTotal, discountAmount, false));

            Assert.AreEqual(scUnitPriceInclTax, result02.UnitPriceInclTax, "unit price including tax");
            Assert.AreEqual(scUnitPriceExclTax, result02.UnitPriceExclTax, "unit price excluding tax");
            Assert.AreEqual(scSubTotalInclTax, result02.SubTotalInclTax, "sub total including tax");
            Assert.AreEqual(scSubTotalExclTax, result02.SubTotalExclTax, "sub total excluding tax");
            Assert.AreEqual(discountAmountInclTax, result02.discountAmountInclTax, "discount including tax");
            Assert.AreEqual(discountAmountExclTax, result02.discountAmountExclTax, "discount excluding tax");
        }

        [TestMethod()]
        public async Task GetProductPriceQuickly_TaxExemptAndPriceIncludingTax_ShouldReturnTheSameValues()
        {
            var product = new Product();
            product.TaxCategoryId = "57516fc81b0dc92b20fdd2ef";
            product.IsTele = false;
            product.IsTaxExempt = true;
            var customer = new Customer();
            customer.IsTaxExempt = true;
            string taxCategoryId = "";

            decimal scUnitPriceWithoutDiscount = 1000.00M;
            decimal scUnitPrice = 1000.00M;
            decimal scSubTotal = 5000.00M;
            decimal discountAmount = 7000.00M;

            var scUnitPriceInclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, true, customer, true)).productprice;
            var scUnitPriceExclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, false, customer, true)).productprice;
            var scSubTotalInclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, true, customer, true)).productprice;
            var scSubTotalExclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, false, customer, true)).productprice;
            var discountAmountInclTax = (await _taxService.GetProductPrice(product, taxCategoryId, discountAmount, true, customer, true)).productprice;
            var discountAmountExclTax = (await _taxService.GetProductPrice(product, taxCategoryId, discountAmount, false, customer, true)).productprice;

            var result02 = (await _taxService.GetTaxProductPrice(product, customer, scUnitPrice, scUnitPriceWithoutDiscount, scSubTotal, discountAmount, true));

            Assert.AreEqual(scUnitPriceInclTax, result02.UnitPriceInclTax, "unit price including tax");
            Assert.AreEqual(scUnitPriceExclTax, result02.UnitPriceExclTax, "unit price excluding tax");
            Assert.AreEqual(scSubTotalInclTax, result02.SubTotalInclTax, "sub total including tax");
            Assert.AreEqual(scSubTotalExclTax, result02.SubTotalExclTax, "sub total excluding tax");
            Assert.AreEqual(discountAmountInclTax, result02.discountAmountInclTax, "discount including tax");
            Assert.AreEqual(discountAmountExclTax, result02.discountAmountExclTax, "discount excluding tax");
        }
        
        [TestMethod()]
        public async Task GetProductPriceQuickly_TaxExemptAndPriceExcludingTax_ShouldReturnTheSameValues()
        {
            var product = new Product();
            product.TaxCategoryId = "57516fc81b0dc92b20fdd2ef";
            product.IsTele = false;
            product.IsTaxExempt = true;
            var customer = new Customer();
            customer.IsTaxExempt = true;
            string taxCategoryId = "";

            decimal scUnitPriceWithoutDiscount = 1000.00M;
            decimal scUnitPrice = 1000.00M;
            decimal scSubTotal = 5000.00M;
            decimal discountAmount = 7000.00M;

            var scUnitPriceInclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, true, customer, false)).productprice;
            var scUnitPriceExclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, false, customer, false)).productprice;
            var scSubTotalInclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, true, customer, false)).productprice;
            var scSubTotalExclTax = (await _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, false, customer, false)).productprice;
            var discountAmountInclTax = (await _taxService.GetProductPrice(product, taxCategoryId, discountAmount, true, customer, false)).productprice;
            var discountAmountExclTax = (await _taxService.GetProductPrice(product, taxCategoryId, discountAmount, false, customer, false)).productprice;

            var result02 = await (_taxService.GetTaxProductPrice(product, customer, scUnitPrice, scUnitPriceWithoutDiscount, scSubTotal, discountAmount, false));

            Assert.AreEqual(scUnitPriceInclTax, result02.UnitPriceInclTax, "unit price including tax");
            Assert.AreEqual(scUnitPriceExclTax, result02.UnitPriceExclTax, "unit price excluding tax");
            Assert.AreEqual(scSubTotalInclTax, result02.SubTotalInclTax, "sub total including tax");
            Assert.AreEqual(scSubTotalExclTax, result02.SubTotalExclTax, "sub total excluding tax");
            Assert.AreEqual(discountAmountInclTax, result02.discountAmountInclTax, "discount including tax");
            Assert.AreEqual(discountAmountExclTax, result02.discountAmountExclTax, "discount excluding tax");
        }

    }
}
