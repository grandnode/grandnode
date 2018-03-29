using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grand.Core.Data;
using Grand.Core.Domain.Knowledgebase;
using Grand.Services.Events;

namespace Grand.Services.Knowledgebase
{
    public class KnowledgebaseService : IKnowledgebaseService
    {
        private IRepository<KnowledgebaseCategory> _knowledgebaseCategoryRepository;
        private readonly IEventPublisher _eventPublisher;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="knowledgebaseCategoryRepository"></param>
        /// <param name="eventPublisher"></param>
        public KnowledgebaseService(IRepository<KnowledgebaseCategory> knowledgebaseCategoryRepository, IEventPublisher eventPublisher)
        {
            this._knowledgebaseCategoryRepository = knowledgebaseCategoryRepository;
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
    }
}
