using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grand.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Localization;
using Grand.Services.Configuration;
using Grand.Services.Events;
using Grand.Services.Stores;
using Moq;
using MongoDB.Driver;
using Grand.Services.Tests;

namespace Grand.Services.Localization.Tests {
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