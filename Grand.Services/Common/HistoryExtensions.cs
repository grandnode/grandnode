using Grand.Core;
using Grand.Core.Infrastructure;
using System;
using System.Collections.Generic;

namespace Grand.Services.Common
{
    public static class HistoryExtensions
    {
        /// <summary>
        /// Save an entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public static void SaveHistory<T>(this BaseEntity entity) where T : BaseEntity, IHistory
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            var historyService = EngineContext.Current.Resolve<IHistoryService>();
            historyService.SaveObject(entity);
        }

        public static IList<T> GetHistory<T>(this BaseEntity entity) where T : BaseEntity, IHistory
        {
            var historyService = EngineContext.Current.Resolve<IHistoryService>();
            return historyService.GetHistoryForEntity<T>(entity);
        }

        public static IList<HistoryObject> GetHistoryObject(this BaseEntity entity)
        {
            var historyService = EngineContext.Current.Resolve<IHistoryService>();
            return historyService.GetHistoryObjectForEntity(entity);
        }
    }
}
