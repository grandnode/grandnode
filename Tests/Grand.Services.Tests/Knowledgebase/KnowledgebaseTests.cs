using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using Grand.Services.Events;
using Grand.Services.Knowledgebase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Tests.Knowledgebase
{
    [TestClass()]
    public class KnowledgebaseTests
    {
        private IRepository<KnowledgebaseArticle> _articleRepository;
        private IRepository<KnowledgebaseArticleComment> _articleCommentRepository;
        private IRepository<KnowledgebaseCategory> _categoryRepository;
        private IKnowledgebaseService _knowledgebaseService;
        private IEventPublisher _eventPublisher;
        private IWorkContext _workContext;
        private IStoreContext _storeContext;
        private ICacheManager _cacheManager;
        private CommonSettings _commonSettings;
        private CatalogSettings _catalogSettings;

        [TestInitialize()]
        public void TestInitialize()
        {
            _articleRepository = new MongoDBRepositoryTest<KnowledgebaseArticle>();
            _categoryRepository = new MongoDBRepositoryTest<KnowledgebaseCategory>();
            _articleCommentRepository = new MongoDBRepositoryTest<KnowledgebaseArticleComment>();

            var eventPublisher = new Mock<IEventPublisher>();
            eventPublisher.Setup(x => x.Publish(new object()));
            _eventPublisher = eventPublisher.Object;

            var customer = new Customer() { Username = "username" };
            var tempWorkContext = new Mock<IWorkContext>();
            {
                _workContext = tempWorkContext.Object;
                tempWorkContext.Setup(x => x.CurrentCustomer).Returns(customer);
            }

            var store = new Store();
            var tempStoreContext = new Mock<IStoreContext>();
            {
                _storeContext = tempStoreContext.Object;
                tempStoreContext.Setup(x => x.CurrentStore).Returns(store);
            }

            var catalogSettings = new Mock<CatalogSettings>();
            _catalogSettings = catalogSettings.Object;

            var commonSettings = new Mock<CommonSettings>();
            _commonSettings = commonSettings.Object;

            var cacheManager = new Mock<ICacheManager>();
            _cacheManager = cacheManager.Object;

            _knowledgebaseService = new KnowledgebaseService(_categoryRepository, _articleRepository, _eventPublisher, _commonSettings, _catalogSettings,
                _workContext, _cacheManager, _storeContext, _articleCommentRepository);
        }

        [TestMethod()]
        public void CanInsertKnowledgebaseArticle()
        {
            DateTime date = DateTime.UtcNow;

            _knowledgebaseService.InsertKnowledgebaseArticle(new KnowledgebaseArticle
            {
                Content = "Content",
                CustomerRoles = new List<string> { "Role" },
                DisplayOrder = 1,
                LimitedToStores = true,
                Locales = new List<LocalizedProperty> { new LocalizedProperty { LanguageId = "LanguageId", LocaleKey = "LocaleKey", LocaleValue = "LocaleValue" } },
                MetaDescription = "MetaDescription",
                MetaKeywords = "MetaKeywords",
                MetaTitle = "MetaTitle",
                Name = "CanInsertKnowledgebaseArticle",
                ParentCategoryId = "ParentCategoryId",
                Published = true,
                RelatedArticles = new List<string> { "RelatedArticle" },
                SeName = "SeName",
                ShowOnHomepage = true,
                Stores = new List<string> { "Store" },
                SubjectToAcl = true
            });

            var inserted = _articleRepository.Table.Where(x => x.Name == "CanInsertKnowledgebaseArticle").FirstOrDefault();

            Assert.AreNotEqual(null, inserted);
            Assert.AreEqual("Content", inserted.Content);
            Assert.AreEqual(date.Date, inserted.CreatedOnUtc.Date);
            Assert.AreEqual(date.Date, inserted.UpdatedOnUtc.Date);
            Assert.AreEqual(1, inserted.CustomerRoles.Count);
            Assert.AreEqual("Role", inserted.CustomerRoles.First());
            Assert.AreEqual(1, inserted.DisplayOrder);
            Assert.AreEqual(true, inserted.LimitedToStores);
            Assert.AreEqual(1, inserted.Locales.Count);
            Assert.AreEqual("LanguageId", inserted.Locales.First().LanguageId);
            Assert.AreEqual("LocaleKey", inserted.Locales.First().LocaleKey);
            Assert.AreEqual("LocaleValue", inserted.Locales.First().LocaleValue);
            Assert.AreEqual("MetaDescription", inserted.MetaDescription);
            Assert.AreEqual("MetaKeywords", inserted.MetaKeywords);
            Assert.AreEqual("MetaTitle", inserted.MetaTitle);
            Assert.AreEqual("ParentCategoryId", inserted.ParentCategoryId);
            Assert.AreEqual(true, inserted.Published);
            Assert.AreEqual(1, inserted.RelatedArticles.Count);
            Assert.AreEqual("RelatedArticle", inserted.RelatedArticles.First());
            Assert.AreEqual("SeName", inserted.SeName);
            Assert.AreEqual(true, inserted.ShowOnHomepage);
            Assert.AreEqual(1, inserted.Stores.Count);
            Assert.AreEqual("Store", inserted.Stores.First());
            Assert.AreEqual(true, inserted.SubjectToAcl);
        }

        [TestMethod()]
        public void CanInsertKnowledgebaseCategory()
        {
            DateTime date = DateTime.UtcNow;

            _knowledgebaseService.InsertKnowledgebaseCategory(new KnowledgebaseCategory
            {
                CustomerRoles = new List<string> { "Role" },
                DisplayOrder = 1,
                LimitedToStores = true,
                Locales = new List<LocalizedProperty> { new LocalizedProperty { LanguageId = "LanguageId", LocaleKey = "LocaleKey", LocaleValue = "LocaleValue" } },
                MetaDescription = "MetaDescription",
                MetaKeywords = "MetaKeywords",
                MetaTitle = "MetaTitle",
                Name = "CanInsertKnowledgebaseCategory",
                ParentCategoryId = "ParentCategoryId",
                Published = true,
                SeName = "SeName",
                Stores = new List<string> { "Store" },
                SubjectToAcl = true,
                Description = "Description"
            });

            var inserted = _categoryRepository.Table.Where(x => x.Name == "CanInsertKnowledgebaseCategory").FirstOrDefault();

            Assert.AreNotEqual(null, inserted);
            Assert.AreEqual(date.Date, inserted.CreatedOnUtc.Date);
            Assert.AreEqual(date.Date, inserted.UpdatedOnUtc.Date);
            Assert.AreEqual(1, inserted.CustomerRoles.Count);
            Assert.AreEqual("Role", inserted.CustomerRoles.First());
            Assert.AreEqual(1, inserted.DisplayOrder);
            Assert.AreEqual(true, inserted.LimitedToStores);
            Assert.AreEqual(1, inserted.Locales.Count);
            Assert.AreEqual("LanguageId", inserted.Locales.First().LanguageId);
            Assert.AreEqual("LocaleKey", inserted.Locales.First().LocaleKey);
            Assert.AreEqual("LocaleValue", inserted.Locales.First().LocaleValue);
            Assert.AreEqual("MetaDescription", inserted.MetaDescription);
            Assert.AreEqual("MetaKeywords", inserted.MetaKeywords);
            Assert.AreEqual("MetaTitle", inserted.MetaTitle);
            Assert.AreEqual("ParentCategoryId", inserted.ParentCategoryId);
            Assert.AreEqual(true, inserted.Published);
            Assert.AreEqual("SeName", inserted.SeName);
            Assert.AreEqual(1, inserted.Stores.Count);
            Assert.AreEqual("Store", inserted.Stores.First());
            Assert.AreEqual(true, inserted.SubjectToAcl);
            Assert.AreEqual("Description", inserted.Description);
        }

        private void ClearArticleRepository()
        {
            foreach (var item in _articleRepository.Table)
            {
                _articleRepository.Delete(item);
            }
        }

        private void ClearCategoryRepository()
        {
            foreach (var item in _categoryRepository.Table)
            {
                _categoryRepository.Delete(item);
            }
        }

        [TestMethod()]
        public void CanDeleteKnowledgebaseArticle()
        {
            ClearArticleRepository();

            var toDelete = new KnowledgebaseArticle();
            _knowledgebaseService.InsertKnowledgebaseArticle(toDelete);

            Assert.AreEqual(1, _articleRepository.Table.Count());
            _knowledgebaseService.DeleteKnowledgebaseArticle(toDelete);
            Assert.AreEqual(0, _articleRepository.Table.Count());
        }

        [TestMethod()]
        public void CanDeleteKnowledgebaseCategory()
        {
            ClearCategoryRepository();

            var toDelete = new KnowledgebaseCategory();
            _knowledgebaseService.InsertKnowledgebaseCategory(toDelete);

            Assert.AreEqual(1, _categoryRepository.Table.Count());
            _knowledgebaseService.DeleteKnowledgebaseCategory(toDelete);
            Assert.AreEqual(0, _categoryRepository.Table.Count());
        }

        [TestMethod()]
        public void CanGetKnowledgebaseArticle()
        {
            var article = new KnowledgebaseArticle() { Name = "CanGetKnowledgebaseArticle" };
            _knowledgebaseService.InsertKnowledgebaseArticle(article);

            var actual = _articleRepository.Table.Where(x => x.Name == "CanGetKnowledgebaseArticle").First();
            var found = _knowledgebaseService.GetKnowledgebaseArticle(actual.Id);

            Assert.AreEqual(actual.Name, found.Name);
        }

        [TestMethod()]
        public void CanGetKnowledgebaseCategory()
        {
            var category = new KnowledgebaseCategory() { Name = "CanGetKnowledgebaseCategory" };
            _knowledgebaseService.InsertKnowledgebaseCategory(category);

            var actual = _categoryRepository.Table.Where(x => x.Name == "CanGetKnowledgebaseCategory").First();
            var found = _knowledgebaseService.GetKnowledgebaseCategory(actual.Id);

            Assert.AreEqual(actual.Name, found.Name);
        }

        [TestMethod()]
        public void CanUpdateKnowledgebaseArticle()
        {
            ClearArticleRepository();

            var article = new KnowledgebaseArticle() { Name = "CanUpdateKnowledgebaseArticle" };
            _knowledgebaseService.InsertKnowledgebaseArticle(article);

            article.Name = "CanUpdateKnowledgebaseArticle1";
            _knowledgebaseService.UpdateKnowledgebaseArticle(article);

            var found = _articleRepository.Table.Where(x => x.Name == "CanUpdateKnowledgebaseArticle1");

            Assert.AreEqual(1, found.Count());
        }

        [TestMethod()]
        public void CanUpdateKnowledgebaseCategory()
        {
            ClearCategoryRepository();

            var category = new KnowledgebaseCategory() { Name = "CanUpdateKnowledgebaseCategory" };
            _knowledgebaseService.InsertKnowledgebaseCategory(category);

            category.Name = "CanUpdateKnowledgebaseCategory1";
            _knowledgebaseService.UpdateKnowledgebaseCategory(category);

            var found = _categoryRepository.Table.Where(x => x.Name == "CanUpdateKnowledgebaseCategory1");

            Assert.AreEqual(1, found.Count());
        }

        [TestMethod()]
        public void CanGetHomepageKnowledgebaseArticles()
        {
            ClearArticleRepository();

            var article1 = new KnowledgebaseArticle { ShowOnHomepage = true, Published = true, Name = "homepage" };
            var article2 = new KnowledgebaseArticle { ShowOnHomepage = false, Published = true, Name = "not homepage" };

            _knowledgebaseService.InsertKnowledgebaseArticle(article1);
            _knowledgebaseService.InsertKnowledgebaseArticle(article2);

            var homepage = _knowledgebaseService.GetHomepageKnowledgebaseArticles();

            Assert.AreEqual(1, homepage.Count);
            Assert.AreEqual("homepage", homepage.First().Name);
        }

        [TestMethod()]
        public void CanGetKnowledgebaseArticles()
        {
            ClearArticleRepository();

            var article1 = new KnowledgebaseArticle();
            var article2 = new KnowledgebaseArticle();
            var article3 = new KnowledgebaseArticle();

            _knowledgebaseService.InsertKnowledgebaseArticle(article1);
            _knowledgebaseService.InsertKnowledgebaseArticle(article2);
            _knowledgebaseService.InsertKnowledgebaseArticle(article3);

            var all = _knowledgebaseService.GetKnowledgebaseArticles();

            Assert.AreEqual(3, all.Count());
        }

        [TestMethod()]
        public void CanGetKnowledgebaseCategories()
        {
            ClearCategoryRepository();

            var category1 = new KnowledgebaseCategory();
            var category2 = new KnowledgebaseCategory();
            var category3 = new KnowledgebaseCategory();

            _knowledgebaseService.InsertKnowledgebaseCategory(category1);
            _knowledgebaseService.InsertKnowledgebaseCategory(category2);
            _knowledgebaseService.InsertKnowledgebaseCategory(category3);

            var all = _knowledgebaseService.GetKnowledgebaseCategories();

            Assert.AreEqual(3, all.Count());
        }

        [TestMethod()]
        public void GetKnowledgebaseArticlesByCategoryId()
        {
            ClearArticleRepository();

            var article1 = new KnowledgebaseArticle { ParentCategoryId = "GetKnowledgebaseArticlesByCategoryId1" };
            var article2 = new KnowledgebaseArticle { ParentCategoryId = "GetKnowledgebaseArticlesByCategoryId1" };
            var article3 = new KnowledgebaseArticle { ParentCategoryId = "GetKnowledgebaseArticlesByCategoryId2" };

            _knowledgebaseService.InsertKnowledgebaseArticle(article1);
            _knowledgebaseService.InsertKnowledgebaseArticle(article2);
            _knowledgebaseService.InsertKnowledgebaseArticle(article3);

            var found = _knowledgebaseService.GetKnowledgebaseArticlesByCategoryId("GetKnowledgebaseArticlesByCategoryId1");

            Assert.AreEqual(2, found.Count);
        }

        [TestMethod()]
        public void GetKnowledgebaseArticlesByName()
        {
            ClearArticleRepository();

            var article1 = new KnowledgebaseArticle { Name = "GetKnowledgebaseArticlesByName1", Published = true };

            _knowledgebaseService.InsertKnowledgebaseArticle(article1);

            var found = _knowledgebaseService.GetKnowledgebaseArticlesByName("GetKnowledgebaseArticlesByName1");

            Assert.AreEqual(1, found.Count);
            Assert.AreEqual("GetKnowledgebaseArticlesByName1", found.First().Name);
        }

        [TestMethod()]
        public void CanGetPublicKnowledgebaseArticle()
        {
            ClearArticleRepository();

            var article1 = new KnowledgebaseArticle { Name = "CanGetPublicKnowledgebaseArticlePublic", Published = true };
            var article2 = new KnowledgebaseArticle { Name = "CanGetPublicKnowledgebaseArticleNotPublic", Published = false };

            _knowledgebaseService.InsertKnowledgebaseArticle(article1);
            _knowledgebaseService.InsertKnowledgebaseArticle(article2);

            var found1 = _knowledgebaseService.GetPublicKnowledgebaseArticle(article1.Id);
            var found2 = _knowledgebaseService.GetPublicKnowledgebaseArticle(article2.Id);

            Assert.AreEqual("CanGetPublicKnowledgebaseArticlePublic", found1.Name);
            Assert.AreEqual(null, found2);
        }

        [TestMethod()]
        public void CanGetPublicKnowledgebaseArticles()
        {
            ClearArticleRepository();

            var article1 = new KnowledgebaseArticle { Name = "CanGetPublicKnowledgebaseArticlesPublic1", Published = true };
            var article2 = new KnowledgebaseArticle { Name = "CanGetPublicKnowledgebaseArticlesPublic2", Published = true };
            var article3 = new KnowledgebaseArticle { Name = "CanGetPublicKnowledgebaseArticlesNotPublic", Published = false };

            _knowledgebaseService.InsertKnowledgebaseArticle(article1);
            _knowledgebaseService.InsertKnowledgebaseArticle(article2);

            var found = _knowledgebaseService.GetPublicKnowledgebaseArticles();

            Assert.AreEqual(2, found.Count);
            Assert.AreEqual(true, found.Any(x => x.Name == "CanGetPublicKnowledgebaseArticlesPublic1"));
            Assert.AreEqual(true, found.Any(x => x.Name == "CanGetPublicKnowledgebaseArticlesPublic2"));
            Assert.AreEqual(false, found.Any(x => x.Name == "CanGetPublicKnowledgebaseArticlesNotPublic"));
        }

        [TestMethod()]
        public void CanGetPublicKnowledgebaseCategory()
        {
            ClearCategoryRepository();

            var category1 = new KnowledgebaseCategory { Name = "CanGetPublicKnowledgebaseCategoryPublic", Published = true };
            var category2 = new KnowledgebaseCategory { Name = "CanGetPublicKnowledgebaseCategoryNotPublic", Published = false };

            _knowledgebaseService.InsertKnowledgebaseCategory(category1);
            _knowledgebaseService.InsertKnowledgebaseCategory(category2);

            var found1 = _knowledgebaseService.GetPublicKnowledgebaseCategory(category1.Id);
            var found2 = _knowledgebaseService.GetPublicKnowledgebaseCategory(category2.Id);

            Assert.AreEqual("CanGetPublicKnowledgebaseCategoryPublic", found1.Name);
            Assert.AreEqual(null, found2);
        }

        [TestMethod()]
        public void CanGetPublicKnowledgebaseCategories()
        {
            ClearCategoryRepository();

            var category1 = new KnowledgebaseCategory { Name = "CanGetPublicKnowledgebaseCategoriesPublic1", Published = true };
            var category2 = new KnowledgebaseCategory { Name = "CanGetPublicKnowledgebaseCategoriesPublic2", Published = true };
            var category3 = new KnowledgebaseCategory { Name = "CanGetPublicKnowledgebaseCategoriesNotPublic", Published = false };

            _knowledgebaseService.InsertKnowledgebaseCategory(category1);
            _knowledgebaseService.InsertKnowledgebaseCategory(category2);

            var found = _knowledgebaseService.GetPublicKnowledgebaseCategories();

            Assert.AreEqual(2, found.Count);
            Assert.AreEqual(true, found.Any(x => x.Name == "CanGetPublicKnowledgebaseCategoriesPublic1"));
            Assert.AreEqual(true, found.Any(x => x.Name == "CanGetPublicKnowledgebaseCategoriesPublic2"));
            Assert.AreEqual(false, found.Any(x => x.Name == "CanGetPublicKnowledgebaseCategoriesNotPublic"));
        }

        [TestMethod()]
        public void CanGetPublicKnowledgebaseArticlesByCategory()
        {
            ClearArticleRepository();

            var article1 = new KnowledgebaseArticle
            {
                Name = "CanGetPublicKnowledgebaseArticlesByCategory1",
                Published = true,
                ParentCategoryId = "CanGetPublicKnowledgebaseArticlesByCategory1"
            };

            var article2 = new KnowledgebaseArticle
            {
                Name = "CanGetPublicKnowledgebaseArticlesByCategory2",
                Published = true,
                ParentCategoryId = "CanGetPublicKnowledgebaseArticlesByCategory1"
            };

            var article3 = new KnowledgebaseArticle
            {
                Name = "CanGetPublicKnowledgebaseArticlesByCategory3",
                Published = true,
                ParentCategoryId = "CanGetPublicKnowledgebaseArticlesByCategory3"
            };

            _knowledgebaseService.InsertKnowledgebaseArticle(article1);
            _knowledgebaseService.InsertKnowledgebaseArticle(article2);
            _knowledgebaseService.InsertKnowledgebaseArticle(article3);

            var found = _knowledgebaseService.GetPublicKnowledgebaseArticlesByCategory("CanGetPublicKnowledgebaseArticlesByCategory1");

            Assert.AreEqual(2, found.Count);
            Assert.AreEqual(true, found.Any(x => x.Name == "CanGetPublicKnowledgebaseArticlesByCategory1"));
            Assert.AreEqual(true, found.Any(x => x.Name == "CanGetPublicKnowledgebaseArticlesByCategory2"));
            Assert.AreEqual(false, found.Any(x => x.Name == "CanGetPublicKnowledgebaseArticlesByCategory3"));
        }

        [TestMethod()]
        public void CanGetPublicKnowledgebaseArticlesByKeyword()
        {
            ClearArticleRepository();

            var article1 = new KnowledgebaseArticle
            {
                Name = "CanGetPublicKnowledgebaseArticlesByKeyword1",
                Published = true,
            };

            var article2 = new KnowledgebaseArticle
            {
                Name = "test",
                Content = "CanGetPublicKnowledgebaseArticlesByKeyword2",
                Published = true,
            };

            var article3 = new KnowledgebaseArticle
            {
                Name = "Tomato",
                Content = "Tomato",
                Published = true,
            };

            _knowledgebaseService.InsertKnowledgebaseArticle(article1);
            _knowledgebaseService.InsertKnowledgebaseArticle(article2);
            _knowledgebaseService.InsertKnowledgebaseArticle(article3);

            var found = _knowledgebaseService.GetPublicKnowledgebaseArticlesByKeyword("keyword");

            Assert.AreEqual(2, found.Count);
            Assert.AreEqual(true, found.Any(x => x.Name == "CanGetPublicKnowledgebaseArticlesByKeyword1"));
            Assert.AreEqual(true, found.Any(x => x.Name == "test"));
            Assert.AreEqual(false, found.Any(x => x.Name == "Tomato"));
        }

        [TestMethod()]
        public void CanGetRelatedKnowledgebaseArticles()
        {
            ClearArticleRepository();

            var article = new KnowledgebaseArticle
            {
                Name = "CanGetRelatedKnowledgebaseArticlesMain",
                Published = true
            };

            var related1 = new KnowledgebaseArticle
            {
                Name = "CanGetRelatedKnowledgebaseArticlesRelated1",
                Published = true
            };

            var related2 = new KnowledgebaseArticle
            {
                Name = "CanGetRelatedKnowledgebaseArticlesRelated2",
                Published = true
            };

            _knowledgebaseService.InsertKnowledgebaseArticle(related1);
            _knowledgebaseService.InsertKnowledgebaseArticle(related2);

            article.RelatedArticles.Add(related1.Id);
            article.RelatedArticles.Add(related2.Id);

            _knowledgebaseService.InsertKnowledgebaseArticle(article);

            var found = _knowledgebaseService.GetRelatedKnowledgebaseArticles(article.Id);

            Assert.AreEqual(2, found.Count);
            Assert.AreEqual(true, found.Any(x=>x.Name == "CanGetRelatedKnowledgebaseArticlesRelated1"));
            Assert.AreEqual(true, found.Any(x=>x.Name == "CanGetRelatedKnowledgebaseArticlesRelated2"));
        }
    }
}
