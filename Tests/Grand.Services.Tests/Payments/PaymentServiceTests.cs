using Grand.Core;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Core.Plugins;
using Grand.Services.Configuration;
using Grand.Services.Directory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Payments.Tests
{
    [TestClass()]
    public class PaymentServiceTests
    {
        private PaymentSettings _paymentSettings;
        private Mock<IPluginFinder> _pluginFinder;
        private ShoppingCartSettings _shoppingCartSettings;
        private Mock<ISettingService >_settingService;
        private IPaymentService _paymentService;
        private ICurrencyService _currencyService;
        private Mock<IServiceProvider> _serviceProvider;

        [TestInitialize()]
        public void TestInitialize()
        {
            //plugin initialization
            new Grand.Services.Tests.ServiceTest().PluginInitializator();
            _serviceProvider = new Mock<IServiceProvider>();
            _paymentSettings = new PaymentSettings();
            _paymentSettings.ActivePaymentMethodSystemNames = new List<string>();
            _paymentSettings.ActivePaymentMethodSystemNames.Add("Payments.TestMethod");
            _pluginFinder = new Mock<IPluginFinder>();
            _shoppingCartSettings = new ShoppingCartSettings();
            _settingService = new Mock<ISettingService>();
            _currencyService = new Mock<ICurrencyService>().Object;
            _pluginFinder.Setup(p => p.ServiceProvider).Returns(_serviceProvider.Object);
            _paymentService = new PaymentService(_paymentSettings, _pluginFinder.Object, _settingService.Object, _currencyService, _shoppingCartSettings);
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

        [TestMethod()]
        public void LoadPaymentMethodBySystemName_ReturnPeyment()
        {
            var systemName = "systemName";
            _paymentService.LoadPaymentMethodBySystemName(systemName);
            _pluginFinder.Verify(c => c.GetPluginDescriptorBySystemName<IPaymentMethod>(systemName, LoadPluginsMode.InstalledOnly), Times.Once);
        }

        [TestMethod()]
        public void GetRestrictedCountryIds()
        {
            var expectedResult =new List<string>() { "1", "2", "3", "4" };
            var method = new Mock<IPaymentMethod>();
            var expectedKey = "PaymentMethodRestictions.systemName";
            method.Setup(m => m.PluginDescriptor).Returns(new PluginDescriptor() { SystemName = "systemName" });
            _settingService.Setup(s => s.GetSettingByKey<List<string>>(It.IsAny<string>(), null, "", false)).Returns(() => expectedResult);

            var result=_paymentService.GetRestrictedCountryIds(method.Object);
            Assert.IsTrue(expectedResult.SequenceEqual(result));
            _settingService.Verify(s => s.GetSettingByKey<List<string>>(expectedKey, null, "", false), Times.Once);
        }

        [TestMethod()] 
        public void GetRestrictedCountryIds_ReturnEmptyList()
        {
            var method = new Mock<IPaymentMethod>();
            var expectedKey = "PaymentMethodRestictions.systemName";
            method.Setup(m => m.PluginDescriptor).Returns(new PluginDescriptor() { SystemName = "systemName" });
            _settingService.Setup(s => s.GetSettingByKey<List<string>>(It.IsAny<string>(), null, "", false)).Returns(() => null);
            var result = _paymentService.GetRestrictedCountryIds(method.Object);
            Assert.IsTrue(result.Count==0);
            _settingService.Verify(s => s.GetSettingByKey<List<string>>(expectedKey, null, "", false), Times.Once);
        }

        [TestMethod()] 
        public async Task SaveRestictedCountryIds_InvokeSettingsService()
        {
            var method = new Mock<IPaymentMethod>();
            var countryIds = new List<string>() { "1", "2", "3", "4" };
            var expectedKey = "PaymentMethodRestictions.systemName";
            method.Setup(m => m.PluginDescriptor).Returns(new PluginDescriptor() { SystemName = "systemName" });
            await _paymentService.SaveRestictedCountryIds(method.Object, countryIds);
            _settingService.Verify(s => s.SetSetting(expectedKey,It.IsAny<List<string>>(),"",true), Times.Once);
        }

        [TestMethod()] 
        public async Task ProcessPayment_OrderTotalZero_ReturnPaidPaymentStatus()
        {
            var request = new ProcessPaymentRequest();
            request.OrderTotal = decimal.Zero;
            var response = await _paymentService.ProcessPayment(request);
            Assert.IsTrue(response.NewPaymentStatus == PaymentStatus.Paid);
        }

        [TestMethod()]
        public async Task ProcessPayment_InvokeProcessPaymentFromPaymentMethod()
        {
            var request = new ProcessPaymentRequest() { PaymentMethodSystemName = "systemName" ,OrderTotal=500};
            var method = new Mock<IPaymentMethod>();
            var descriptorMock = new Mock<PluginDescriptor>();
            descriptorMock.Setup(c => c.Instance<IPaymentMethod>(It.IsAny<IServiceProvider>())).Returns(() => method.Object);
            _pluginFinder.Setup(c => c.GetPluginDescriptorBySystemName<IPaymentMethod>(It.IsAny<string>(), LoadPluginsMode.InstalledOnly)).Returns(() => descriptorMock.Object);
            await _paymentService.ProcessPayment(request);
            method.Verify(m => m.ProcessPayment(request), Times.Once);
        }

        [TestMethod()]
        public void ProcessPayment_NotFoundPaymentMethod_ThrowException()
        {
            var request = new ProcessPaymentRequest() { PaymentMethodSystemName = "systemName", OrderTotal = 500 };
            var method = new Mock<IPaymentMethod>();
            var descriptorMock = new Mock<PluginDescriptor>();
            descriptorMock.Setup(c => c.Instance<IPaymentMethod>(It.IsAny<IServiceProvider>())).Returns(() => null);
            _pluginFinder.Setup(c => c.GetPluginDescriptorBySystemName<IPaymentMethod>(It.IsAny<string>(), LoadPluginsMode.InstalledOnly)).Returns(() => descriptorMock.Object);
            Assert.ThrowsExceptionAsync<GrandException>(async ()=> await _paymentService.ProcessPayment(request));
        }

        [TestMethod()] 
        public async Task PostProcessPayment_InvokePostProccessFromPaymentMethod()
        {
            var request = new PostProcessPaymentRequest (){Order=new Order() {PaymentStatus=PaymentStatus.Authorized} };
            var method = new Mock<IPaymentMethod>();
            var descriptorMock = new Mock<PluginDescriptor>();
            descriptorMock.Setup(c => c.Instance<IPaymentMethod>(It.IsAny<IServiceProvider>())).Returns(() =>method.Object);
            _pluginFinder.Setup(c => c.GetPluginDescriptorBySystemName<IPaymentMethod>(It.IsAny<string>(), LoadPluginsMode.InstalledOnly)).Returns(() => descriptorMock.Object);
            await _paymentService.PostProcessPayment(request);
            method.Verify(m => m.PostProcessPayment(request), Times.Once);
        }

        [TestMethod()]
        public void PostProcessPayment_NotFoundPaymentMethod_ThrowException()
        {
            var request = new PostProcessPaymentRequest() { Order = new Order() { PaymentStatus = PaymentStatus.Authorized } };
            var method = new Mock<IPaymentMethod>();
            var descriptorMock = new Mock<PluginDescriptor>();
            descriptorMock.Setup(c => c.Instance<IPaymentMethod>(It.IsAny<IServiceProvider>())).Returns(() => null);
            _pluginFinder.Setup(c => c.GetPluginDescriptorBySystemName<IPaymentMethod>(It.IsAny<string>(), LoadPluginsMode.InstalledOnly)).Returns(() => descriptorMock.Object);
            _paymentService.PostProcessPayment(request);
            Assert.ThrowsExceptionAsync<GrandException>(async() => await _paymentService.PostProcessPayment(request), "Payment method couldn't be loaded");
        }


        [TestMethod()]
        public async Task ProcessRecurringPayment_InvokeProcessRecurringPaymentFromPaymentMethod()
        {
            var request = new ProcessPaymentRequest (){OrderTotal=500 };
            var method = new Mock<IPaymentMethod>();
            var descriptorMock = new Mock<PluginDescriptor>();
            descriptorMock.Setup(c => c.Instance<IPaymentMethod>(It.IsAny<IServiceProvider>())).Returns(() => method.Object);
            _pluginFinder.Setup(c => c.GetPluginDescriptorBySystemName<IPaymentMethod>(It.IsAny<string>(), LoadPluginsMode.InstalledOnly)).Returns(() => descriptorMock.Object);
            await _paymentService.ProcessRecurringPayment(request);
            method.Verify(m => m.ProcessRecurringPayment(request), Times.Once);
        }
    }
}