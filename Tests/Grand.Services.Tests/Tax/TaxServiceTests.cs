using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Tax;
using Grand.Core.Plugins;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Grand.Services.Tax.Tests
{
    [TestClass()]
    public class TaxServiceTests
    {
        private IAddressService _addressService;
        private IWorkContext _workContext;
        private TaxSettings _taxSettings;
        private ITaxService _taxService;
        private IGeoLookupService _geoLookupService;
        private ICountryService _countryService;
        private CustomerSettings _customerSettings;
        private AddressSettings _addressSettings;
        private ILogger _logger;
        private IPluginFinder _pluginFinder;

        [TestInitialize()]
        public void TestInitialize()
        {
            //plugin initialization
            new Grand.Services.Tests.ServiceTest().PluginInitializator();

            _pluginFinder = new PluginFinder();
            _taxSettings = new TaxSettings();
            _workContext = null;
            _addressService = new Mock<IAddressService>().Object;
            _geoLookupService = new Mock<IGeoLookupService>().Object;
            _countryService = new Mock<ICountryService>().Object;
            _customerSettings = new CustomerSettings();
            _addressSettings = new AddressSettings();
            _logger = new NullLogger();

            _taxService = new TaxService(_addressService, _workContext, _taxSettings,
                _pluginFinder, _geoLookupService, _countryService, _logger,
                _customerSettings, _addressSettings);
        }

        [TestMethod()]
        public void Can_load_taxProviders()
        {

            var providers = _taxService.LoadAllTaxProviders();
            Assert.IsNotNull(providers);
            Assert.IsTrue(providers.Count > 0);
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
        public void Can_get_productPrice_priceIncludesTax_includingTax_taxable()
        {
            var customer = new Customer();
            var product = new Product();
            decimal taxRate;

            Assert.AreEqual(1000, _taxService.GetProductPrice(product, "0", 1000M, true, customer, true, out taxRate));
        }

        [TestMethod()]
        public void Can_get_productPrice_priceIncludesTax_includingTax_non_taxable()
        {

            var customer = new Customer { IsTaxExempt = true }; //not taxable
            var product = new Product();


            decimal taxRate;
            Assert.AreEqual(909.0909090909090909090909091M, _taxService.GetProductPrice(product, "0", 1000M, true, customer, true, out taxRate));
            Assert.AreEqual(1000M, _taxService.GetProductPrice(product, "0", 1000M, true, customer, false, out taxRate));
            Assert.AreEqual(909.0909090909090909090909091M, _taxService.GetProductPrice(product, "0", 1000M, false, customer, true, out taxRate));
            Assert.AreEqual(1000, _taxService.GetProductPrice(product, "0", 1000M, false, customer, false, out taxRate));
        }

        [TestMethod()]
        public void Should_assume_valid_VAT_number_if_EuVatAssumeValid_setting_is_true()
        {
            _taxSettings.EuVatAssumeValid = true;
            string name, address;

            VatNumberStatus vatNumberStatus = _taxService.GetVatNumberStatus("GB", "000 0000 00",
                out name, out address);
            Assert.AreEqual(VatNumberStatus.Valid, vatNumberStatus);
        }

        [TestMethod()]
        public void GetProductPriceQuickly_NonTaxExemptAndPriceIncludingTax_ShouldReturnTheSameValues()
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

            decimal taxRate = default(decimal);
            //these 6 methods..
            var scUnitPriceInclTax = _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, true, customer, true, out taxRate);
            var scUnitPriceExclTax = _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, false, customer, true, out taxRate);
            var scSubTotalInclTax = _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, true, customer, true, out taxRate);
            var scSubTotalExclTax = _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, false, customer, true, out taxRate);
            var discountAmountInclTax = _taxService.GetProductPrice(product, taxCategoryId, discountAmount, true, customer, true, out taxRate);
            var discountAmountExclTax = _taxService.GetProductPrice(product, taxCategoryId, discountAmount, false, customer, true, out taxRate);
            //..should return the same value as this one method's properties are having
            var result02 = _taxService.GetTaxProductPrice(product, customer, out taxRate, scUnitPrice, scUnitPriceWithoutDiscount, scSubTotal, discountAmount, true);

            Assert.AreEqual(scUnitPriceInclTax, result02.UnitPriceInclTax, "unit price including tax");
            Assert.AreEqual(scUnitPriceExclTax, result02.UnitPriceExclTax, "unit price excluding tax");
            Assert.AreEqual(scSubTotalInclTax, result02.SubTotalInclTax, "sub total including tax");
            Assert.AreEqual(scSubTotalExclTax, result02.SubTotalExclTax, "sub total excluding tax");
            Assert.AreEqual(discountAmountInclTax, result02.discountAmountInclTax, "discount including tax");
            Assert.AreEqual(discountAmountExclTax, result02.discountAmountExclTax, "discount excluding tax");
        }

        [TestMethod()]
        public void GetProductPriceQuickly_NonTaxExemptAndPriceExcludingTax_ShouldReturnTheSameValues()
        {
            var product = new Product();
            product.TaxCategoryId = "57516fc81b0dc92b20fdd2ef";
            product.IsTelecommunicationsOrBroadcastingOrElectronicServices = false;
            product.IsTaxExempt = false;
            var customer = new Customer();
            customer.IsTaxExempt = false;
            string taxCategoryId = "";  //as in code

            decimal scUnitPriceWithoutDiscount = 1000.00M;
            decimal scUnitPrice = 1000.00M;
            decimal scSubTotal = 5000.00M;
            decimal discountAmount = 7000.00M;

            decimal taxRate = default(decimal);
            var scUnitPriceInclTax = _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, true, customer, false, out taxRate);
            var scUnitPriceExclTax = _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, false, customer, false, out taxRate);
            var scSubTotalInclTax = _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, true, customer, false, out taxRate);
            var scSubTotalExclTax = _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, false, customer, false, out taxRate);
            var discountAmountInclTax = _taxService.GetProductPrice(product, taxCategoryId, discountAmount, true, customer, false, out taxRate);
            var discountAmountExclTax = _taxService.GetProductPrice(product, taxCategoryId, discountAmount, false, customer, false, out taxRate);

            var result02 = _taxService.GetTaxProductPrice(product, customer, out taxRate, scUnitPrice, scUnitPriceWithoutDiscount, scSubTotal, discountAmount, false);

            Assert.AreEqual(scUnitPriceInclTax, result02.UnitPriceInclTax, "unit price including tax");
            Assert.AreEqual(scUnitPriceExclTax, result02.UnitPriceExclTax, "unit price excluding tax");
            Assert.AreEqual(scSubTotalInclTax, result02.SubTotalInclTax, "sub total including tax");
            Assert.AreEqual(scSubTotalExclTax, result02.SubTotalExclTax, "sub total excluding tax");
            Assert.AreEqual(discountAmountInclTax, result02.discountAmountInclTax, "discount including tax");
            Assert.AreEqual(discountAmountExclTax, result02.discountAmountExclTax, "discount excluding tax");
        }

        [TestMethod()]
        public void GetProductPriceQuickly_TaxExemptAndPriceIncludingTax_ShouldReturnTheSameValues()
        {
            var product = new Product();
            product.TaxCategoryId = "57516fc81b0dc92b20fdd2ef";
            product.IsTelecommunicationsOrBroadcastingOrElectronicServices = false;
            product.IsTaxExempt = true;
            var customer = new Customer();
            customer.IsTaxExempt = true;
            string taxCategoryId = "";

            decimal scUnitPriceWithoutDiscount = 1000.00M;
            decimal scUnitPrice = 1000.00M;
            decimal scSubTotal = 5000.00M;
            decimal discountAmount = 7000.00M;

            decimal taxRate = default(decimal);
            var scUnitPriceInclTax = _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, true, customer, true, out taxRate);
            var scUnitPriceExclTax = _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, false, customer, true, out taxRate);
            var scSubTotalInclTax = _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, true, customer, true, out taxRate);
            var scSubTotalExclTax = _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, false, customer, true, out taxRate);
            var discountAmountInclTax = _taxService.GetProductPrice(product, taxCategoryId, discountAmount, true, customer, true, out taxRate);
            var discountAmountExclTax = _taxService.GetProductPrice(product, taxCategoryId, discountAmount, false, customer, true, out taxRate);

            var result02 = _taxService.GetTaxProductPrice(product, customer, out taxRate, scUnitPrice, scUnitPriceWithoutDiscount, scSubTotal, discountAmount, true);

            Assert.AreEqual(scUnitPriceInclTax, result02.UnitPriceInclTax, "unit price including tax");
            Assert.AreEqual(scUnitPriceExclTax, result02.UnitPriceExclTax, "unit price excluding tax");
            Assert.AreEqual(scSubTotalInclTax, result02.SubTotalInclTax, "sub total including tax");
            Assert.AreEqual(scSubTotalExclTax, result02.SubTotalExclTax, "sub total excluding tax");
            Assert.AreEqual(discountAmountInclTax, result02.discountAmountInclTax, "discount including tax");
            Assert.AreEqual(discountAmountExclTax, result02.discountAmountExclTax, "discount excluding tax");
        }

        [TestMethod()]
        public void GetProductPriceQuickly_TaxExemptAndPriceExcludingTax_ShouldReturnTheSameValues()
        {
            var product = new Product();
            product.TaxCategoryId = "57516fc81b0dc92b20fdd2ef";
            product.IsTelecommunicationsOrBroadcastingOrElectronicServices = false;
            product.IsTaxExempt = true;
            var customer = new Customer();
            customer.IsTaxExempt = true;
            string taxCategoryId = "";

            decimal scUnitPriceWithoutDiscount = 1000.00M;
            decimal scUnitPrice = 1000.00M;
            decimal scSubTotal = 5000.00M;
            decimal discountAmount = 7000.00M;

            decimal taxRate = default(decimal);
            var scUnitPriceInclTax = _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, true, customer, false, out taxRate);
            var scUnitPriceExclTax = _taxService.GetProductPrice(product, taxCategoryId, scUnitPrice, false, customer, false, out taxRate);
            var scSubTotalInclTax = _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, true, customer, false, out taxRate);
            var scSubTotalExclTax = _taxService.GetProductPrice(product, taxCategoryId, scSubTotal, false, customer, false, out taxRate);
            var discountAmountInclTax = _taxService.GetProductPrice(product, taxCategoryId, discountAmount, true, customer, false, out taxRate);
            var discountAmountExclTax = _taxService.GetProductPrice(product, taxCategoryId, discountAmount, false, customer, false, out taxRate);

            var result02 = _taxService.GetTaxProductPrice(product, customer, out taxRate, scUnitPrice, scUnitPriceWithoutDiscount, scSubTotal, discountAmount, false);

            Assert.AreEqual(scUnitPriceInclTax, result02.UnitPriceInclTax, "unit price including tax");
            Assert.AreEqual(scUnitPriceExclTax, result02.UnitPriceExclTax, "unit price excluding tax");
            Assert.AreEqual(scSubTotalInclTax, result02.SubTotalInclTax, "sub total including tax");
            Assert.AreEqual(scSubTotalExclTax, result02.SubTotalExclTax, "sub total excluding tax");
            Assert.AreEqual(discountAmountInclTax, result02.discountAmountInclTax, "discount including tax");
            Assert.AreEqual(discountAmountExclTax, result02.discountAmountExclTax, "discount excluding tax");
        }

    }
}