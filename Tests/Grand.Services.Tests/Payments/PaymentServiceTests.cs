using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Core.Plugins;
using Grand.Services.Configuration;
using Grand.Services.Directory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Payments.Tests
{
    [TestClass()]
    public class PaymentServiceTests
    {
        private PaymentSettings _paymentSettings;
        private ShoppingCartSettings _shoppingCartSettings;
        private ISettingService _settingService;
        private IPaymentService _paymentService;
        private ICurrencyService _currencyService;
        private IServiceProvider _serviceProvider;

        [TestInitialize()]
        public void TestInitialize()
        {
            //plugin initialization
            new Grand.Services.Tests.ServiceTest().PluginInitializator();
            _serviceProvider = new Mock<IServiceProvider>().Object;
            _paymentSettings = new PaymentSettings();
            _paymentSettings.ActivePaymentMethodSystemNames = new List<string>();
            _paymentSettings.ActivePaymentMethodSystemNames.Add("Payments.TestMethod");
            var pluginFinder = new PluginFinder(_serviceProvider);
            _shoppingCartSettings = new ShoppingCartSettings();
            _settingService = new Mock<ISettingService>().Object;
            _currencyService = new Mock<ICurrencyService>().Object;

            _paymentService = new PaymentService(_paymentSettings, pluginFinder, _settingService, _currencyService, _shoppingCartSettings);
        }

        //TO DO
        //[TestMethod()]
        //public async Task Can_load_paymentMethods()
        //{
        //    var srcm = await _paymentService.LoadActivePaymentMethods();
        //    Assert.IsNotNull(srcm);
        //    Assert.IsTrue(srcm.Count > 0);
        //}

        //[TestMethod()]
        //public void Can_load_paymentMethod_by_systemKeyword()
        //{
        //    var srcm = _paymentService.LoadPaymentMethodBySystemName("Payments.TestMethod");
        //    Assert.IsNotNull(srcm);
        //}

        //[TestMethod()]
        //public async Task Can_load_active_paymentMethods()
        //{
        //    var srcm = await _paymentService.LoadActivePaymentMethods();
        //    Assert.IsNotNull(srcm);
        //    Assert.IsTrue(srcm.Count > 0);
        //}

        [TestMethod()]
        public void Can_get_masked_credit_card_number()
        {
            Assert.AreEqual("", _paymentService.GetMaskedCreditCardNumber(""));
            Assert.AreEqual("123", _paymentService.GetMaskedCreditCardNumber("123"));
            Assert.AreEqual("************3456", _paymentService.GetMaskedCreditCardNumber("1234567890123456"));
        }
    }
}