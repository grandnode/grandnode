using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Knowledgebase;
using Grand.Services.Customers;
using Grand.Services.Events;
using MongoDB.Driver;

namespace Grand.Services.Knowledgebase
{
    public class KnowledgebaseService : IKnowledgebaseService
    {
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CATEGORIES_PATTERN_KEY = "Knowledgebase.category.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string ARTICLES_PATTERN_KEY = "Knowledgebase.article.";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : category ID
        /// </remarks>
        private const string CATEGORY_BY_ID = "Knowledgebase.category.id-{0}";

        /// <summary>
        /// Key for caching
        /// {0} : customer roles
        /// {1} : store id
        /// </summary>
        private const string CATEGORIES = "Knowledgebase.category.all-{0}-{1}";

        /// <summary>
        /// Key for caching
        /// {0} : customer roles
        /// {1} : store id
        /// </summary>
        private const string ARTICLES = "Knowledgebase.article.all-{0}-{1}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : article ID
        /// {1} : customer roles
        /// {2} : store id
        /// </remarks>
        private const string ARTICLE_BY_ID = "Knowledgebase.article.id-{0}-{1}-{2}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : category ID
        /// {1} : customer roles
        /// {2} : store id
        /// </remarks>
        private const string ARTICLES_BY_CATEGORY_ID = "Knowledgebase.article.categoryid-{0}-{1}-{2}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : keyword
        /// {1} : customer roles
        /// {2} : store id
        /// </remarks>
        private const string ARTICLES_BY_KEYWORD = "Knowledgebase.article.keyword-{0}-{1}-{2}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : keyword
        /// {1} : customer roles
        /// {2} : store id
        /// </remarks>
        private const string CATEGORIES_BY_KEYWORD = "Knowledgebase.category.keyword-{0}-{1}-{2}";

        /// <summary>
        /// Key for caching
        /// {0} : customer roles
        /// {1} : store id
        /// </summary>
        private const string HOMEPAGE_ARTICLES = "Knowledgebase.article.homepage-{0}-{1}";

        private readonly IRepository<KnowledgebaseCategory> _knowledgebaseCategoryRepository;
        private readonly IRepository<KnowledgebaseArticle> _knowledgebaseArticleRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly CommonSettings _commonSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreContext _storeContext;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="knowledgebaseCategoryRepository"></param>
        /// <param name="knowledgebaseArticleRepository"></param>
        /// <param name="eventPublisher"></param>
        public KnowledgebaseService(IRepository<KnowledgebaseCategory> knowledgebaseCategoryRepository,
            IRepository<KnowledgebaseArticle> knowledgebaseArticleRepository, IEventPublisher eventPublisher, CommonSettings commonSettings,
            CatalogSettings catalogSettings, IWorkContext workContext, ICacheManager cacheManager, IStoreContext storeContext)
        {
            this._knowledgebaseCategoryRepository = knowledgebaseCategoryRepository;
            this._knowledgebaseArticleRepository = knowledgebaseArticleRepository;
            this._eventPublisher = eventPublisher;
            this._commonSettings = commonSettings;
            this._catalogSettings = catalogSettings;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
            this._storeContext = storeContext;
        }

        /// <summary>
        /// Deletes knowledgebase category
        /// </summary>
        /// <param name="id"></param>
        public virtual void DeleteKnowledgebaseCategory(KnowledgebaseCategory kc)
        {
            var children = _knowledgebaseCategoryRepository.Table.Where(x => x.ParentCategoryId == kc.Id).ToList();
            _knowledgebaseCategoryRepository.Delete(kc);
            foreach (var child in children)
            {
                child.ParentCategoryId = "";
                UpdateKnowledgebaseCategory(child);
            }

            _cacheManager.RemoveByPattern(ARTICLES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);

            _eventPublisher.EntityDeleted(kc);
        }

