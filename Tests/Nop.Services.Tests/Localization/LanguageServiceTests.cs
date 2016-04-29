using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nop.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Localization;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Stores;
using Moq;
using MongoDB.Driver;
using Nop.Services.Tests;

namespace Nop.Services.Localization.Tests {
    [TestClass()]
    public class LanguageServiceTests {
        private IRepository<Language> _languageRepo;
        private IStoreMappingService _storeMappingService;
        private ILanguageService _languageService;
        private ISettingService _settingService;
        private IEventPublisher _eventPublisher;
        private LocalizationSettings _localizationSettings;

        [TestInitialize()]
        public void TestInitialize() {
            

            var lang1 = new Language {
                Name = "English",
                LanguageCulture = "en-Us",
                FlagImageFileName = "us.png",
                Published = true,
                DisplayOrder = 1
            };
            var lang2 = new Language {
                Name = "Russian",
                LanguageCulture = "ru-Ru",
                FlagImageFileName = "ru.png",
                Published = true,
                DisplayOrder = 2
            };

            _languageRepo = new MongoDBRepositoryTest<Language>();
            _languageRepo.Insert(lang1);
            _languageRepo.Insert(lang2);

            _storeMappingService = new Mock<IStoreMappingService>().Object;
            _settingService = new Mock<ISettingService>().Object;
            var tempEventPublisher = new Mock<IEventPublisher>();
            {
                tempEventPublisher.Setup(x => x.Publish(It.IsAny<object>()));
                _eventPublisher = tempEventPublisher.Object;
            }
            _localizationSettings = new LocalizationSettings();

            _languageService = new LanguageService(new NopNullCache(), _languageRepo, _storeMappingService,
                _settingService, _localizationSettings, _eventPublisher);
        }

        [TestMethod()]
        public void Can_get_all_languages() {
            var languages = _languageService.GetAllLanguages();
            Assert.IsNotNull(languages);
            Assert.IsTrue(languages.Count > 0);
        }
    }
}