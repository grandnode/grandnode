using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Directory;
using Grand.Core.Plugins;
using Grand.Services.Events;
using Grand.Services.Stores;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using Moq;
using System;
using System.Linq;

namespace Grand.Services.Directory.Tests
{
    [TestClass()]
    public class CurrencyServiceTests {
        private IRepository<Currency> _currencyRepository;
        private IStoreMappingService _storeMappingService;
        private CurrencySettings _currencySettings;
        private IEventPublisher _eventPublisher;
        private ICurrencyService _currencyService;

        private Currency currencyUSD, currencyRUR, currencyEUR;

        [TestInitialize()]
        public void TestInitialize() {
            //plugin initialization
            new Grand.Services.Tests.ServiceTest().PluginInitializator();

            currencyUSD = new Currency {
                Id = "1",
                Name = "US Dollar",
                CurrencyCode = "USD",
                Rate = 1.2M,
                DisplayLocale = "en-US",
                CustomFormatting = "",
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };
            currencyEUR = new Currency {
                Id = "2",
                Name = "Euro",
                CurrencyCode = "EUR",
                Rate = 1,
                DisplayLocale = "",
                CustomFormatting = "€0.00",
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };
            currencyRUR = new Currency {
                Id = "3",
                Name = "Russian Rouble",
                CurrencyCode = "RUB",
                Rate = 34.5M,
                DisplayLocale = "ru-RU",
                CustomFormatting = "",
                Published = true,
                DisplayOrder = 3,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };

            var tempCurrencyRepository = new Mock<IRepository<Currency>>();
            {
                var IMongoCollection = new Mock<IMongoCollection<Currency>>().Object;
                IMongoCollection.InsertOne(currencyUSD);
                IMongoCollection.InsertOne(currencyEUR);
                IMongoCollection.InsertOne(currencyRUR);

                tempCurrencyRepository.Setup(x => x.Table).Returns(IMongoCollection.AsQueryable());
                tempCurrencyRepository.Setup(x => x.GetById(currencyUSD.Id)).Returns(currencyUSD);
                tempCurrencyRepository.Setup(x => x.GetById(currencyEUR.Id)).Returns(currencyEUR);
                tempCurrencyRepository.Setup(x => x.GetById(currencyRUR.Id)).Returns(currencyRUR);
                _currencyRepository = tempCurrencyRepository.Object;
            }

            _storeMappingService = new Mock<IStoreMappingService>().Object;
            var cacheManager = new GrandNullCache();
            _currencySettings = new CurrencySettings();
            _currencySettings.PrimaryStoreCurrencyId = currencyUSD.Id;
            _currencySettings.PrimaryExchangeRateCurrencyId = currencyEUR.Id;

            var tempEventPublisher = new Mock<IEventPublisher>();
            {
                tempEventPublisher.Setup(x => x.Publish(It.IsAny<object>()));
                _eventPublisher = tempEventPublisher.Object;
            }
            
            _currencyService = new CurrencyService(
                cacheManager, _currencyRepository, _storeMappingService,
                _currencySettings, new PluginFinder(), _eventPublisher);
        }

        [TestMethod()]
        public void Can_load_exchangeRateProviders() {
            var providers = _currencyService.LoadAllExchangeRateProviders();
            Assert.IsNotNull(providers);
            Assert.AreEqual(1, providers.Count);
        }

        [TestMethod()]
        public void Can_load_exchangeRateProvider_by_systemKeyword() {
            var provider001 = _currencyService.LoadExchangeRateProviderBySystemName("CurrencyExchange.TestProvider");
            Assert.IsNotNull(provider001);
        }

        [TestMethod()]
        public void Can_load_active_exchangeRateProvider() {
            var provider = _currencyService.LoadActiveExchangeRateProvider();
            Assert.IsNotNull(provider);
        }

        [TestMethod()]
        public void Can_convert_currency_1() {
            //e.g. 
            //10.1 * 1.5 = 15.15
            Assert.AreEqual(15.15M, _currencyService.ConvertCurrency(10.1M, 1.5M));
            Assert.AreEqual(10.122M, _currencyService.ConvertCurrency(10.122M, 1));
            Assert.AreEqual(34.4148M, _currencyService.ConvertCurrency(10.122M, 3.4M));
            Assert.AreEqual(0, _currencyService.ConvertCurrency(10.1M, 0));
            Assert.AreEqual(0, _currencyService.ConvertCurrency(0, 5));
        }

        [TestMethod()]
        public void Can_convert_currency_2() {
            //e.g.
            //10euro * 34.5rubleRate = 345 rubles
            Assert.AreEqual(345M, _currencyService.ConvertCurrency(10M, currencyEUR, currencyRUR));
            Assert.AreEqual(10.1M, _currencyService.ConvertCurrency(10.1M, currencyEUR, currencyEUR));
            Assert.AreEqual(10.1M, _currencyService.ConvertCurrency(10.1M, currencyRUR, currencyRUR));
            Assert.AreEqual(345M, _currencyService.ConvertCurrency(12M, currencyUSD, currencyRUR));
            Assert.AreEqual(12M, _currencyService.ConvertCurrency(345M, currencyRUR, currencyUSD));
        }
    }
}