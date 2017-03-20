using Grand.Core;

namespace Grand.Services.Common
{
    /// <summary>
    /// Generic attribute service interface
    /// </summary>
    public partial interface IGenericAttributeService
    {
        void SaveAttribute<TPropType>(BaseEntity entity, string key, TPropType value, string storeId = "");
        TPropType GetAttributesForEntity<TPropType>(BaseEntity entity, string key, string storeId = "");
    }
}