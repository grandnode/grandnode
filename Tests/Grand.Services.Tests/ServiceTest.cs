using Grand.Core.Plugins;
using Grand.Services.Tests.Directory;
using Grand.Services.Tests.Payments;
using Grand.Services.Tests.Shipping;
using Grand.Services.Tests.Tax;
using System.Collections.Generic;

namespace Grand.Services.Tests
{
    public class ServiceTest
    {
        public void PluginInitializator()
        {
            InitPlugins();
        }

        private void InitPlugins()
        {
            var plugins = new List<PluginDescriptor>();
            plugins.Add(new PluginDescriptor(typeof(FixedRateTestTaxProvider).Assembly,
                null, typeof(FixedRateTestTaxProvider))
            {
                SystemName = "FixedTaxRateTest",
                FriendlyName = "Fixed tax test rate provider",
                Installed = true,
            });
            plugins.Add(new PluginDescriptor(typeof(FixedRateTestShippingRateComputationMethod).Assembly,
                null, typeof(FixedRateTestShippingRateComputationMethod))
            {
                SystemName = "FixedRateTestShippingRateComputationMethod",
                FriendlyName = "Fixed rate test shipping computation method",
                Installed = true,
            });
            plugins.Add(new PluginDescriptor(typeof(TestPaymentMethod).Assembly,
                null, typeof(TestPaymentMethod))
            {
                SystemName = "Payments.TestMethod",
                FriendlyName = "Test payment method",
                Installed = true,
            });
            plugins.Add(new PluginDescriptor(typeof(TestExchangeRateProvider).Assembly,
                null, typeof(TestExchangeRateProvider))
            {
                SystemName = "CurrencyExchange.TestProvider",
                FriendlyName = "Test exchange rate provider",
                Installed = true,
            });
            PluginManager.ReferencedPlugins = plugins;
        }
    }
}