        /// <summary>
        /// Edits knowledgebase category
        /// </summary>
        /// <param name="kc"></param>
        public virtual void UpdateKnowledgebaseCategory(KnowledgebaseCategory kc)
        {
            kc.UpdatedOnUtc = DateTime.UtcNow;
            _knowledgebaseCategoryRepository.Update(kc);
            _cacheManager.RemoveByPattern(ARTICLES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _eventPublisher.EntityUpdated(kc);
        }

        /// <summary>
        /// Gets knowledgebase category
        /// </summary>
        /// <param name="id"></param>
        /// <returns>knowledgebase category</returns>
        public virtual KnowledgebaseCategory GetKnowledgebaseCategory(string id)
        {
            return _knowledgebaseCategoryRepository.Table.Where(x => x.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Gets knowledgebase category
        /// </summary>
        /// <param name="id"></param>
        /// <returns>knowledgebase category</returns>
        public virtual KnowledgebaseCategory GetPublicKnowledgebaseCategory(string id)
        {
            string key = string.Format(CATEGORY_BY_ID, id, _workContext.CurrentCustomer.GetCustomerRoleIds(),
                _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var builder = Builders<KnowledgebaseCategory>.Filter;
                var filter = FilterDefinition<KnowledgebaseCategory>.Empty;
                filter = filter & builder.Where(x => x.Published);
                filter = filter & builder.Where(x => x.Id == id);

                if (!_catalogSettings.IgnoreAcl)
                {
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                    filter = filter & (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));
                }

                if (!_catalogSettings.IgnoreStoreLimitations)
                {
                    //Store mapping
                    var currentStoreId = new List<string> { _storeContext.CurrentStore.Id };
                    filter = filter & (builder.AnyIn(x => x.Stores, currentStoreId) | builder.Where(x => !x.LimitedToStores));
                }

                var toReturn = _knowledgebaseCategoryRepository.Collection.Find(filter);
                return toReturn.FirstOrDefault();
            });
        }

        /// <summary>
        /// Inserts knowledgebase category
        /// </summary>
        /// <param name="kc"></param>
        public virtual void InsertKnowledgebaseCategory(KnowledgebaseCategory kc)
        {
            kc.CreatedOnUtc = DateTime.UtcNow;
            kc.UpdatedOnUtc = DateTime.UtcNow;
            _knowledgebaseCategoryRepository.Insert(kc);
            _cacheManager.RemoveByPattern(ARTICLES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _eventPublisher.EntityInserted(kc);
        }

        /// <summary>
        /// Gets knowledgebase categories
        /// </summary>
        /// <returns>List of knowledgebase categories</returns>
        public virtual List<KnowledgebaseCategory> GetKnowledgebaseCategories()
        {
            return _knowledgebaseCategoryRepository.Table.OrderBy(x => x.ParentCategoryId).ThenBy(x => x.DisplayOrder).ToList().SortCategoriesForTree();
        }

        /// <summary>
        /// Gets knowledgebase article
        /// </summary>
        /// <param name="id"></param>
        /// <returns>knowledgebase article</returns>
        public virtual KnowledgebaseArticle GetKnowledgebaseArticle(string id)
        {
            return _knowledgebaseArticleRepository.Table.Where(x => x.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Gets knowledgebase articles
        /// </summary>
        /// <returns>List of knowledgebase articles</returns>
        public virtual List<KnowledgebaseArticle> GetKnowledgebaseArticles()
        {
            return _knowledgebaseArticleRepository.Table.OrderBy(x => x.DisplayOrder).ToList();
        }

        /// <summary>
        /// Inserts knowledgebase article
        /// </summary>
        /// <param name="ka"></param>
        public virtual void InsertKnowledgebaseArticle(KnowledgebaseArticle ka)
        {
            ka.CreatedOnUtc = DateTime.UtcNow;
            ka.UpdatedOnUtc = DateTime.UtcNow;
            _knowledgebaseArticleRepository.Insert(ka);
            _cacheManager.RemoveByPattern(ARTICLES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _eventPublisher.EntityInserted(ka);
        }

        /// <summary>
        /// Edits knowledgebase article
        /// </summary>
        /// <param name="ka"></param>
        public virtual void UpdateKnowledgebaseArticle(KnowledgebaseArticle ka)
        {
            ka.UpdatedOnUtc = DateTime.UtcNow;
            _knowledgebaseArticleRepository.Update(ka);
            _cacheManager.RemoveByPattern(ARTICLES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _eventPublisher.EntityUpdated(ka);
        }

        /// <summary>
        /// Deletes knowledgebase article
        /// </summary>
        /// <param name="id"></param>
        public virtual void DeleteKnowledgebaseArticle(KnowledgebaseArticle ka)
        {
            _knowledgebaseArticleRepository.Delete(ka);
            _cacheManager.RemoveByPattern(ARTICLES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _eventPublisher.EntityDeleted(ka);
        }

        /// <summary>
        /// Gets knowledgebase articles by category id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IPagedList<KnowledgebaseArticle></returns>
        public virtual IPagedList<KnowledgebaseArticle> GetKnowledgebaseArticlesByCategoryId(string id, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var articles = _knowledgebaseArticleRepository.Table.Where(x => x.ParentCategoryId == id).ToList();
            return new PagedList<KnowledgebaseArticle>(articles, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets public(published etc) knowledgebase categories
        /// </summary>
        /// <returns>List of public knowledgebase categories</returns>
        public virtual List<KnowledgebaseCategory> GetPublicKnowledgebaseCategories()
        {
            var key = string.Format(CATEGORIES, string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var builder = Builders<KnowledgebaseCategory>.Filter;
                var filter = FilterDefinition<KnowledgebaseCategory>.Empty;
                filter = filter & builder.Where(x => x.Published);

                if (!_catalogSettings.IgnoreAcl)
                {
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                    filter = filter & (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));
                }

                if (!_catalogSettings.IgnoreStoreLimitations)
                {
                    //Store mapping
                    var currentStoreId = new List<string> { _storeContext.CurrentStore.Id };
                    filter = filter & (builder.AnyIn(x => x.Stores, currentStoreId) | builder.Where(x => !x.LimitedToStores));
                }

                var builderSort = Builders<KnowledgebaseCategory>.Sort.Ascending(x => x.DisplayOrder);
                var toReturn = _knowledgebaseCategoryRepository.Collection.Find(filter).Sort(builderSort);
                return toReturn.ToList();
            });
        }

        /// <summary>
        /// Gets public(published etc) knowledgebase articles
        /// </summary>
        /// <returns>List of public knowledgebase articles</returns>
        public virtual List<KnowledgebaseArticle> GetPublicKnowledgebaseArticles()
        {
            var key = string.Format(ARTICLES, string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var builder = Builders<KnowledgebaseArticle>.Filter;
                var filter = FilterDefinition<KnowledgebaseArticle>.Empty;
                filter = filter & builder.Where(x => x.Published);

                if (!_catalogSettings.IgnoreAcl)
                {
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                    filter = filter & (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));
                }

                if (!_catalogSettings.IgnoreStoreLimitations)
                {
                    //Store mapping
                    var currentStoreId = new List<string> { _storeContext.CurrentStore.Id };
                    filter = filter & (builder.AnyIn(x => x.Stores, currentStoreId) | builder.Where(x => !x.LimitedToStores));
                }

                var builderSort = Builders<KnowledgebaseArticle>.Sort.Ascending(x => x.DisplayOrder);
                var toReturn = _knowledgebaseArticleRepository.Collection.Find(filter).Sort(builderSort);
                return toReturn.ToList();
            });
        }

        /// <summary>
        /// Gets knowledgebase article if it is published etc
        /// </summary>
        /// <returns>knowledgebase article</returns>
        public virtual KnowledgebaseArticle GetPublicKnowledgebaseArticle(string id)
        {
            var key = string.Format(ARTICLE_BY_ID, id, string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var builder = Builders<KnowledgebaseArticle>.Filter;
                var filter = FilterDefinition<KnowledgebaseArticle>.Empty;
                filter = filter & builder.Where(x => x.Published);
                filter = filter & builder.Where(x => x.Id == id);

                if (!_catalogSettings.IgnoreAcl)
                {
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                    filter = filter & (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));
                }

                if (!_catalogSettings.IgnoreStoreLimitations)
                {
                    //Store mapping
                    var currentStoreId = new List<string> { _storeContext.CurrentStore.Id };
                    filter = filter & (builder.AnyIn(x => x.Stores, currentStoreId) | builder.Where(x => !x.LimitedToStores));
                }

                var toReturn = _knowledgebaseArticleRepository.Collection.Find(filter).FirstOrDefault();
                return toReturn;
            });
        }

        /// <summary>
        /// Gets public(published etc) knowledgebase articles for category id
        /// </summary>
        /// <returns>List of public knowledgebase articles</returns>
        public virtual List<KnowledgebaseArticle> GetPublicKnowledgebaseArticlesByCategory(string categoryId)
        {
            var key = string.Format(ARTICLES_BY_CATEGORY_ID, categoryId, string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var builder = Builders<KnowledgebaseArticle>.Filter;
                var filter = FilterDefinition<KnowledgebaseArticle>.Empty;
                filter = filter & builder.Where(x => x.Published);
                filter = filter & builder.Where(x => x.ParentCategoryId == categoryId);

                if (!_catalogSettings.IgnoreAcl)
                {
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                    filter = filter & (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));
                }

                if (!_catalogSettings.IgnoreStoreLimitations)
                {
                    //Store mapping
                    var currentStoreId = new List<string> { _storeContext.CurrentStore.Id };
                    filter = filter & (builder.AnyIn(x => x.Stores, currentStoreId) | builder.Where(x => !x.LimitedToStores));
                }

                var builderSort = Builders<KnowledgebaseArticle>.Sort.Ascending(x => x.DisplayOrder);
                var toReturn = _knowledgebaseArticleRepository.Collection.Find(filter).Sort(builderSort);
                return toReturn.ToList();
            });
        }

        /// <summary>
        /// Gets public(published etc) knowledgebase articles for keyword
        /// </summary>
        /// <returns>List of public knowledgebase articles</returns>
        public virtual List<KnowledgebaseArticle> GetPublicKnowledgebaseArticlesByKeyword(string keyword)
        {
            var key = string.Format(ARTICLES_BY_KEYWORD, keyword, string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var builder = Builders<KnowledgebaseArticle>.Filter;
                var filter = FilterDefinition<KnowledgebaseArticle>.Empty;
                filter = filter & builder.Where(x => x.Published);

                if (_commonSettings.UseFullTextSearch)
                {
                    keyword = "\"" + keyword + "\"";
                    keyword = keyword.Replace("+", "\" \"");
                    keyword = keyword.Replace(" ", "\" \"");
                    filter = filter & builder.Text(keyword);
                }
                else
                {
                    filter = filter & builder.Where(p => p.Locales.Any(x => x.LocaleValue != null && x.LocaleValue.ToLower().Contains(keyword.ToLower()))
                    || p.Name.ToLower().Contains(keyword.ToLower()) || p.Content.ToLower().Contains(keyword.ToLower()));
                }

                if (!_catalogSettings.IgnoreAcl)
                {
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                    filter = filter & (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));
                }

                if (!_catalogSettings.IgnoreStoreLimitations)
                {
                    //Store mapping
                    var currentStoreId = new List<string> { _storeContext.CurrentStore.Id };
                    filter = filter & (builder.AnyIn(x => x.Stores, currentStoreId) | builder.Where(x => !x.LimitedToStores));
                }

                var builderSort = Builders<KnowledgebaseArticle>.Sort.Ascending(x => x.DisplayOrder);
                var toReturn = _knowledgebaseArticleRepository.Collection.Find(filter).Sort(builderSort);
                return toReturn.ToList();
            });
        }

