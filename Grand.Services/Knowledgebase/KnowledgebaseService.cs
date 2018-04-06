using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Knowledgebase;
using Grand.Services.Events;

namespace Grand.Services.Knowledgebase
{
    public class KnowledgebaseService : IKnowledgebaseService
    {
        private IRepository<KnowledgebaseCategory> _knowledgebaseCategoryRepository;
        private IRepository<KnowledgebaseArticle> _knowledgebaseArticleRepository;
        private readonly IEventPublisher _eventPublisher;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="knowledgebaseCategoryRepository"></param>
        /// <param name="knowledgebaseArticleRepository"></param>
        /// <param name="eventPublisher"></param>
        public KnowledgebaseService(IRepository<KnowledgebaseCategory> knowledgebaseCategoryRepository,
            IRepository<KnowledgebaseArticle> knowledgebaseArticleRepository, IEventPublisher eventPublisher)
        {
            this._knowledgebaseCategoryRepository = knowledgebaseCategoryRepository;
            this._knowledgebaseArticleRepository = knowledgebaseArticleRepository;
            this._eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Deletes knowledgebase category
        /// </summary>
        /// <param name="id"></param>
        public void DeleteKnowledgebaseCategory(KnowledgebaseCategory kc)
        {
            _knowledgebaseCategoryRepository.Delete(kc);
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
            return _knowledgebaseCategoryRepository.Table.ToList();
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
            return _knowledgebaseArticleRepository.Table.ToList();
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
    }
}
