using Grand.Domain;
using Grand.Domain.History;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Common
{
    /// <summary>
    /// History service interface
    /// </summary>
    public partial interface IHistoryService
    {
        Task SaveObject<T>(T entity) where T : BaseEntity;
        Task<IList<T>> GetHistoryForEntity<T>(BaseEntity entity) where T : BaseEntity;
        Task<IList<HistoryObject>> GetHistoryObjectForEntity(BaseEntity entity);
    }
}