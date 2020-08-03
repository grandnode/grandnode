using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Knowledgebase;
using Grand.Services.Customers;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// {1} : customer roles
        /// {2} : store id
        /// </remarks>
        private const string CATEGORY_BY_ID = "Knowledgebase.category.id-{0}-{1}-{2}";

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
        private readonly IMediator _mediator;
        private readonly CommonSettings _commonSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IRepository<KnowledgebaseArticleComment> _articleCommentRepository;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="knowledgebaseCategoryRepository"></param>
        /// <param name="knowledgebaseArticleRepository"></param>
        /// <param name="mediator">Mediator</param>
        public KnowledgebaseService(IRepository<KnowledgebaseCategory> knowledgebaseCategoryRepository,
            IRepository<KnowledgebaseArticle> knowledgebaseArticleRepository, IMediator mediator, CommonSettings commonSettings,
            CatalogSettings catalogSettings, IWorkContext workContext, ICacheManager cacheManager, IStoreContext storeContext,
            IRepository<KnowledgebaseArticleComment> articleCommentRepository)
        {
            _knowledgebaseCategoryRepository = knowledgebaseCategoryRepository;
            _knowledgebaseArticleRepository = knowledgebaseArticleRepository;
            _mediator = mediator;
            _commonSettings = commonSettings;
            _catalogSettings = catalogSettings;
            _workContext = workContext;
            _cacheManager = cacheManager;
            _storeContext = storeContext;
            _articleCommentRepository = articleCommentRepository;
        }

        /// <summary>
        /// Deletes knowledgebase category
        /// </summary>
        /// <param name="id"></param>
        public virtual async Task DeleteKnowledgebaseCategory(KnowledgebaseCategory kc)
        {
            var children = await _knowledgebaseCategoryRepository.Table.Where(x => x.ParentCategoryId == kc.Id).ToListAsync();
            await _knowledgebaseCategoryRepository.DeleteAsync(kc);
            foreach (var child in children)
            {
                child.ParentCategoryId = "";
                await UpdateKnowledgebaseCategory(child);
            }

            await _cacheManager.RemoveByPrefix(ARTICLES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(CATEGORIES_PATTERN_KEY);

            await _mediator.EntityDeleted(kc);
        }

        /// <summary>
        /// Edits knowledgebase category
        /// </summary>
        /// <param name="kc"></param>
        public virtual async Task UpdateKnowledgebaseCategory(KnowledgebaseCategory kc)
        {
            kc.UpdatedOnUtc = DateTime.UtcNow;
            await _knowledgebaseCategoryRepository.UpdateAsync(kc);
            await _cacheManager.RemoveByPrefix(ARTICLES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(CATEGORIES_PATTERN_KEY);
            await _mediator.EntityUpdated(kc);
        }

        /// <summary>
        /// Gets knowledgebase category
        /// </summary>
        /// <param name="id"></param>
        /// <returns>knowledgebase category</returns>
        public virtual Task<KnowledgebaseCategory> GetKnowledgebaseCategory(string id)
        {
            return _knowledgebaseCategoryRepository.Table.Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets knowledgebase category
        /// </summary>
        /// <param name="id"></param>
        /// <returns>knowledgebase category</returns>
        public virtual async Task<KnowledgebaseCategory> GetPublicKnowledgebaseCategory(string id)
        {
            string key = string.Format(CATEGORY_BY_ID, id, _workContext.CurrentCustomer.GetCustomerRoleIds(),
                _storeContext.CurrentStore.Id);
            return await _cacheManager.GetAsync(key, () =>
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
                return toReturn.FirstOrDefaultAsync();
            });
        }

        /// <summary>
        /// Inserts knowledgebase category
        /// </summary>
        /// <param name="kc"></param>
        public virtual async Task InsertKnowledgebaseCategory(KnowledgebaseCategory kc)
        {
            kc.CreatedOnUtc = DateTime.UtcNow;
            kc.UpdatedOnUtc = DateTime.UtcNow;
            await _knowledgebaseCategoryRepository.InsertAsync(kc);
            await _cacheManager.RemoveByPrefix(ARTICLES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(CATEGORIES_PATTERN_KEY);
            await _mediator.EntityInserted(kc);
        }

        /// <summary>
        /// Gets knowledgebase categories
        /// </summary>
        /// <returns>List of knowledgebase categories</returns>
        public virtual async Task<List<KnowledgebaseCategory>> GetKnowledgebaseCategories()
        {
            var categories = await _knowledgebaseCategoryRepository.Table.OrderBy(x => x.ParentCategoryId).ThenBy(x => x.DisplayOrder).ToListAsync();
            return categories.SortCategoriesForTree();
        }

        /// <summary>
        /// Gets knowledgebase article
        /// </summary>
        /// <param name="id"></param>
        /// <returns>knowledgebase article</returns>
        public virtual Task<KnowledgebaseArticle> GetKnowledgebaseArticle(string id)
        {
            return _knowledgebaseArticleRepository.Table.Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets knowledgebase articles
        /// </summary>
        /// <returns>List of knowledgebase articles</returns>
        /// <param name="storeId">Store ident</param>
        public virtual async Task<List<KnowledgebaseArticle>> GetKnowledgebaseArticles(string storeId = "")
        {
            var builder = Builders<KnowledgebaseArticle>.Filter;
            var filter = FilterDefinition<KnowledgebaseArticle>.Empty;
            filter &= builder.Where(x => x.Published);

            if (!_catalogSettings.IgnoreAcl)
            {
                var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                filter &= (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));
            }

            if (!_catalogSettings.IgnoreStoreLimitations && !string.IsNullOrEmpty(storeId))
            {
                //Store mapping
                var currentStoreId = new List<string> { storeId };
                filter &= (builder.AnyIn(x => x.Stores, currentStoreId) | builder.Where(x => !x.LimitedToStores));
            }

            var builderSort = Builders<KnowledgebaseArticle>.Sort.Ascending(x => x.DisplayOrder);
            var toReturn = _knowledgebaseArticleRepository.Collection.Find(filter).Sort(builderSort);
            return await toReturn.ToListAsync();
        }

        /// <summary>
        /// Inserts knowledgebase article
        /// </summary>
        /// <param name="ka"></param>
        public virtual async Task InsertKnowledgebaseArticle(KnowledgebaseArticle ka)
        {
            ka.CreatedOnUtc = DateTime.UtcNow;
            ka.UpdatedOnUtc = DateTime.UtcNow;
            await _knowledgebaseArticleRepository.InsertAsync(ka);
            await _cacheManager.RemoveByPrefix(ARTICLES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(CATEGORIES_PATTERN_KEY);
            await _mediator.EntityInserted(ka);
        }

        /// <summary>
        /// Edits knowledgebase article
        /// </summary>
        /// <param name="ka"></param>
        public virtual async Task UpdateKnowledgebaseArticle(KnowledgebaseArticle ka)
        {
            ka.UpdatedOnUtc = DateTime.UtcNow;
            await _knowledgebaseArticleRepository.UpdateAsync(ka);
            await _cacheManager.RemoveByPrefix(ARTICLES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(CATEGORIES_PATTERN_KEY);
            await _mediator.EntityUpdated(ka);
        }

        /// <summary>
        /// Deletes knowledgebase article
        /// </summary>
        /// <param name="id"></param>
        public virtual async Task DeleteKnowledgebaseArticle(KnowledgebaseArticle ka)
        {
            await _knowledgebaseArticleRepository.DeleteAsync(ka);
            await _cacheManager.RemoveByPrefix(ARTICLES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(CATEGORIES_PATTERN_KEY);
            await _mediator.EntityDeleted(ka);
        }

        /// <summary>
        /// Gets knowledgebase articles by category id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IPagedList<KnowledgebaseArticle></returns>
        public virtual async Task<IPagedList<KnowledgebaseArticle>> GetKnowledgebaseArticlesByCategoryId(string id, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var articles = await _knowledgebaseArticleRepository.Table.Where(x => x.ParentCategoryId == id).OrderBy(x => x.DisplayOrder).ToListAsync();
            return new PagedList<KnowledgebaseArticle>(articles, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets public(published etc) knowledgebase categories
        /// </summary>
        /// <returns>List of public knowledgebase categories</returns>
        public virtual async Task<List<KnowledgebaseCategory>> GetPublicKnowledgebaseCategories()
        {
            var key = string.Format(CATEGORIES, string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return await _cacheManager.GetAsync(key, () =>
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
                return toReturn.ToListAsync();
            });
        }

        /// <summary>
        /// Gets public(published etc) knowledgebase articles
        /// </summary>
        /// <returns>List of public knowledgebase articles</returns>
        public virtual async Task<List<KnowledgebaseArticle>> GetPublicKnowledgebaseArticles()
        {
            var key = string.Format(ARTICLES, string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return await _cacheManager.GetAsync(key, () =>
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
                return toReturn.ToListAsync();
            });
        }

        /// <summary>
        /// Gets knowledgebase article if it is published etc
        /// </summary>
        /// <returns>knowledgebase article</returns>
        public virtual async Task<KnowledgebaseArticle> GetPublicKnowledgebaseArticle(string id)
        {
            var key = string.Format(ARTICLE_BY_ID, id, string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return await _cacheManager.GetAsync(key, () =>
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

                return _knowledgebaseArticleRepository.Collection.Find(filter).FirstOrDefaultAsync();
            });
        }

        /// <summary>
        /// Gets public(published etc) knowledgebase articles for category id
        /// </summary>
        /// <returns>List of public knowledgebase articles</returns>
        public virtual async Task<List<KnowledgebaseArticle>> GetPublicKnowledgebaseArticlesByCategory(string categoryId)
        {
            var key = string.Format(ARTICLES_BY_CATEGORY_ID, categoryId, string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return await _cacheManager.GetAsync(key, () =>
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
                return toReturn.ToListAsync();
            });
        }

        /// <summary>
        /// Gets public(published etc) knowledgebase articles for keyword
        /// </summary>
        /// <returns>List of public knowledgebase articles</returns>
        public virtual async Task<List<KnowledgebaseArticle>> GetPublicKnowledgebaseArticlesByKeyword(string keyword)
        {
            var key = string.Format(ARTICLES_BY_KEYWORD, keyword, string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return await _cacheManager.GetAsync(key, () =>
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
                return toReturn.ToListAsync();
            });
        }

        /// <summary>
        /// Gets public(published etc) knowledgebase categories for keyword
        /// </summary>
        /// <returns>List of public knowledgebase categories</returns>
        public virtual async Task<List<KnowledgebaseCategory>> GetPublicKnowledgebaseCategoriesByKeyword(string keyword)
        {
            var key = string.Format(CATEGORIES_BY_KEYWORD, keyword, string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return await _cacheManager.GetAsync(key, () =>
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
                return toReturn.ToListAsync();
            });
        }

        /// <summary>
        /// Gets homepage knowledgebase articles
        /// </summary>
        /// <returns>List of homepage knowledgebase articles</returns>
        public virtual async Task<List<KnowledgebaseArticle>> GetHomepageKnowledgebaseArticles()
        {
            var key = string.Format(HOMEPAGE_ARTICLES, string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return await _cacheManager.GetAsync(key, () =>
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
                return toReturn.ToListAsync();
            });
        }

        /// <summary>
        /// Gets knowledgebase articles by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>IPagedList<KnowledgebaseArticle></returns>
        public virtual async Task<IPagedList<KnowledgebaseArticle>> GetKnowledgebaseArticlesByName(string name, int pageIndex = 0, int pageSize = int.MaxValue)
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
            var toReturn = await _knowledgebaseArticleRepository.Collection.Find(filter).Sort(builderSort).ToListAsync();

            return new PagedList<KnowledgebaseArticle>(toReturn, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets related knowledgebase articles
        /// </summary>
        /// <param name="name"></param>
        /// <returns>IPagedList<KnowledgebaseArticle></returns>
        public virtual async Task<IPagedList<KnowledgebaseArticle>> GetRelatedKnowledgebaseArticles(string articleId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var article = await GetKnowledgebaseArticle(articleId);
            List<KnowledgebaseArticle> toReturn = new List<KnowledgebaseArticle>();

            foreach (var id in article.RelatedArticles)
            {
                var relatedArticle = await GetKnowledgebaseArticle(id);
                if (relatedArticle != null)
                    toReturn.Add(relatedArticle);
            }

            return new PagedList<KnowledgebaseArticle>(toReturn.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Inserts an article comment
        /// </summary>
        /// <param name="articleComment">Article comment</param>
        public virtual async Task InsertArticleComment(KnowledgebaseArticleComment articleComment)
        {
            if (articleComment == null)
                throw new ArgumentNullException("articleComment");

            await _articleCommentRepository.InsertAsync(articleComment);

            //event notification
            await _mediator.EntityInserted(articleComment);
        }

        /// <summary>
        /// Gets all comments
        /// </summary>
        /// <param name="customerId">Customer identifier; "" to load all records</param>
        /// <returns>Comments</returns>
        public virtual async Task<IList<KnowledgebaseArticleComment>> GetAllComments(string customerId)
        {
            var query = from c in _articleCommentRepository.Table
                        orderby c.CreatedOnUtc
                        where (customerId == "" || c.CustomerId == customerId)
                        select c;
            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets an article comment
        /// </summary>
        /// <param name="articleId">Article identifier</param>
        /// <returns>Article comment</returns>
        public virtual Task<KnowledgebaseArticleComment> GetArticleCommentById(string articleId)
        {
            return _articleCommentRepository.GetByIdAsync(articleId);
        }

        /// <summary>
        /// Get article comments by identifiers
        /// </summary>
        /// <param name="commentIds"Article comment identifiers</param>
        /// <returns>Article comments</returns>
        public virtual async Task<IList<KnowledgebaseArticleComment>> GetArticleCommentsByIds(string[] commentIds)
        {
            if (commentIds == null || commentIds.Length == 0)
                return new List<KnowledgebaseArticleComment>();

            var query = from bc in _articleCommentRepository.Table
                        where commentIds.Contains(bc.Id)
                        select bc;
            var comments = await query.ToListAsync();
            //sort by passed identifiers
            var sortedComments = new List<KnowledgebaseArticleComment>();
            foreach (string id in commentIds)
            {
                var comment = comments.Find(x => x.Id == id);
                if (comment != null)
                    sortedComments.Add(comment);
            }
            return sortedComments;
        }

        public virtual async Task<IList<KnowledgebaseArticleComment>> GetArticleCommentsByArticleId(string articleId)
        {
            var query = from c in _articleCommentRepository.Table
                        where c.ArticleId == articleId
                        orderby c.CreatedOnUtc
                        select c;
            return await query.ToListAsync();
        }

        public virtual async Task DeleteArticleComment(KnowledgebaseArticleComment articleComment)
        {
            if (articleComment == null)
                throw new ArgumentNullException("articleComment");

            await _articleCommentRepository.DeleteAsync(articleComment);
        }
    }
}
