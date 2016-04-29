using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Tax;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Directory;
using Moq;
using Nop.Services.Logging;

namespace Nop.Services.Tax.Tests {
    [TestClass()]
    public class TaxServiceTests {
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
        public void TestInitialize() {
            //plugin initialization
            new Nop.Services.Tests.ServiceTest().PluginInitializator();

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
        public void Can_load_taxProviders() {

            var providers = _taxService.LoadAllTaxProviders();
            Assert.IsNotNull(providers);
            Assert.IsTrue(providers.Count > 0);
        }

        [TestMethod()]
        public void Can_check_taxExempt_product() {

            var product = new Product();
            product.IsTaxExempt = true;
            Assert.IsTrue(_taxService.IsTaxExempt(product, null));
            product.IsTaxExempt = false;
            Assert.IsFalse(_taxService.IsTaxExempt(product, null));
        }

        [TestMethod()]
        public void Can_check_taxExempt_customer() {

            var customer = new Customer();
            customer.IsTaxExempt = true;
            Assert.IsTrue(_taxService.IsTaxExempt(null, customer));
            customer.IsTaxExempt = false;
            Assert.IsFalse(_taxService.IsTaxExempt(null, customer));
        }

        [TestMethod()]
        public void Can_check_taxExempt_customer_in_taxExemptCustomerRole() {
            var customer = new Customer();
            customer.IsTaxExempt = false;
            Assert.IsFalse(_taxService.IsTaxExempt(null, customer));

            var customerRole = new CustomerRole {
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
        public void Can_get_productPrice_priceIncludesTax_includingTax_taxable() {
            var customer = new Customer();
            var product = new Product();
            decimal taxRate;

            Assert.AreEqual(1000, _taxService.GetProductPrice(product, "0", 1000M, true, customer, true, out taxRate));
        }

        [TestMethod()]
        public void Can_get_productPrice_priceIncludesTax_includingTax_non_taxable() {

            var customer = new Customer { IsTaxExempt = true }; //not taxable
            var product = new Product();


            decimal taxRate;
            Assert.AreEqual(909.0909090909090909090909091M, _taxService.GetProductPrice(product, "0", 1000M, true, customer, true, out taxRate));
            Assert.AreEqual(1000M, _taxService.GetProductPrice(product, "0", 1000M, true, customer, false, out taxRate));
            Assert.AreEqual(909.0909090909090909090909091M, _taxService.GetProductPrice(product, "0", 1000M, false, customer, true, out taxRate));
            Assert.AreEqual(1000, _taxService.GetProductPrice(product, "0", 1000M, false, customer, false, out taxRate));
        }

        [TestMethod()]
        public void Should_assume_valid_VAT_number_if_EuVatAssumeValid_setting_is_true() {
            _taxSettings.EuVatAssumeValid = true;
            string name, address;

            VatNumberStatus vatNumberStatus = _taxService.GetVatNumberStatus("GB", "000 0000 00",
                out name, out address);
            Assert.AreEqual(VatNumberStatus.Valid, vatNumberStatus);
        }
    }
}