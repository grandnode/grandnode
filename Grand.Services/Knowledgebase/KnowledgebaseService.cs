using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Grand.Core;
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
        private readonly IRepository<KnowledgebaseCategory> _knowledgebaseCategoryRepository;
        private readonly IRepository<KnowledgebaseArticle> _knowledgebaseArticleRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly CommonSettings _commonSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly IWorkContext _workContext;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="knowledgebaseCategoryRepository"></param>
        /// <param name="knowledgebaseArticleRepository"></param>
        /// <param name="eventPublisher"></param>
        public KnowledgebaseService(IRepository<KnowledgebaseCategory> knowledgebaseCategoryRepository,
            IRepository<KnowledgebaseArticle> knowledgebaseArticleRepository, IEventPublisher eventPublisher, CommonSettings commonSettings,
            CatalogSettings catalogSettings, IWorkContext workContext)
        {
            this._knowledgebaseCategoryRepository = knowledgebaseCategoryRepository;
            this._knowledgebaseArticleRepository = knowledgebaseArticleRepository;
            this._eventPublisher = eventPublisher;
            this._commonSettings = commonSettings;
            this._catalogSettings = catalogSettings;
            this._workContext = workContext;
        }

        /// <summary>
        /// Deletes knowledgebase category
        /// </summary>
        /// <param name="id"></param>
        public void DeleteKnowledgebaseCategory(KnowledgebaseCategory kc)
        {
            var children = _knowledgebaseCategoryRepository.Table.Where(x => x.ParentCategoryId == kc.Id).ToList();
            _knowledgebaseCategoryRepository.Delete(kc);
            foreach (var child in children)
            {
                child.ParentCategoryId = "";
                UpdateKnowledgebaseCategory(child);
            }

            _eventPublisher.EntityDeleted(kc);
        }

        /// <summary>
        /// Edits knowledgebase category
        /// </summary>
        /// <param name="kc"></param>
        public void UpdateKnowledgebaseCategory(KnowledgebaseCategory kc)
        {
            _knowledgebaseCategoryRepository.Update(kc);
            _eventPublisher.EntityUpdated(kc);
        }

        /// <summary>
        /// Gets knowledgebase category
        /// </summary>
        /// <param name="id"></param>
        /// <returns>knowledgebase category</returns>
        public KnowledgebaseCategory GetKnowledgebaseCategory(string id)
        {
            return _knowledgebaseCategoryRepository.Table.Where(x => x.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Inserts knowledgebase category
        /// </summary>
        /// <param name="kc"></param>
        public void InsertKnowledgebaseCategory(KnowledgebaseCategory kc)
        {
            _knowledgebaseCategoryRepository.Insert(kc);
            _eventPublisher.EntityInserted(kc);
        }

        /// <summary>
        /// Gets knowledgebase categories
        /// </summary>
        /// <returns>List of knowledgebase categories</returns>
        public List<KnowledgebaseCategory> GetKnowledgebaseCategories()
        {
            return _knowledgebaseCategoryRepository.Table.OrderBy(x => x.DisplayOrder).ToList();
        }

        /// <summary>
        /// Gets knowledgebase article
        /// </summary>
        /// <param name="id"></param>
        /// <returns>knowledgebase article</returns>
        public KnowledgebaseArticle GetKnowledgebaseArticle(string id)
        {
            return _knowledgebaseArticleRepository.Table.Where(x => x.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Gets knowledgebase articles
        /// </summary>
        /// <returns>List of knowledgebase articles</returns>
        public List<KnowledgebaseArticle> GetKnowledgebaseArticles()
        {
            return _knowledgebaseArticleRepository.Table.OrderBy(x => x.DisplayOrder).ToList();
        }

        /// <summary>
        /// Inserts knowledgebase article
        /// </summary>
        /// <param name="ka"></param>
        public void InsertKnowledgebaseArticle(KnowledgebaseArticle ka)
        {
            _knowledgebaseArticleRepository.Insert(ka);
            _eventPublisher.EntityInserted(ka);
        }

        /// <summary>
        /// Edits knowledgebase article
        /// </summary>
        /// <param name="ka"></param>
        public void UpdateKnowledgebaseArticle(KnowledgebaseArticle ka)
        {
            _knowledgebaseArticleRepository.Update(ka);
            _eventPublisher.EntityUpdated(ka);
        }

        /// <summary>
        /// Deletes knowledgebase article
        /// </summary>
        /// <param name="id"></param>
        public void DeleteKnowledgebaseArticle(KnowledgebaseArticle ka)
        {
            _knowledgebaseArticleRepository.Delete(ka);
            _eventPublisher.EntityDeleted(ka);
        }

        /// <summary>
        /// Gets knowledgebase articles by category id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IPagedList<KnowledgebaseArticle></returns>
        public IPagedList<KnowledgebaseArticle> GetKnowledgebaseArticlesByCategoryId(string id, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var articles = _knowledgebaseArticleRepository.Table.Where(x => x.ParentCategoryId == id).ToList();
            return new PagedList<KnowledgebaseArticle>(articles, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets public(published etc) knowledgebase categories
        /// </summary>
        /// <returns>List of public knowledgebase categories</returns>
        public List<KnowledgebaseCategory> GetPublicKnowledgebaseCategories()
        {
            var builder = Builders<KnowledgebaseCategory>.Filter;
            var filter = FilterDefinition<KnowledgebaseCategory>.Empty;
            filter = filter & builder.Where(x => x.Published);

            if (!_catalogSettings.IgnoreAcl)
            {
                var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                filter = filter & (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));
            }

            var builderSort = Builders<KnowledgebaseCategory>.Sort.Ascending(x => x.DisplayOrder);
            var toReturn = _knowledgebaseCategoryRepository.Collection.Find(filter).Sort(builderSort);
            return toReturn.ToList();
        }

        /// <summary>
        /// Gets public(published etc) knowledgebase articles
        /// </summary>
        /// <returns>List of public knowledgebase articles</returns>
        public List<KnowledgebaseArticle> GetPublicKnowledgebaseArticles()
        {
            var builder = Builders<KnowledgebaseArticle>.Filter;
            var filter = FilterDefinition<KnowledgebaseArticle>.Empty;
            filter = filter & builder.Where(x => x.Published);

            if (!_catalogSettings.IgnoreAcl)
            {
                var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                filter = filter & (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));
            }

            var builderSort = Builders<KnowledgebaseArticle>.Sort.Ascending(x => x.DisplayOrder);
            var toReturn = _knowledgebaseArticleRepository.Collection.Find(filter).Sort(builderSort);
            return toReturn.ToList();
        }

        /// <summary>
        /// Gets public(published etc) knowledgebase articles for category id
        /// </summary>
        /// <returns>List of public knowledgebase articles</returns>
        public List<KnowledgebaseArticle> GetPublicKnowledgebaseArticlesByCategory(string categoryId)
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

            var builderSort = Builders<KnowledgebaseArticle>.Sort.Ascending(x => x.DisplayOrder);
            var toReturn = _knowledgebaseArticleRepository.Collection.Find(filter).Sort(builderSort);
            return toReturn.ToList();
        }

        /// <summary>
        /// Gets public(published etc) knowledgebase articles for keyword
        /// </summary>
        /// <returns>List of public knowledgebase articles</returns>
        public List<KnowledgebaseArticle> GetPublicKnowledgebaseArticlesByKeyword(string keyword)
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

            var builderSort = Builders<KnowledgebaseArticle>.Sort.Ascending(x => x.DisplayOrder);
            var toReturn = _knowledgebaseArticleRepository.Collection.Find(filter).Sort(builderSort);
            return toReturn.ToList();
        }

        /// <summary>
        /// Gets homepage knowledgebase articles
        /// </summary>
        /// <returns>List of homepage knowledgebase articles</returns>
        public List<KnowledgebaseArticle> GetHomepageKnowledgebaseArticles()
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

            var builderSort = Builders<KnowledgebaseArticle>.Sort.Ascending(x => x.DisplayOrder);
            var toReturn = _knowledgebaseArticleRepository.Collection.Find(filter).Sort(builderSort);
            return toReturn.ToList();
        }
    }
}
