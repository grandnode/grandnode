using Grand.Domain;
using System.Threading.Tasks;

namespace Grand.Services.Common
{
    /// <summary>
    /// Generic attribute service interface
    /// </summary>
    public partial interface IGenericAttributeService
    {
        Task SaveAttribute<TPropType>(BaseEntity entity, string key, TPropType value, string storeId = "");
        Task SaveAttribute<TPropType>(string entity, string entityId, string key, TPropType value, string storeId = "");
        Task<TPropType> GetAttributesForEntity<TPropType>(BaseEntity entity, string key, string storeId = "");
    }
}