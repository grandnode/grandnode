using Grand.Core;
using System.Collections.Generic;

namespace Grand.Services.Common
{
    /// <summary>
    /// History service interface
    /// </summary>
    public partial interface IHistoryService
    {
        void SaveObject<T>(T entity) where T : BaseEntity;
        IList<T> GetHistoryForEntity<T>(BaseEntity entity) where T : BaseEntity;
        IList<HistoryObject> GetHistoryObjectForEntity(BaseEntity entity);
    }
}