        /// <summary>
        /// Gets public(published etc) knowledgebase categories for keyword
        /// </summary>
        /// <returns>List of public knowledgebase categories</returns>
        public virtual List<KnowledgebaseCategory> GetPublicKnowledgebaseCategoriesByKeyword(string keyword)
        {
            var key = string.Format(CATEGORIES_BY_KEYWORD, keyword, string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var builder = Builders<KnowledgebaseCategory>.Filter;
                var filter = FilterDefinition<KnowledgebaseCategory>.Empty;
                filter = filter & builder.Where(x => x.Published);

                if (_commonSettings.UseFullTextSearch)
                {
                    keyword = "\"" + keyword + "\"";
                    keyword = keyword.Replace("+", "\" \"");
                    keyword = keyword.Replace(" ", "\" \"");
                    filter = filter & builder.Text(keyword);
                }
                else
                {
                    filter = filter & builder.Where(p => p.Locales.Any(x => x.LocaleValue != null && x.LocaleValue.ToLower().Contains(keyword.ToLower()))
                    || p.Name.ToLower().Contains(keyword.ToLower()) || p.Description.ToLower().Contains(keyword.ToLower()));
                }

                if (!_catalogSettings.IgnoreAcl)
                {
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                    filter = filter & (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));
                }

                if (!_catalogSettings.IgnoreStoreLimitations)
                {
                    //Store mapping
                    var currentStoreId = new List<string> { _storeContext.CurrentStore.Id };
                    filter = filter & (builder.AnyIn(x => x.Stores, currentStoreId) | builder.Where(x => !x.LimitedToStores));
                }

                var builderSort = Builders<KnowledgebaseCategory>.Sort.Ascending(x => x.DisplayOrder);
                var toReturn = _knowledgebaseCategoryRepository.Collection.Find(filter).Sort(builderSort);
                return toReturn.ToList();
            });
        }

        /// <summary>
        /// Gets homepage knowledgebase articles
        /// </summary>
        /// <returns>List of homepage knowledgebase articles</returns>
        public virtual List<KnowledgebaseArticle> GetHomepageKnowledgebaseArticles()
        {
            var key = string.Format(HOMEPAGE_ARTICLES, string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var builder = Builders<KnowledgebaseArticle>.Filter;
                var filter = FilterDefinition<KnowledgebaseArticle>.Empty;
                filter = filter & builder.Where(x => x.Published);
                filter = filter & builder.Where(x => x.ShowOnHomepage);

                if (!_catalogSettings.IgnoreAcl)
                {
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                    filter = filter & (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));
                }

                if (!_catalogSettings.IgnoreStoreLimitations)
                {
                    //Store mapping
                    var currentStoreId = new List<string> { _storeContext.CurrentStore.Id };
                    filter = filter & (builder.AnyIn(x => x.Stores, currentStoreId) | builder.Where(x => !x.LimitedToStores));
                }

                var builderSort = Builders<KnowledgebaseArticle>.Sort.Ascending(x => x.DisplayOrder);
                var toReturn = _knowledgebaseArticleRepository.Collection.Find(filter).Sort(builderSort);
                return toReturn.ToList();
            });
        }

        /// <summary>
        /// Gets knowledgebase articles by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>IPagedList<KnowledgebaseArticle></returns>
        public virtual IPagedList<KnowledgebaseArticle> GetKnowledgebaseArticlesByName(string name, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var builder = Builders<KnowledgebaseArticle>.Filter;
            var filter = FilterDefinition<KnowledgebaseArticle>.Empty;
            filter = filter & builder.Where(x => x.Published);

            if (!string.IsNullOrEmpty(name))
            {
                if (_commonSettings.UseFullTextSearch)
                {
                    name = "\"" + name + "\"";
                    name = name.Replace("+", "\" \"");
                    name = name.Replace(" ", "\" \"");
                    filter = filter & builder.Text(name);
                }
                else
                {
                    filter = filter & builder.Where(p => p.Locales.Any(x => x.LocaleKey == "Name" && x.LocaleValue != null && x.LocaleValue.ToLower().
                        Contains(name.ToLower()))
                    || p.Name.ToLower().Contains(name.ToLower()));
                }
            }

            var builderSort = Builders<KnowledgebaseArticle>.Sort.Ascending(x => x.DisplayOrder);
            var toReturn = _knowledgebaseArticleRepository.Collection.Find(filter).Sort(builderSort);

            return new PagedList<KnowledgebaseArticle>(toReturn.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets related knowledgebase articles
        /// </summary>
        /// <param name="name"></param>
        /// <returns>IPagedList<KnowledgebaseArticle></returns>
        public virtual IPagedList<KnowledgebaseArticle> GetRelatedKnowledgebaseArticles(string articleId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var article = GetKnowledgebaseArticle(articleId);
            List<KnowledgebaseArticle> toReturn = new List<KnowledgebaseArticle>();

            foreach (var id in article.RelatedArticles)
            {
                var relatedArticle = GetKnowledgebaseArticle(id);
                if (relatedArticle != null)
                    toReturn.Add(relatedArticle);
            }

            return new PagedList<KnowledgebaseArticle>(toReturn.ToList(), pageIndex, pageSize);
        }
    }
}
