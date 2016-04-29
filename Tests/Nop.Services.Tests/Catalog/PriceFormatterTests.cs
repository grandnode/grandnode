using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Tax;
using Nop.Core.Plugins;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Stores;
using System;
using Moq;
using MongoDB.Driver;
using System.Threading;

namespace Nop.Services.Catalog.Tests {
    [TestClass()]
    public class PriceFormatterTests {
        private IRepository<Currency> _currencyRepo;
        private IStoreMappingService _storeMappingService;
        private ICurrencyService _currencyService;
        private CurrencySettings _currencySettings;
        private IWorkContext _workContext;
        private ILocalizationService _localizationService;
        private TaxSettings _taxSettings;
        private IPriceFormatter _priceFormatter;

        [TestInitialize()]
        public void TestInitialize() {
            var cacheManager = new NopNullCache();
            _workContext = null;
            _currencySettings = new CurrencySettings();
            var currency01 = new Currency {
                Id = "1",
                Name = "Euro",
                CurrencyCode = "EUR",
                DisplayLocale = "",
                CustomFormatting = "€0.00",
                DisplayOrder = 1,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            var currency02 = new Currency {
                Id = "1",
                Name = "US Dollar",
                CurrencyCode = "USD",
                DisplayLocale = "en-US",
                CustomFormatting = "",
                DisplayOrder = 2,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            
            var tempCurrencyRepo = new Mock<IRepository<Currency>>();
            {
                var tempIMongoCollection = new Mock<IMongoCollection<Currency>>().Object;
                tempIMongoCollection.InsertOne(currency01);
                tempIMongoCollection.InsertOne(currency02);
                tempCurrencyRepo.Setup(x => x.Table).Returns(tempIMongoCollection.AsQueryable());
            }

            _storeMappingService = new Mock<IStoreMappingService>().Object;

            _currencyRepo = new Mock<IRepository<Currency>>().Object;

            var pluginFinder = new PluginFinder();
            _currencyService = new CurrencyService(
                cacheManager,
                _currencyRepo,
                _storeMappingService,
                _currencySettings,
                pluginFinder,
                null);

            _taxSettings = new TaxSettings();

            var tempLocalizationService = new Mock<ILocalizationService>();
            {
                tempLocalizationService.Setup(x => x.GetResource("Products.InclTaxSuffix", "1", false, "", false)).Returns("{0} incl tax");
                tempLocalizationService.Setup(x => x.GetResource("Products.ExclTaxSuffix", "1", false, "", false)).Returns("{0} excl tax");
                _localizationService = tempLocalizationService.Object;
            }

            _priceFormatter = new PriceFormatter(_workContext, _currencyService, _localizationService, _taxSettings, _currencySettings);
        }

        [TestMethod()]
        public void Can_formatPrice_with_custom_currencyFormatting() {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            var currency0111 = new Currency {
                Id = "1",
                Name = "Euro",
                CurrencyCode = "EUR",
                DisplayLocale = "",
                CustomFormatting = "€0.00",
                DisplayOrder = 1,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            var language0111 = new Language {
                Id = "1",
                Name = "English",
                LanguageCulture = "en-US"
            };

            Assert.AreEqual("€412.20", _priceFormatter.FormatPrice(412.2M, false, currency0111, language0111, false, false));
        }

        [TestMethod()]
        public void Can_formatPrice_with_distinct_currencyDisplayLocale() {

            var usd_currency = new Currency {
                Id = "1",
                Name = "US Dollar",
                CurrencyCode = "USD",
                DisplayLocale = "en-US",
            };
            var gbp_currency = new Currency {
                Id = "2",
                Name = "great british pound",
                CurrencyCode = "GBP",
                DisplayLocale = "en-GB",
            };
            var euro_currency = new Currency {
                Id = "3",
                Name = "Euro",
                CurrencyCode = "EUR",
                DisplayLocale = "en_150",
            };
            var language = new Language {
                Id = "1",
                Name = "English",
                LanguageCulture = "en-US"
            };
            Assert.AreEqual("$1,234.50", _priceFormatter.FormatPrice(1234.5M, false, usd_currency, language, false, false));
            Assert.AreEqual("£1,234.50", _priceFormatter.FormatPrice(1234.5M, false, gbp_currency, language, false, false));
        }

        [TestMethod()]
        public void Can_formatPrice_with_showTax() {
            //$18,888.10                    priceIncludestax=false || showTax=false
            //$18,888.10 incl tax           priceIncludestax=true || showTax=true
            //$18,888.10 excl tax           priceIncludestax=false || showTax=true

            var currency = new Currency {
                Id = "1",
                Name = "US Dollar",
                CurrencyCode = "USD",
                DisplayLocale = "en-US",
            };
            var language = new Language {
                Id = "1",
                Name = "English",
                LanguageCulture = "en-US"
            };

            //
            Assert.AreEqual("$18,888.10", _priceFormatter.FormatPrice(18888.1M, false, currency, language, false, false));
            //
            Assert.AreEqual("$18,888.10 incl tax", _priceFormatter.FormatPrice(18888.1M, false, currency, language, true, true));
            //
            Assert.AreEqual("$18,888.10 excl tax", _priceFormatter.FormatPrice(18888.1M, false, currency, language, false, true));
        }

        [TestMethod()]
        public void Can_formatPrice_with_showCurrencyCode() {
            //DisplayCurrecyLabel = true            $123.00 (USD)
            //DisplayCurrecyLabel = false           $123.00

            var currency = new Currency {
                Id = "1",
                Name = "US Dollar",
                CurrencyCode = "USD",
                DisplayLocale = "en-US",
            };
            var language = new Language {
                Id = "1",
                Name = "English",
                LanguageCulture = "en-US"
            };

            _currencySettings.DisplayCurrencyLabel = true;
            Assert.AreEqual("$18,888.10 (USD)", _priceFormatter.FormatPrice(18888.1M, true, currency, language, false, false));

            _currencySettings.DisplayCurrencyLabel = false;
            Assert.AreEqual("$18,888.10", _priceFormatter.FormatPrice(18888.1M, true, currency, language, false, false));
        }
    }
}