using Grand.Core;
using Grand.Core.Data;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Grand.Services.Common
{
    /// <summary>
    /// History service interface
    /// </summary>
    public partial class HistoryService : IHistoryService
    {
        private readonly IRepository<HistoryObject> _historyRepository;

        public HistoryService(IRepository<HistoryObject> historyRepository)
        {
            _historyRepository = historyRepository;
        }

        public virtual void SaveObject<T>(T entity) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            var history = new HistoryObject()
            {
                CreatedOnUtc = DateTime.UtcNow,
                Object = entity
            };
            _historyRepository.Insert(history);

        }

        public virtual IList<T> GetHistoryForEntity<T>(BaseEntity entity) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            var history = _historyRepository.Table.Where(x => x.Object.Id == entity.Id).Select(x => (T)x.Object).ToList();
            return history;
        }

        public virtual IList<HistoryObject> GetHistoryObjectForEntity(BaseEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            var history = _historyRepository.Table.Where(x => x.Object.Id == entity.Id).ToList();
            return history;
        }
    }
}